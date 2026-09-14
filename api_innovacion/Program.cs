using ApiInnovacion.Respuestas;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexion: appsettings.json (por defecto localhost,11467 para correr
// sin Docker) sobrescribible por variable de entorno ConnectionStrings__SqlServer.
var cadenaConexion = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("ConnectionStrings:SqlServer no está configurada.");

var origenFrontend = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:8037";

// -----------------------------------------------------------------------------
// Controladores + serializacion. Las entidades se serializan en snake_case
// (la API responde con los contratos de 6_contracts.md), mientras que las
// respuestas auxiliares usan nombres explicitos via JsonPropertyName.
// -----------------------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(opciones =>
    {
        opciones.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
    });

// Respuesta 422 personalizada: body invalido -> {estado, mensaje, errores[...]}.
// La validacion ocurre ANTES de tocar SQL Server. Fuente: 6_contracts.md §0.5.
builder.Services.Configure<ApiBehaviorOptions>(opciones =>
{
    opciones.InvalidModelStateResponseFactory = contexto =>
    {
        var errores = contexto.ModelState.Values
            .SelectMany(valor => valor.Errors)
            .Select(error => error.ErrorMessage)
            .Where(mensaje => !string.IsNullOrWhiteSpace(mensaje))
            .ToList();

        return new ObjectResult(new
        {
            estado = StatusCodes.Status422UnprocessableEntity,
            mensaje = "Datos inválidos.",
            errores
        })
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity
        };
    };
});

// CORS para el Frontend (http://localhost:8037). Fuente: 3_plan.md, Seccion 5.
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("Frontend", politica =>
        politica.WithOrigins(origenFrontend)
                .AllowAnyHeader()
                .AllowAnyMethod());
});

// Swagger: documentacion interactiva en /swagger. Fuente: 6_contracts.md §10.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------------------------------------------------------------
// ENSAMBLADOR: registros de dependencias solos (nadie mas conoce clases concretas).
// Un par por tabla: IRepositorio<Tabla> -> Repositorio<Tabla>SqlServer y
// IServicio<Tabla> -> Servicio<Tabla>. Fuente: 3_plan.md, Seccion 4.4.
// -----------------------------------------------------------------------------
builder.Services.AddScoped<IRepositorioAreaConocimiento>(
    _ => new RepositorioAreaConocimientoSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAreaConocimiento, ServicioAreaConocimiento>();

builder.Services.AddScoped<IRepositorioUniversidad>(
    _ => new RepositorioUniversidadSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioUniversidad, ServicioUniversidad>();

var app = builder.Build();

// -----------------------------------------------------------------------------
// Pipeline: CORS -> Swagger (solo dev) -> rutas.
// -----------------------------------------------------------------------------
app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// -----------------------------------------------------------------------------
// GET / -> Diagnostico (RF7). Fuente: 6_contracts.md §1.
// -----------------------------------------------------------------------------
app.MapGet("/", () => Results.Json(new RespuestaDiagnostico
{
    Mensaje = "API Innovación Curricular funcionando",
    Version = "v1",
    Contratos = "docs/spec_kit/versiones/v1_producto_sqlserver/6_contracts.md"
}));

app.Run();