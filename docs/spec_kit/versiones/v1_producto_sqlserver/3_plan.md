# Plan técnico — Versión 1: catálogos base (7 tablas sin FK) + SQL Server (C#/ASP.NET Core)

> **Versión 1** · CÓMO construir lo especificado en [2_spec.md](2_spec.md).
> El porqué de cada decisión: [4_research.md](4_research.md) · el modelo
> de datos: [5_data_model.md](5_data_model.md) · contratos exactos:
> `6_contracts.md` · orden de trabajo: `8_tasks.md`.

---

## 1. Stack

| Pieza | Elección | Por qué |
|---|---|---|
| Lenguaje / framework | **C# sobre ASP.NET Core (.NET 10)** | Controladores con atributos, DI integrada, async nativo |
| Acceso a datos | **Dapper** sobre `Microsoft.Data.SqlClient`, con SQL parametrizado a mano | SQL visible en el código, sin ORM de entidades |
| Validación | **Tres peticiones por tabla** (Crear, Reemplazo, Actualizar) con anotaciones | El framework valida el body y responde 422 |
| Motor | **SQL Server 2022** (contenedor oficial) | Único motor de esta versión |
| Ejecución de la API en el contenedor | `dotnet run` | Arranque directo y predecible; sin recompilación en caliente |
| Base de datos | `innovacion_curricular` (script provisto, 25 tablas; esta versión solo nombra 7) | La BD viene dada completa |

## 2. Estructura de carpetas

La versión trabaja con **7 entidades sin relaciones entre sí**:
`area_conocimiento`, `universidad`, `aspecto_normativo`,
`practica_estrategia`, `enfoque`, `car_innovacion`, `aliado`. Cada una
repite el mismo patrón de capas — controlador, servicio, repositorio, con
sus propias peticiones — sin una capa genérica compartida entre ellas.

```
(raíz del proyecto)
├── docker-compose.yml                    # UN comando: sqlserver + init + api
├── db/
│   ├── innovacion_curricular.ss.sql      # la BD completa, PROVISTA (se copia, no se genera)
│   ├── 01_activar_borrado_logico.sql     # agrega la columna `activo` a las 7 tablas de esta versión
│   ├── 02_datos_iniciales.sql            # carga los registros base de `area_conocimiento` y `universidad`
│   └── init.sh                           # corre los tres scripts, en orden, la primera vez
└── api_innovacion/
    ├── ApiInnovacion.csproj              # el proyecto .NET (paquetes: SqlClient, Dapper, Swashbuckle)
    ├── Program.cs                        # punto de entrada: ENSAMBLADOR (DI) + 422 + rutas
    ├── appsettings.json                  # cadena de conexión (default localhost:11467)
    ├── Dockerfile                        # sdk:10.0 + dotnet run (puerto 8036)
    ├── Modelos/
    │   └── <Tabla>.cs                    # la ENTIDAD: propiedades tipadas + Activo
    ├── Peticiones/
    │   ├── <Tabla>Crear.cs               # petición del POST (todo obligatorio, incluida la PK)
    │   ├── <Tabla>Reemplazo.cs           # petición del PUT (todo obligatorio, sin la PK)
    │   └── <Tabla>Actualizar.cs          # petición del PATCH (todo opcional)
    ├── Controllers/
    │   └── <Tabla>Controller.cs          # HTTP: atributos de verbo, try/catch → códigos
    ├── Servicios/
    │   ├── IServicio<Tabla>.cs           # interface del servicio
    │   └── Servicio<Tabla>.cs            # reglas de negocio; recibe IRepositorio<Tabla>
    ├── Repositorios/
    │   ├── IRepositorio<Tabla>.cs        # interface: 5 métodos de datos (async)
    │   └── Repositorio<Tabla>SqlServer.cs   # Dapper + SQL a mano parametrizado
    ├── Excepciones/
    │   └── NoEncontradoExcepcion.cs      # la excepción de negocio que el controller vuelve 404
    └── pruebas/
        ├── PruebaCapas.csproj            # proyecto de consola aparte (criterio 5)
        └── Programa.cs                   # un servicio con un repositorio falso, sin BD
```

`<Tabla>` se reemplaza por cada una de las siete entidades:

| Tabla (BD) | Prefijo de clase C# | Llave primaria |
|---|---|---|
| `area_conocimiento` | `AreaConocimiento` | `id` |
| `universidad` | `Universidad` | `id` |
| `aspecto_normativo` | `AspectoNormativo` | `id` |
| `practica_estrategia` | `PracticaEstrategia` | `id` |
| `enfoque` | `Enfoque` | `id` |
| `car_innovacion` | `CarInnovacion` | `id` |
| `aliado` | `Aliado` | `nit` |

Ninguna de las siete tablas usa `IDENTITY`: la llave primaria la asigna
quien hace el `POST`.

## 3. Arquitectura en capas (flujo de una petición)

```
HTTP → ASP.NET routing        (los atributos [HttpGet]/[HttpPost]… deciden el método)
     → validación de la PETICIÓN (anotaciones de la petición del verbo → 422 automático)
     → <Tabla>Controller      (try/catch: traduce excepciones a códigos HTTP)
     → IServicio<Tabla>       (interfaz — reglas de negocio)
     → IRepositorio<Tabla>    (interfaz — el servicio no sabe qué motor hay detrás)
     → Repositorio<Tabla>SqlServer (Dapper + parámetros @)
     → SQL Server
```

Este flujo se repite siete veces en paralelo, sin acoplamiento entre
tablas. Lo único compartido es `NoEncontradoExcepcion` y la cadena de
conexión.

**Regla de dependencias:** controller → servicio → interfaz de
repositorio. Solo el ENSAMBLADOR (la sección de DI de `Program.cs`)
conoce clases concretas.

## 4. Decisiones de diseño clave

### 4.1 Interfaces de C# desde el día uno

```csharp
public interface IRepositorioAreaConocimiento
{
    Task<List<AreaConocimiento>> ObtenerTodosAsync(int limite);
    Task<AreaConocimiento?> ObtenerPorIdAsync(int id);
    Task CrearAsync(AreaConocimiento entidad);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos); // PUT y PATCH
    Task<int> EliminarAsync(int id);                                    // UPDATE activo = 0
}
```

El servicio recibe la interfaz por constructor (la inyecta el
ensamblador). `ActualizarAsync` recibe un diccionario porque el PUT envía
todos los campos y el PATCH solo los que llegan — la diferencia la marca
la petición del verbo, no el método del repositorio.

### 4.2 La validación vive en las peticiones (tres por tabla)

- `<Tabla>Crear`      → POST: todos los campos obligatorios, incluida la llave primaria.
- `<Tabla>Reemplazo`  → PUT: todos los campos obligatorios, sin la llave primaria (va en la URL).
- `<Tabla>Actualizar` → PATCH: todos los campos opcionales.

Cada `VARCHAR(n)` de la tabla es el largo máximo permitido; ningún campo
de texto puede llegar vacío; los enteros de llave primaria deben ser
positivos. El body vacío en PATCH es 400 y lo decide el **servicio**: no
es un problema de forma sino de regla de negocio.

### 4.3 Formato de las respuestas

| Caso | Código | Forma |
|---|---|---|
| Lista con resultados | 200 | `{"tabla", "limite", "total", "datos"}` |
| Lista vacía | 204 | sin cuerpo |
| Registro por id | 200 | el objeto, sin sobre |
| Registro no encontrado | 404 | `{"estado", "mensaje", "detalle"}` |
| Creación exitosa | 200 | `{"estado", "mensaje"}` |
| Reemplazo o actualización exitosa | 200 | `{"estado", "mensaje", "filasAfectadas"}` |
| Eliminación exitosa | 200 | `{"estado", "mensaje", "filasEliminadas"}` |
| Body inválido | 422 | `{"estado", "mensaje", "detalle", "errores"}` |
| Regla de negocio violada | 400 | `{"estado", "mensaje", "detalle"}` |
| Error del motor | 500 | `{"estado", "mensaje", "detalle"}` |
| Diagnóstico (`GET /`) | 200 | `{"mensaje", "version", "contratos"}` |

### 4.4 El ensamblador: la sección de DI de Program.cs

```csharp
builder.Services.AddScoped<IRepositorioAreaConocimiento>(
    _ => new RepositorioAreaConocimientoSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioAreaConocimiento, ServicioAreaConocimiento>();

// el mismo par, una vez por cada una de las siete tablas
```

Sin fábrica ni selección de motor: esta versión trabaja con un único
motor y el código lo dice directamente.

### 4.5 SQL del repositorio (Dapper, siempre parametrizado)

```sql
SELECT TOP (@limite) id, gran_area, area, disciplina, activo
  FROM area_conocimiento WHERE activo = 1 ORDER BY id
SELECT id, gran_area, area, disciplina, activo
  FROM area_conocimiento WHERE id = @id AND activo = 1
INSERT INTO area_conocimiento (id, gran_area, area, disciplina, activo)
  VALUES (@id, @gran_area, @area, @disciplina, 1)
UPDATE area_conocimiento SET gran_area = @gran_area, area = @area, disciplina = @disciplina
  WHERE id = @id AND activo = 1
UPDATE area_conocimiento SET activo = 0 WHERE id = @id AND activo = 1
```

Todas las lecturas filtran `activo = 1`. La eliminación es un `UPDATE`,
nunca un `DELETE` físico. Conexión por operación con `await using`; todo
`async`. (En `aliado` la llave primaria es `nit` en vez de `id`; el resto
de la plantilla es idéntico.)

### 4.6 Traducción de excepciones a HTTP (en el controller)

| Situación | HTTP |
|---|---|
| Body con errores de forma | 422 |
| `ArgumentException` (regla de negocio, p. ej. PATCH con body vacío) | 400 |
| `NoEncontradoExcepcion` (id inexistente o ya borrado) | 404 |
| `SqlException` y cualquier otra | 500 |

### 4.7 Inicialización de la base de datos

El contenedor `sqlserver-init` corre, en orden:
1. `innovacion_curricular.ss.sql` — crea las 25 tablas de la base.
2. `01_activar_borrado_logico.sql` — agrega `activo BIT NOT NULL DEFAULT 1` a las siete tablas de esta versión.
3. `02_datos_iniciales.sql` — inserta los registros base de `area_conocimiento` y `universidad`.

Los tres pasos son idempotentes, para que repetir `docker compose up` no
falle ni duplique datos.

## 5. Docker: un solo comando

`docker compose up -d --build` deja funcionando tres servicios:
`sqlserver` (puerto 11467), `sqlserver-init` (corre los scripts de `db/`
una sola vez) y `api-innovacion` (puerto 8036). La API corre con
`dotnet run` dentro del contenedor: cada cambio de código requiere
reconstruir la imagen, pero se evita el comportamiento a veces inestable
de la recarga en caliente durante la ejecución.