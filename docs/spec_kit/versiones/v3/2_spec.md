# Especificación — Versión 3: control de acceso (API + Frontend)

> **Versión 3** del desarrollo incremental ([mapa de versiones](../0_mapa_versiones.md)).
> Rige la constitución del proyecto: [../../1_constitution.md](../../1_constitution.md).
> Documenta el delta **ya implementado** (login JWT + autorización por rol).
> Es deliberadamente más liviana que la de v2 en 3 a 8: la v3 no agrega tablas
> de catálogo ni procedimientos de dominio académico, así que el plan, la
> investigación, el modelo de datos, el quickstart y las tareas son más
> cortos — pero están los nueve documentos.

| Documento de esta versión | Contenido |
|---|---|
| **2_spec.md** (este) | QUÉ se construyó en v3, decisiones y criterios de aceptación |
| [3_plan.md](3_plan.md) | CÓMO: stack nuevo, pipeline de middlewares y estructura de capas |
| [4_research.md](4_research.md) | Decisiones y alternativas *(lectura opcional)*: JWT sin estado, bcrypt, 401 vs 403 |
| [5_data_model.md](5_data_model.md) | Las 3 tablas de usuarios (`usuario`, `rol`, `rol_usuario`) y los 2 procedimientos nuevos |
| [6_contracts.md](6_contracts.md) | Endpoints de autenticación, formato del JWT y el nuevo **403** |
| [7_quickstart.md](7_quickstart.md) | Arranque y smoke test de la v3 (login, sesión, 401/403/429) |
| [8_tasks.md](8_tasks.md) | Orden de construcción por fases, con lo pendiente marcado |
| [9_frontend.md](9_frontend.md) | Login, guardias de ruta, interceptor y ocultamiento de acciones por rol |
| [9_checklist.md](9_checklist.md) | Criterios de aceptación + regresión de v1/v2 |

---

## 1. Propósito de la v3

Las v1 y v2 dejaron 22 tablas con CRUD público: cualquiera podía leer y
escribir sin identificarse. La v3 agrega una **capa de control de acceso**
transversal: inicio de sesión con **JWT**, contraseñas **hasheadas con
bcrypt**, y una regla de autorización que separa **quién puede leer** de
**quién puede escribir**, aplicada tanto en la API (autoridad final) como en
el Frontend (oculta acciones que de todos modos la API rechazaría).

No se tocan las 22 tablas de v1/v2: ningún endpoint de catálogo cambia de
ruta, de verbo ni de formato de respuesta. Lo único nuevo es que, a partir de
esta versión, **toda ruta exige sesión** (salvo `POST /api/login` y
`GET /` de diagnóstico) y las escrituras exigen además un rol habilitado.

---

## 2. Alcance

**Incluye (lo que ya está mergeado en `rama-valentina`):**

- `POST /api/login`: valida usuario + contraseña contra el hash **bcrypt**
  guardado en `usuario.password` y devuelve un **JWT HS256** con el usuario y
  sus roles vigentes.
- `GET /api/auth/me`: devuelve la sesión activa a partir de los claims del
  token (sin volver a tocar la base).
- `POST /api/auth/logout`: cierra la sesión del lado del cliente (JWT sin
  estado: no hay tabla de sesiones que invalidar en el servidor).
- Middleware de **autorización por rol en escritura**: `POST`/`PUT`/`PATCH`/
  `DELETE` de cualquier tabla de v1/v2 responde **403** si el usuario
  autenticado no tiene un rol habilitado para escribir. `GET`/`HEAD`/`OPTIONS`
  siguen abiertos a cualquier usuario autenticado.
- Política de respaldo de ASP.NET: **toda ruta exige `[Authorize]`** salvo
  las marcadas `[AllowAnonymous]` (`POST /api/login`, `GET /`). Sin token →
  **401**.
- **Rate limiting** del login: 10 intentos por minuto por IP → **429**.
- Script `db/09_seguridad_v3.sql`: procedimientos
  `sp_consultar_usuario_por_username` y `sp_listar_roles_usuario`, y
  migración de las contraseñas de demostración de texto plano a bcrypt.
- Seeders (`db/08_seeders.sql`): 5 roles (`Administrador`, `Coordinador
  académico`, `Docente`, `Secretaría académica`, `Consulta`) y 5 usuarios de
  demostración, uno por rol, con la contraseña `Demo123!` (hasheada).
- Frontend: pantalla de **login**, `AuthService` (sesión en `signal`,
  persistida por `SesionStorage`), `authInterceptor` (adjunta el `Bearer` y
  cierra sesión ante un 401), `authGuard`/`invitadoGuard` (rutas privadas vs.
  pública), y ocultamiento de los botones de crear/editar/eliminar en
  catálogo y en el maestro-detalle de `programa` cuando `puedeEscribir()` es
  falso (`ROLES_ESCRITURA` espejo de `PoliticaRoles` del backend).
- Pruebas de capa sin SQL Server (`PruebasAutenticacion.cs`): hash, rechazos
  genéricos, emisión de JWT y la política de roles.

**No incluye todavía (pendiente dentro de la v3, no es v4):**

- **CRUD de `usuario`, `rol` y `rol_usuario` para administradores.** El mapa
  de versiones lo describe como parte de la v3; esta entrega cubre la
  **autenticación y la autorización**, que son el prerrequisito. Falta el
  controlador/servicio/repositorio de gestión de usuarios y su pantalla
  admin. Se deja como siguiente incremento de esta misma versión (no se abre
  la v4 sin esto).
- Expiración/revocación de sesión más allá de la expiración natural del JWT
  (480 minutos por defecto, `Jwt:ExpirationMinutes`). No hay lista negra de
  tokens ni refresh token.
- Reactivar un usuario inactivo desde la interfaz (no hay pantalla de
  usuarios todavía — ver punto anterior).
- Cualquier cambio a las 22 tablas de v1/v2 o a sus contratos: siguen
  exactamente como en `v2/6_contracts.md`.

---

## 3. Decisiones del equipo

### 3.1 Qué roles pueden escribir

**Decisión: solo `Administrador`, `Coordinador académico` y `Secretaría
académica` pueden crear, editar o retirar. `Docente` y `Consulta` (y
cualquier usuario sin rol) quedan en solo lectura.**

La regla vive en un solo lugar del backend (`PoliticaRoles.PuedeEscribir`,
usada por `RestringirEscrituraMiddleware`) y se refleja en el Frontend
(`ROLES_ESCRITURA` en `models/auth.ts`) solo para **ocultar** botones; la API
es quien decide de verdad. Un `PUT`/`POST`/`PATCH`/`DELETE` enviado a mano
con un token de `Docente` o `Consulta` se rechaza igual con 403, aunque la
interfaz no muestre el botón.

### 3.2 Mensaje de login y protección por tiempo

**Decisión: el 401 de credenciales inválidas es siempre el mismo mensaje
genérico** (`"Usuario o contraseña incorrectos."`), sin distinguir usuario
inexistente, inactivo o contraseña incorrecta, y **siempre se verifica un
hash bcrypt** (uno de relleno si el usuario no existe) para que el tiempo de
respuesta no delate si el usuario existe.

Justificación: evitar que alguien enumere usuarios válidos probando el
endpoint de login (por mensaje distinto o por diferencia de tiempo).

### 3.3 Límite de intentos de login

**Decisión: 10 intentos por minuto por IP → 429**, con una partición
`"desconocido"` compartida si no se puede resolver la IP (nunca una ventana
global que bloquee a todos los usuarios por igual).

---

## 4. Requisitos funcionales

### RF1 — Iniciar sesión
`POST /api/login` con `{username, password}` → 200 con `token`, `expira` y el
`usuario` autenticado (incluye `roles`). Credenciales inválidas, usuario
inactivo o inexistente → **401** genérico. Body vacío → **400**. Más de 10
intentos/minuto por IP → **429**.

### RF2 — Consultar la sesión activa
`GET /api/auth/me` con `Authorization: Bearer <token>` → 200 con los datos
del usuario reconstruidos desde los **claims** del JWT (no vuelve a SQL
Server). Sin token o token expirado/inválido → **401**.

### RF3 — Cerrar sesión
`POST /api/auth/logout` (autenticado) → 200. No invalida el token en el
servidor (JWT sin estado); el cliente debe descartarlo.

### RF4 — Autorización por rol en escrituras
Cualquier `POST`/`PUT`/`PATCH`/`DELETE` de las 22 tablas de v1/v2, hecho por
un usuario autenticado sin rol de escritura → **403**, con el mismo sobre de
error del resto de la API. `GET`/`HEAD`/`OPTIONS` no se bloquean por rol.

### RF5 — Toda ruta exige sesión por defecto
Cualquier ruta sin `[AllowAnonymous]` sin un JWT válido → **401**. Las únicas
rutas públicas son `POST /api/login` y `GET /` (diagnóstico).

### RF6 — Login (Frontend)
`/login` pide usuario y contraseña, llama a `POST /api/login`, guarda la
sesión y redirige a `returnUrl` o al dashboard. Con sesión activa, `/login`
redirige al dashboard (`invitadoGuard`). Credenciales inválidas muestran el
mensaje del 401 sin jerga.

### RF7 — Rutas privadas y expiración (Frontend)
Toda ruta fuera de `/login` exige sesión (`authGuard`); sin sesión redirige a
`/login?returnUrl=...`. Un 401 de cualquier petición limpia la sesión y
redirige a `/login`, conservando la URL para volver tras autenticarse.

### RF8 — Ocultar escritura según el rol (Frontend)
En catálogo y en el maestro-detalle de `programa`, los botones de crear,
editar y retirar solo se muestran si `AuthService.puedeEscribir()` es
verdadero. Esto es una ayuda de interfaz, no una barrera de seguridad: la
barrera real es RF4.

---

## 5. Requisitos no funcionales

- **Contraseñas:** hasheadas con **bcrypt** (costo 12); nunca en texto plano
  ni en los logs. Límite de 72 bytes por contraseña (máximo que procesa
  bcrypt) → sobre ese límite, 400.
- **JWT:** HS256, validado por emisor, audiencia, firma y expiración
  (`ClockSkew` de 30s); `Jwt:Secret` viene de variable de entorno
  (`JWT_SECRET` → `Jwt__Secret`), nunca hardcodeado; mínimo 32 caracteres.
- **Capas:** el controlador no valida roles ni toca JWT directamente (lo hace
  el middleware/`[Authorize]`); el servicio de autenticación no conoce HTTP;
  el repositorio de usuario solo consulta por procedimientos almacenados.
- **Cero SQL a mano:** `sp_consultar_usuario_por_username` y
  `sp_listar_roles_usuario`, igual que las tablas de la v2.
- **Un solo lugar para la regla de roles:** `PoliticaRoles` en el backend;
  el Frontend la replica solo para UI, no para seguridad.
- **Sin anticipar v4:** no hay consultas multitabla ni gráficos nuevos en
  esta versión.
- **Idioma:** mensajes de error en castellano, igual que v1/v2; identificadores
  del código en castellano según el molde ya establecido.

---

## 6. Criterios de aceptación

1. `docker compose up -d --build` levanta todo; `db/09_seguridad_v3.sql`
   corre tras los seeders sin error (contraseñas de demo quedan hasheadas).
2. `POST /api/login` con cada uno de los 5 usuarios de demostración
   (`admin`, `coordinador`, `docente.demo`, `secretaria`, `consulta` —
   contraseña `Demo123!`) devuelve 200 con token y roles.
3. Credenciales incorrectas, usuario inexistente o inactivo → **401** con el
   mismo mensaje genérico.
4. Sin token, cualquier ruta protegida → **401**.
5. Con token de `docente.demo` o `consulta`, un `POST`/`PUT`/`DELETE` sobre
   cualquier tabla de v1/v2 → **403**.
6. Con token de `admin`, `coordinador` o `secretaria`, el CRUD de v1/v2 sigue
   funcionando igual que antes de la v3 (regresión).
7. En el Frontend, iniciar sesión con `docente.demo` o `consulta` oculta los
   botones de crear/editar/eliminar; con `admin` se ven.
8. Un 401 de cualquier petición redirige al login y limpia la sesión
   guardada.
9. 11 intentos de login en menos de un minuto desde la misma IP → el 11.º
   responde **429**.
10. La regresión de v1 y v2 sigue en verde (ver [9_checklist.md](9_checklist.md) §D).

**Pendiente para cerrar la v3 por completo** (ver §2 "No incluye"): CRUD de
`usuario`/`rol`/`rol_usuario` para administradores. Mientras no esté, el tag
`v3` no debería crearse — este spec documenta el estado actual del merge,
no el cierre de la versión.

---

## 7. Reglas fundamentales de la v3

1. La contraseña **nunca** se compara ni se guarda en texto plano: siempre
   bcrypt.
2. El 401 de login **no distingue** usuario inexistente de contraseña
   incorrecta.
3. Solo `Administrador`, `Coordinador académico` y `Secretaría académica`
   escriben; `Docente` y `Consulta` son de solo lectura.
4. La regla de roles se evalúa en la **API** (autoridad final); el Frontend
   solo la refleja para ocultar botones.
5. Toda ruta exige sesión salvo `POST /api/login` y `GET /`.
6. El JWT no tiene estado en el servidor: cerrar sesión es una acción del
   cliente.
7. El secreto del JWT viene de una variable de entorno, nunca del código.
8. Ninguna tabla ni contrato de v1/v2 cambia en esta versión.
