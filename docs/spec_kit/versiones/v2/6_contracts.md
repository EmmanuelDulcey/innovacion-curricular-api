# Contratos HTTP — Versión 2: las 15 tablas con clave foránea

> **Versión 2** · Base: `http://localhost:8036`.
> Estos contratos definen los verbos, rutas, códigos de respuesta y formatos
> exactos que debe cumplir la API en esta versión. Son un **delta** de
> [`../v1_producto_sqlserver/6_contracts.md`](../v1_producto_sqlserver/6_contracts.md):
> todo lo que la v1 define y no se contradice aquí **sigue vigente**.
>
> Documentación interactiva: `/swagger`.

---

## 0. Convenciones globales

### 0.1 Tablas disponibles

```text
facultad            programa            acreditacion       registro_calificado
activ_academica     pasantia            premio             docente_departamento
alianza             programa_ac         programa_pe        programa_ci
an_programa         enfoque_rc          aa_rc
docente (catálogo auxiliar local)
```

### 0.2 Rutas

**PK simple** (`docente`, `facultad`, `programa`, `activ_academica`, `pasantia`, `premio`):

```text
/api/{tabla}
/api/{tabla}/{id}
```

**PK simple con nombre propio** (`acreditacion` → `resolucion`,
`registro_calificado` → `codigo`):

```text
/api/acreditacion/{resolucion}
/api/registro_calificado/{codigo}
```

**PK compuesta** — las dos claves van en la ruta, **en el orden de la PK**.
En `docente_departamento` y `alianza` las claves identifican de forma fija la
relación mientras PUT/PATCH cambian únicamente sus atributos; las cuatro
puentes puras de Felipe sólo tienen asignar y retirar:

```text
/api/programa_ac/{programa}/{area_conocimiento}
/api/programa_pe/{programa}/{practica_estrategia}
/api/programa_ci/{programa}/{car_innovacion}
/api/an_programa/{aspecto_normativo}/{programa}
/api/enfoque_rc/{enfoque}/{registro_calificado}
/api/aa_rc/{activ_academicas_idcurso}/{registro_calificado_codigo}
/api/docente_departamento/{docente}/{departamento}
/api/alianza/{aliado}/{departamento}
```

### 0.3 Lecturas con envoltura

Las operaciones de **listado** usan la envoltura que **la v1 ya implementó**:

```json
{
  "tabla": "facultad",
  "limite": 1000,
  "total": 3,
  "datos": [ {} ]
}
```

`limite` es un entero mayor que cero; su valor por defecto es `1000`.

> **Envoltura exigida por la guía vs implementada.**
> La guía de la v2 (y el Artículo 8 de la constitución) describen la envoltura en
> **inglés**: `{ "table", "limit", "total", "data" }` y errores
> `{ "status", "message", "detail" }`. La v1 **cerró** con la envoltura en
> **castellano** (`{tabla, limite, total, datos}` / `{estado, mensaje, detalle}`).
> La v2 **mantiene la de la v1** para no romper su regresión; la desviación y su
> riesgo se documentan en [2_spec.md](2_spec.md) §5, §4 y
> [4_research.md](4_research.md) D8. Si la evaluación exige el inglés, el cambio
> se acota a `Respuestas.cs` y a los campos del Frontend.

### 0.4 Lectura individual

La consulta de un registro devuelve **directamente el objeto**, sin envoltura,
incluyendo los nombres de las FK (`universidad_nombre`, `programa_nombre`, …):

```json
{
  "id": 1,
  "nombre": "Facultad de Ingeniería",
  "tipo": "Pública",
  "fecha_fun": "1960-03-15",
  "universidad": 1,
  "universidad_nombre": "Universidad de Antioquia",
  "activo": true
}
```

### 0.5 Formato de errores

Error general:

```json
{
  "estado": 400,
  "mensaje": "Parámetros inválidos.",
  "detalle": "Descripción del error."
}
```

Validación del body (422), con lista de errores:

```json
{
  "estado": 422,
  "mensaje": "Datos inválidos.",
  "errores": [
    "El campo nombre es obligatorio."
  ]
}
```

### 0.6 Códigos HTTP

| Situación | HTTP |
|---|---|
| Operación exitosa | **200** |
| Tabla sin registros activos | **204** |
| Regla de negocio inválida (body vacío, fecha incoherente) | **400** |
| Registro inexistente o ya retirado | **404** |
| Body inválido según la petición | **422** |
| **Conflicto de integridad referencial (nuevo)** | **409** |
| Error inesperado de SQL Server | **500** |

### 0.7 FK en las respuestas

Cada fila de las tablas con FK incluye, además de la **clave** de la relación
(`universidad`, `programa`, …), el **nombre legible** resuelto con un `LEFT JOIN`
**dentro del procedimiento**, aliasado como `<fk>_nombre`:

| Tabla | Clave que se envía | Nombre que se devuelve |
|---|---|---|
| `facultad` | `universidad` | `universidad_nombre` |
| `programa` | `facultad` | `facultad_nombre` |
| `acreditacion`, `registro_calificado`, `activ_academica`, `pasantia`, `premio` | `programa` | `programa_nombre` |
| `activ_academica` | `disenio` (opcional) | `disenio_nombre` |
| `alianza` | `aliado` / `departamento` | `aliado_nombre` / `departamento_nombre` |
| `docente_departamento` | `departamento` | `departamento_nombre` |
| `programa_ac` … | las dos claves | `<fk1>_nombre`, `<fk2>_nombre` |

---

# 1. `GET /` — Diagnóstico

```http
GET /
```

```json
{
  "mensaje": "API Innovación Curricular funcionando",
  "version": "v2",
  "contratos": "docs/spec_kit/versiones/v2/6_contracts.md"
}
```

---

# 2. `GET /api/{tabla}` — Listar registros

Lista los registros **activos** con los nombres de FK resueltos.

### Endpoints (selección)

```http
GET /api/facultad
GET /api/programa
GET /api/acreditacion
GET /api/pasantia
GET /api/premio
GET /api/programa_ac
```

Mientras se integra el CRUD de `programa` de su módulo responsable, los
formularios de las relaciones de Felipe consultan
`GET /api/opciones/programa?limite=N`: devuelve solo `{id, nombre}` de programas
activos con el mismo sobre de lectura (`tabla: "programa"`) o 204 si no hay.
Esta ruta de apoyo no reemplaza `GET /api/programa` ni permite modificarlo.

### Query string

```text
?limite=N        (opcional; entero > 0; por defecto 1000)
```

### Ejemplo — `facultad`

```http
GET /api/facultad?limite=3
```

```json
{
  "tabla": "facultad",
  "limite": 3,
  "total": 2,
  "datos": [
    {
      "id": 1,
      "nombre": "Facultad de Ingeniería",
      "tipo": "Pública",
      "fecha_fun": "1960-03-15",
      "universidad": 1,
      "universidad_nombre": "Universidad de Antioquia",
      "activo": true
    }
  ]
}
```

### Ejemplo — `programa_ac` (tabla puente)

La lista de una puente muestra **los dos nombres**:

```http
GET /api/programa_ac
```

```json
{
  "tabla": "programa_ac",
  "limite": 1000,
  "total": 1,
  "datos": [
    {
      "programa": 10,
      "area_conocimiento": 3,
      "programa_nombre": "Ingeniería de Sistemas",
      "area_conocimiento_nombre": "Matemáticas Aplicada",
      "activo": true
    }
  ]
}
```

### Tabla vacía → 204

```http
HTTP/1.1 204 No Content
```

### `limite <= 0` → 400

```json
{
  "estado": 400,
  "mensaje": "Parámetros inválidos.",
  "detalle": "El parámetro limite debe ser mayor que 0."
}
```

---

# 3. `GET /api/{tabla}/{pk}` — Obtener un registro

Con PK compuesta se envía **las dos claves**:

```http
GET /api/programa_ac/10/3
```

```json
{
  "programa": 10,
  "area_conocimiento": 3,
  "programa_nombre": "Ingeniería de Sistemas",
  "area_conocimiento_nombre": "Matemáticas Aplicada",
  "activo": true
}
```

### Registro inexistente → 404

```json
{
  "estado": 404,
  "mensaje": "Registro no encontrado.",
  "detalle": "No existe un registro activo con id = 999999."
}
```

---

# 4. `POST /api/{tabla}` — Crear registro

SQL Server genera el ID interno automáticamente. El cliente no lo incluye en el POST. `activo` no se envía: se crea en `1`. **La respuesta incluye la fila guardada con el ID asignado** (lo exige el **tropiezo 8**).

### 4.1 `POST /api/facultad`

```http
POST /api/facultad
Content-Type: application/json
```

```json
{
  "nombre": "Facultad de Ciencias",
  "tipo": "Pública",
  "fecha_fun": "1975-08-01",
  "universidad": 1
}
```

**Respuesta:**

```json
{
  "estado": 200,
  "mensaje": "Registro creado exitosamente.",
  "datos": {
    "id": 3,
    "nombre": "Facultad de Ciencias",
    "tipo": "Pública",
    "fecha_fun": "1975-08-01",
    "universidad": 1,
    "universidad_nombre": "Universidad de Antioquia",
    "activo": true
  }
}
```

### 4.2 `POST /api/pasantia` (FK obligatoria a `programa`)

```json
{
  "nombre": "Pasantía internacional",
  "pais": "Chile",
  "empresa": "ACME S.A.",
  "descripcion": "Práctica profesional en el exterior",
  "programa": 10
}
```

### 4.3 Body inválido → 422

```json
{
  "nombre": "",
  "tipo": "Pública",
  "fecha_fun": "1975-08-01",
  "universidad": 1
}
```

```json
{
  "estado": 422,
  "mensaje": "Datos inválidos.",
  "errores": [
    "El campo nombre es obligatorio."
  ]
}
```

### 4.4 Clave de negocio duplicada → 409

Los IDs internos se generan en SQL Server y no se envían en el POST. Las claves
de negocio que siguen siendo PK (por ejemplo, el NIT de `aliado` o una
resolución) y el documento único del catálogo docente sí pueden repetirse; son
conflictos con el estado de la base, no errores inesperados.

```json
{
  "estado": 409,
  "mensaje": "Conflicto con los datos.",
  "detalle": "Ya existe un registro con esa clave (3)."
}
```

### 4.5 FK inexistente → 409

`POST /api/facultad` con `universidad: 999` (que no existe):

```json
{
  "estado": 409,
  "mensaje": "Conflicto con los datos.",
  "detalle": "La universidad indicada no existe o está inactiva."
}
```

---

# 5. `PUT /api/{tabla}/{pk}` — Reemplazo completo

Todos los campos modificables son obligatorios. La PK va en la URL. En
`docente_departamento` y `alianza`, se incluyen las dos claves; el body contiene
solo atributos editables. En las cuatro puentes puras, **no aplica**: no hay
`PUT`.

```http
PUT /api/facultad/3
Content-Type: application/json
```

```json
{
  "nombre": "Facultad de Ciencias Exactas",
  "tipo": "Pública",
  "fecha_fun": "1975-08-01",
  "universidad": 1
}
```

```json
{
  "estado": 200,
  "mensaje": "Registro reemplazado exitosamente.",
  "filasAfectadas": 1
}
```

- Campo faltante → **422**.
- Inexistente → **404**.
- FK inexistente en el body → **409**.

---

# 6. `PATCH /api/{tabla}/{pk}` — Actualización parcial

Sólo se modifican los campos enviados. El **servicio** lee el registro actual, lo
combina con el body y llama al procedimiento de actualización (parámetros fijos).
Para `docente_departamento` y `alianza`, la ruta lleva su pareja compuesta y el
body nunca cambia las claves. Las puentes puras no tienen `PATCH`.

```http
PATCH /api/facultad/3
```

```json
{ "universidad": 2 }
```

```json
{
  "estado": 200,
  "mensaje": "Registro actualizado exitosamente.",
  "filasAfectadas": 1
}
```

- Body vacío → **400** (`"No se envió ningún campo para actualizar."`).
- Inexistente → **404**. FK inexistente → **409**.

---

# 7. `DELETE /api/{tabla}/{pk}` — Eliminación lógica

Marca `activo = 0`. El registro **sigue en la base de datos**.

```http
DELETE /api/pasantia/5
```

```json
{
  "estado": 200,
  "mensaje": "Registro eliminado exitosamente.",
  "filasEliminadas": 1
}
```

- Segundo `DELETE` (o inexistente) → **404**.
- **Padre con hijos activos → 409** (regla §3.1 de [2_spec.md](2_spec.md)):

```http
DELETE /api/programa/10
```

```json
{
  "estado": 409,
  "mensaje": "No se puede retirar el registro.",
  "detalle": "El programa tiene registros asociados activos; primero retírelos."
}
```

---

# 8. Tablas puente puras — asignar y retirar por pareja

Las puentes **no tienen `PUT`** (no se editan). Tienen **asignar** (`POST`) y
**retirar** (`DELETE`).

### 8.1 Asignar — `POST /api/{puente}`

```http
POST /api/programa_ac
Content-Type: application/json
```

```json
{ "programa": 10, "area_conocimiento": 3 }
```

```json
{
  "estado": 200,
  "mensaje": "Pareja asignada exitosamente.",
  "datos": {
    "programa": 10,
    "area_conocimiento": 3,
    "programa_nombre": "Ingeniería de Sistemas",
    "area_conocimiento_nombre": "Matemáticas Aplicada",
    "activo": true
  }
}
```

**Pareja repetida → 409** (también si fue retirada antes: el borrado lógico conserva la PK y la v2 no define reactivación):

```json
{
  "estado": 409,
  "mensaje": "Conflicto con los datos.",
  "detalle": "Esa pareja ya está registrada."
}
```

**FK inexistente → 409:**

```json
{
  "estado": 409,
  "mensaje": "Conflicto con los datos.",
  "detalle": "El programa indicado no existe o está inactivo."
}
```

### 8.2 Retirar — `DELETE /api/{puente}/{pk1}/{pk2}`

```http
DELETE /api/programa_ac/10/3
```

```json
{
  "estado": 200,
  "mensaje": "Pareja retirada exitosamente.",
  "filasEliminadas": 1
}
```

Pareja inexistente (o ya retirada) → **404**.

> `aa_rc` pertenece al issue de Valentina; este bloque describe las cuatro
> puentes puras de Felipe. Las relaciones con atributos de Felipe se describen
> a continuación.

## 8.3 Relaciones con atributos propios — Issue 3 de Felipe

`docente_departamento` y `alianza` tienen PK compuesta y además datos propios.
Se crean por `POST`; se consultan por pareja; se reemplazan por `PUT`; se
actualizan parcialmente por `PATCH`; y se retiran por `DELETE`. En las rutas se
usan las claves en este orden:

```text
PUT/PATCH/DELETE /api/docente_departamento/{docente}/{departamento}
PUT/PATCH/DELETE /api/alianza/{aliado}/{departamento}
```

El `PUT` de `docente_departamento` requiere `dedicacion`, `modalidad` y
`fecha_ingreso`, con `fecha_salida` opcional. El `PUT` de `alianza` requiere
`fecha_inicio`, con `fecha_fin` y `docente` opcionales. Ningún body de edición
acepta una clave distinta para mover la relación.

El `PATCH` conserva los campos ausentes; un campo opcional enviado como `null`
se limpia. Enviar `{}` → **400**. Una pareja inexistente o retirada → **404**.
Si `fecha_salida < fecha_ingreso` o `fecha_fin < fecha_inicio` → **400**.
Un cuerpo mal formado o un campo requerido ausente en PUT → **422**.

---

# 9. Validaciones de los cuerpos de petición

Toda FK se referencia por su **clave** (nunca se digita un texto libre). Las
tareas por tabla (obligatoriedad, largos, formatos) se resumen así:

## 9.1 `facultad`

| Campo | Crear | PUT | PATCH | Regla |
|---|---|---|---|---|
| `id` | — (lo genera SQL Server) | URL | URL | Entero positivo |
| `nombre` | ✓ | ✓ | Opcional | Obligatorio, máx. 60 |
| `tipo` | ✓ | ✓ | Opcional | Obligatorio, máx. 45 |
| `fecha_fun` | ✓ | ✓ | Opcional | Obligatorio, tipo fecha |
| `universidad` | ✓ | ✓ | Opcional | Obligatorio, entero (FK existente) |

## 9.2 `programa`

| Campo | Crear | PUT | PATCH | Regla |
|---|---|---|---|---|
| `id` | — (lo genera SQL Server) | URL | URL | Entero positivo |
| `nombre` | ✓ | ✓ | Opcional | Obligatorio, máx. 60 |
| `tipo` | ✓ | ✓ | Opcional | Obligatorio, máx. 45 |
| `nivel` | ✓ | ✓ | Opcional | Obligatorio, máx. 45 |
| `fecha_creacion` | ✓ | ✓ | Opcional | Obligatorio (texto) |
| `fecha_cierre` | — | Opcional | Opcional | Texto, puede ser nulo |
| `numero_cohortes` | ✓ | ✓ | Opcional | Obligatorio (texto) |
| `cant_graduados` | ✓ | ✓ | Opcional | Obligatorio (texto) |
| `fecha_actualizacion` | ✓ | ✓ | Opcional | Obligatorio (texto) |
| `ciudad` | ✓ | ✓ | Opcional | Obligatorio, máx. 45 |
| `facultad` | ✓ | ✓ | Opcional | Obligatorio (FK existente) |
| `num_acreditaciones` | — | — | — | **Sólo lectura** (disparador) |

## 9.3 `pasantia` / `premio`

| Tabla | Campos obligatorios |
|---|---|
| `pasantia` | `nombre`(45), `pais`(45), `empresa`(45), `descripcion`(45), `programa`(FK); `id` automático |
| `premio` | `nombre`(45), `descripcion`(45), `fecha`(DATE), `entidad_otorgante`(45), `pais`(45), `programa`(FK); `id` automático |

## 9.4 `activ_academica` — única FK opcional

`disenio` (FK a `programa`) es **opcional**. Si el usuario elige «(ninguna)», el
Frontend envía **`null`**, nunca `""`. Si llega `null`, la API lo guarda como
`NULL` (sin intentar convertir la cadena vacía).

## 9.4 DOCENTE — catálogo auxiliar de Felipe

- GET /api/docente: lista fichas activas con {tabla, limite, total, datos}.
- POST /api/docente: recibe documento y nombre; SQL asigna id y la respuesta
  incluye la ficha creada.
- PUT/PATCH/DELETE siguen el CRUD del catálogo. Documento duplicado responde
  409; un docente con relaciones activas no se puede desactivar.
- Documento es único, admite hasta 20 caracteres y no es la llave primaria.

## 9.5 `alianza` y `docente_departamento` — validación de fechas (Issue 3)

| Tabla | Regla |
|---|---|
| `alianza` | Obligatorios: `aliado`(FK), `departamento`(FK), `fecha_inicio`. Opcionales: `fecha_fin`, `docente`. **`fecha_fin ≥ fecha_inicio`** si viene. |
| `docente_departamento` | Obligatorios: `docente`(FK a docente local), `departamento`(FK), `dedicacion`, `modalidad`, `fecha_ingreso`. Opcional: `fecha_salida`. **`fecha_salida ≥ fecha_ingreso`** si viene. |

Una fecha incoherente → **400** (regla de negocio, `ArgumentException`).

---

# 10. El 409 en detalle

El **409** es la novedad de la versión. Se produce en **tres** situaciones, todas
de **integridad referencial** o de **estado** ([4_research.md](4_research.md) D1):

| # | Situación | Ejemplo |
|---|---|---|
| 1 | **FK inexistente** | `POST /api/programa` con `facultad: 999` |
| 2 | **Pareja duplicada en puente** | `POST /api/programa_ac` con `{10,3}` ya existente |
| 3 | **Borrado de padre con hijos activos** | `DELETE /api/programa/10` con detalle activo |

**Quién traduce:** el **SERVICIO** captura el `SqlException` del motor
(números 547 = FK, 2627/2601 = PK/UNIQUE) y lanza `ConflictoExcepcion`; el
**controlador** la convierte en 409 con el `mensaje`/`detalle` en castellano. El
500 queda reservado para errores **inesperados** del motor.

> **Pregunta de sustentación.** Saber si la FK existe **exige ir a la base**; por
> eso no es 422. Y la base **funciona**; sólo rechazó la relación; por eso no es
> 500. Ver [4_research.md](4_research.md) D1.

---

# 11. Swagger

```http
GET /swagger
```

Documenta y permite probar los endpoints de las **15 tablas de la v2** (además de
los de la v1).

---

# 12. Resumen de endpoints

Para **PK simple** (`facultad`, `programa`, `activ_academica`, `pasantia`,
`premio`):

| Verbo | Ruta | Operación | Éxito |
|---|---|---|---|
| GET | `/api/{tabla}` | Listar | 200 |
| GET | `/api/{tabla}/{id}` | Obtener uno | 200 |
| POST | `/api/{tabla}` | Crear (devuelve la fila) | 200 |
| PUT | `/api/{tabla}/{id}` | Reemplazar | 200 |
| PATCH | `/api/{tabla}/{id}` | Actualizar parcial | 200 |
| DELETE | `/api/{tabla}/{id}` | Borrado lógico | 200 |

Para **PK con nombre** (`acreditacion`, `registro_calificado`): igual, con
`{resolucion}` / `{codigo}`.

Para **relaciones con PK compuesta**:

| Verbo | Ruta | Operación | Éxito |
|---|---|---|---|
| GET | `/api/{puente}` | Listar (con nombres) | 200 |
| GET | `/api/{puente}/{pk1}/{pk2}` | Obtener la pareja | 200 |
| POST | `/api/{puente}` | **Asignar** | 200 |
| DELETE | `/api/{puente}/{pk1}/{pk2}` | **Retirar** | 200 |

Las cuatro puentes puras de Felipe (`programa_ac`, `programa_pe`,
`programa_ci`, `an_programa`) **no** tienen `PUT`, `PATCH` ni botón de editar.
`docente_departamento` y `alianza` también tienen GET/POST/DELETE, y además
`PUT /api/{tabla}/{pk1}/{pk2}` y `PATCH /api/{tabla}/{pk1}/{pk2}` para sus
atributos, sin cambiar las claves.

Además: `GET /` (diagnóstico) y `GET /swagger`.

---

# 13. Matriz de comportamiento HTTP

| Operación | 200 | 204 | 400 | 404 | 409 | 422 | 500 |
|---|---|---|---|---|---|---|---|
| GET lista | ✓ | ✓ | ✓ | | | | ✓ |
| GET por llave | ✓ | | | ✓ | | | ✓ |
| POST crear | ✓ | | | | ✓ | ✓ | ✓ |
| PUT | ✓ | | | ✓ | ✓ | ✓ | ✓ |
| PATCH | ✓ | | ✓ | ✓ | ✓ | ✓ | ✓ |
| DELETE | ✓ | | | ✓ | ✓ | | ✓ |
| POST relación compuesta (asignar) | ✓ | | | | ✓ | ✓ | ✓ |
| PUT relación con atributos (por pareja) | ✓ | | | ✓ | ✓ | ✓ | ✓ |
| PATCH relación con atributos (por pareja) | ✓ | | ✓ | ✓ | ✓ | ✓ | ✓ |
| DELETE relación compuesta (retirar) | ✓ | | | ✓ | | | ✓ |

---

# 14. Reglas fundamentales de la v2

1. Sólo se exponen las **15 tablas con FK** (la v1 sigue aparte).
2. Todo el CRUD pasa por **procedimientos almacenados**; **cero SQL** en C#.
3. `sp_crear_*` devuelve la **fila guardada**; el `POST` la incluye en `datos`.
4. Las 15 tablas tienen `activo` ⇒ borrado lógico y listados con `activo = 1`.
5. Las FK se **eligen** (desplegable); no se digitan.
6. **409** (no 500) en: FK inexistente, pareja duplicada y borrado de padre con
   hijos.
7. Un **disparador** mantiene `programa.num_acreditaciones`; la API no lo
   escribe.
8. Las claves de una relación compuesta son inmutables. `docente_departamento`
   y `alianza` permiten editar sus atributos por pareja; las cuatro puentes
   puras de Felipe sólo se asignan y retiran.
9. La envoltura y los mensajes van en **castellano** (decisión de la versión;
   ver §0.3).
10. La versión de estos contratos corresponde exclusivamente a la **v2**.
