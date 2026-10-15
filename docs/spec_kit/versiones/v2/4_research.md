# Investigación y decisiones — Versión 2: la versión de la clave foránea

> **Versión 2** · **Lectura opcional** (el porqué de cada decisión del plan,
> con las alternativas evaluadas y descartadas). Complementa a
> [3_plan.md](3_plan.md); el modelo de datos exacto está en
> [5_data_model.md](5_data_model.md).

---

## D1 — El 409: la decisión que define la versión

**Contexto.** Alguien crea un `programa` con una `facultad` que no existe (o
repite una pareja en una tabla puente, o intenta retirar un padre con hijos
activos). ¿Qué responde la API?

**Opciones evaluadas:**

| Opción | Argumento | ¿Sirve? |
|---|---|---|
| **422** Unprocessable Entity | "El dato está mal" | **No.** El dato tiene la forma correcta; el 422 se reserva para lo que se rechaza **sin consultar nada** (campo faltante, tipo equivocado). Saber si la facultad existe **exige ir a la base**. |
| **500** Internal Server Error | "Me caí" | **No.** La base funciona; sólo rechazó la relación. Un 500 es un error inesperado, no una regla. |
| **409** Conflict | Lo que se rompe es el **estado** de la base: esa fila no existe, o la pareja ya existe, o hay hijos | **Sí.** Describe exactamente la situación: el pedido es válido pero incompatible con el estado actual de los datos. |

**Decisión: 409.** Es el código que corresponde a un conflicto con el **estado**
(no con la **forma**) de los datos.

**Consecuencias.** (+) La interfaz puede distinguir "revisa lo que escribiste"
(422) de "el sistema cambió / esa relación no existe" (409). (+) El 500 vuelve a
significar lo que debe: un error inesperado. **¿Quién traduce el error y en qué
capa? → el SERVICIO**, porque es el único que conoce el error del motor
(`SqlException`) **y** la regla de negocio, y no conoce HTTP. El **controlador**
sólo convierte la excepción de negocio en el código 409.

## D2 — Procedimientos almacenados como única vía al SQL

**Contexto.** La v1 escribía SQL a mano en el repositorio (regla 21: "la lógica
de negocio pertenece a la API, no a triggers o procedimientos"). La v2 **invierte**
esa regla: todo el CRUD pasa por procedimientos almacenados.

**Opciones evaluadas:** (a) seguir con SQL a mano en el repositorio (v1) ·
(b) **procedimientos almacenados**, invocados con `CommandType.StoredProcedure` ·
(c) un ORM.

**Decisión: (b).** El repositorio **no contiene SQL**: sólo arma parámetros y
llama al procedimiento. Ventajas: el plan de ejecución se cachea, el SQL queda en
un artefacto versionado de la base, y —lo que más importa en el curso— **el
repositorio se vuelve trivial de auditar** (si aparece un `SELECT`/`INSERT` en un
`.cs`, es un error).

### Gotcha 1 — Parámetros snake_case vs propiedades PascalCase

Los procedimientos declaran sus parámetros con el **nombre de la columna**
(`@fecha_fun`, `@fecha_creacion`). Dapper **no** empareja `@fecha_fun` con la
propiedad `FechaFun` de un objeto anónimo pasado como `param`. Por eso el
repositorio **construye un `DynamicParameters`** y agrega cada parámetro con el
nombre snake_case **exacto**. (Si no, el parámetro llega `NULL` y el `INSERT`
falla — o peor, guarda `NULL`.)

### Gotcha 2 — Alias en los JOIN

Los listados traen el **nombre legible** de cada FK resuelto **dentro del
procedimiento** (`LEFT JOIN universidad … SELECT u.nombre AS universidad_nombre`).
El alias debe llamarse `<fk>_nombre` para que caiga en la propiedad
`<Fk>Nombre` del modelo. Sin alias, la columna llega con su nombre original y la
propiedad queda `null` (**tropiezo 3**).

## D3 — El disparador: propuestas A–E y la elegida

La guía §5 propone cinco disparadores posibles (A–E). El equipo elige **uno** y
lo justifica.

| Prop. | Idea | Comentario para la v2 |
|---|---|---|
| **A** | Contador de hijos activos en el maestro (`programa.num_acreditaciones`) | **Elegida.** Estado derivado claro; evita calcularlo en la API; demostrable en pantalla. |
| **B** | Fecha de última modificación (`programa.fecha_actualizacion`) automática | Descartada como **principal** porque `fecha_actualizacion` ya es `VARCHAR` en el script y ya se envía desde la API; no es un estado *derivado* por hijos. |
| **C** | Validación "no más de N hijos" por programa | Descartada: es una regla arbitraria, no un dato que el usuario vea cambiar. |
| **D** | Normalizar un campo a partir de otro (p. ej. copiar el `facultad` del programa al hijo) | Descartada: introduce redundancia y riesgo de inconsistencia. |
| **E** | Auditoría (tabla histórica de cambios) | Descartada: pertenece a la v3/v4; la v2 no anticipa. |

**Decisión: (A) contador `programa.num_acreditaciones`, mantenido por
`trg_acreditacion_contador` `AFTER INSERT, UPDATE, DELETE` sobre `acreditacion`.**

Por qué importa el `UPDATE`: como el borrado es **lógico**, **retirar** una
acreditación es un `UPDATE activo = 0`, no un `DELETE`. Un disparador que sólo
escuchara `INSERT`/`DELETE` dejaría el contador **mal** al retirar (**tropiezo
6**). El disparador recalcula el contador del `programa` afectado contando sus
acreditaciones **activas**:

```sql
CREATE OR ALTER TRIGGER trg_acreditacion_contador
ON acreditacion AFTER INSERT, UPDATE, DELETE AS
BEGIN
    -- El programa afectado se toma de inserted o deleted (según la operación).
    UPDATE p
       SET num_acreditaciones = (SELECT COUNT(*) FROM acreditacion a
                                  WHERE a.programa = p.id AND a.activo = 1)
      FROM programa p
     WHERE p.id IN (SELECT programa FROM inserted UNION SELECT programa FROM deleted);
END
```

**Consecuencia clave:** la API **nunca** escribe `num_acreditaciones`. Ese dato
sólo cambia por el disparador ⇒ es **comprobable desde la interfaz** (criterio 9).

## D4 — Retirar un maestro con hijos: ¿404 o 409?

**Contexto.** `sp_eliminar_programa` devuelve `filas = 0` en dos casos distintos:
(i) el programa **no existe** (o ya está retirado), o (ii) el programa **existe
pero tiene hijos activos**. Los dos casos no pueden responder lo mismo.

**Opciones evaluadas:** (a) que el **procedimiento** devuelva un código
distinto; (b) que el **servicio** consulte el maestro antes de borrar.

**Decisión: (b), en el servicio.** Antes de llamar a `EliminarAsync`, el servicio
lee el maestro con `ObtenerPorIdAsync`:

- No existe → `NoEncontradoExcepcion` (**404**).
- Existe y `EliminarAsync` devuelve 0 filas → tenía hijos activos →
  `ConflictoExcepcion` (**409**).

Por qué en el servicio y no en el procedimiento: mantiene el procedimiento
simple (un `UPDATE` condicionado) y deja la **regla de negocio** donde la
constitución la quiere (Art. 3: "el servicio no conoce HTTP ni el motor", pero sí
las reglas). El procedimiento nunca devuelve un "motivo"; el servicio decide el
código.

## D5 — Borrado lógico: `activo` en las 15 tablas

**Verificación.** Ninguna de las 15 tablas de la v2 trae `activo` en el script
provisto (sólo lo traen las tablas de usuarios, de la v3). Ver
[2_spec.md](2_spec.md) §3.2.

**Decisión:** agregar `activo BIT NOT NULL DEFAULT 1` a las 15 mediante
`db/03_activar_borrado_logico_v2.sql` (idempotente), igual que la v1 hizo con sus
siete. Consecuencia: **todas** filtran `activo = 1` y el `DELETE` es un `UPDATE`.

## D6 — `docente_departamento` y `alianza`: el docente de otro módulo

**Contexto.** El script declara `docente_departamento(docente INT, departamento
INT, …)` con `FOREIGN KEY (departamento) REFERENCES programa(id)` — nótese que
`departamento` apunta a **programa**. Pero **`docente` no tiene FK**: el docente
pertenece al módulo de **Gestión Profesoral**, que vive en **otra base de datos**.
Lo mismo pasa con el `docente` opcional de `alianza`.

**Opciones evaluadas:**

| Opción | Problema |
|---|---|
| (a) Desplegable de docentes cargado desde la API | **Imposible**: no hay tabla de docentes en `innovacion_curricular` ni endpoint que la exponga. |
| (b) **Tratar `docente` como un identificador entero** (código/cédula del docente) digitado por el usuario | ✓ Es lo único que hay: no se puede poblar un desplegable sin fuente. |
| (c) Inventar una tabla/endpoint de docentes | Anticipa trabajo fuera de alcance; viola "no se anticipa". |

**Decisión: (b).** `docente` se maneja como un **entero identificador** (campo
numérico), no como desplegable, y se documenta que su catálogo vive en el módulo
de Gestión Profesoral (otra BD) y queda **fuera del alcance de la v2**. En
`alianza`, `docente` es **opcional** (`NULL` permitido). Las demás FK de esas
tablas (`departamento`→`programa`, `aliado`→`aliado`) **sí** son desplegables
cargados desde la API.

## D7 — Tablas puente: asignar y retirar, sin editar

**Decisión.** Las seis puentes (`programa_ac`, `programa_pe`, `programa_ci`,
`an_programa`, `enfoque_rc`, `aa_rc`) tienen PK compuesta (dos o más claves) y no
tienen atributos propios que "editar" salvo en `aa_rc` (`componente`, `semestre`).
El patrón es **asignar** (crear la pareja) y **retirar** (desactivarla); **no**
hay `PUT` ni botón de editar (**tropiezo 9**): una pareja existe o no existe.
Repetir la pareja la rechaza la **PK compuesta** → el servicio la traduce a
**409**.

> `aa_rc` lleva `componente` y `semestre` (obligatorios) y `alianza`/
> `docente_departamento` llevan fechas; esas son las tablas "puente con
> atributos": se siguen manejando por pareja, con sus atributos como parte del
> `INSERT`, sin edición posterior.

## D8 — La envoltura en castellano (y su riesgo)

**Contexto.** La guía y el Artículo 8 piden la envoltura **en inglés**
(`{table, limit, total, data}` / `{status, message, detail}`). La v1 ya cerró con
la envoltura **en castellano** (`{tabla, limite, total, datos}` /
`{estado, mensaje, detalle}`).

**Decisión:** mantener la envoltura de la v1 en castellano, para no reabrir la v1
ni mezclar idiomas (ver [2_spec.md](2_spec.md) §5).

**Riesgo documentado.** Si la evaluación exige la envoltura en inglés al pie de
la letra, el punto a cambiar es acotado: renombrar las propiedades
`JsonPropertyName` de `Respuestas.cs` y los campos equivalentes del Frontend. Se
deja señalado, no resuelto, porque **cambiarlo ahora rompe la regresión de la v1**.

## D9 — Los desplegables de FK

**Decisión.** Toda FK se elige en un desplegable poblado desde la API (nombres
visibles, clave enviada, **sólo activos**). Para una FK **opcional** se ofrece la
opción **«(ninguna)»**, que envía **`null`** y nunca la cadena vacía `""`
(**tropiezo 4**: `""` no es `null` y produce error de conversión).

**Alternativas descartadas:** un `input` de texto para la clave (viola "ninguna
clave foránea se digita a mano" y el **tropiezo 2**); cargar los desplegables
**uno tras otro** (con la API lenta, la pantalla tarda — **tropiezo 5**); por eso
el Frontend los carga **en paralelo** (`forkJoin`).

## D10 — `POST` devuelve la fila creada

**Contexto.** La v1 devolvía `{estado, mensaje}` en el `POST`. La guía de la v2
exige que `sp_crear_*` devuelva la fila **como quedó guardada** (**tropiezo 8**:
"procedimiento que no devuelve la fila ⇒ la interfaz no puede mostrar lo que
acaba de crear").

**Decisión.** El `POST` de la v2 responde `{estado, mensaje, datos:{…}}`. Se
agrega una clase `RespuestaCreacion<T>` a `Respuestas.cs`. El Frontend puede
insertar la fila devuelta sin un `GET` adicional.

## D11 — Maestro-detalle de `programa` en un solo envío

**Contexto.** La guía pide que varios renglones del detalle se envíen **en un
solo envío** (una única petición) y sin salir de la pantalla.

**Decisión.** La pantalla `programa-detalle` permite dar de alta el `programa` y,
a continuación, **varios renglones de detalle** de tipos heterogéneos
(pasantías, premios, acreditaciones, actividades académicas, registros
calificados). Como las tablas de detalle son **tablas distintas** (no una sola
tabla de líneas), "un solo envío" significa: **una única petición por tipo de
detalle**, disparadas desde la misma pantalla sin navegación, en paralelo cuando
es posible. El alcance exacto se fija en [9_frontend.md](9_frontend.md) §8 para
no depender de una lectura ambigua de la guía.

> **Nota de reconstrucción.** La guía §8 no está en el repositorio; la
> interpretación de "un solo envío" se documenta explícitamente y debe validarse
> con el profesor.

## D12 — Capas por tabla, sin repositorio genérico

**Decisión (heredada de la v1, D2).** Cada tabla tiene su propio juego
controller → servicio → repositorio con interfaces. No se generaliza con
`RepositorioGenerico<T>`: la prueba de capas con un repositorio falso es la
evidencia objetiva de que cada juego quedó bien cortado.

**Matiz de la v2.** Aunque el molde es idéntico, las **cuatro tablas maestro**
(`facultad`, `programa`) y las **seis puentes** tienen métodos propios
(`EliminarAsync` condicionado; `AsignarAsync`/`RetirarAsync`), lo que refuerza
que no conviene una capa genérica.

## D13 — Tipos de fecha mixtos: `DATE` vs `VARCHAR`

**Contexto.** El script provisto es inconsistente: unas fechas son `DATE`
(`facultad.fecha_fun`, `registro_calificado.fecha_inicio/fin`, `premio.fecha`,
`docente_departamento.fecha_ingreso/salida`, `alianza.fecha_inicio/fin`) y otras
son `VARCHAR(45)` (`programa.fecha_creacion/cierre/actualizacion`,
`acreditacion.fecha_inicio/fin`).

**Decisión.** Respetar el script: las columnas `DATE` viajan como `DateTime` en
C# y como `string`/`Date` en el Frontend; las columnas `VARCHAR` viajan como
`string`. **No se "corrige" el esquema** (la base viene dada). El
[5_data_model.md](5_data_model.md) marca el tipo de cada fecha para que
repositorio, modelo y formulario coincidan.

## D14 — Separar los scripts de la v2 por responsable

**Decisión.** Los procedimientos y disparadores de la v2 se reparten en tres
archivos (`04_procedimientos_v2_emmanuel.sql`, `_valentina.sql`, `_felipe.sql`)
para que cada integrante tenga su archivo versionable y la integración no genere
conflictos en un único archivo gigante. El inicializador los corre a todos.
