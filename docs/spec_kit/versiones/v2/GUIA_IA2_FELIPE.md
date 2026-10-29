# Guía de IA — Versión 2 · Felipe

> Su camino: **IDE agéntico con el checkout abierto en Rider**. El agente lee
> el spec kit y el código directamente en la rama existente para que el cambio
> quede en el proyecto que se compila y se usa.
>
> Rama de trabajo existente: `felipe-correa` (se conserva el checkout de Rider; no se crea una rama nueva ni se trabaja en `main`).

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

### Prompt realmente usado en esta implementación

El punto de partida fue el prompt de la sección 3 de esta guía, enviado por
Felipe en la conversación del 15 de octubre de 2026. La instrucción final
que fijó la prioridad fue: «Verifica la v1. Es la regla de negocio principal
y todo debe seguir ese flujo. Con base en eso asegúrate que los documentos
del spec kit de la v2 estén integrados correctamente... haz todo lo del issue».
La instrucción final de ubicación fue «Escribe el código en la rama de
Felipe-Correa». El alcance detallado es el Issue 3 publicado en GitHub como #15. Las
correcciones de rama y de checkout están transcritas en la tabla de la sección 6.
El código quedó en el checkout existente de Rider, en `felipe-correa`; la rama
se avanzó por fast-forward para incorporar la base v2 y luego recibió estos
cambios. No se creó otra rama, no se trabajó en `main`, no se hizo push ni se
abrió el PR.

**Decisiones verificadas contra la v1 y el contrato de la v2:**

- Respuestas en castellano, como la v1 real: tabla/limite/total/datos y
  estado/mensaje/detalle. El prompt plantilla pedía inglés; no se usa
  una envoltura incompatible con el frontend existente.
- Clases y carpetas siguen el proyecto real (Modelos, Peticiones,
  Repositorios, Servicios, Controllers), con propiedades PascalCase y
  serialización snake_case. No se crea una solución nueva.
- `docente_departamento` y `alianza` tienen cinco procedimientos CRUD porque
  guardan atributos editables; sus claves compuestas quedan fijas en la ruta.
  Las cuatro puentes puras usan listar, consultar, asignar y retirar, sin editar.
  Esta distinción corrige la regla demasiado amplia que el spec kit anterior
  aplicaba a todas las relaciones compuestas.
- El ID del docente pertenece a un catálogo local incremental; el documento
  identifica a la persona por separado. Los formularios eligen docentes activos
  por nombre y envían el ID interno.

## 4. Tropiezos que le tocan de cerca

- **Tropiezo 9 (el suyo):** cuatro puentes, cuatro oportunidades de dejar un botón de editar. Ninguna lo lleva.
- **Tropiezo 5:** `alianza` necesita al menos dos desplegables (aliado y programa) y quizá más: cárguelos en paralelo con `forkJoin`.
- **Tropiezo 2:** `alianza` usa el NIT como clave del aliado; compruebe que el tipo y el nombre de la propiedad coinciden con la columna.
- **Prueba de la pareja:** asignar → repetir (409) → retirar, para cada puente.

## 5. Cómo revisar lo que devuelve la IA (antes del commit)

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
| 1 | Seguir el comportamiento cerrado de la v1. | El prompt plantilla exigía envoltura y nombres ingleses, pero la v1 real y el frontend usan castellano. | «Verifica la v1. Es la regla de negocio principal y todo debe seguir ese flujo». Se conservaron los sobres y capas reales. |
| 2 | Conservar el proyecto abierto en Rider. | El trabajo inicial estaba en una copia de trabajo separada y Rider no tenía esos cambios. | Felipe indicó escribir el código en Rider; se avanzó el checkout existente `felipe-correa` desde `b5b905b` hasta la base v2 y se aplicaron allí los cambios, sin crear otra rama. |
| 3 | Trabajar en la rama propia existente. | Al inicio se propuso una rama secundaria en vez de continuar directamente sobre la rama del usuario. | «Escribe el código en la rama de Felipe-Correa». Se mantuvo `felipe-correa`; no se creó una rama nueva para esta entrega. |
| 4 | Mostrar programas en los desplegables. | La rama actual no tiene `GET /api/programa`; el formulario quedaba sin opciones. | Se añadió `GET /api/opciones/programa`, de solo lectura, con programas activos y procedimiento almacenado. El frontend muestra claramente si falla la carga. |
| 5 | Levantar y verificar la solución con Docker. | El contexto del frontend copiaba dependencias locales por encima de las instaladas en la imagen. | Se conserva `frontend/.dockerignore`. En este cierre: `dotnet build` (0 errores/advertencias), 96 pruebas de capas, Angular compilado, 46 respuestas HTTP CRUD esperadas en una BD temporal aislada y siete conflictos 409 por padres inexistentes. `npm ci` informa 9 vulnerabilidades (2 moderadas, 4 altas, 3 críticas); no se actualizaron dependencias fuera del alcance. |
| 6 | Generar IDs automáticamente y no escribir códigos de docente. | La investigación inicial D6 decía que el docente era externo y no proponía un catálogo local. | Felipe aclaró que el docente debe poder crearse con ID interno automático y documento separado. Se actualizó D6, se añadió `docente` como catálogo auxiliar y se cambiaron los desplegables a nombres. |
| 7 | Reemplazar los datos de prueba por ejemplos reales sin atribuir premios falsos. | Había aliados, normas, prácticas, facultad, pasantía y premio con nombres de prueba o genéricos. | Se eligieron nombres y datos institucionales documentados; el premio se vincula a Ingeniería Química sólo porque la Resolución Académica 3884 de la UdeA identifica ese programa como ganador. |
| 8 | Completar el CRUD indicado por el Issue 3 y aclarar si las alianzas se editan. | Varias secciones del spec kit agrupaban las seis relaciones de Felipe como “sin edición”, aunque `docente_departamento` y `alianza` guardan datos propios y el issue pide CRUD; esa regla confundía ambas clases de relación. | Se delimitó el alcance: ambas relaciones con atributos tienen cinco procedimientos CRUD y PUT/PATCH por pareja inmutable; `programa_ac`, `programa_pe`, `programa_ci` y `an_programa` son puentes puros y sólo permiten asignar/retirar. Se corrigieron los contratos, la guía y el frontend para reflejarlo. |

## 7. Preguntas de sustentación para ensayar

1. Cuénteme uno de sus procedimientos sin leérmelo línea por línea.
2. ¿Por qué el SQL vive en la base de datos y no en el repositorio?
3. Si la base rechaza la clave foránea, ¿qué recibe quien llamó y qué ve la persona en pantalla?
4. Si mañana hay que agregar un campo, ¿dónde se toca?
