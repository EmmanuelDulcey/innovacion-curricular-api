# Modelo de datos — Versión 3: control de acceso

> **Versión 3** · Las 3 tablas `usuario`/`rol`/`rol_usuario` **ya existían**
> en el script base (`db/innovacion_curricular.ss.sql`), sin usar hasta esta
> versión. La v3 agrega los **procedimientos** para consultarlas, **migra**
> las contraseñas de demostración a bcrypt (`db/09_seguridad_v3.sql`) y
> suma **una tabla nueva**, `token_revocado`, para la revocación de sesión
> (`db/10_revocacion_tokens_v3.sql`).

---

## 1. Dónde encajan estas tablas en la base completa

`innovacion_curricular` tenía **26 tablas** en dos módulos antes de la v3.
v1 opera las 7 sin FK, v2 opera las 15 con FK más el catálogo `docente`. Las
3 restantes son el módulo de usuarios, que la v3 activa. La v3 además
**agrega una tabla nueva** (`token_revocado`, 27.ª), propia de esta versión
y sin relación de FK con el resto:

```
INNOVACIÓN CURRICULAR (23, v1/v2)         USUARIOS (3)                  SESIÓN (1, nueva)
──────────────────────────────            ──────────────────────────    ─────────────────
(sin cambios en v3)                       usuario ──┐                   token_revocado
                                                     ├─ rol_usuario ──> rol  (sin FK)
```

## 2. `rol`

| Columna | Tipo | Regla | Notas |
|---|---|---|---|
| `id` | `INT IDENTITY(1,1)` | **PK** | |
| `nombre` | `VARCHAR(100)` | **Único**, obligatorio | `Administrador`, `Coordinador académico`, `Docente`, `Secretaría académica`, `Consulta` (seeders) |
| `descripcion` | `VARCHAR(MAX)` | Opcional | |
| `activo` | `BIT` | `DEFAULT 1` | `sp_listar_roles_usuario` solo trae roles con `activo = 1` |
| `fecha_creacion` | `DATETIME` | `DEFAULT GETDATE()` | |

## 3. `usuario`

| Columna | Tipo | Regla | Notas |
|---|---|---|---|
| `id` | `INT IDENTITY(1,1)` | **PK** | |
| `username` | `VARCHAR(100)` | **Único**, obligatorio | Se recorta (`Trim()`) antes de comparar en el login |
| `password` | `VARCHAR(255)` | Obligatorio | **Siempre** un hash bcrypt (`$2a$12$...`, 60 caracteres); nunca texto plano a partir de esta versión |
| `email` | `VARCHAR(150)` | **Único**, obligatorio | No se valida formato en esta versión (no hay endpoint de registro) |
| `nombre_completo` | `VARCHAR(200)` | Opcional | Si es nulo, la API usa `username` como nombre a mostrar |
| `activo` | `BIT` | `DEFAULT 1` | Un usuario inactivo no puede iniciar sesión (401 genérico, igual que una contraseña incorrecta) |
| `fecha_creacion` | `DATETIME` | `DEFAULT GETDATE()` | |
| `fecha_actualizacion` | `DATETIME` | `DEFAULT GETDATE()` | La migración de contraseñas de `09_seguridad_v3.sql` la actualiza al convertir a bcrypt |

## 4. `rol_usuario` (puente)

| Columna | Tipo | Regla | Notas |
|---|---|---|---|
| `usuario_id` | `INT` | **PK compuesta**, FK → `usuario(id)` | `ON DELETE CASCADE` |
| `rol_id` | `INT` | **PK compuesta**, FK → `rol(id)` | `ON DELETE CASCADE` |

Un usuario puede tener **0, 1 o varios** roles (el JWT incluye un claim
`role` por cada uno). Un usuario sin fila en `rol_usuario` se autentica
igual, pero no puede escribir (ver [2_spec.md](2_spec.md) §3.1) —
comprobado en `PruebasAutenticacion.cs` con el usuario `sinrol`.

## 5. `token_revocado` (tabla nueva, revocación de sesión)

| Columna | Tipo | Regla | Notas |
|---|---|---|---|
| `jti` | `VARCHAR(50)` | **PK** | El claim `jti` del JWT (un GUID, 36 caracteres) |
| `fecha_revocacion` | `DATETIME` | `DEFAULT GETDATE()` | Cuándo se cerró esa sesión |

Sin FK a `usuario`: el `jti` por sí solo identifica un token, no hace falta
saber de quién es para rechazarlo. Un `jti` presente aquí es un JWT que
**ya no debe aceptarse**, aunque su firma y su expiración sigan siendo
válidas (ver [4_research.md](4_research.md) D1.1).

> **Limitación conocida:** no guarda la expiración del token, así que no
> hay una consulta trivial para purgar filas de tokens que de todas formas
> ya vencieron — la tabla solo crece. Aceptable para el volumen de un
> proyecto de aula; ver [2_spec.md](2_spec.md) §2.

## 6. Procedimientos nuevos (`db/09_seguridad_v3.sql`)

### 6.1 `sp_consultar_usuario_por_username`

```sql
sp_consultar_usuario_por_username @username VARCHAR(100)
```

Devuelve `id, username, password, email, nombre_completo, activo` de
**una** fila (o ninguna). Incluye el **hash** de la contraseña a propósito:
es el único punto de la API que lee esa columna, y lo hace el servicio de
autenticación, nunca un controlador ni una respuesta HTTP.

### 6.2 `sp_listar_roles_usuario`

```sql
sp_listar_roles_usuario @usuario_id INT
```

Devuelve los `nombre` de `rol` para ese usuario, solo los que tengan
`activo = 1`, ordenados alfabéticamente.

## 7. Procedimientos de revocación (`db/10_revocacion_tokens_v3.sql`)

### 7.1 `sp_revocar_token`

```sql
sp_revocar_token @jti VARCHAR(50)
```

Inserta el `jti` en `token_revocado` **si no estaba ya** (`IF NOT EXISTS`):
revocar dos veces el mismo token no falla ni duplica la fila. Lo llama
`ServicioRevocacionToken.RevocarAsync`, invocado desde
`AuthController.Logout`.

### 7.2 `sp_token_revocado`

```sql
sp_token_revocado @jti VARCHAR(50)
```

Devuelve `1` (`BIT`) si ese `jti` está en la lista negra, `0` si no. Lo
consulta `ServicioRevocacionToken.EstaRevocadoAsync`, llamado desde
`OnTokenValidated` en **cada** petición autenticada (ver
[3_plan.md](3_plan.md) §3).

## 8. Migración de contraseñas de demostración

Los 5 usuarios sembrados por `08_seeders.sql` (`admin`, `coordinador`,
`docente.demo`, `secretaria`, `consulta`) entran con la contraseña literal
`Demo123!`. `09_seguridad_v3.sql` la reemplaza por su hash bcrypt **solo si
sigue siendo ese literal** (ver [4_research.md](4_research.md) D7):

```sql
UPDATE usuario
   SET password = N'$2a$12$...',
       fecha_actualizacion = GETDATE()
 WHERE username = N'admin'
   AND password = N'Demo123!';
```

Esto hace que el script sea **idempotente**: correrlo dos veces, o sobre
una base que ya migró, no cambia nada.

## 9. Convenciones

- Las PK de `usuario` y `rol` son `IDENTITY`, igual que el catálogo
  `docente` de v2 — ningún identificador se digita. La PK de
  `token_revocado` es el propio `jti` (no autogenerada: ya viene del JWT).
- `usuario.password` **nunca** sale de la API: `UsuarioAutenticadoRespuesta`
  (la forma pública del usuario) no tiene esa propiedad; solo
  `RepositorioUsuarioSqlServer` y `ServicioAutenticacion` la tocan.
