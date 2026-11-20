using System.Text;
using System.Threading.RateLimiting;
using ApiInnovacion.Configuracion;
using ApiInnovacion.Middlewares;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

// -----------------------------------------------------------------------------
// Configuracion JWT (v3). El secreto NUNCA debe quedar hardcodeado en un
// repositorio real: en Docker se inyecta Jwt__Secret desde JWT_SECRET.
// -----------------------------------------------------------------------------
var jwt = builder.Configuration.GetSection(JwtOpciones.Seccion).Get<JwtOpciones>()
    ?? throw new InvalidOperationException("La sección Jwt no está configurada.");

if (string.IsNullOrWhiteSpace(jwt.Secret) || Encoding.UTF8.GetByteCount(jwt.Secret) < 32)
    throw new InvalidOperationException("Jwt:Secret debe tener al menos 32 caracteres.");

if (jwt.ExpirationMinutes <= 0)
    throw new InvalidOperationException("Jwt:ExpirationMinutes debe ser mayor que 0.");

builder.Services.Configure<JwtOpciones>(builder.Configuration.GetSection(JwtOpciones.Seccion));

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
// Se agrega el candado Bearer para poder probar endpoints protegidos con el
// token devuelto por /api/login.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue el token JWT devuelto por POST /api/login."
    });

    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// -----------------------------------------------------------------------------
// Autenticacion y autorizacion (v3).
//  * JWT HS256 validado con emisor, audiencia, firma y expiracion.
//  * Politica de respaldo: toda ruta exige usuario autenticado salvo las que
//    declaran [AllowAnonymous] (login y diagnostico).
//  * Rate limit del login: 10 intentos por minuto y por IP.
// -----------------------------------------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "unique_name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(opciones =>
{
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Una ventana fija por IP: 10 intentos por minuto. Si no se puede resolver
    // la IP se usa una partición compartida ("desconocido"), nunca una global.
    opciones.AddPolicy("login", contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    opciones.OnRejected = async (contexto, token) =>
    {
        contexto.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        await contexto.HttpContext.Response.WriteAsJsonAsync(new RespuestaError
        {
            Estado = StatusCodes.Status429TooManyRequests,
            Mensaje = "Demasiados intentos. Espere un minuto e intente de nuevo.",
            Detalle = "Límite de intentos de inicio de sesión."
        }, cancellationToken: token);
    };
});

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

// v3 - autenticacion. El hasher y el emisor de tokens no tienen estado por
// peticion (bcrypt usa sal interna y el JWT se firma con la configuracion),
// por eso son singleton. El repositorio y el servicio trabajan por peticion.
builder.Services.AddSingleton<IServicioHashContrasena, ServicioHashContrasena>();
builder.Services.AddSingleton<ITokenJwt, ServicioTokenJwt>();
builder.Services.AddScoped<IRepositorioUsuario>(
    _ => new RepositorioUsuarioSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();

var app = builder.Build();

// -----------------------------------------------------------------------------
// Pipeline: CORS -> rate limit -> autenticacion -> autorizacion -> regla de
// escritura por rol -> Swagger (solo dev) -> rutas.
// -----------------------------------------------------------------------------
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RestringirEscrituraMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// -----------------------------------------------------------------------------
// GET / -> Diagnostico (RF7). Fuente: 6_contracts.md §1.
// Es publico a proposito: sirve para verificar que la API esta viva antes de
// iniciar sesion. El resto de rutas queda protegido por la politica de respaldo.
// -----------------------------------------------------------------------------
app.MapGet("/", () => Results.Json(new RespuestaDiagnostico
{
    Mensaje = "API Innovación Curricular funcionando",
    Version = "v2",
    Contratos = "docs/spec_kit/versiones/v2/6_contracts.md"
})).AllowAnonymous();

app.Run();
