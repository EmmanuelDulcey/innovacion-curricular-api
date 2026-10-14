# Issues — Versión 2 · Innovación Curricular

> Cada bloque `## ISSUE N` se copia y pega como un issue independiente en GitHub
> (el título va en el campo *Title*, y las etiquetas/asignado en la barra lateral).

## Reparto de trabajo (resumen)

| Integrante | Rol principal | Tablas de la v2 | Nº |
|---|---|---|---|
| **Emmanuel** | Spec kit v2 (docs 2–9) + 3 tablas | `facultad`, `pasantia`, `premio` | 3 |
| **Valentina** | Maestro `programa` + bloque de registro/acreditación + disparador | `programa`, `acreditacion`, `registro_calificado`, `activ_academica`, `enfoque_rc`, `aa_rc` | 6 |
| **Felipe** | Docentes, alianzas y puentes del programa | `docente_departamento`, `alianza`, `programa_ac`, `programa_pe`, `programa_ci`, `an_programa` | 6 |

Orden de dependencias entre tablas: `facultad` → `programa` → (todo lo demás).
Por eso Emmanuel entrega primero el spec kit y `facultad`.

---

## ISSUE 1

**Title:** `[v2] Spec kit de la v2 (documentos 2–9) y CRUD de facultad, pasantia y premio`

**Assignee:** Emmanuel
**Labels:** `v2` `documentación` `spec-kit` `backend` `frontend`
**Rama:** `rama-emmanuel`
**Prioridad:** Alta — bloquea al resto del equipo (la spec se escribe ANTES del código)

### Contexto

La v2 agrega las **15 tablas con clave foránea** del módulo. Según la guía del
profesor, el spec kit se escribe **antes** de construir y describe **solo el
delta** respecto a la v1. Emmanuel es el responsable de dejar la especificación
lista para que Valentina y Felipe puedan construir sobre ella, y además de
implementar tres tablas.

### Parte A — Spec kit de la v2 (responsabilidad principal)

Ruta: `docs/spec_kit/versiones/v2_<nombre>/`

- [ ] `2_spec.md` — qué se construye en la v2 y qué NO (§10 de la guía: nada de login, usuarios ni consultas multitabla). Debe incluir las **decisiones del equipo** que la guía exige escribir:
  - [ ] Regla de negocio: si el borrado es lógico, ¿se puede retirar un maestro (p. ej. `programa`) que aún tiene detalle activo? Decidir y justificar.
  - [ ] Decisión sobre tablas sin columna `activo`, si las hay (verificar contra el script SQL).
  - [ ] Cuál(es) disparador(es) se implementan (propuestas A–E de la guía §5) y por qué.
- [ ] `3_plan.md` — arquitectura y orden de construcción (dependencias `facultad` → `programa` → resto).
- [ ] `4_research.md` — decisiones técnicas y alternativas. Incluir una nota sobre `docente_departamento` y `alianza`: el docente pertenece al módulo de Gestión Profesoral (otra BD); definir cómo se resuelve ese campo aquí.
- [ ] `5_data_model.md` — las 15 tablas de la v2 con PK, FK, campos opcionales/obligatorios, columna `activo` y la columna/tabla que requiera el disparador.
- [ ] `6_contracts.md` — endpoints, formatos y códigos de estado. Debe cubrir el sobre de respuesta `{table, limit, total, data}`, errores `{status, message, detail}`, y el **nuevo 409** (padre inexistente, pareja duplicada en puente, borrado de padre con hijos).
- [ ] `7_quickstart.md` — cómo levantar y probar la v2 (`docker compose up -d --build`, colección de pruebas).
- [ ] `8_tasks.md` — tareas desglosadas por integrante, alineadas con estos issues.
- [ ] `9_checklist.md` — los **12 criterios de aceptación** de la guía (§8) + la regresión de la v1 (§9).

> ⚠️ **Actualizar la constitución.** El Artículo 2 dice «Dapper con SQL escrito a mano» y el Artículo 3 no incluye el 409. La v2 exige **todo el CRUD por procedimientos almacenados** (sin SQL en el repositorio) y **409** para integridad referencial. Dejar documentado en `2_spec.md` y proponer el ajuste a la constitución (cuando haya conflicto, la constitución prevalece, así que debe corregirse).

### Parte B — Tablas a implementar (API + SQL + Frontend)

| Tabla | Notas |
|---|---|
| `facultad` | FK a `universidad`. Es maestro de `programa`. Dropdown de universidad con nombres. **Prioridad: entregar pronto**, `programa` depende de ella. |
| `pasantia` | FK a `programa`. Detalle del programa. |
| `premio` | FK a `programa`, con entidad otorgante. Detalle del programa. |

Para cada una:
- [ ] Script SQL: `sp_listar_`, `sp_consultar_`, `sp_crear_` (devuelve la fila como quedó guardada), `sp_actualizar_`, `sp_eliminar_` (lógico si hay `activo`: `UPDATE ... SET activo = 0`). El listado filtra inactivos.
- [ ] Repositorio que **solo llama** a los procedimientos (cero SQL a mano).
- [ ] Capas respetadas: Controller → Service → Repository, con interfaces.
- [ ] Traducción de errores: FK inexistente → **409** en castellano (nunca 500).
- [ ] Frontend Angular: listar, crear, editar y retirar, con desplegables cargados desde la API (muestran nombre, mandan la clave; solo ofrecen activos).
- [ ] Peticiones en la colección de pruebas, incluidas las que deben fallar.

### Guía de IA personal

- [ ] Completar y mantener `docs/spec_kit/versiones/v2_<nombre>/GUIA_IA2_EMMANUEL.md`: el prompt realmente usado, y el **registro de lo que hubo que corregirle a la IA** (se llena mientras trabaja, no al final).

### Criterios de aceptación

- [ ] Los 8 documentos del spec kit (2 a 9) existen en la carpeta correcta, completos y coherentes entre sí.
- [ ] La spec queda lista **antes** de que Valentina y Felipe empiecen a codificar.
- [ ] `facultad`, `pasantia` y `premio` pasan los criterios 2, 3, 5, 7 y 10 de la guía.
- [ ] La interfaz no usa jerga («PUT», «422», «FK»).
- [ ] La regresión de la v1 sigue en verde.

### Sustentación individual (4 de 20 puntos)

El profesor elige un archivo del repositorio y pide explicar en voz alta: qué hace, por qué así, qué pasa si falla y qué cambiaría. Estar preparado para sustentar **sus procedimientos, su repositorio y todo el spec kit**.

### Definition of Done

- Trabajo en `rama-emmanuel`, nunca en `main`; commits pequeños y descriptivos en español.
- Pull Request al encargado del main, con descripción de lo cambiado.
- Código comentado para claridad académica; nombres en inglés.

---

## ISSUE 2

**Title:** `[v2] Maestro programa con su detalle, bloque de registro calificado/acreditación y disparador`

**Assignee:** Valentina
**Labels:** `v2` `backend` `frontend` `base-de-datos` `maestro-detalle`
**Rama:** `rama-valentina`
**Prioridad:** Alta

### Contexto

`programa` es el **maestro central** del módulo: de él cuelgan acreditación, actividades académicas, pasantías, premios y registros calificados. Valentina implementa el maestro, su bloque académico-normativo (registro calificado y sus puentes) y el **disparador** exigido por la guía.

### Tablas a implementar (6)

| Tabla | Notas |
|---|---|
| `programa` | FK a `facultad` (dependencia: esperar o coordinar con Emmanuel). **Pantalla maestro-detalle**: al abrir un programa se ve y se agrega su detalle (acreditaciones, actividades académicas, registros calificados, y también pasantías y premios, construidos por Emmanuel) sin salir de la pantalla. |
| `acreditacion` | PK `resolucion`, FK a `programa`; tipo, calificación y fechas. |
| `registro_calificado` | PK `codigo`, FK a `programa`; créditos, horas, metodología, duración, titulación. |
| `activ_academica` | Cursos/asignaturas: créditos, horas, idioma, cursos espejo. |
| `enfoque_rc` | **Tabla puente** `enfoque` ↔ `registro_calificado` (PK compuesta). |
| `aa_rc` | **Tabla puente** `activ_academica` ↔ `registro_calificado`, con componente y semestre (PK compuesta). |

### Tareas

**Base de datos**
- [ ] Los 5 procedimientos (`sp_listar/consultar/crear/actualizar/eliminar_<tabla>`) de cada tabla. `sp_crear_*` devuelve la fila tal como quedó guardada.
- [ ] Borrado lógico donde exista `activo`: `UPDATE ... SET activo = 0`; los listados filtran inactivos.
- [ ] Implementar **al menos un disparador** (propuestas A–E de la guía §5, según lo que decida `2_spec.md`). Si es contador/estado derivado, debe escuchar **INSERT, UPDATE y DELETE** (retirar es un UPDATE).
- [ ] Probar el disparador **desde la interfaz**: el dato debe cambiar sin que la API lo envíe.

**API (C# / ASP.NET Core)**
- [ ] Repositorios que solo llaman a los procedimientos, con parámetros.
- [ ] Interfaces `I...`, servicios y controladores según el Artículo 3 de la constitución.
- [ ] Errores traducidos: FK inexistente → **409**; pareja repetida en puente → **409**; borrado de padre con hijos (si es físico) → **409**. Mensajes en castellano.
- [ ] Aplicar en el servicio la **regla de negocio** decidida en el spec (retirar programa con detalle activo).
- [ ] Endpoints de puente: asignar y retirar por pareja (sin PUT).

**Frontend (Angular)**
- [ ] CRUD visual de las 6 tablas.
- [ ] Todas las FK como desplegable cargado desde la API (nombre visible, clave enviada). Opcionales con «(ninguna)» → `null`, no cadena vacía.
- [ ] Tablas puente **sin botón de editar**: solo asignar y retirar.
- [ ] Vista maestro-detalle de `programa`; varios renglones de detalle se envían en **un solo envío** (una única petición).
- [ ] Sin jerga técnica en pantalla.

**Pruebas**
- [ ] Peticiones en la colección, incluidas las que deben fallar (padre inexistente, pareja duplicada).

### Guía de IA personal

- [ ] Completar y mantener `docs/spec_kit/versiones/v2_<nombre>/GUIA_IA2_VALENTINA.md`: el prompt realmente usado, y el **registro de lo que hubo que corregirle a la IA** (se llena mientras trabaja, no al final).

### Criterios de aceptación

- [ ] Criterios 2, 3, 4, 5, 6, 7, 8, 9, 10 y 11 de la guía para sus tablas.
- [ ] El disparador funciona y se demuestra por la interfaz.
- [ ] Un `programa` no se crea sin una `facultad` existente (409 si no existe).
- [ ] Repetir una pareja en `enfoque_rc` o `aa_rc` devuelve 409.
- [ ] Regresión de la v1 en verde.

### Sustentación individual (4 de 20 puntos)

Debe poder explicar **sin leer línea por línea**: sus procedimientos, por qué el disparador vive en la base y no en C#, qué recibe el usuario si la FK falla y dónde se tocaría para agregar un campo al detalle.

### Definition of Done

- Trabajo en `rama-valentina`, nunca en `main`; commits descriptivos en español; PR al encargado del main.
- Código comentado; prompts usados (y correcciones a la IA) registrados en la carpeta de prompts de la versión.

---

## ISSUE 3

**Title:** `[v2] Docentes por programa, alianzas y tablas puente de programa (áreas, prácticas, innovación, normativa)`

**Assignee:** Felipe
**Labels:** `v2` `backend` `frontend` `base-de-datos` `tablas-puente`
**Rama:** `rama-felipe`
**Prioridad:** Alta

### Contexto

Felipe implementa las relaciones que conectan el programa con personas, aliados y los catálogos de la v1. Son tablas con dos claves foráneas, varias de ellas **tablas puente** con PK compuesta, donde los errores típicos son el 500 en vez del 409 y los desplegables mal cargados.

### Tablas a implementar (6)

| Tabla | Notas |
|---|---|
| `docente_departamento` | Vinculación docente ↔ programa: dedicación, modalidad, fechas. El docente pertenece al módulo de Gestión Profesoral: seguir la decisión que documente Emmanuel en `4_research.md`. |
| `alianza` | Alianza `aliado` (NIT) ↔ `programa`, con fechas y docente responsable. |
| `programa_ac` | Puente `programa` ↔ `area_conocimiento`. |
| `programa_pe` | Puente `programa` ↔ `practica_estrategia`. |
| `programa_ci` | Puente `programa` ↔ `car_innovacion`. |
| `an_programa` | Puente `aspecto_normativo` ↔ `programa`. |

### Tareas

**Base de datos**
- [ ] Los 5 procedimientos por tabla; `sp_crear_*` devuelve la fila como quedó guardada.
- [ ] Borrado lógico donde exista `activo`; listados filtrando inactivos (revisar el script SQL para cada tabla).
- [ ] En las tablas puente: procedimientos de asignar y retirar por pareja, y listado que muestra nombres (JOIN dentro del procedimiento, no en C#).

**API (C# / ASP.NET Core)**
- [ ] Repositorios que solo llaman procedimientos (cero SQL a mano ni concatenado).
- [ ] Capas con interfaces; sobre de respuesta `{table, limit, total, data}`.
- [ ] **409** en castellano para: padre inexistente, pareja repetida en puente, borrado de padre con hijos (si aplica). Nunca 500.
- [ ] Validaciones de fechas coherentes en `alianza` y `docente_departamento` (fecha fin ≥ fecha inicio) con 400/422 según la constitución.

**Frontend (Angular)**
- [ ] CRUD visual de las 6 tablas.
- [ ] Desplegables de programa, aliado, área, práctica, característica de innovación y aspecto normativo cargados desde la API, mostrando nombres y enviando claves; solo ofrecen activos.
- [ ] Campos opcionales con «(ninguna)» → `null`.
- [ ] Puentes **sin botón de editar**: asignar y retirar; se muestran como bloques dentro del detalle del programa cuando sea posible.
- [ ] Sin jerga («PUT», «422», «FK») en ninguna pantalla.

**Pruebas**
- [ ] Colección con casos válidos y casos que deben fallar (padre inexistente, pareja duplicada, fechas inválidas).

### Guía de IA personal

- [ ] Completar y mantener `docs/spec_kit/versiones/v2_<nombre>/GUIA_IA2_FELIPE.md`: el prompt realmente usado, y el **registro de lo que hubo que corregirle a la IA** (se llena mientras trabaja, no al final).

### Criterios de aceptación

- [ ] Criterios 2, 3, 5, 6, 7, 8, 10 y 11 de la guía para sus tablas.
- [ ] Cada puente: asignar → repetir (409) → retirar funciona de punta a punta.
- [ ] Los desplegables aparecen **llenos** y con nombres, no códigos.
- [ ] Regresión de la v1 en verde.

### Sustentación individual (4 de 20 puntos)

Debe poder explicar un procedimiento de su autoría, cómo se traduce el error de la base a 409 y qué ve el usuario, y por qué una tabla puente no se edita.

### Definition of Done

- Trabajo en `rama-felipe`, nunca en `main`; commits descriptivos en español; PR al encargado del main.
- Código comentado; prompts usados y correcciones a la IA registrados en la carpeta de prompts.

---

## Checklist común del equipo (pegar en el issue de cierre o en el PR final)

- [ ] v1 cerrada y con tag `v1` antes de empezar (las FK necesitan datos en los catálogos).
- [ ] Spec kit v2 escrito antes del código.
- [ ] 22 tablas operables desde la interfaz (menos usuario/rol/rol_usuario, que son de la v3).
- [ ] Todo el CRUD por procedimientos almacenados; sin SQL en los repositorios.
- [ ] Al menos un disparador funcionando y comprobable.
- [ ] Ninguna clave foránea digitada a mano.
- [ ] 409 (nunca 500) en integridad referencial.
- [ ] Regresión de la v1 completa.
- [ ] Índice `GUIA_IA2.md` (integrador) y una guía personal por integrante, con los prompts y las correcciones hechas a la IA.
- [ ] Tag `v2` sobre el commit que cumple los 12 criterios.
