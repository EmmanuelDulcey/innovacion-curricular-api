# Investigación y decisiones — Versión 1: catálogos base (7 tablas) + SQL Server

> **Versión 1** · **Lectura opcional** (el porqué de las decisiones del
> plan, con las alternativas evaluadas y descartadas). Complementa a
> [3_plan.md](3_plan.md); el modelo de datos exacto está en
> [5_data_model.md](5_data_model.md).

---

## D1 — El ejecutor de la capa de datos: Dapper

**Contexto.** Los siete repositorios necesitan (1) ejecutar SQL contra el
motor y (2) mapear filas ↔ objetos del modelo. El SQL debe quedar visible
en el código y siempre parametrizado.

**Opciones evaluadas:** (a) Entity Framework Core · (b) **Dapper** · (c)
ADO.NET crudo (`SqlConnection` + `SqlCommand` con mapeo manual).

| Criterio | EF Core | Dapper | ADO.NET crudo |
|---|---|---|---|
| SQL visible en el repositorio | ✗ (lo genera LINQ) | ✓ | ✓ |
| Parametrización | ✓ | ✓ (`@param`) | ✓ (`@param`) |
| Costo de repetir el patrón siete veces | alto | bajo | alto |
| Riesgo de abstracción con fuga | alto | bajo | ninguno |
| Dependencias | pesada | 1 paquete MIT, estable | ninguna |

**Decisión: (b) Dapper.** `QueryAsync<T>`/`ExecuteAsync` reciben el SQL
escrito a mano; Dapper solo mapea columna→propiedad por nombre.

**Consecuencias.** (+) Siete repositorios cortos y uniformes. (+) Sin
lock-in: quitar Dapper es volver al mapeo manual, el SQL no cambia. (−)
Una dependencia más, mitigada por su licencia y madurez.

## D2 — Capas completas por tabla, sin generalizar con un repositorio genérico

**Alternativa descartada:** una sola clase `RepositorioGenerico<T>` /
`ServicioGenerico<T>` reutilizada para las siete tablas.

**Decisión:** controller → servicio → repositorio con interfaces propias
de cada tabla.

**Por qué:** aunque las siete tablas son catálogos simples y parecidos, el
criterio de aceptación de probar el servicio con un repositorio falso, sin
base de datos, es la prueba objetiva de que cada juego de capas quedó bien
cortado. Una capa genérica sería más corta, pero esconde justamente lo que
se busca demostrar.

## D3 — Sin fábrica ni selección de motor

**Decisión:** catorce registros `AddScoped` (dos por tabla) que instancian
la única combinación existente.

**Por qué:** una fábrica con un solo motor es código muerto. Las siete
interfaces `IRepositorio<Tabla>` sí se escriben desde ya — son la puerta
por la que entrará un segundo motor en versiones futuras; el mecanismo de
selección se agrega cuando exista algo que seleccionar.

## D4 — La base de datos completa desde el inicio

**Decisión:** `innovacion_curricular.ss.sql` crea la base completa (22
tablas del módulo de Innovación Curricular más 3 de gestión de usuarios);
el código de esta versión solo puede nombrar las siete tablas sin llave
foránea: `area_conocimiento`, `universidad`, `aspecto_normativo`,
`practica_estrategia`, `enfoque`, `car_innovacion`, `aliado`.

**Por qué:** evita migraciones de esquema entre versiones — la base no
cambia de forma, solo crece el código que la usa.

## D5 — La validación vive en las peticiones (tres por tabla)

**Alternativas descartadas:** validar con `if`s en el controlador; una
clase validadora aparte; una sola petición reutilizada para POST y PUT.

**Decisión:** tres clases de petición por tabla (`<Tabla>Crear`,
`<Tabla>Reemplazo`, `<Tabla>Actualizar`) con anotaciones (`[Required]`,
`[StringLength]`, `[Range]`); ASP.NET valida el body y responde 422 con la
lista de errores.

**Por qué:** el mismo body puede fallar en PUT (le faltan campos) y pasar
en PATCH — la semántica de cada verbo queda declarada, no verificada a
mano. El body vacío en PATCH es 400, decidido por el servicio, porque no
es un error de forma sino de negocio (nada que actualizar).

## D6 — SQL Server como motor único y su inicialización en tres pasos

**Decisión:** el contenedor `sqlserver-init` corre en orden (1) el script
que crea las 25 tablas, (2) el script que agrega la columna de borrado
lógico y (3) el script que carga los registros base.

**Por qué:** SQL Server no ejecuta automáticamente scripts montados —
de ahí el contenedor de inicialización, que además solo termina cuando la
base ya existe con sus datos (`service_completed_successfully` como
semáforo para la API).

## D7 — dotnet watch dentro del contenedor (imagen SDK, no runtime)

**Alternativa descartada:** imagen multi-stage con `publish` (más
pequeña, estilo producción).

**Decisión:** la imagen del SDK corriendo `dotnet watch`, con el código
montado como volumen y `bin/`+`obj/` en volúmenes anónimos.

**Por qué:** el ciclo del curso es guardar → recompila solo → refrescar.
Una imagen de producción optimizada no enseña nada en esta versión y
rompe ese ciclo. El matiz de los volúmenes anónimos importa: los
compilados de Linux (los del contenedor) no deben mezclarse con los de
Windows (los del IDE del estudiante).

## D8 — Docker compose desde el inicio (tres servicios)

**Decisión:** `docker-compose.yml` con `sqlserver` + `sqlserver-init` +
`api-innovacion` desde esta versión — `docker compose up -d --build` deja
todo funcionando.

**Por qué:** el sistema debe quedar completo con un solo comando; la
infraestructura se construye por incrementos igual que la API.

## D9 — Borrado lógico agregado mediante una columna adicional

**Contexto.** El script provisto no define una columna de estado en las
siete tablas de esta versión (sí la trae en las tablas de gestión de
usuarios, que corresponden a una versión posterior). El borrado debe ser
lógico, no físico.

**Opciones evaluadas:** (a) una tabla aparte que registre los ids dados de
baja; (b) agregar `activo BIT NOT NULL DEFAULT 1` a las siete tablas
mediante un script adicional que corre después del script provisto.

**Decisión: (b).** Es el cambio más pequeño posible, no reescribe el
script original, y usa el mismo patrón de `activo` que ya existe en las
tablas de usuarios.

**Consecuencias.** (+) El script original queda intacto. (+) Mismo
mecanismo de borrado lógico en toda la base. (−) Hay que mantener dos
archivos `.sql` sincronizados si el esquema original cambia.

## D10 — Sobre de respuesta y nombres de campo

**Decisión:** las lecturas en lista devuelven
`{"tabla", "limite", "total", "datos"}`; la lectura por id devuelve el
objeto sin sobre; los errores devuelven
`{"estado", "mensaje", "detalle"}` (más `"errores"` en 422); las
escrituras exitosas devuelven `{"estado", "mensaje"}` y, cuando aplica,
`"filasAfectadas"` o `"filasEliminadas"`.

**Por qué:** son los nombres de campo que exige la especificación de esta
versión — se siguen tal cual, sin traducir ni reinterpretar.

## D11 — Lista vacía responde 204, límite por defecto 1000

**Decisión:** `GET /api/{tabla}` sin resultados devuelve 204 sin cuerpo;
el parámetro `limite` vale 1000 si no se envía.

**Por qué:** así lo exige la especificación de esta versión.

## D12 — Carga de los datos base mediante un script de inicialización

**Decisión:** los registros iniciales de `area_conocimiento` (218) y
`universidad` (6) se cargan con un script SQL adicional
(`02_datos_iniciales.sql`), ejecutado por el mismo contenedor de
inicialización, después de crear las tablas y agregar la columna de
borrado lógico.

**Por qué:** mantiene el mismo mecanismo de inicialización que el resto
de la base — un contenedor que prepara todo antes de que la API arranque,
sin pasos manuales adicionales.

