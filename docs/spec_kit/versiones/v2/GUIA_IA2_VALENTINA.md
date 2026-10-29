# Guía de IA — Versión 2 · Valentina

> Su camino: **B · IDE agéntico** (la IA corre dentro del repositorio: lee los
> archivos del spec kit y del código y escribe los archivos ella misma; no hay
> que subir ni copiar y pegar nada).
>
> Rama: `rama-valentina-v2`
 
## 1. Lo que le toca

- **Tablas:** `programa` (FK a `facultad`; maestro con pantalla maestro-detalle), `acreditacion`, `registro_calificado`, `activ_academica`, y las puentes `enfoque_rc` y `aa_rc`.
- **Disparador:** al menos uno funcionando y comprobable (propuestas A–E de la guía del profesor; la elegida queda escrita en `2_spec.md`).
- **Pantalla de programa:** al abrirlo se ve su detalle (acreditaciones, actividades académicas, registros calificados, y pasantías y premios hechos por Emmanuel) y se pueden agregar renglones sin salir; varios renglones viajan en **un solo envío**.

## 2. Con qué trabaja la IA (ya está en el repositorio)

No se sube nada: el agente lee los archivos desde las carpetas del proyecto.

| Qué | Dónde |
|---|---|
| Constitución | `docs/spec_kit/1_constitution.md` |
| Spec kit de la v2 | `docs/spec_kit/versiones/v2/` (`2_spec` a `9_frontend`) |
| Script de la base de datos | `db/innovacion_curricular.ss.sql` y los demás scripts de `db/` |
| Molde de la v1 | `api_innovacion/` y el frontend (una tabla ya hecha) y `docs/spec_kit/versiones/v1_producto_sqlserver/` |
| Molde de la v2 | La rebanada de `facultad` de Emmanuel, **cuando ya esté en `main`** |

> **Antes de empezar:** `git pull` de `main` (para tener lo de Emmanuel) y
> `git checkout -b rama-valentina-v2`. Comprobar con `git status` que la IA va a
> trabajar en **su** rama y no en `main`.
>
> **No se le pide que lea** `9_checklist.md` (es la compuerta de ustedes) ni
> `0_mapa_versiones.md` (revelaría lo que viene).

## 3. El prompt

Un solo prompt, una sola fase. La IA construye **todo** lo de Valentina leyendo
el spec kit de la v2. **No hay corchetes que llenar**: las tablas, el
disparador y la regla de negocio se toman de los archivos del repositorio. Se
pega en el agente (parado en la rama `rama-valentina-v2`) después de hacer
`git pull`.

```text
ROL
Eres un desarrollador senior de C# / ASP.NET Core, SQL Server y Angular. Trabajas en el proyecto "Innovación Curricular", VERSIÓN 2. Yo soy Valentina, integrante del equipo, y construyo mi parte completa en esta única fase. Haz exactamente lo que se pide: ni menos, ni más.

PASO 0 — VERIFICACIÓN DE LOS ARCHIVOS (antes de todo)
Los archivos del repositorio deben ser de "Innovación Curricular" (programa, facultad, acreditacion, registro_calificado, activ_academica, enfoque_rc, aa_rc...). Si hablan de facturas, clientes, vendedores, "bdfacturas", PostgreSQL o Blazor, DETENTE y dime que no corresponden a este proyecto.

ARCHIVOS DEL REPOSITORIO Y SU AUTORIDAD (léelos tú con tus herramientas; no te los voy a pegar)
1. docs/spec_kit/1_constitution.md: reglas permanentes.
2. Spec kit de la v2 en docs/spec_kit/versiones/v2/: 2_spec.md (alcance, decisiones del equipo, criterios), 3_plan.md, 4_research.md, 5_data_model.md (las 15 tablas), 6_contracts.md (rutas, formatos y códigos exactos), 8_tasks.md (fases y qué me toca), 9_frontend.md (la interfaz Angular). NO leas 9_checklist.md.
3. db/innovacion_curricular.ss.sql y los demás scripts de db/: columnas, tipos, PK y FK.
4. Moldes de código: una tabla ya terminada de la v1 (api_innovacion/ y el frontend) y, si ya existe en el repositorio, la rebanada de facultad de la v2 (procedimientos, ConflictoExcepcion, RespuestaCreacion<T>). Búscalos tú y dime cuáles elegiste.
Orden de autoridad ante contradicción: 6_contracts.md (rutas, formatos, códigos) > 5_data_model.md (columnas) > 2_spec.md (decisiones) > moldes > constitución. En particular, los Art. 2, 3 y 8 de la constitución están en conflicto con la v2 y la v2 manda (ver 2_spec.md §4 y §5): procedimientos almacenados, 409 con ConflictoExcepcion y envoltura EN CASTELLANO. Si algo no está en los documentos, pregúntame; no lo inventes.

MODO DE TRABAJO EN EL REPOSITORIO
- Antes de escribir nada, ejecuta "git status" y "git branch --show-current": debo estar en rama-valentina-v2. Si estás en main, detente y avísame.
- Crea y edita los archivos directamente en sus carpetas. No me pegues el código en el chat: al terminar, dame la lista de archivos creados y modificados.
- Puedes compilar y correr pruebas ("dotnet build", "dotnet run --project pruebas", compilación de Angular) y arreglar lo que falle. NO ejecutes commit, push, merge, checkout, reset ni nada que cambie el historial de Git: los commits los hago yo, en español, por fases.
- No toques .env ni .gitignore ni ningún secreto.

ALCANCE — LO QUE SÍ CONSTRUYES (sigue las fases 2, 3, 4, 5, 6 y 7 de 8_tasks.md, en ese orden)
1. programa (maestro; FK obligatoria a facultad; trae num_acreditaciones de solo lectura).
2. acreditacion (PK resolucion), registro_calificado (PK codigo), activ_academica (FK OPCIONAL disenio -> programa).
3. enfoque_rc y aa_rc (tablas puente; aa_rc lleva además componente y semestre).
4. El disparador trg_acreditacion_contador.
5. Frontend: mis tablas en models/catalogo.ts y la pantalla pages/programa-detalle (según 9_frontend.md §8).
6. Pruebas: dobles de mis tablas en pruebas/Programa.cs y casos (válidos y que DEBEN fallar) en la colección de peticiones (api_innovacion.http).

ALCANCE — LO QUE NO HACES
- No tocas facultad, pasantia, premio, docente_departamento, alianza, programa_ac, programa_pe, programa_ci ni an_programa (de Emmanuel y Felipe). Solo LEES esas tablas cuando mi lógica lo exige (por ejemplo, comprobar hijos activos).
- No modificas la v1 ni db/innovacion_curricular.ss.sql. No creas db/03_activar_borrado_logico_v2.sql: es de Emmanuel; asume que ya agregó activo a las 15 tablas y programa.num_acreditaciones. Si no existen, dímelo en el resumen y no los crees tú.
- No reescribas archivos compartidos (Program.cs, models/catalogo.ts, api.service.ts, db/init.sh, Respuestas.cs): agrega SOLO mis líneas/entradas y dime exactamente cuáles. No recrees ConflictoExcepcion ni RespuestaCreacion<T> si ya existen.
- No agregues endpoints, campos, validaciones, paquetes ni archivos que no estén en el spec kit (por ejemplo, un endpoint "por lotes" para el detalle: 6_contracts.md no lo define).
- Nada de v3 (login, JWT, usuarios, roles) ni v4 (consultas multitabla, gráficos, PWA). No generes README ni documentación aparte.

REGLAS TÉCNICAS (obligatorias)
- Stack: C# / ASP.NET Core con Dapper y Microsoft.Data.SqlClient; Angular standalone en TypeScript estricto. API http://localhost:8036; Frontend 8037; apiUrl en environments/environment.ts. Cadena de conexión solo por la variable DB_CONNECTION (cero secretos en el código).
- Nombres de archivos y clases: como el molde de la v1 (castellano para el dominio): Modelos/Programa.cs, Peticiones/ProgramaPeticiones.cs, Repositorios/IRepositorioPrograma.cs + RepositorioProgramaSqlServer.cs, Servicios/IServicioPrograma.cs + ServicioPrograma.cs, Controllers/ProgramaController.cs. Lo mismo para cada tabla mía.
- Capas: el controlador no toca SQL y solo traduce errores de DOMINIO a HTTP; el servicio no conoce HTTP y es el ÚNICO punto que traduce errores del motor (SqlException 547 de FK, 2627/2601 de PK duplicada) a ConflictoExcepcion -> 409; el repositorio no conoce HTTP. Errores: 422 body inválido, 400 regla de negocio (body vacío en PATCH, limite <= 0), 404 inexistente o ya retirado, 409 conflicto, 500 solo lo inesperado. Un 409 nunca sale como 500.
- CERO SQL en C#: ningún SELECT/INSERT/UPDATE/DELETE en un .cs. El repositorio solo invoca procedimientos con CommandType.StoredProcedure y DynamicParameters con los nombres snake_case EXACTOS de las columnas (@fecha_creacion, @activ_academicas_idcurso...), sin depender del orden (gotcha de Dapper en 4_research.md D2).
- Procedimientos en un archivo NUEVO: db/04_procedimientos_v2_valentina.sql, con CREATE OR ALTER (se puede ejecutar varias veces). PK con id: sp_listar_, sp_consultar_, sp_crear_, sp_actualizar_, sp_eliminar_. Puentes: sp_listar_, sp_consultar_, sp_asignar_, sp_retirar_. Agrega en db/init.sh solo la línea que falta para que corra después del 03 (sin reescribir el archivo).
- Listados y consultas filtran activo = 1 y resuelven los nombres de las FK con LEFT JOIN dentro del procedimiento, con alias <fk>_nombre (facultad_nombre, programa_nombre, disenio_nombre...). registro_calificado no tiene columna nombre: propón en el resumen qué texto legible mostrar para su desplegable (por ejemplo codigo + tipo_titulacion) y sigue.
- PK: las asigna quien hace el POST (no hay IDENTITY); el id va en la petición de creación. sp_crear_ devuelve la fila COMO QUEDÓ GUARDADA. Validar dentro del procedimiento que cada FK exista Y esté activa (mensaje: "El/La <x> indicado(a) no existe o está inactivo(a)."), porque la restricción de la base no detecta una FK retirada.
- Borrado lógico (UPDATE ... SET activo = 0). Padre con hijos activos -> el procedimiento NO actualiza ninguna fila y el servicio devuelve 409 ("El programa tiene registros asociados activos; primero retírelos."). Mapa de hijos en 5_data_model.md §4.2: programa (acreditacion, registro_calificado, activ_academica, pasantia, premio, docente_departamento, alianza, programa_ac, programa_pe, programa_ci, an_programa), registro_calificado (enfoque_rc, aa_rc), activ_academica (aa_rc). Una puente activa cuenta como hijo.
- Respuestas exactas de 6_contracts.md: listado {tabla, limite, total, datos} (limite por defecto 1000; vacío -> 204); consulta individual = el objeto directo, sin envoltura; POST -> {estado, mensaje, datos:{fila guardada}}; PUT y PATCH -> {estado, mensaje, filasAfectadas}; DELETE -> {estado, mensaje, filasEliminadas}; errores {estado, mensaje, detalle} y 422 con errores:[...]. PATCH: el servicio lee el registro actual, combina solo los campos enviados y llama a sp_actualizar_ (parámetros fijos).
- Rutas EXACTAS del contrato §0.2: /api/programa/{id}; /api/acreditacion/{resolucion}; /api/registro_calificado/{codigo}; /api/activ_academica/{id}; /api/enfoque_rc/{enfoque}/{registro_calificado}; /api/aa_rc/{activ_academicas_idcurso}/{registro_calificado_codigo}. Puentes: GET listar, GET pareja, POST asignar, DELETE retirar; SIN PUT ni PATCH. Pareja repetida -> 409; FK inexistente -> 409; retirar pareja inexistente -> 404. aa_rc exige además componente y semestre.
- Tipos según 5_data_model.md: programa guarda sus fechas y contadores como TEXTO (string), registro_calificado usa DATE (DateTime), activ_academica.espejo es TINYINT 0/1. Un campo OPCIONAL llega como null, nunca como cadena vacía (disenio, fecha_cierre).
- DISPARADOR trg_acreditacion_contador: CREATE OR ALTER TRIGGER sobre acreditacion, AFTER INSERT, UPDATE, DELETE, que recalcula programa.num_acreditaciones = cantidad de acreditaciones con activo = 1 de cada programa afectado. Debe escuchar el UPDATE (retirar es UPDATE activo = 0), funcionar con VARIAS filas a la vez (usar inserted y deleted, nunca variables escalares) y recalcular tanto el programa viejo como el nuevo si un UPDATE cambia la FK. La API NUNCA escribe num_acreditaciones. Entrégame un script de prueba y los pasos para demostrarlo desde la interfaz (asignar y retirar).
- Frontend: la interfaz usa la página genérica de catálogo: describe mis 6 tablas en models/catalogo.ts marcando FK (desplegable), fecha (input de fecha) y opcional («(ninguna)» -> null). Los desplegables se cargan en paralelo (forkJoin), muestran el nombre, envían la clave y solo ofrecen activos. En las puentes se oculta el botón Editar (solo asignar y retirar). El listado se lee entrando a "datos". Muestra el 409 con su mensaje en castellano; los textos van en castellano SIN jerga ("PUT", "422", "FK"). Si la API no responde, la pantalla no se rompe: muestra un aviso. Si el mecanismo genérico (desplegables, null, ocultar Editar, asignar/retirar en api.service.ts) aún no existe, agrega solo la pieza mínima y dime cuál, porque mis compañeros trabajan sobre los mismos archivos.
- COMENTA todo en castellano: cada archivo empieza con un bloque QUÉ ES y QUÉ PAPEL cumple; cada método no evidente explica POR QUÉ está escrito así, no qué hace la línea.

FORMA DE RESPONDER
1. PRIMERO, un resumen breve de lo que entendiste: por cada una de mis 6 tablas, PK, FK (cuáles opcionales), tipos relevantes y sus endpoints; qué piezas de Emmanuel ya existen y cuáles faltan; qué archivos usas como molde; el texto legible que propones para registro_calificado; y las ambigüedades. Una que ya detecto: el spec no dice qué pasa al asignar de nuevo una pareja retirada (activo = 0): por defecto aplica el contrato literal (409 "ya registrada") y avísame. Si alguna ambigüedad bloquea de verdad, detente y pregunta; si no, continúa de inmediato.
2. LUEGO construye TODO escribiendo los archivos en el repositorio, en este orden y sin saltarte nada:
   a) db/04_procedimientos_v2_valentina.sql: procedimientos de las 6 tablas, el disparador y el script de prueba (orden de construcción: programa; luego acreditacion, registro_calificado, activ_academica; luego las puentes; luego el disparador).
   b) Modelos y Peticiones (Crear, Reemplazo y Actualizar; las puentes solo Crear).
   c) Repositorios (interfaz + implementación).
   d) Servicios (interfaz + implementación) con traducción de errores, regla del padre con hijos y combinación del PATCH.
   e) Controllers.
   f) Tus líneas en Program.cs (solo agregar; no reescribir el archivo).
   g) Frontend: entradas de catalogo.ts, cambios mínimos en api.service.ts si hacen falta y pages/programa-detalle.
   h) pruebas/Programa.cs (dobles de mis tablas) y las peticiones de la colección: válidas y las que DEBEN fallar (FK inexistente -> 409, pareja duplicada -> 409, padre con hijos -> 409, body inválido -> 422, id inexistente o ya retirado -> 404).
3. Después de cada bloque (a a h) compila y corre las pruebas que existan. Si te quedas sin espacio, termina el archivo en curso, dime en qué punto quedaste y continúa cuando yo escriba "continúa".
4. Al terminar: la lista de archivos creados y modificados, lo que compiló y lo que no, y la lista de verificaciones que debo hacer a mano, tomada de las fases 2 a 7 de 8_tasks.md y de los criterios 2, 3, 4, 5, 6, 7, 8, 9, 10 y 11 de 2_spec.md §8.
```

> **Antes de enviarlo:** `git pull` de `main` para tener lo de Emmanuel. Si su
> `facultad` todavía no está en el repositorio, el prompt hace que la IA se lo
> diga en el resumen en lugar de inventarlo.

## 4. Tropiezos que le tocan de cerca

- **Tropiezo 6 (el suyo):** su disparador debe oír el **UPDATE**. Pruebe retirando un hijo por la interfaz y mirando que el dato cambie solo.
- **Tropiezo 9:** `enfoque_rc` y `aa_rc` sin botón de editar.
- **Regla de negocio:** implemente en el **servicio** lo que el spec decida sobre retirar un programa con detalle activo.
- **Maestro-detalle:** varios renglones en un solo envío (una petición), no una por renglón. Si el tercero falla, no debe quedar ninguno.
- **Tropiezo 3:** los listados de detalle con JOIN devuelven nombres; compruebe que lleguen sin `null`.

## 5. Cómo revisar lo que hizo la IA (antes de hacer commit)

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