# Plan técnico — Versión 2: 15 tablas con FK, procedimientos y disparador

> **Versión 2** · CÓMO construir lo especificado en [2_spec.md](2_spec.md).
> El porqué de cada decisión: [4_research.md](4_research.md) · el modelo de
> datos: [5_data_model.md](5_data_model.md) · contratos exactos:
> [6_contracts.md](6_contracts.md) · interfaz: [9_frontend.md](9_frontend.md) ·
> orden de trabajo: [8_tasks.md](8_tasks.md).

---

## 1. Stack

La v2 **no cambia el stack de la v1**; sólo añade piezas.

| Pieza | Elección | Por qué |
|---|---|---|
| Lenguaje / framework | **C# sobre ASP.NET Core (.NET 10)** | Igual que la v1 |
| Acceso a datos | **Dapper** sobre `Microsoft.Data.SqlClient`, **invocando procedimientos almacenados** | La v2 mueve el SQL a la base; el repositorio sólo llama |
| Validación | **Tres peticiones por tabla** (Crear, Reemplazo, Actualizar) con anotaciones | Igual que la v1: el framework responde 422 |
| Motor | **SQL Server 2022** (contenedor oficial) | Único motor |
| **Nuevo:** procedimientos | `CREATE OR ALTER PROCEDURE` en `db/04_procedimientos_v2_*.sql` | Una sola vía al SQL; el repositorio no escribe SQL |
| **Nuevo:** disparador | `trg_acreditacion_contador` (`AFTER INSERT, UPDATE, DELETE`) | Mantiene `programa.num_acreditaciones` |
| Base de datos | `innovacion_curricular` (misma de la v1) | La BD viene dada; la v2 sólo agrega `activo`, `num_acreditaciones` y procedimientos |
| Frontend | **Angular** (TypeScript), *standalone*, `HttpClient` | Igual que la v1, ampliado |
| Ejecución API / Front | `dotnet run` (8036) · `ng serve --host 0.0.0.0 --port 8037` | Igual que la v1 |

## 2. Orden de construcción (dependencias de las FK)

```
facultad ──► programa ──► { acreditacion, registro_calificado, activ_academica,
                            pasantia, premio, docente_departamento, alianza,
                            programa_ac, programa_pe, programa_ci,
                            an_programa }
     │                          │
universidad (v1)          enfoque_rc, aa_rc (cuelgan de registro_calificado /
                          activ_academica, que ya cuelgan de programa)
```

**Emmanuel va primero** (`facultad`, sin la cual no se puede crear `programa`).
Luego **Valentina** (`programa` + su bloque) y **Felipe** en paralelo. El
integrador cierra con la regresión de la v1 y el tag `v2`. El detalle por fase
está en [8_tasks.md](8_tasks.md).

## 3. Estructura de carpetas

La v2 **reutiliza** todo el andamiaje de la v1 y agrega, por cada tabla nueva,
el mismo juego de archivos. La estructura de la v1 (`Modelos/`, `Peticiones/`,
`Controllers/`, `Servicios/`, `Repositorios/`, `Excepciones/`, `pruebas/`) no se
mueve.

```
(raíz del proyecto)
├── docker-compose.yml                         # sin cambios estructurales; la API ya levanta todo
├── db/
│   ├── innovacion_curricular.ss.sql           # script provisto (intacto)
│   ├── 01_activar_borrado_logico.sql          # v1: `activo` en las 7 tablas sin FK
│   ├── 02_datos_iniciales.sql                 # v1: catálogos base
│   ├── 03_activar_borrado_logico_v2.sql       # NUEVO: `activo` en las 15 tablas con FK
│   ├── 04_procedimientos_v2_emmanuel.sql      # NUEVO: procs + disparador de las tablas de Emmanuel
│   ├── 04_procedimientos_v2_valentina.sql     # NUEVO: procs + disparador (Valentina)
│   ├── 04_procedimientos_v2_felipe.sql        # NUEVO: procs de puentes/personas (Felipe)
│   └── init.sh                                # actualizado: corre 01..04 en orden, idempotente
├── api_innovacion/
│   ├── Program.cs                             # DI de las 15 tablas + 409 + `version = "v2"`
│   ├── Respuestas/Respuestas.cs               # + RespuestaCreacion<T>  (POST devuelve la fila)
│   ├── Excepciones/
│   │   ├── NoEncontradoExcepcion.cs           # (v1) → 404
│   │   └── ConflictoExcepcion.cs              # NUEVO: → 409 (integridad referencial)
│   ├── Modelos/<Tabla>.cs                     # entidad: una propiedad por columna + Activo + <Fk>Nombre
│   ├── Peticiones/<TablaPeticiones>.cs        # Crear / Reemplazo / Actualizar (o Pareja en puentes)
│   ├── Controllers/<Tabla>Controller.cs       # HTTP: verbos + try/catch → códigos (añade 409)
│   ├── Servicios/IServicio<Tabla>.cs · Servicio<Tabla>.cs
│   ├── Repositorios/IRepositorio<Tabla>.cs · Repositorio<Tabla>SqlServer.cs
│   │                                          # sólo llaman procedimientos (StoredProcedure)
│   └── pruebas/Programa.cs                    # extendido con dobles de las tablas de la v2
└── frontend/ (estructura de la v1, ampliada)
    └── src/app/
        ├── models/catalogo.ts                 # + tablas de la v2, columnas de FK y de fecha
        ├── services/api.service.ts            # + asignar()/retirar() de puentes
        ├── components/nav-bar/                # se genera desde TABLAS (crece solo)
        └── pages/
            ├── dashboard/                     # tarjetas: crece con TABLAS
            ├── catalogo/                      # + desplegables de FK y campos de fecha
            └── programa-detalle/              # NUEVO: maestro-detalle de programa
```

`<Tabla>` se reemplaza por cada una de las 15 entidades. La correspondencia
tabla ↔ clase ↔ PK ↔ FKs está en [5_data_model.md](5_data_model.md).

> **Nombres.** Se sigue el **molde de la v1**: nombres de dominio en castellano
> (`ServicioFacultad`, `RepositorioFacultadSqlServer`, `FacultadPeticiones`,
> `ConflictoExcepcion`), controladores con sufijo `Controller`
> (`FacultadController`) y rutas `[Route("api/facultad")]` (nombre real de la
> tabla, Art. 7). Esto es coherente con la v1 y con la decisión de envoltura en
> castellano ([2_spec.md](2_spec.md) §5).

## 4. Arquitectura en capas (flujo de una petición)

```
HTTP → ASP.NET routing          (atributos [HttpGet]/[HttpPost]…)
     → validación de la PETICIÓN (anotaciones → 422 automático)
     → <Tabla>Controller        (try/catch: excepciones → códigos HTTP, INCLUIDO 409)
     → IServicio<Tabla>         (reglas de negocio; traduce SqlException → ConflictoExcepcion)
     → IRepositorio<Tabla>      (el servicio no sabe qué motor hay detrás)
     → Repositorio<Tabla>SqlServer  (Dapper + CommandType.StoredProcedure, DynamicParameters)
     → SQL Server (PROCEDIMIENTO ALMACENADO)
```

**Regla de dependencias:** controller → servicio → interfaz de repositorio. Sólo
el **ensamblador** (`Program.cs`) conoce clases concretas.

**Diferencia con la v1:** donde la v1 hacía `QueryAsync(sql)`, la v2 hace
`QueryAsync<T>("sp_listar_facultad", parametros, commandType: CommandType.StoredProcedure)`.
El SQL ya no vive en el repositorio.

## 5. Los cinco procedimientos por tabla

Para cada una de las 15 tablas se define el mismo juego. Nombres y formas
exactas:

| Procedimiento | Hace | Devuelve |
|---|---|---|
| `sp_listar_<tabla>(@limite)` | `SELECT TOP (@limite) … WHERE activo = 1` + `LEFT JOIN` a los maestros para traer `<fk>_nombre` | filas |
| `sp_consultar_<tabla>(@pk)` | la fila por PK, `activo = 1`, con los nombres de FK | 1 fila |
| `sp_crear_<tabla>(@cols…)` | `INSERT` (siempre `activo = 1`) y luego `SELECT` de la fila recién creada | **la fila guardada** |
| `sp_actualizar_<tabla>(@pk, @cols…)` | `UPDATE … WHERE pk = @pk AND activo = 1` | `SELECT @@ROWCOUNT AS filas` |
| `sp_eliminar_<tabla>(@pk)` | `UPDATE … SET activo = 0 WHERE pk = @pk AND activo = 1` | `SELECT @@ROWCOUNT AS filas` |

Reglas:

- **Listados filtran `activo = 1`** (tropiezo 7).
- **`sp_crear_*` termina con un `SELECT`** de la fila guardada (tropiezo 8), para
  que la interfaz pueda mostrar lo que creó.
- Los nombres de las columnas que devuelve el `SELECT` deben **coincidir** con
  las propiedades del modelo; los nombres de FK se devuelven con **alias**
  (`universidad_nombre`, `programa_nombre`, …) — ver el gotcha de Dapper en
  [4_research.md](4_research.md) D2.
- El borrado es **lógico**, nunca un `DELETE` físico.

### 5.1 Borrado del maestro con hijos (regla §3.1 de la spec)

`sp_eliminar_programa` (y `sp_eliminar_facultad`) **no** usa el molde plano.
Antes de marcar `activo = 0` comprueban que **no haya hijos activos**; si los
hay, **no actualizan ninguna fila** (devuelven `filas = 0`) y el **servicio**
lo traduce a 409. Ejemplo de forma:

```sql
CREATE OR ALTER PROCEDURE sp_eliminar_programa @id INT AS
BEGIN
    IF EXISTS (SELECT 1 FROM pasantia  WHERE programa = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM premio    WHERE programa = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM acreditacion WHERE programa = @id AND activo = 1)
    /* … una comprobación por cada tabla hija … */
        RETURN;                 -- no se retira: el servicio responde 409
    UPDATE programa SET activo = 0 WHERE id = @id AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
```

> Cuáles tablas cuentan como "hijos activos" de `programa` se lista en
> [5_data_model.md](5_data_model.md) §4.

### 5.2 Tablas puente (asignar / retirar por pareja)

Las tablas puente **no** tienen el juego anterior completo. Tienen:

- `sp_listar_<puente>(@limite)` — filas activas **con los dos nombres** (JOIN
  dentro del procedimiento).
- `sp_asignar_<puente>(@pk1, @pk2, …)` — inserta la pareja (`activo = 1`);
  devuelve la fila. Una pareja repetida la rechaza la **PK compuesta** (409).
- `sp_retirar_<puente>(@pk1, @pk2)` — `UPDATE … SET activo = 0` de la pareja
  (`@@ROWCOUNT`).

No hay `PUT` ni edición: una pareja existe o no existe (tropiezo 9).

## 6. Repositorio: sólo procedimientos, con `DynamicParameters`

El repositorio **no** escribe SQL. La sutileza que más rompe es el **nombre de
los parámetros**: el procedimiento los declara en **snake_case** (`@fecha_fun`,
`@fecha_creacion`) y Dapper **no** los empareja con propiedades PascalCase de un
objeto anónimo. Por eso **se arma un `DynamicParameters` con los nombres exactos
snake_case**:

```csharp
// El repositorio NO escribe SQL: sólo arma parámetros y llama al procedimiento.
var parametros = new DynamicParameters();
parametros.Add("@nombre", entidad.Nombre);
parametros.Add("@tipo", entidad.Tipo);
parametros.Add("@fecha_fun", entidad.FechaFun);        // snake_case, igual que la columna
parametros.Add("@universidad", entidad.Universidad);

const string procedimiento = "sp_crear_facultad";
// Devuelve la fila guardada (el procedimiento termina con SELECT).
var fila = await conexion.QuerySingleAsync<Facultad>(
    procedimiento, parametros, commandType: CommandType.StoredProcedure);
```

Interfaz estándar de la v2 (una por tabla):

```csharp
public interface IRepositorioFacultad
{
    Task<List<Facultad>> ObtenerTodosAsync(int limite);
    Task<Facultad?> ObtenerPorIdAsync(int id);
    Task<Facultad> CrearAsync(Facultad entidad);   // devuelve la fila guardada
    Task<int> ActualizarAsync(Facultad entidad);   // 0 → lo traduce el servicio a 404
    Task<int> EliminarAsync(int id);               // 0 → 404 (o 409, según la regla §3.1)
}
```

> **Diferencia con la v1:** en la v1 `ActualizarAsync` recibía un
> `Dictionary<string, object>` (para PUT/PATCH). En la v2 el procedimiento tiene
> **parámetros fijos**, así que el repositorio recibe la **entidad completa**; el
> `PATCH` lo resuelve el **servicio** leyendo el registro actual y combinando
> (§7).

## 7. Servicio: reglas de negocio y traducción de errores

El servicio:

- Valida reglas de negocio (por ejemplo, `limite > 0`; `PATCH` vacío →
  `ArgumentException` → 400).
- **PATCH:** lee el registro con `ObtenerPorIdAsync`, aplica sólo los campos
  enviados sobre una copia, y llama a `ActualizarAsync` con la entidad completa.
- Traduce a `NoEncontradoExcepcion` cuando el id no existe o ya está retirado
  (`filas = 0`).
- **Traduce `SqlException` de integridad referencial a `ConflictoExcepcion`**
  (número de error 547 = FK, 2627/2601 = PK/UNIQUE duplicada). Es el punto exacto
  que la guía marca como pregunta de sustentación (*"¿quién traduce el error del
  motor y en qué capa?"*): **el servicio**, no el controlador.
- Aplica la **regla §3.1**: si `EliminarAsync` de un maestro devuelve 0 filas
  **porque tiene hijos activos**, lanza `ConflictoExcepcion` (409). El servicio
  distingue "no existe" (404) de "no se puede retirar por hijos" (409)
  consultando el maestro antes de borrar (ver [4_research.md](4_research.md) D4).

## 8. El controlador: traducción a códigos HTTP

| Situación | Excepción / origen | HTTP |
|---|---|---|
| Body con errores de forma | validación de la petición | **422** |
| Regla de negocio (PATCH vacío, fecha incoherente) | `ArgumentException` | **400** |
| Registro inexistente / ya retirado | `NoEncontradoExcepcion` | **404** |
| **FK inexistente, pareja duplicada, padre con hijos** | **`ConflictoExcepcion`** | **409** |
| Error inesperado del motor | `SqlException` (no de integridad) | 500 |

El controlador es idéntico al de la v1 **más una cláusula** `catch
(ConflictoExcepcion) → 409`. Todas las tablas comparten el mismo patrón.

## 9. El ensamblador (`Program.cs`)

- **DI:** un par `AddScoped` (repositorio + servicio) por cada una de las **15
  tablas nuevas** (además de los 7 pares de la v1).
- **409:** el mapeo vive en cada controlador (no hay middleware); el 422 sigue
  global (`InvalidModelStateResponseFactory`), sin cambios.
- **Cadena de conexión:** se mantiene `ConnectionStrings:SqlServer` y se agrega
  el *fallback* por variable de entorno
  `Environment.GetEnvironmentVariable("DB_CONNECTION") ?? builder.Configuration.GetConnectionString("SqlServer")`
  (cumple el Art. 8 sobre secretos por entorno sin romper la v1). El
  `docker-compose.yml` define `DB_CONNECTION`.
- **Diagnóstico:** `GET /` responde `version = "v2"` y
  `contratos = "docs/spec_kit/versiones/v2/6_contracts.md"`.

## 10. Nombres de columna, alias y mapeo (gotchas de Dapper)

La v2 depende de que el **nombre de columna** (o de alias) coincida con la
**propiedad** del modelo. `Program.cs` ya activa
`Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true` (v1), que mapea
`fecha_fun` → `FechaFun`. Las dos trampas a respetar:

1. **Listados con JOIN:** el `SELECT` debe **aliasar** la columna del maestro a
   `<fk>_nombre` para que caiga en la propiedad `<Fk>Nombre`.
2. **Parámetros de procedimiento:** siempre `DynamicParameters` con nombre
   snake_case exacto (§6).

## 11. Docker: un solo comando (sin cambios estructurales)

`docker compose up -d --build` sigue levantando los cuatro servicios. Lo que
cambia es lo que corre `sqlserver-init`:

```
1. innovacion_curricular.ss.sql        (provisto: 25 tablas)         ← intacto
2. 01_activar_borrado_logico.sql       (v1: activo en 7 tablas)      ← intacto
3. 02_datos_iniciales.sql              (v1: catálogos base)          ← intacto
4. 03_activar_borrado_logico_v2.sql    (v2: activo en 15 tablas)     ← NUEVO
5. 04_procedimientos_v2_*.sql          (v2: procs + disparador)      ← NUEVO
```

Todos los pasos son **idempotentes** (`IF COL_LENGTH(...) IS NULL`,
`CREATE OR ALTER PROCEDURE`, `DROP TRIGGER IF EXISTS` + `CREATE TRIGGER`). La API
espera a `sqlserver-init` con `service_completed_successfully`, como en la v1.

## 12. Frontend Angular — diseño de la interfaz

El detalle de páginas, componentes y flujo vive en
[9_frontend.md](9_frontend.md). Las decisiones técnicas de la v2:

- **Una página de CRUD reutilizada** (`pages/catalogo`): recibe la tabla por
  ruta. La tabla de configuración (`models/catalogo.ts`) crece con las 15 tablas
  nuevas, sus columnas y **qué columnas son FK** (para pintar desplegables) y
  cuáles son **fecha** (para pintar selectores de fecha).
- **Desplegables de FK** cargados desde la API en paralelo (`forkJoin`) para no
  encadenar peticiones (tropiezo 5); cada uno muestra el nombre, envía la clave
  y sólo ofrece activos.
- **«(ninguna)» → `null`** en FK opcionales; nunca `""` (tropiezo 4).
- **Tablas puente sin botón Editar**: sólo asignar/retirar.
- **`pages/programa-detalle`** (nueva): maestro-detalle de `programa`.
- `api.service.ts` gana `asignar(tabla, pareja)` y `retirar(tabla, pk1, pk2)`.
- El **dashboard** y la **nav-bar** crecen solos: se generan desde la lista
  `TABLAS`, a la que sólo se le agregan las 15 entradas nuevas.
- **409 en pantalla:** el `mensaje` castellano del conflicto se muestra como un
  aviso legible ("Esa pareja ya está registrada", "El programa tiene registros
  asociados"), sin jerga.
