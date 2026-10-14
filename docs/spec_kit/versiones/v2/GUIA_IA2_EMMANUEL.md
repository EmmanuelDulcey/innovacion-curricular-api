# Guía de IA — Versión 2 · Emmanuel

> Su camino: **A · Chat web** (pegar el prompt, pegar los archivos de contexto,
> revisar y copiar el resultado al repositorio). Si su equipo usa un IDE
> agéntico, el mismo prompt sirve; solo cambia que el agente lee los archivos
> directamente.
>
> Rama: `emmanuel-dulcey`

## 1. Lo que le toca

- **Spec kit de la v2**: `2_spec.md`, `3_plan.md`, `4_research.md`, `5_data_model.md`, `6_contracts.md`, `7_quickstart.md`, `8_tasks.md`, `9_checklist.md` en `docs/spec_kit/versiones/v2_<nombre>/`. **Se escribe ANTES del código.**
- **Tablas:** `facultad` (FK a `universidad`), `pasantia` (FK a `programa`), `premio` (FK a `programa`).
- Entregar `facultad` pronto: `programa` depende de ella.

## 2. Qué le sube a la IA (en este orden)

1. `1_constitution.md`
2. El spec kit de la v2 (`2_spec.md`, `5_data_model.md`, `6_contracts.md`) — **solo cuando Emmanuel lo termine**
3. El script `innovacion_curricular.ss.sql` (solo las tablas que le tocan, para no saturar)
4. Una clase de la v1 como molde (Controller, Service, Repository y componente Angular de una tabla ya hecha)

> No le suba el `9_checklist.md` ni el mapa de versiones.

## 3. El prompt

Copie, reemplace lo que está entre `[corchetes]` y pegue **después** de subir los archivos.

```text
Eres un desarrollador senior de C# / ASP.NET Core y Angular. Trabajas en el proyecto "Innovación Curricular", versión 2. Te adjunto la constitución, el spec kit de la v2, el script de la base de datos y un ejemplo de la v1 como molde. Sigue el molde de la v1.

MI TAREA: Implementar el CRUD completo (procedimientos, API y Angular) de las tablas facultad, pasantia y premio. facultad tiene FK a universidad; pasantia y premio tienen FK a programa (programa lo construye otra persona; asume el endpoint GET /api/programa para llenar el desplegable).

REGLAS DEL PROYECTO (obligatorias):
- Stack: API en C# / ASP.NET Core con Dapper y Microsoft.Data.SqlClient; frontend en Angular (TypeScript, HttpClient) consumiendo http://localhost:8036. Base de datos SQL Server (innovacion_curricular), que ya existe: NO propongas crear ni modificar tablas salvo lo que mi tarea indique.
- Arquitectura en capas con interfaces: Controller -> Service -> Repository. El controlador no toca SQL; el servicio no conoce HTTP; el repositorio no conoce HTTP. Registro de dependencias en Program.cs.
- TODO el CRUD pasa por procedimientos almacenados: sp_listar_<tabla>, sp_consultar_<tabla>, sp_crear_<tabla>, sp_actualizar_<tabla>, sp_eliminar_<tabla>. El SQL va en el script de la base de datos (CREATE OR ALTER PROCEDURE). El repositorio C# SOLO llama al procedimiento con parámetros (Dapper, commandType StoredProcedure). Prohibido SQL escrito a mano o concatenado en C#.
- sp_crear_<tabla> devuelve la fila COMO QUEDÓ GUARDADA (SELECT final).
- Si la tabla tiene columna `activo`, el borrado es lógico (UPDATE ... SET activo = 0) y TODOS los listados filtran activo = 1.
- Respuestas de lectura: {table, limit, total, data}. Errores: {status, message, detail} (y errors:[...] en 422).
- Errores: body inválido -> 422; ArgumentException -> 400; NoEncontradoExcepcion -> 404; violación de clave foránea o clave primaria duplicada -> 409 con mensaje en castellano; nunca 500 por eso. SqlException inesperada -> 500.
- Nombres de clases, rutas y archivos en inglés. Textos de la interfaz en español, SIN jerga técnica (nada de "PUT", "422", "FK").
- Ninguna clave foránea se digita: desplegable cargado desde la API, muestra el nombre, envía la clave, solo ofrece registros activos. Si la FK es opcional, opción "(ninguna)" que envía null (no cadena vacía).
- Tablas puente (PK compuesta, dos FK): solo asignar y retirar. NO tienen botón de editar. Repetir la pareja -> 409.
- Cero secretos en el código: la cadena de conexión viene de la variable de entorno DB_CONNECTION.
- COMENTA todo el código que generes, en español. Cada archivo empieza con un bloque que dice QUÉ ES y QUÉ PAPEL cumple. Cada método no evidente lleva comentario que explique POR QUÉ está escrito así, no qué hace la línea.
- No construyas nada de v3 (login, JWT, usuarios, roles) ni de v4 (consultas multitabla, gráficos).

CONTEXTO DE MIS TABLAS (definiciones reales, tomadas de innovacion_curricular.ss.sql):

CREATE TABLE facultad (
    id INT NOT NULL,
    nombre VARCHAR(60) NOT NULL,
    tipo VARCHAR(45) NOT NULL,
    fecha_fun DATE NOT NULL,
    universidad INT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (universidad) REFERENCES universidad(id)
);

CREATE TABLE pasantia (
    id INT NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    pais VARCHAR(45) NOT NULL,
    empresa VARCHAR(45) NOT NULL,
    descripcion VARCHAR(45) NOT NULL,
    programa INT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (programa) REFERENCES programa(id)
);

CREATE TABLE premio (
    id INT NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    descripcion VARCHAR(45) NOT NULL,
    fecha DATE NOT NULL,
    entidad_otorgante VARCHAR(45) NOT NULL,
    pais VARCHAR(45) NOT NULL,
    programa INT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (programa) REFERENCES programa(id)
);

NOTA: el script original NO trae columna `activo`; la v2 la agrega con
03_activar_borrado_logico_v2.sql (activo BIT NOT NULL DEFAULT 1). Las tres tablas
llevan borrado lógico. Además, pasantia.programa y premio.programa son NOT NULL
(no opcionales), así que NO aplica la opción «(ninguna)».

ENTREGA, en este orden y un archivo a la vez:
1. Script SQL con los procedimientos (CREATE OR ALTER PROCEDURE) de cada tabla.
2. Models / Requests.
3. Repository (interfaz + implementación que solo llama procedimientos).
4. Service (interfaz + implementación) con las traducciones de error.
5. Controller.
6. Frontend Angular: modelo, servicio, componente de lista y de formulario.
7. Peticiones para la colección de pruebas, incluidas las que DEBEN fallar (409, 422, 404).

Antes de escribir código, dime en una lista corta qué vas a hacer y qué decisiones dudosas ves. Si algo del spec es ambiguo, pregúntame; no lo inventes.
```

## 4. Tropiezos que le tocan de cerca

- **Tropiezo 4:** verificado contra el script — **ni `pasantia` ni `premio` tienen FK opcional** (ambas FK son `NOT NULL`), así que aquí NO aplica la opción «(ninguna)» → `null`.
- **Tropiezo 7:** el desplegable de universidad y de programa debe ofrecer solo activos.
- **Del spec kit:** el `4_research.md` debe explicar el **409 vs 422** (pregunta casi segura) y el `2_spec.md` debe escribir la regla de negocio: ¿se puede retirar un maestro con detalle activo?
- Mientras los demás construyen sobre su spec, cualquier ambigüedad que ellos encuentren es un error **suyo**: corríjalo en el documento, no en el chat.

## 5. Cómo revisar lo que devuelve la IA (antes de copiarlo)

- [ ] ¿Hay algún `SELECT`, `INSERT`, `UPDATE` o `DELETE` dentro de un archivo `.cs`? Si sí, **rechácelo**.
- [ ] ¿Los listados tienen `WHERE activo = 1` (si la tabla tiene `activo`)?
- [ ] ¿`sp_crear_*` termina con un `SELECT` de la fila guardada?
- [ ] ¿Cada violación de FK/PK se traduce a 409 con mensaje en castellano?
- [ ] ¿Los desplegables salen de la API, muestran nombres y envían claves?
- [ ] ¿Todo está comentado y puede explicar cada comentario? **El comentario puede estar equivocado: la IA comenta lo que cree que hizo.**

## 6. Registro de lo que hubo que corregirle a la IA

> **Esto es lo que más vale.** Se entrega con la versión. Llénelo mientras
> trabaja, no al final.

| # | Qué pidió | Qué salió mal | Qué corrección lo arregló (prompt de corrección) |
|---|---|---|---|
| 1 | CRUD v2 de las 3 tablas siguiendo el molde de la v1 | Arrancó con la envoltura en inglés `{table,limit,total,data}` / `{status,message,detail}` (lo que decía el borrador del issue) y así habría roto un criterio de aceptación de la v1 | «Mantené la envoltura en castellano de la v1: listas `{tabla,limite,total,datos}`, errores `{estado,mensaje,detalle}` y el POST devolviendo la fila guardada. Dejá la desviación documentada en `2_spec.md`» |
| 2 | Nombres de clases, archivos y rutas | Propuso nombres en inglés (`FacultyController`, `FacultyService`…) siguiendo el prompt | «Nombres de dominio en castellano, como el molde real de la v1: `FacultadController`, `ServicioFacultad`, `RepositorioFacultadSqlServer`, `Modelos/Facultad.cs`» |
| 3 | Repositorio que solo llama procedimientos | Armó los parámetros con un objeto anónimo (`new { FechaFun = … }`), que Dapper NO liga a `@fecha_fun` (snake_case) → la llamada habría fallado | «Los procedimientos esperan snake_case: usá `DynamicParameters` y agregá con el nombre exacto (`parametros.Add("fecha_fun", …)`, sin `@`)» |
| 4 | `sp_crear_*` devuelve la fila guardada | El POST respondía solo `{estado, mensaje}` y no incluía la fila recién creada | «Agregá `RespuestaCreacion<T>` con el campo `datos` y devolvé la fila que retorna `sp_crear_*`» |
| 5 | Fechas (`fecha_fun`, `fecha`) en el JSON | Serializaba la fecha como fecha-hora ISO completa; el contrato y el `<input type="date">` piden `yyyy-MM-dd` | «Registrá un `JsonConverter<DateTime>` (`SoloFechaJsonConverter`) que escriba `yyyy-MM-dd` y lea con `CultureInfo.InvariantCulture`» |
| 6 | Coherencia interna del spec kit | `2_spec.md` §7 decía «el servicio no conoce el motor», contradiciendo `3_plan.md` §7 y `6_contracts.md` §10 (el servicio SÍ traduce el `SqlException`) | «Corregí el §7 de `2_spec.md`: el servicio es el único punto que traduce los errores del motor a errores de dominio; el controlador solo mapea a HTTP» |
| 7 | Colección de pruebas (`api_innovacion.http`) | El archivo seguía con el placeholder del andamiaje (`GET /weatherforecast`) | «Reemplazá por la colección v2: listar/crear/editar/retirar y, sobre todo, los casos que DEBEN fallar (409, 422, 404)» |

## 7. Preguntas de sustentación para ensayar

1. Cuénteme uno de sus procedimientos sin leérmelo línea por línea.
Le cuento sp_eliminar_facultad. Recibe solo el id. Primero pregunta si esa facultad todavía tiene programas activos (IF EXISTS (SELECT 1 FROM programa WHERE facultad = @id AND activo = 1)): si los tiene, devuelve filas = 0 y no toca nada, porque la regla de negocio dice que un padre con hijos activos no se retira. Si no tiene hijos, hace el borrado lógico (UPDATE facultad SET activo = 0) y devuelve @@ROWCOUNT. El que decide si ese 0 es un 404 o un 409 es el servicio, no el procedimiento.
2. ¿Por qué el SQL vive en la base de datos y no en el repositorio?
Porque la v2 exige todo el CRUD por procedimientos. Así el SQL queda en un solo lugar (el script) y el repositorio en C# solo los invoca con parámetros y CommandType.StoredProcedure, sin una línea de SQL embebida ni concatenada. Gano que la lógica de datos se mantiene aparte del código, se reutiliza, y SQL Server cachea el plan de cada procedimiento. Es también lo que pide la constitución ajustada.
3. Si la base rechaza la clave foránea, ¿qué recibe quien llamó y qué ve la persona en pantalla?
SQL Server lanza un SqlException con el código 547. El servicio lo atrapa y lo traduce a una excepción de conflicto; el controlador responde 409 con {estado, mensaje, detalle} (nunca 500). En este caso: mensaje «Conflicto con los datos.» y detalle «La universidad indicada no existe o está inactiva.». Quien llamó recibe ese 409, y en pantalla el usuario ve ese mismo texto en el cartel de error, en castellano y sin jerga técnica.
4. Si mañana hay que agregar un campo, ¿dónde se toca?
Por capas, porque cada una tiene su propio contrato: (1) la tabla en el script de la base; (2) los procedimientos que lo usen (listar, consultar, crear, actualizar); (3) el modelo y las peticiones en C#, con su validación; (4) el repositorio, que lo agrega a los DynamicParameters con el nombre exacto; y (5) la configuración del catálogo en el frontend, agregándolo a CATALOGOS. No hay un único punto justamente porque separamos responsabilidades.