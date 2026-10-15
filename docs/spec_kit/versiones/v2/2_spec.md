# Especificación — Versión 2: las 15 tablas con clave foránea (API + Frontend)

> **Versión 2** del desarrollo incremental ([mapa de versiones](../0_mapa_versiones.md)).
> Rige la constitución del proyecto: [../../1_constitution.md](../../1_constitution.md).
> La v2 agrega las **15 tablas con clave foránea** del módulo y con ellas cuatro
> cosas nuevas que definen la versión: el código **409**, el **desplegable** en
> lugar de la clave digitada, los **procedimientos almacenados** como única vía
> al SQL y al menos un **disparador**.
>
> Este documento describe **solo el delta respecto de la v1**. Todo lo que la v1
> dejó funcionando (las 7 tablas sin FK, la envoltura de lectura, el borrado
> lógico, las tres peticiones por verbo, la arquitectura en capas) sigue vigente
> y no se reescribe aquí.

| Documento de esta versión | Contenido |
|---|---|
| **2_spec.md** (este) | QUÉ construir en v2 (API + Frontend), las decisiones del equipo y los criterios de aceptación |
| [3_plan.md](3_plan.md) | CÓMO: stack, estructura, diseño de capas y de los procedimientos (API y Frontend) |
| [4_research.md](4_research.md) | Decisiones y alternativas *(lectura opcional)*: el 409 vs 422, disparador, `docente_departamento` |
| [5_data_model.md](5_data_model.md) | Las **15 tablas** de la v2: PK, FK, columnas opcionales, `activo` y la columna del disparador |
| [6_contracts.md](6_contracts.md) | Los endpoints con formatos exactos, incluido el **nuevo 409** |
| [7_quickstart.md](7_quickstart.md) | Arranque y smoke test de la v2 (API + Frontend) |
| [8_tasks.md](8_tasks.md) | Orden de construcción por fases verificables (API + Frontend) |
| [9_frontend.md](9_frontend.md) | La interfaz Angular: maestro-detalle, desplegables, tablas puente |
| [9_checklist.md](9_checklist.md) | Los 12 criterios de aceptación + la regresión de la v1 |

---

## 1. Propósito de la v2

Construir la **segunda rebanada vertical** del módulo Innovación Curricular: el
**CRUD completo de las 15 tablas que tienen clave foránea**, esta vez **sin una
sola línea de SQL en el código C#** (todo pasa por **procedimientos
almacenados**) y con **integridad referencial traducida a HTTP 409**.

La v1 dejó siete catálogos aislados y un CRUD genérico por tabla. La v2 los
**conecta**: `programa` cuelga de `facultad`, y de `programa` cuelgan
acreditaciones, registros calificados, actividades académicas, pasantías,
premios, docentes y aliados. La interfaz ya no puede pedir que alguien **digite
una clave**: la clave de una relación se **elige en un desplegable** cargado
desde la API.

Tablas incluidas en la v2 (15), agrupadas por responsable:

| Grupo | Tablas | Integrante |
|---|---|---|
| Dependencia raíz | `facultad` → `universidad` | Emmanuel |
| Detalle de programa | `pasantia`, `premio` → `programa` | Emmanuel |
| Maestro + bloque normativo | `programa` → `facultad`; `acreditacion`, `registro_calificado`, `activ_academica` → `programa` | Valentina |
| Puentes del registro | `enfoque_rc`, `aa_rc` | Valentina |
| Personas y aliados | `docente_departamento`, `alianza` | Felipe |
| Puentes del programa | `programa_ac`, `programa_pe`, `programa_ci`, `an_programa` | Felipe |

La v2 es deliberadamente la versión **más grande en cantidad de tablas** pero la
más uniforme: las 15 comparten el mismo molde de capas y el mismo juego de cinco
procedimientos, y lo único que cambia entre ellas es el número de claves
foráneas.

---

## 2. Alcance

**Incluye:**

- CRUD completo de las 15 tablas con FK, **por procedimientos almacenados**
  (`sp_listar_`, `sp_consultar_`, `sp_crear_`, `sp_actualizar_`, `sp_eliminar_`).
- Repositorios que **solo llaman** a los procedimientos (Dapper con
  `CommandType.StoredProcedure`); **cero SQL a mano o concatenado en C#**.
- `sp_crear_<tabla>` devuelve la fila **tal como quedó guardada** (`SELECT`
  final), para que la interfaz pueda mostrar lo que acaba de crear.
- Traducción de integridad referencial a **409** en castellano: clave foránea
  inexistente, pareja duplicada en tabla puente y borrado de un padre con hijos
  activos.
- **Al menos un disparador** en la base de datos (propuesta elegida en la §4),
  comprobable desde la interfaz.
- Frontend Angular: CRUD visual de las 15 tablas, **todas las FK como
  desplegable** (muestra el nombre, envía la clave, solo ofrece activos),
  opciones opcionales con «(ninguna)» → `null`, tablas puente **sin botón de
  editar** y vista **maestro-detalle** de `programa`.
- Endpoints para las tablas puente: **asignar** y **retirar** por pareja (sin
  `PUT`).
- Script SQL de la v2 (borrado lógico + procedimientos) y actualización del
  inicializador de Docker.
- Extensión de la suite de pruebas de capas y de la colección de peticiones,
  incluidas las que **deben fallar** (409, 422, 404).

**No incluye (y es deliberado — ver [mapa de versiones](../0_mapa_versiones.md)):**

- **Login, usuarios y roles** (v3). La v2 no tiene autenticación: todas las
  operaciones siguen siendo públicas, igual que en la v1.
- **Consultas multitabla, gráficos, páginas corporativas, PWA y publicación**
  (v4). El dashboard de la v2 se **amplía** con una tarjeta más por tabla, pero
  no se agregan gráficos ni reportes.
- **Reescribir las 7 tablas de la v1**: la v1 ya está cerrada con tag `v1`. La
  v2 **no la reabre**; su regresión debe seguir en verde.
- **Migrar las 7 tablas de la v1 a procedimientos almacenados**: la conversión
  obligatoria es para las 15 tablas de la v2. Cambiar la v1 queda fuera de
  alcance para no romper su regresión.
- ORM de entidades (Entity Framework): se sigue usando Dapper.

---

## 3. Decisiones del equipo (exigidas por la guía)

La guía de la v2 pide que el equipo **escriba** tres decisiones antes de
construir. Estas son.

### 3.1 Regla de negocio — ¿se puede retirar un maestro que aún tiene detalle activo?

**Decisión: NO. Retirar un maestro (por ejemplo `programa`) que todavía tiene
detalle activo responde `409 Conflict` y no modifica nada.**

Justificación:

- El borrado de la v2 es **lógico** (`activo = 0`). Si se permitiera retirar un
  `programa` con `pasantia`, `premio`, `acreditacion`, etc. activos, esas filas
  hijas quedarían **huérfanas y visibles**: seguirían apareciendo en sus propios
  listados y en desplegables, apuntando a un padre que ya no está. La interfaz
  mostraría detalle de algo que "no existe".
- La alternativa (retirar el padre y **cascada** a los hijos) es peor para la
  v2: es un borrado masivo silencioso que el usuario no pidió y que la
  constitución no autoriza (no se anticipa lógica de v3/v4).
- Por eso se elige la opción **conservadora y explícita**: bloquear. El mensaje
  en castellano explica **qué** impide el retiro ("El programa tiene registros
  asociados; primero retírelos."), y corresponde a un **estado** de la base —
  no a un dato mal formado —, que es exactamente lo que el 409 representa
  (ver [4_research.md](4_research.md), D1).

Esta decisión hace obligatoria una **verificación en el procedimiento de
borrado del maestro**: `sp_eliminar_programa` sólo marca `activo = 0` si no hay
hijos activos; si los hay, no actualiza ninguna fila (y el servicio lo traduce a
409). El detalle exacto vive en [6_contracts.md](6_contracts.md).

> **Alcance de la regla.** Aplica al maestro `programa` (que tiene un detalle
> numeroso y heterogéneo). **No** aplica a `facultad`: `facultad` es maestro de
> `programa`, pero se decidió que retirar una facultad con programas activos
> también se bloquea con 409 por el mismo razonamiento (no dejar huérfanos). La
> regla general es: **un padre con hijos activos no se retira**.

### 3.2 Tablas sin columna `activo`

**Verificación contra el script SQL (`db/innovacion_curricular.ss.sql`):
NINGUNA de las 15 tablas de la v2 trae columna `activo`.**

El script provisto sólo define `activo` en las tablas del módulo de usuarios
(`rol`, `usuario`), que son de la v3. Por lo tanto, igual que la v1 hizo con sus
siete tablas, la v2 **agrega `activo BIT NOT NULL DEFAULT 1` a las 15 tablas**
mediante un script adicional (`db/03_activar_borrado_logico_v2.sql`), que corre
después del script provisto y es idempotente (`IF COL_LENGTH(...) IS NULL`).

Consecuencia: **las 15 tablas tienen borrado lógico** y **todos los listados
filtran `activo = 1`**. La regla "tablas sin `activo`" queda sin casos, y se
deja constancia de que se verificó.

### 3.3 Disparador(es) a implementar

**Decisión: se implementa UN disparador de estado derivado — un contador de
hijos activos en el maestro.**

- Columna/tabla que requiere: se agrega **`programa.num_acreditaciones INT NOT
  NULL DEFAULT 0`** (una columna nueva en el maestro).
- Disparador: **`trg_acreditacion_contador`**, `AFTER INSERT, UPDATE, DELETE`
  sobre `acreditacion`, que recalcula el contador del `programa` afectado.
- Regla clave: como el borrado es **lógico**, **retirar una acreditación es un
  `UPDATE`**, no un `DELETE`. El disparador debe escuchar también `UPDATE` (el
  **tropiezo 6**: "disparador sordo al UPDATE" deja el contador mal).
- Comprobación desde la interfaz: al **asignar** o **retirar** una acreditación
  desde el navegador, el número de acreditaciones del programa cambia **sin que
  la API lo envíe**. La API nunca escribe `num_acreditaciones`.

Las propuestas A–E de la guía §5 se listan y comparan en
[4_research.md](4_research.md) (D3); aquí se fija la elegida y su porqué: el
contador es el caso más claro de **estado derivado que no debe calcular la API**
(evita N+1 y condiciones de carrera) y el más fácil de **demostrar en pantalla**.

> El disparador lo implementa **Valentina** (es su tabla `acreditacion`). El
> spec lo deja decidido y documentado para que los tres construyan sobre él. Los
> tres lo prueban en la interfaz.

---

## 4. Ajuste a la constitución (documentado y propuesto)

La v2 entra en conflicto con tres artículos de
[1_constitution.md](../../1_constitution.md). Como "cuando hay conflicto, la
constitución prevalece", la constitución **debe corregirse** para dar cabida a la
v2. Se documenta aquí y se propone el ajuste al encargado del main.

| Artículo | Dice hoy | La v2 exige | Ajuste propuesto |
|---|---|---|---|
| **Art. 2** | "Dapper (SQL escrito a mano, parametrizado)" | **Todos** los accesos por **procedimiento almacenado**; cero SQL en el repositorio | Añadir: *"Desde la v2, el acceso a datos de las tablas con FK se realiza exclusivamente mediante procedimientos almacenados; el repositorio sólo los invoca."* |
| **Art. 3** | Traduce `ArgumentException`→400, `NoEncontradoExcepcion`→404, `SqlException`→500, body→422 | Un `SqlException` de **integridad referencial** (FK inexistente, pareja duplicada, padre con hijos) → **409**, no 500 | Añadir la fila **`ConflictoExcepcion` → 409** y aclarar que **el 500 se reserva** para errores inesperados del motor. |
| **Art. 8** | Respuesta: `{table, limit, total, data}` · Errores `{status, message, detail}` | La API real (v1 cerrada) responde en **castellano**: `{tabla, limite, total, datos}` y `{estado, mensaje, detalle}` (+ `errores` en 422) | Alinear el artículo con la **envoltura realmente implementada** (castellano), o aprobar formalmente el cambio. Ver §5. |

> **Mientras la constitución no se corrija**, la v2 sigue el **precedente
> implementado en la v1** (envoltura en castellano) para no romper su regresión,
> y deja la corrección del artículo como tarea del encargado del main
> ([8_tasks.md](8_tasks.md)).

---

## 5. Envoltura de respuesta: decisión de la versión

La guía (y el Artículo 8 de la constitución) describen la envoltura **en
inglés** (`{table, limit, total, data}` / `{status, message, detail}`). La v1,
sin embargo, **ya implementó y cerró** la envoltura **en castellano**
(`Respuestas.cs`: `{tabla, limite, total, datos}` y `{estado, mensaje, detalle}`).

**Decisión de la v2: se mantiene la envoltura en castellano, idéntica a la v1,
para todas las tablas y endpoints.**

Justificación:

- **No romper la regresión de la v1.** El Frontend de la v1 ya consume
  `{tabla, limite, total, datos}`; cambiar la envoltura obligaría a reescribir la
  v1 y su `api.service.ts`, que están cerrados con tag.
- **Uno solo de los dos idiomas.** Mezclar una envoltura en inglés con mensajes
  y campos en castellano produce una API incoherente.
- **Documentado, no oculto.** Esta desviación se registra aquí, se propone el
  ajuste del Artículo 8 (§4) y se marca como **riesgo de calificación** en
  [4_research.md](4_research.md): si el profesor exige la envoltura en inglés al
  pie de la letra, éste es el punto a cambiar (un solo archivo, `Respuestas.cs`,
  más los nombres de campo del Frontend).

El detalle exacto de la envoltura (incluida la forma castellana y la inglesa que
pide la guía) vive en [6_contracts.md](6_contracts.md) §0.

---

## 6. Requisitos funcionales

La v2 reutiliza los cinco verbos de la v1 (GET, POST, PUT, PATCH, DELETE) y
añade un **sexto patrón** para las tablas puente (asignar/retirar por pareja).
Los contratos completos están en [6_contracts.md](6_contracts.md).

### RF1 — Listar registros (con nombres de FK)
`GET /api/{tabla}` → 200 con `{tabla, limite, total, datos:[…]}`.
Cada fila incluye, además de sus columnas, el **nombre legible** de cada FK
resuelto **dentro del procedimiento** (alias `universidad_nombre`,
`programa_nombre`, …). Filtra `activo = 1`. Tabla sin registros activos → 204.

### RF2 — Obtener por id
`GET /api/{tabla}/{pk}` → 200 con el registro (con nombres de FK); inexistente o
retirado → 404. En las tablas cuya PK es compuesta, la ruta lleva **las dos
claves** (ver RF8).

### RF3 — Crear registro
`POST /api/{tabla}` con body validado por la petición. Éxito → 200
`{estado, mensaje, datos:{…}}`, donde `datos` es la fila **como quedó guardada**
(la devuelve el procedimiento). Body inválido → 422. PK duplicada → 409. FK
inexistente → 409.

### RF4 — Reemplazar registro (PUT)
`PUT /api/{tabla}/{pk}` con **todos** los campos obligatorios. Éxito → 200
`{estado, mensaje, filasAfectadas}`. Falta un campo → 422. Inexistente → 404.

### RF5 — Actualizar parcialmente (PATCH)
`PATCH /api/{tabla}/{pk}` con body parcial. Sólo se modifican los campos
enviados; el servicio lee el registro actual, lo combina y llama al
procedimiento de actualización (los procedimientos tienen parámetros fijos).
Éxito → 200 `{estado, mensaje, filasAfectadas}`. Body vacío → 400. Inexistente →
404.

### RF6 — Eliminar registro (DELETE, lógico)
`DELETE /api/{tabla}/{pk}` → `activo = 0`. Éxito → 200
`{estado, mensaje, filasEliminadas}`. Inexistente o ya retirado → 404. **Padre
con hijos activos → 409** (regla §3.1).

### RF7 — Asignar una pareja (tablas puente)
`POST /api/{puente}` con las dos claves. Éxito → 200
`{estado, mensaje, datos:{…}}`. **Pareja ya existente → 409.** Cualquiera de las
dos FK inexistente → 409. Las tablas puente **no tienen PUT ni edición**: una
pareja existe o no existe.

### RF8 — Retirar una pareja (tablas puente)
`DELETE /api/{puente}/{pk1}/{pk2}` → si existe, la desactiva (`activo = 0`) →
200 `{estado, mensaje, filasEliminadas}`; si no existe → 404.

### RF9 — Diagnóstico
`GET /` → JSON con mensaje, versión (`"v2"`) y la ruta de estos contratos.

### RF10 — Dashboard ampliado (Frontend)
`/` del Frontend muestra una tarjeta por cada tabla operable (7 de la v1 + 15 de
la v2) con el **total de registros activos** (`GET /api/{tabla}`), menú de
navegación y estado de conexión con la API.

### RF11 — CRUD visual con desplegables (Frontend)
`/tablas/{tabla}`: listado, búsqueda, y Crear/Editar/Eliminar. Cada FK es un
**desplegable** cargado desde la API (nombre visible, clave enviada, sólo
activos). FK opcional → opción «(ninguna)» que envía `null` (nunca `""`). Las
tablas puente muestran **asignar** y **retirar**, sin editar.

### RF12 — Maestro-detalle de `programa` (Frontend)
Al abrir un `programa` se ve y se agrega su detalle (acreditaciones, actividades
académicas, registros calificados, pasantías y premios) **sin salir de la
pantalla**. Varios renglones de detalle se envían en **un solo envío** (una única
petición por renglón dentro de una operación de la pantalla; ver
[9_frontend.md](9_frontend.md) §8 para el alcance exacto).

### RF13 — Consumo de contratos y errores (Frontend)
El Frontend consume exactamente [6_contracts.md](6_contracts.md). Muestra 422
por campo, 404 como "registro no encontrado", **409 con su mensaje en
castellano** ("padre inexistente", "pareja ya registrada", "padre con
registros asociados") y 500 como error del servidor. **Ninguna pantalla usa
jerga** («PUT», «422», «FK»).

---

## 7. Requisitos no funcionales

- **Capas estrictas (API):** el controlador no toca SQL y sólo traduce errores de
  **dominio** a códigos HTTP; el servicio no conoce HTTP pero **sí es el único
  punto que traduce los errores del motor** (`SqlException` de violación de FK/PK
  → `ConflictoExcepcion`) a errores de dominio; el repositorio no conoce HTTP.
  (Art. 3.)
- **Cero SQL en C#:** todo el acceso a datos pasa por procedimientos
  almacenados; el repositorio sólo los invoca con parámetros (`@`) y
  `CommandType.StoredProcedure`.
- **Nombres de parámetros:** los procedimientos usan los nombres **snake_case**
  de las columnas (`@fecha_fun`, `@fecha_creacion`, …); el repositorio arma un
  `DynamicParameters` con esos nombres exactos (ver [4_research.md](4_research.md)
  D2 — gotcha de Dapper).
- **Asíncrona:** todo acceso a datos con `async/await`.
- **Errores uniformes:** `{estado, mensaje, detalle}` (+ `errores:[…]` en 422);
  los conflictos de integridad agregan la forma 409 (ver §5).
- **Sin anticipación:** nada de login, JWT, roles ni consultas multitabla.
- **Idioma:** identificadores y comentarios del código según el **molde de la
  v1** (castellano en nombres propios del dominio, comentarios explicativos);
  textos de la interfaz en castellano y **sin jerga**; la API responde la
  envoltura en castellano de la §5.
- **Frontend:** TypeScript estricto; componentes Angular *standalone*; `apiUrl`
  en `environments/environment.ts` (`http://localhost:8036`); CORS habilitado
  para `http://localhost:8037`; sin secretos en el cliente.
- **Disparador:** la lógica del contador vive en la **base de datos**, no en C#;
  la API nunca escribe `programa.num_acreditaciones`.

---

## 8. Criterios de aceptación

> **Nota de reconstrucción.** La guía del profesor §8 (los 12 criterios) no está
> en el repositorio; el índice [`../v2/GUIA_IA2.md`](GUIA_IA2.md) §4 y §7 y los
> issues permiten reconstruirlos. Se listan abajo y **deben verificarse contra
> la guía física** antes del tag. La §9 (regresión de la v1) sí se conoce por el
> cierre de la v1.

**Criterios de la v2 (reconstruidos):**

1. `docker compose up -d --build` levanta SQL Server (con las 15 tablas y sus
   procedimientos) + API + Frontend; `GET /` responde `"version":"v2"`.
2. CRUD punta a punta **por procedimientos almacenados** de las 15 tablas (sin
   SQL en C#).
3. Todas las FK se eligen en **desplegables** cargados desde la API (muestran
   nombre, envían clave, sólo ofrecen activos).
4. Existe la **pantalla maestro-detalle** de `programa`.
5. **Borrado lógico**: retirar → desaparece del listado y sigue en la base;
   segundo `DELETE` → 404.
6. Las **tablas puente** no tienen botón de editar (sólo asignar y retirar).
7. **FK inexistente → 409** (nunca 500).
8. **Pareja duplicada en puente → 409.**
9. El **disparador** es comprobable desde la interfaz (el dato cambia sin que la
   API lo envíe, incluido **retirar** un hijo).
10. La interfaz está en **castellano y sin jerga**; opcionales con «(ninguna)» →
    `null`.
11. La **regresión de la v1** sigue en verde (criterios de `../v1_producto_sqlserver/2_spec.md` §5).
12. Tag `v2` sobre el commit que cumple todo lo anterior.

**Correspondencia con los issues** (`issues_v2.md`):

- **Issue 1 (Emmanuel)**: `facultad`, `pasantia`, `premio` → criterios **2, 3, 5,
  7, 10** + los 8 documentos del spec kit.
- **Issue 2 (Valentina)**: `programa` + bloque normativo + disparador →
  criterios **2, 3, 4, 5, 6, 7, 8, 9, 10, 11**.
- **Issue 3 (Felipe)**: docentes, alianzas y puentes → criterios **2, 3, 5, 6, 7,
  8, 10, 11**.

**Cierre:**

- El `9_checklist.md` lo marca **una persona**, con todo en verde, antes del tag.
- Tag `v2` en `main` (Art. 9).

---

## 9. Reglas fundamentales de la v2

1. Sólo se exponen las **15 tablas con FK**; las 7 de la v1 no se tocan.
2. Todo el CRUD pasa por **procedimientos almacenados**; cero SQL en el
   repositorio.
3. `sp_crear_<tabla>` termina devolviendo la **fila guardada**.
4. Las 15 tablas tienen **`activo`** ⇒ borrado lógico y listados que filtran
   `activo = 1`.
5. **Ninguna FK se digita**: desplegable cargado desde la API.
6. FK inexistente, pareja duplicada en puente o **padre con hijos activos** →
   **409** en castellano (nunca 500).
7. Un **disparador** mantiene un estado derivado y es comprobable desde la
   interfaz.
8. La **lógica de negocio vive en la base de datos** (procedimientos +
   disparador) **y** en el servicio (reglas de la §3.1); el controlador sólo
   traduce errores a HTTP. *(Esto invierte la regla 21 de la v1.)*
9. Los puentes se manejan **por pareja**: asignar y retirar, sin edición.
10. La envoltura y los mensajes van **en castellano** (decisión §5, a la espera
    del ajuste del Artículo 8).
11. La versión de estos contratos corresponde exclusivamente a la **v2**.
