# Guía de IA — Versión 2 · Felipe

> Su camino: **A · Chat web** (pegar el prompt, pegar los archivos de contexto,
> revisar y copiar el resultado al repositorio). Si su equipo usa un IDE
> agéntico, el mismo prompt sirve; solo cambia que el agente lee los archivos
> directamente.
>
> Rama: `rama-felipe-v2`

## 1. Lo que le toca

- **Tablas:** `docente_departamento`, `alianza`, y las puentes `programa_ac`, `programa_pe`, `programa_ci`, `an_programa`.
- Las cuatro puentes siguen el mismo patrón: es la oportunidad de que **una sola** versión bien revisada se replique.

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

MI TAREA: Implementar el CRUD completo (procedimientos, API y Angular) de docente_departamento, alianza, programa_ac, programa_pe, programa_ci y an_programa. docente_departamento vincula un docente con un programa (dedicación, modalidad, fechas); alianza vincula un aliado (por NIT) con un programa, con fechas y docente responsable.

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

CONTEXTO DE MIS TABLAS: [pegue aquí los CREATE TABLE de docente_departamento, alianza, programa_ac, programa_pe, programa_ci y an_programa]. El docente puede pertenecer a otro módulo; sigue lo que diga el research del spec sobre cómo se resuelve ese campo.

VALIDACIONES: en alianza y docente_departamento la fecha de fin no puede ser anterior a la de inicio (ArgumentException -> 400).

TABLAS PUENTE (programa_ac, programa_pe, programa_ci, an_programa): PK compuesta; endpoints para asignar y retirar por pareja; sin PUT; repetir la pareja devuelve 409; el listado muestra nombres (JOIN dentro del procedimiento, no en C#). Genera un patrón único y reutilizable para las cuatro.

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

- **Tropiezo 9 (el suyo):** cuatro puentes, cuatro oportunidades de dejar un botón de editar. Ninguna lo lleva.
- **Tropiezo 5:** `alianza` necesita al menos dos desplegables (aliado y programa) y quizá más: cárguelos en paralelo con `forkJoin`.
- **Tropiezo 2:** `alianza` usa el NIT como clave del aliado; compruebe que el tipo y el nombre de la propiedad coinciden con la columna.
- **Prueba de la pareja:** asignar → repetir (409) → retirar, para cada puente.

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
| 1 | | | |
| 2 | | | |
| 3 | | | |

## 7. Preguntas de sustentación para ensayar

1. Cuénteme uno de sus procedimientos sin leérmelo línea por línea.
2. ¿Por qué el SQL vive en la base de datos y no en el repositorio?
3. Si la base rechaza la clave foránea, ¿qué recibe quien llamó y qué ve la persona en pantalla?
4. Si mañana hay que agregar un campo, ¿dónde se toca?
