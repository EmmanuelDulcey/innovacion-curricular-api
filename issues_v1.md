# Issues — División del trabajo de la Entrega v1

> **Estado:** planificación. La v1 aún NO se ejecuta; estos issues organizan el
> trabajo que se hará. La versión se cierra recién cuando todos los criterios
> de aceptación estén en verde y exista el tag `v1` en `main`
> (`docs/spec_kit/versiones/v1_producto_sqlserver/2_spec.md` §5, criterio 10).
>
> **Objetivo general:** CRUD de las 7 tablas sin FK del módulo — API REST +
> Frontend funcionando — Criterios en verde + tag `v1`.

## Repartición

| Issue | Responsible | Tablas |
|---|---|---|
| [#1 — CRUD `area_conocimiento` y `universidad`](#issue-1-crud-area_conocimiento-y-universidad-enmanuel) | **Emmanuel** | `area_conocimiento`, `universidad` |
| [#2 — CRUD `aspecto_normativo`, `practica_estrategia` y `enfoque`](#issue-2-crud-aspecto_normativo-practica_estrategia-y-enfoque-felipe) | **Felipe** | `aspecto_normativo`, `practica_estrategia`, `enfoque` |
| [#3 — CRUD `car_innovacion` y `aliado`](#issue-3-crud-car_innovacion-y-aliado-valentina) | **Valentina** | `car_innovacion`, `aliado` |

## Trabajo compartido (coordinación entre los tres)

El esqueleto de la v1 es infraestructura compartida que no puede repartirse
por tabla y **debe quedar disponible para todos antes de empezar** (o se
construye una única vez entre los tres). Cada issue asume que esto existe:

- **BD + Docker:** `db/` con `innovacion_curricular.ss.sql`,
  `01_activar_borrado_logico.sql`, `02_datos_iniciales.sql` e `init.sh`, y el
  `docker-compose.yml` (SQL Server 2022, puerto 11467, healthcheck).
  Ver: `8_tasks.md` Fase 0, `5_data_model.md` §3–4.
- **Proyecto .NET:** `api_innovacion/` con `ApiInnovacion.csproj`,
  `appsettings.json` (cadena de conexión), `Excepciones/NoEncontradoExcepcion.cs`
  y el `Dockerfile`. Ver: `8_tasks.md` Fases 1–2 y 6.
- **Frontend genérico:** `frontend/` scafoldeado (`ng new`), `api.service.ts`
  con los métodos `listar/obtener/crear/reemplazar/actualizar/eliminar`,
  página `pages/catalogo` genérica reutilizada, `components/nav-bar` y
  `pages/dashboard`. Ver: `8_tasks.md` Fases 7–11, `9_frontend.md` §5–6.

Cada issue aporta **solo sus tablas** dentro de ese esqueleto. Para evitar
conflictos en `Program.cs` (registro DI) y en `nav-bar`/`dashboard`, se
recomienda una rama por issue y resolver los merge a `main` de forma
coordinada (README: "cada estudiante trabaja en su rama").

---

## Issue #1 — CRUD `area_conocimiento` y `universidad` (Emmanuel)

- **Título:** CRUD completo de `area_conocimiento` y `universidad` — API REST + Frontend
- **Asignado a:** Emmanuel
- **Tag:** `v1`
- **Relación del cuadro de entrega:** "CRUD de las tablas sin FK del módulo — API REST + Frontend funcionando · Criterios en verde + tag v1" (→ `2_spec.md` §5)
- **Fuentes:** `5_data_model.md` §2, `6_contracts.md` §2–8, `9_frontend.md` §4 y §9, `8_tasks.md` Fases 1–5 y 8–10.

### Objetivo

Implementar el CRUD completo (GET listar, GET por id, POST, PUT, PATCH, DELETE)
de las dos primeras tablas sin FK, de punta a punta: repositorio → servicio →
controlador en la API, y su listado/formulario en el Frontend.

### Tablas y campos

**`area_conocimiento`** (llave `id`, PK sin IDENTITY; inicia con 218 registros):

| Campo | Tipo | Regla |
|---|---|---|
| `id` | INT | Entero positivo (lo asigna el cliente al crear) |
| `gran_area` | VARCHAR(60) | Obligatorio, no vacío, máx. 60 |
| `area` | VARCHAR(60) | Obligatorio, no vacío, máx. 60 |
| `disciplina` | VARCHAR(60) | Obligatorio, no vacío, máx. 60 |
| `activo` | BIT | 1 visible, 0 borrado lógico (default 1) |

**`universidad`** (llave `id`; inicia con 6 registros):

| Campo | Tipo | Regla |
|---|---|---|
| `id` | INT | Entero positivo |
| `nombre` | VARCHAR(60) | Obligatorio, no vacío, máx. 60 |
| `tipo` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `ciudad` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `activo` | BIT | Idem |

### Descripción — API REST (capa por capa, para cada una de las 2 tablas)

1. **`Modelos/`**: clase entidad `AreaConocimiento.cs` y `Universidad.cs`, una
   propiedad tipada por columna más `Activo` por defecto `true`
   (`5_data_model.md` §2, patrón del bloque C#).
2. **`Peticiones/`**: tres clases por tabla — `<Tabla>Crear.cs` (todo
   obligatorio incluida la PK), `<Tabla>Reemplazo.cs` (todo obligatorio sin la
   PK) y `<Tabla>Actualizar.cs` (todo opcional), con `[Required]` /
   `[StringLength]` / `[Range]` según `6_contracts.md` §8.1 y §8.2.
3. **`Repositorios/`**: `IRepositorioAreaConocimiento` / `IRepositorioUniversidad`
   (5 métodos asíncronos, plantilla de `3_plan.md` §4.1) y
   `RepositorioAreaConocimientoSqlServer` / `RepositorioUniversidadSqlServer`
   con Dapper: `SELECT TOP (@limite) … WHERE activo = 1`, lectura por llave con
   `activo = 1`, `INSERT` que fija `activo = 1`, UPDATE parametrizado y borrado
   lógico (`UPDATE … SET activo = 0`). SQL parametrizado (`@param`), nunca
   `DELETE` físico, todo `async`.
4. **`Servicios/`**: `IServicio<Tabla>` y `Servicio<Tabla>` — el PATCH sin campos
   lanza `ArgumentException` (→ 400) y el registro inexistente lanza
   `NoEncontradoExcepcion` (→ 404).
5. **`Controllers/`**: `AreaConocimientoController.cs` y
   `UniversidadController.cs` con ruta base `[Route("api/{tabla}")]`, los 6
   métodos HTTP y try/catch estricto traduciendo excepciones a códigos (422 /
   400 / 404 / 500).
6. **`Program.cs`**: registrar en DI repositorio + servicio de las 2 tablas
   (`AddScoped`, patrón de `3_plan.md` §4.4) sin romper los registros de los
   otros issues.

### Descripción — Avance del Frontend

7. **`src/app/models/catalogo.ts`**: interfaces en camelCase para las dos
   tablas siguiendo el mapeo de `9_frontend.md` §4 — `AreaConocimiento`:
   `id`, `granArea`, `area`, `disciplina`, `activo`; `Universidad`: `id`,
   `nombre`, `tipo`, `ciudad`, `activo`.
8. **`pages/catalogo`**: incorporar la configuración de columnas y campos del
   formulario (Crear/Editar) de las 2 tablas, con las reglas de `6_contracts.md`
   §8 (obligatoriedad y longitud máx.); la llave `id` obligatoria solo al crear,
   de solo lectura al editar; envío de `PUT` al editar (cuerpo completo).
9. **`components/nav-bar`**: enlace a las 2 tablas; **`pages/dashboard`**: las 2
   tarjetas con su `total` (218 y 6). Refrescar el listado tras cada operación
   exitosa.

### Verificación (Definition of Done)

- `dotnet build` compila sin errores.
- Con la BD arriba: `GET /api/area_conocimiento` → 200 con 218 registros y con
  `?limite=3` → 3 (`6_contracts.md` §2.1); `GET /api/universidad` → 200 con 6.
- CRUD completo probado en las 2 tablas: POST crea → PUT reemplaza → PATCH
  actualiza parcialmente → GET confirma → DELETE marca inactivo → segundo DELETE
  responde 404.
- POST con body inválido (`gran_area` vacío sin `disciplina`) → 422 con
  `errores:[…]`; PATCH con `{}` → 400.
- Desde `http://localhost:8037`: las tarjetas de `area_conocimiento` (218) y
  `universidad` (6); crear/editar/eliminar un registro de cualquiera de las 2
  tablas y el cambio persiste al recargar; formulario inválido muestra los
  errores 422 por campo sin romper la UI.

---

## Issue #2 — CRUD `aspecto_normativo`, `practica_estrategia` y `enfoque` (Felipe)

- **Título:** CRUD completo de `aspecto_normativo`, `practica_estrategia` y `enfoque` — API REST + Frontend
- **Asignado a:** Felipe
- **Tag:** `v1`
- **Relación del cuadro de entrega:** "CRUD de las tablas sin FK del módulo — API REST + Frontend funcionando · Criterios en verde + tag v1" (→ `2_spec.md` §5)
- **Fuentes:** `5_data_model.md` §2, `6_contracts.md` §2–8, `9_frontend.md` §4 y §9, `8_tasks.md` Fases 1–5 y 8–10.

### Objetivo

Implementar el CRUD completo (GET listar, GET por id, POST, PUT, PATCH, DELETE)
de las tres tablas siguientes sin FK, de punta a punta: repositorio → servicio →
controlador en la API, y su listado/formulario en el Frontend.

### Tablas y campos (inician vacías)

**`aspecto_normativo`** (llave `id`):

| Campo | Tipo | Regla |
|---|---|---|
| `id` | INT | Entero positivo |
| `tipo` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `descripcion` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `fuente` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `activo` | BIT | Idem |

**`practica_estrategia`** (llave `id`):

| Campo | Tipo | Regla |
|---|---|---|
| `id` | INT | Entero positivo |
| `tipo` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `nombre` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `descripcion` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `activo` | BIT | Idem |

**`enfoque`** (llave `id`):

| Campo | Tipo | Regla |
|---|---|---|
| `id` | INT | Entero positivo |
| `nombre` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `descripcion` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `activo` | BIT | Idem |

### Descripción — API REST (capa por capa, para cada una de las 3 tablas)

1. **`Modelos/`**: `AspectoNormativo.cs`, `PracticaEstrategia.cs`, `Enfoque.cs` —
   una propiedad tipada por columna más `Activo` por defecto `true`.
2. **`Peticiones/`**: tres clases por tabla (`<Tabla>Crear`, `<Tabla>Reemplazo`,
   `<Tabla>Actualizar`) con `[Required]` / `[StringLength]` / `[Range]` según
   `6_contracts.md` §8.3–8.5.
3. **`Repositorios/`**: interfaz (5 métodos asíncronos, `3_plan.md` §4.1) e
   implementación Dapper con SQL parametrizado, filtrado `activo = 1` y
   borrado lógico (`activo = 0`).
4. **`Servicios/`**: interfaz y `Servicio<Tabla>` — PATCH sin campos → 400;
   registro inexistente → `NoEncontradoExcepcion` (404).
5. **`Controllers/`**: `[Route("api/{tabla}")]`, 6 métodos HTTP, try/catch
   estricto (422 / 400 / 404 / 500).
6. **`Program.cs`**: registrar DI de las 3 tablas sin romper los registros de
   los otros issues.

### Descripción — Avance del Frontend

7. **`src/app/models/catalogo.ts`**: interfaces en camelCase (`9_frontend.md`
   §4) — `AspectoNormativo`: `id`, `tipo`, `descripcion`, `fuente`, `activo`;
   `PracticaEstrategia`: `id`, `tipo`, `nombre`, `descripcion`, `activo`;
   `Enfoque`: `id`, `nombre`, `descripcion`, `activo`.
8. **`pages/catalogo`**: configuración de columnas y formulario (Crear/Editar)
   de las 3 tablas con `6_contracts.md` §8; llave `id` obligatoria solo al
   crear, solo lectura al editar; `PUT` al editar.
9. **`components/nav-bar`**: enlaces a las 3 tablas; **`pages/dashboard`**: las
   3 tarjetas con su `total` (0 al primer arranque). Refrescar el listado tras
   cada operación exitosa.

### Verificación (Definition of Done)

- `dotnet build` compila sin errores.
- Con la BD arriba: `GET /api/aspecto_normativo`, `GET /api/practica_estrategia`
  y `GET /api/enfoque` sin registros → 204 sin cuerpo; con registros → 200 con
  envoltura `{tabla, limite, total, datos}`.
- CRUD completo probado en las 3 tablas: POST crea → PUT reemplaza → PATCH
  actualiza parcialmente → GET confirma → DELETE marca inactivo → segundo DELETE
  responde 404; POST con body inválido → 422; PATCH con `{}` → 400.
- Desde `http://localhost:8037`: crear/editar/eliminar registros de las 3
  tablas, el listado se refresca y el cambio persiste al recargar; formulario
  inválido muestra los errores 422 por campo sin romper la UI; estado vacío
  ("no hay registros") cuando la tabla no tiene datos.

---

## Issue #3 — CRUD `car_innovacion` y `aliado` (Valentina)

- **Título:** CRUD completo de `car_innovacion` y `aliado` — API REST + Frontend
- **Asignado a:** Valentina
- **Tag:** `v1`
- **Relación del cuadro de entrega:** "CRUD de las tablas sin FK del módulo — API REST + Frontend funcionando · Criterios en verde + tag v1" (→ `2_spec.md` §5)
- **Fuentes:** `5_data_model.md` §2, `6_contracts.md` §2–8, `9_frontend.md` §4 y §9, `8_tasks.md` Fases 1–5 y 8–10.

### Objetivo

Implementar el CRUD completo (GET listar, GET por id, POST, PUT, PATCH, DELETE)
de las dos últimas tablas sin FK, de punta a punta: repositorio → servicio →
controlador en la API, y su listado/formulario en el Frontend. Incluye la tabla
`aliado`, sobre la que se prueba el ciclo CRUD completo del criterio 3 de
`2_spec.md` §5.

### Tablas y campos (inician vacías)

**`car_innovacion`** (llave `id`):

| Campo | Tipo | Regla |
|---|---|---|
| `id` | INT | Entero positivo |
| `nombre` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `descripcion` | VARCHAR(MAX) | Obligatorio, no vacío, sin límite de longitud por la API |
| `tipo` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `activo` | BIT | Idem |

**`aliado`** (llave `nit` — aquí la llave se llama `nit`, no `id`):

| Campo | Tipo | Regla |
|---|---|---|
| `nit` | INT | Entero positivo (lo asigna el cliente al crear) |
| `razon_social` | VARCHAR(60) | Obligatorio, no vacío, máx. 60 |
| `nombre_contacto` | VARCHAR(60) | Obligatorio, no vacío, máx. 60 |
| `correo` | VARCHAR(70) | Obligatorio, no vacío, máx. 70, **formato de correo** |
| `telefono` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `ciudad` | VARCHAR(45) | Obligatorio, no vacío, máx. 45 |
| `activo` | BIT | Idem |

### Descripción — API REST (capa por capa, para cada una de las 2 tablas)

1. **`Modelos/`**: `CarInnovacion.cs` y `Aliado.cs` (`5_data_model.md` §2) —
   propiedad tipada por columna más `Activo` por defecto `true`; en `aliado` la
   PK se llama `Nit`.
2. **`Peticiones/`**: tres clases por tabla (`<Tabla>Crear`, `<Tabla>Reemplazo`,
   `<Tabla>Actualizar`) con `[Required]` / `[StringLength]` / `[Range]` según
   `6_contracts.md` §8.6–8.7; en `Aliado` el `[Range]` de la llave aplica sobre
   `nit` y `correo` usa formato de correo (`[EmailAddress]` o equivalente,
   `6_contracts.md` §4.7).
3. **`Repositorios/`**: interfaz (5 métodos asíncronos, `3_plan.md` §4.1) e
   implementación Dapper con SQL parametrizado, filtrado `activo = 1` y borrado
   lógico (`activo = 0`). En `aliado` las columnas son `razon_social`,
   `nombre_contacto`, `correo`, `telefono`, `ciudad` y la llave es `nit`
   (`3_plan.md` §4.5: "el resto de la plantilla es idéntico").
4. **`Servicios/`**: interfaz y `Servicio<Tabla>` — PATCH sin campos → 400;
   registro inexistente → `NoEncontradoExcepcion` (404).
5. **`Controllers/`**: `CarInnovacionController.cs` (ruta `api/car_innovacion` y
   `{id}`) y `AliadoController.cs` (ruta `api/aliado` y `{nit}`), 6 métodos
   HTTP, try/catch estricto (422 / 400 / 404 / 500).
6. **`Program.cs`**: registrar DI de las 2 tablas sin romper los registros de
   los otros issues.

### Descripción — Avance del Frontend

7. **`src/app/models/catalogo.ts`**: interfaces en camelCase (`9_frontend.md`
   §4) — `CarInnovacion`: `id`, `nombre`, `descripcion`, `tipo`, `activo`;
   `Aliado`: `nit`, `razonSocial`, `nombreContacto`, `correo`, `telefono`,
   `ciudad`, `activo`.
8. **`pages/catalogo`**: configuración de columnas y formulario (Crear/Editar)
   de las 2 tablas con `6_contracts.md` §8; en `aliado` los encabezados usan
   nombre legible (`razón social`, `nombre de contacto`, etc., `9_frontend.md`
   §9); llave `id`/`nit` obligatoria solo al crear, solo lectura al editar;
   `PUT` al editar; validación de correo en el formulario de `aliado`.
9. **`components/nav-bar`**: enlaces a las 2 tablas; **`pages/dashboard`**: las
   2 tarjetas con su `total` (0 al primer arranque). Refrescar el listado tras
   cada operación exitosa.

### Verificación (Definition of Done)

- `dotnet build` compila sin errores.
- **CRUD completo probado en `aliado`** (criterio 3 de `2_spec.md` §5): POST
  crea → PUT reemplaza → PATCH actualiza parcialmente → GET confirma → DELETE
  marca inactivo → segundo DELETE responde 404.
- POST de `aliado` con `correo` inválido → 422 con `["El campo correo debe
  tener un formato válido."]` (`6_contracts.md` §4.7); PK `nit`/`id` duplicada →
  500 con detalle del motor; PATCH con `{}` → 400.
- Desde `http://localhost:8037`: crear/editar/eliminar un `aliado` y el cambio
  persiste al recargar; igual para `car_innovacion`; formulario inválido muestra
  los errores 422 por campo sin romper la UI.