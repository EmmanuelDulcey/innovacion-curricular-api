# Plan técnico — Versión 3: control de acceso

> **Versión 3** · CÓMO construir lo especificado en [2_spec.md](2_spec.md).
> El porqué de cada decisión: [4_research.md](4_research.md) · el modelo de
> datos: [5_data_model.md](5_data_model.md) · contratos exactos:
> [6_contracts.md](6_contracts.md) · interfaz: [9_frontend.md](9_frontend.md) ·
> orden de trabajo: [8_tasks.md](8_tasks.md).

---

## 1. Stack

La v3 **no cambia** el stack de v1/v2; agrega dos paquetes para autenticación.

| Pieza | Elección | Por qué |
|---|---|---|
| Lenguaje / framework | C# sobre ASP.NET Core (.NET 10) | Igual que v1/v2 |
| Acceso a datos | Dapper sobre `Microsoft.Data.SqlClient`, por procedimientos | Igual que v2; `usuario`/`rol` se consultan igual que cualquier tabla |
| **Nuevo:** hash de contraseñas | `BCrypt.Net-Next` 4.0.3, work factor 12 | Estándar para contraseñas (Art. 10 de la constitución); sal aleatoria por hash |
| **Nuevo:** JWT | `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.15, HS256 | Autenticación sin estado en el servidor |
| **Nuevo:** rate limiting | `Microsoft.AspNetCore.RateLimiting` (incluido en el SDK) | Limitar intentos de login por IP |
| Motor | SQL Server 2022 (mismo contenedor) | `usuario`, `rol`, `rol_usuario` ya existían en el script base, sin usar hasta ahora |
| Frontend | Angular (TypeScript), *standalone*, `HttpClient` | Igual que v1/v2; se agrega un `HttpInterceptorFn` |

## 2. Configuración y secretos

```json
// appsettings.json (valor de desarrollo; NUNCA el secreto real de producción)
"Jwt": {
  "Secret": "InnovacionCurricular-Dev-Secret-2026-...",
  "Issuer": "InnovacionCurricular",
  "Audience": "InnovacionCurricularFrontend",
  "ExpirationMinutes": 480
}
```

- En Docker, `docker-compose.yml` sobreescribe `Jwt__Secret` desde la
  variable de entorno `JWT_SECRET` (con el mismo valor de desarrollo como
  respaldo si no se define). El Artículo 10 de la constitución exige
  secretos en variables de entorno; el valor de `appsettings.json` es solo
  para correr la API fuera de Docker.
- `Program.cs` valida al arrancar que `Jwt:Secret` tenga **al menos 32
  caracteres** y que `Jwt:ExpirationMinutes` sea mayor que 0; si no, la API
  no levanta (falla rápido, no en el primer login).

## 3. Pipeline de middlewares

El orden importa: cada middleware solo puede confiar en lo que el anterior
ya resolvió.

```
UseCors            →  UseRateLimiter       →  UseAuthentication
→  UseAuthorization →  RestringirEscrituraMiddleware  →  MapControllers
```

1. **CORS**: igual que v1/v2.
2. **Rate limiter**: protege `POST /api/login` (10/min/IP) antes de que
   cualquier otra cosa procese la petición.
3. **Authentication**: valida el JWT (firma, emisor, audiencia, expiración)
   y puebla `HttpContext.User` con los claims. No decide si la ruta
   **requiere** sesión; solo identifica si hay una válida.
4. **Authorization**: aplica la **política de respaldo**
   (`RequireAuthenticatedUser`), que exige sesión en toda ruta salvo las
   marcadas `[AllowAnonymous]`. Sin sesión válida → 401 aquí, antes de
   llegar al controlador.
5. **`RestringirEscrituraMiddleware`** (nuevo, propio): solo se ejecuta si
   el paso 4 ya dejó pasar la petición (es decir, el usuario está
   autenticado). Si el verbo no es de lectura y el rol no está en
   `PoliticaRoles.RolesEscritura` → 403. Va **después** de la autorización
   JWT a propósito: nunca ve una petición sin autenticar (esa ya se frenó en
   401).
6. **Controladores**: ya reciben solo peticiones autenticadas y, si
   escriben, autorizadas por rol.

## 4. Capas (igual patrón que v1/v2, aplicado a autenticación)

```
AuthController          # HTTP: no valida contraseñas ni genera tokens
    │  try/catch: NoAutorizadoExcepcion→401, ArgumentException→400, SqlException/Exception→500
    ▼
IServicioAutenticacion   # no conoce HTTP; valida credenciales y orquesta
    │  usa IServicioHashContrasena (bcrypt) + ITokenJwt (firma el JWT)
    ▼
IRepositorioUsuario      # no conoce HTTP ni reglas de negocio
    │  solo llama sp_consultar_usuario_por_username / sp_listar_roles_usuario
    ▼
SQL Server (usuario, rol, rol_usuario)
```

`PoliticaRoles` (en `Seguridad/`) es la **única** fuente de verdad sobre qué
roles escriben; no vive en el servicio de autenticación ni en cada
controlador, porque la regla la aplica el middleware para **todas** las
tablas por igual, no cada controlador por separado.

## 5. Inyección de dependencias (`Program.cs`)

```csharp
builder.Services.AddSingleton<IServicioHashContrasena, ServicioHashContrasena>();
builder.Services.AddSingleton<ITokenJwt, ServicioTokenJwt>();
builder.Services.AddScoped<IRepositorioUsuario>(_ => new RepositorioUsuarioSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
```

El hasher y el emisor de JWT son **singleton**: no guardan estado por
petición (bcrypt genera su propia sal en cada llamada; el emisor solo lee la
configuración). El repositorio y el servicio de autenticación son
**scoped**, igual que el resto de repositorios/servicios de v1/v2 (abren su
propia conexión por petición).

## 6. Estructura de carpetas (solo lo nuevo de la v3)

```
api_innovacion/
├── Configuracion/JwtOpciones.cs
├── Controllers/AuthController.cs
├── Excepciones/NoAutorizadoExcepcion.cs
├── Middlewares/RestringirEscrituraMiddleware.cs
├── Modelos/{Usuario.cs, SesionUsuario.cs}
├── Peticiones/LoginPeticion.cs
├── Repositorios/{IRepositorioUsuario.cs, RepositorioUsuarioSqlServer.cs}
├── Respuestas/RespuestasAuth.cs
├── Seguridad/PoliticaRoles.cs
├── Servicios/{IServicioAutenticacion.cs, ServicioAutenticacion.cs,
│              IServicioHashContrasena.cs, ServicioHashContrasena.cs,
│              ITokenJwt.cs, ServicioTokenJwt.cs}
└── pruebas/PruebasAutenticacion.cs

db/09_seguridad_v3.sql        # procedimientos + migración de contraseñas

frontend/src/app/
├── guards/{auth.guard.ts, invitado.guard.ts}
├── layout/{layout.ts, layout.html, layout.css}        # envoltura de rutas privadas
├── models/auth.ts
├── pages/login/{login.ts, login.html, login.css}
└── services/{auth.service.ts, auth.interceptor.ts, sesion-storage.ts}
```

No hay carpetas nuevas de nivel superior: todo cuelga del mismo andamiaje de
v1/v2 (`Controllers/`, `Servicios/`, `Repositorios/`, `Excepciones/`).

## 7. Lo que queda fuera de este plan

El CRUD de `usuario`/`rol`/`rol_usuario` para administradores (controlador,
servicio, repositorio de escritura y pantalla admin) **no está planeado
aquí**: es el siguiente incremento de la v3 (ver [2_spec.md](2_spec.md) §2 y
[8_tasks.md](8_tasks.md) Fase 5).
