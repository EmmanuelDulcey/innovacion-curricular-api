# Contratos HTTP — Versión 3: control de acceso

> **Versión 3** · Base: `http://localhost:8036`.
> Delta de [`../v2/6_contracts.md`](../v2/6_contracts.md): todas las rutas de
> las 22 tablas de v1/v2 **siguen igual** (mismas rutas, verbos, formatos).
> Lo nuevo de esta versión es: tres rutas de autenticación, el requisito de
> `Authorization: Bearer <token>` en toda ruta que no sea pública, y el
> código **403**.
>
> Documentación interactiva: `/swagger` (con candado Bearer).

---

## 0. Convenciones nuevas de esta versión

### 0.1 Rutas públicas (sin token)

```text
GET  /            diagnóstico
POST /api/login
```

Todo lo demás exige `Authorization: Bearer <token>`. Sin ese header, o con un
token inválido/expirado → **401**.

### 0.2 Encabezado de autorización

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 0.3 Claims del token

| Claim | Contenido |
|---|---|
| `nameid` | id numérico del usuario |
| `unique_name` | username |
| `email` | correo |
| `nombre_completo` | nombre para mostrar |
| `role` | uno por cada rol vigente (puede repetirse) |
| `jti` | identificador único del token |

### 0.4 Código nuevo: 403

Se agrega a la matriz de [`../v2/6_contracts.md`](../v2/6_contracts.md) §0.6:

| Situación | HTTP |
|---|---|
| Usuario autenticado sin rol de escritura intenta POST/PUT/PATCH/DELETE | **403** |
| Sin token en una ruta protegida, o token inválido/expirado | **401** |
| Más de 10 intentos de login por minuto desde la misma IP | **429** |

---

## 1. `POST /api/login`

```http
POST /api/login
Content-Type: application/json
```

```json
{ "username": "admin", "password": "Demo123!" }
```

**200 — credenciales válidas:**

```json
{
  "estado": 200,
  "mensaje": "Inicio de sesión exitoso.",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "tipo_token": "Bearer",
  "expira": "2026-11-27T20:00:00+00:00",
  "usuario": {
    "id": 1,
    "username": "admin",
    "email": "admin@innovacion.edu.co",
    "nombre_completo": "Administrador del sistema",
    "roles": ["Administrador"]
  }
}
```

**401 — usuario, contraseña incorrecta, inexistente o inactivo (mismo mensaje
para los tres casos):**

```json
{
  "estado": 401,
  "mensaje": "Usuario o contraseña incorrectos.",
  "detalle": ""
}
```

**400 — campos vacíos o contraseña de más de 72 bytes:**

```json
{
  "estado": 400,
  "mensaje": "Parámetros inválidos.",
  "detalle": "El usuario y la contraseña son obligatorios."
}
```

**429 — más de 10 intentos en un minuto desde la misma IP:**

```json
{
  "estado": 429,
  "mensaje": "Demasiados intentos. Espere un minuto e intente de nuevo.",
  "detalle": "Límite de intentos de inicio de sesión."
}
```

---

## 2. `GET /api/auth/me`

```http
GET /api/auth/me
Authorization: Bearer <token>
```

**200:**

```json
{
  "estado": 200,
  "mensaje": "Sesión activa.",
  "datos": {
    "id": 1,
    "username": "admin",
    "email": "admin@innovacion.edu.co",
    "nombre_completo": "Administrador del sistema",
    "roles": ["Administrador"]
  }
}
```

Sin token o inválido/expirado → **401** (mismo formato de error general).

---

## 3. `POST /api/auth/logout`

```http
POST /api/auth/logout
Authorization: Bearer <token>
```

```json
{ "estado": 200, "mensaje": "Sesión cerrada." }
```

El servidor no invalida el token (JWT sin estado); el cliente lo descarta.

---

## 4. Escritura restringida por rol — 403

Cualquier `POST`/`PUT`/`PATCH`/`DELETE` sobre **cualquier tabla de v1/v2**
(`/api/facultad`, `/api/programa`, `/api/acreditacion`, …), hecho con un token
válido pero **sin** rol `Administrador`, `Coordinador académico` o
`Secretaría académica`:

```http
DELETE /api/pasantia/5
Authorization: Bearer <token-de-docente.demo>
```

```json
{
  "estado": 403,
  "mensaje": "No tiene permisos para modificar registros.",
  "detalle": "Su rol es de solo lectura."
}
```

`GET`/`HEAD`/`OPTIONS` con ese mismo token funcionan igual que con cualquier
otro rol (200/204/404, según [`../v2/6_contracts.md`](../v2/6_contracts.md)).

---

## 5. Sin token — 401

```http
GET /api/programa
```

(sin header `Authorization`)

```http
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer
```

Cuerpo **vacío**: este 401 lo genera el middleware de autenticación de
ASP.NET (política de respaldo `RequireAuthenticatedUser`), sin pasar por el
`AuthController`, así que no tiene el sobre `{estado, mensaje, detalle}` del
resto de la API. El Frontend no depende del cuerpo para este caso — reacciona
al **código 401** (ver `auth.interceptor.ts`). Riesgo de calificación
documentado: si se exige el sobre en castellano también aquí, se agrega un
`events.OnChallenge`/`OnForbidden` al `AddJwtBearer` que escriba el mismo
formato de error.

---

## 6. Resumen de endpoints de esta versión

| Verbo | Ruta | Auth | Operación | Éxito |
|---|---|---|---|---|
| POST | `/api/login` | pública | Iniciar sesión | 200 |
| GET | `/api/auth/me` | Bearer | Sesión activa | 200 |
| POST | `/api/auth/logout` | Bearer | Cerrar sesión | 200 |
| GET | `/` | pública | Diagnóstico | 200 |

Todo lo demás (22 tablas) conserva exactamente las rutas de
[`../v2/6_contracts.md`](../v2/6_contracts.md), ahora detrás de `Bearer` y de
la regla de escritura por rol.

---

## 7. Reglas fundamentales de la v3 (contratos)

1. `POST /api/login` y `GET /` son las **únicas** rutas públicas.
2. El 401 de login nunca distingue el motivo exacto.
3. 403 es **por rol**, no por ausencia de token (eso es 401).
4. 429 protege el login, no el resto de la API.
5. Ningún contrato de las 22 tablas de v1/v2 cambia en esta versión.
