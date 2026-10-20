using ApiInnovacion.Respuestas;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

// -----------------------------------------------------------------------------
// Mapeo columna -> propiedad en Dapper.
//
// La base nombra las columnas en snake_case (gran_area, razon_social,
// nombre_contacto...) y las entidades en C# usan PascalCase (GranArea,
// RazonSocial, NombreContacto...). Sin esta linea Dapper solo compara nombres
// ignorando mayusculas, asi que las columnas con guion bajo NO se mapean y
// llegan como null al JSON de respuesta.
//
// La convencion se declara una sola vez, aqui en el ensamblador, porque es
// global a todos los repositorios; el SQL de cada repositorio se mantiene tal
// como lo muestra 3_plan.md, Seccion 4.5, sin alias por columna.
// -----------------------------------------------------------------------------
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexion: variable de entorno DB_CONNECTION (la usa docker-compose) o,
// si no esta, appsettings.json (por defecto localhost,11467 para correr sin Docker).
// Tambien es sobrescribible por ConnectionStrings__SqlServer.
var cadenaConexion = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? builder.Configuration.GetConnectionString("SqlServer")
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

        // Las columnas DATE (fecha_fun, premio.fecha) se serializan como
        // "yyyy-MM-dd", sin hora: lo que espera el <input type="date"> del frontend.
        // Fuente: 6_contracts.md §0.4.
        opciones.JsonSerializerOptions.Converters.Add(new SoloFechaJsonConverter());
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

builder.Services.AddScoped<IRepositorioEnfoque>(
    _ => new RepositorioEnfoqueSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioEnfoque, ServicioEnfoque>();

builder.Services.AddScoped<IRepositorioAspectoNormativo>(
    _ => new RepositorioAspectoNormativoSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAspectoNormativo, ServicioAspectoNormativo>();

builder.Services.AddScoped<IRepositorioPracticaEstrategia>(
    _ => new RepositorioPracticaEstrategiaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioPracticaEstrategia, ServicioPracticaEstrategia>();

// Issue #3 - car_innovacion y aliado.
builder.Services.AddScoped<IRepositorioCarInnovacion>(
    _ => new RepositorioCarInnovacionSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioCarInnovacion, ServicioCarInnovacion>();

builder.Services.AddScoped<IRepositorioAliado>(
    _ => new RepositorioAliadoSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAliado, ServicioAliado>();

// Issue #1 (v2) - facultad, pasantia y premio (procedimientos almacenados).
builder.Services.AddScoped<IRepositorioFacultad>(
    _ => new RepositorioFacultadSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioFacultad, ServicioFacultad>();

builder.Services.AddScoped<IRepositorioPasantia>(
    _ => new RepositorioPasantiaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioPasantia, ServicioPasantia>();

builder.Services.AddScoped<IRepositorioPremio>(
    _ => new RepositorioPremioSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioPremio, ServicioPremio>();

// Issue #2 (v2) - programa, acreditacion, registro_calificado, activ_academica
// y las puentes enfoque_rc y aa_rc (procedimientos almacenados).
builder.Services.AddScoped<IRepositorioPrograma>(
    _ => new RepositorioProgramaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioPrograma, ServicioPrograma>();

builder.Services.AddScoped<IRepositorioAcreditacion>(
    _ => new RepositorioAcreditacionSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAcreditacion, ServicioAcreditacion>();

builder.Services.AddScoped<IRepositorioRegistroCalificado>(
    _ => new RepositorioRegistroCalificadoSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioRegistroCalificado, ServicioRegistroCalificado>();

builder.Services.AddScoped<IRepositorioActivAcademica>(
    _ => new RepositorioActivAcademicaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioActivAcademica, ServicioActivAcademica>();

builder.Services.AddScoped<IRepositorioEnfoqueRc>(
    _ => new RepositorioEnfoqueRcSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioEnfoqueRc, ServicioEnfoqueRc>();

builder.Services.AddScoped<IRepositorioAaRc>(
    _ => new RepositorioAaRcSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAaRc, ServicioAaRc>();

// Issue 3 (v2): relaciones de Felipe. Cada par conserva el ensamblaje de la v1.
builder.Services.AddScoped<IRepositorioProgramaOpciones>(
    _ => new RepositorioProgramaOpcionesSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioProgramaOpciones, ServicioProgramaOpciones>();

// Catálogo local: su ID identifica la ficha del sistema, el documento identifica
// a la persona y nunca ocupa el lugar de la llave primaria.
builder.Services.AddScoped<IRepositorioDocente>(
    _ => new RepositorioDocenteSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioDocente, ServicioDocente>();

builder.Services.AddScoped<IRepositorioDocenteDepartamento>(
    _ => new RepositorioDocenteDepartamentoSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioDocenteDepartamento, ServicioDocenteDepartamento>();

builder.Services.AddScoped<IRepositorioAlianza>(
    _ => new RepositorioAlianzaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAlianza, ServicioAlianza>();

builder.Services.AddScoped<IRepositorioProgramaAc>(
    _ => new RepositorioProgramaAcSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioProgramaAc, ServicioProgramaAc>();

builder.Services.AddScoped<IRepositorioProgramaPe>(
    _ => new RepositorioProgramaPeSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioProgramaPe, ServicioProgramaPe>();

builder.Services.AddScoped<IRepositorioProgramaCi>(
    _ => new RepositorioProgramaCiSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioProgramaCi, ServicioProgramaCi>();

builder.Services.AddScoped<IRepositorioAnPrograma>(
    _ => new RepositorioAnProgramaSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAnPrograma, ServicioAnPrograma>();

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
    Version = "v2",
    Contratos = "docs/spec_kit/versiones/v2/6_contracts.md"
}));

app.Run();
