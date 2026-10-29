# Tareas — Versión 2: 15 tablas con FK, procedimientos y disparador

> **Versión 2** · El orden de construcción, partiendo de la v1 **cerrada con tag
> `v1`**. Cada fase termina en algo verificable.
> Requisitos: [2_spec.md](2_spec.md) · técnica: [3_plan.md](3_plan.md) ·
> contratos: [6_contracts.md](6_contracts.md) · validación final:
> [7_quickstart.md](7_quickstart.md) · cierre: [9_checklist.md](9_checklist.md).
>
> **Regla de oro del orden:** `facultad` → `programa` → el resto. **Emmanuel va
> primero** (spec kit + `facultad`); sin spec no se construye y sin `facultad` no
> se puede crear `programa`.

---

## Fase 0 — Especificación y cimiento (Emmanuel)

- [ ] Escribir **todo el spec kit de la v2** (`2_spec.md` … `9_checklist.md`) en
  `docs/spec_kit/versiones/v2/`, **antes** de cualquier código.
- [ ] Decidir y escribir: la regla de negocio del maestro con hijos, las tablas
  sin `activo`, y el disparador (propuestas A–E) — [2_spec.md](2_spec.md) §3.
- [ ] Documentar el ajuste a la constitución (Art. 2, 3, 8) —
  [2_spec.md](2_spec.md) §4.
- [ ] `db/03_activar_borrado_logico_v2.sql`: `activo` en las 15 tablas +
  `programa.num_acreditaciones` (idempotente).
- [ ] Actualizar `db/init.sh` para correr `03_…` y `04_…` en orden.
- **Verificar:** `docker compose down -v && docker compose up -d --build` deja la
  BD con `activo` en las 15 tablas y `num_acreditaciones` en `programa`.

## Fase 1 — `facultad` de punta a punta (Emmanuel) — desbloquea el resto

- [ ] `db/04_procedimientos_v2_emmanuel.sql`: los 5 procedimientos de `facultad`
  (listar con `universidad_nombre`, consultar, crear con `SELECT` final,
  actualizar, eliminar condicionado).
- [ ] `Modelos/Facultad.cs`, `Peticiones/FacultadPeticiones.cs`.
- [ ] `Repositorios/IRepositorioFacultad.cs` + `RepositorioFacultadSqlServer.cs`
  (**sólo** `CommandType.StoredProcedure` + `DynamicParameters`).
- [ ] `Servicios/IServicioFacultad.cs` + `ServicioFacultad.cs` (traduce 409/404).
- [ ] `Excepciones/ConflictoExcepcion.cs`; `Respuestas/RespuestaCreacion<T>`.
- [ ] `Controllers/FacultadController.cs` (catch → 409).
- [ ] `Program.cs`: DI del par + `version = "v2"` + fallback `DB_CONNECTION`.
- **Verificar:** CRUD de `facultad` por `curl` (POST devuelve la fila; FK
  inexistente → 409). **Entregar en cuanto pase**, para que Valentina arranque
  `programa`.

## Fase 2 — `programa` (Valentina) — maestro central

- [ ] Procedimientos de `programa` (listar con `facultad_nombre`, incl. la
  columna **`num_acreditaciones`** de sólo lectura; **eliminar condicionado** a
  que no haya hijos activos).
- [ ] Modelo, peticiones, repo (sólo procs), servicio (regla §3.1 → 409), controller.
- [ ] DI en `Program.cs`.
- **Verificar:** `POST /api/programa` con `facultad` inexistente → 409; CRUD
  básico por `curl`.

## Fase 3 — Detalle de `programa` (Emmanuel, Valentina y Felipe, en paralelo)

- [ ] **Emmanuel:** procedimientos + capas de `pasantia` y `premio` (FK a
  `programa`).
- [ ] **Valentina:** procedimientos + capas de `acreditacion` (PK `resolucion`),
  `registro_calificado` (PK `codigo`), `activ_academica` (**FK opcional
  `disenio`** → `null`).
- [x] **Felipe:** procedimientos + capas de `docente_departamento` (PK compuesta,
  `docente` local con ID automático, fechas) y `alianza` (PK compuesta, fechas, `docente`
  opcional). Validar fechas coherentes → 400.
- [x] Catálogo auxiliar `docente`: ID interno incremental, documento separado,
  listado/formulario, selección por nombre en sus relaciones y retiro lógico.
- [x] **Verificar Felipe:** create/list/get/PUT/PATCH/DELETE para ambas
  relaciones en una BD temporal aislada; claves inmutables y nombres resueltos;
  PATCH vacío y fechas inválidas → 400; duplicado → 409; segundo retiro → 404.
- **Verificar:** cada tabla lista/crea/retira por `curl`, con los nombres de FK en
  el listado.

## Fase 4 — Tablas puente (Valentina y Felipe)

- [ ] **Valentina:** `enfoque_rc`, `aa_rc` (con `componente` y `semestre`).
- [x] **Felipe:** `programa_ac`, `programa_pe`, `programa_ci`, `an_programa`.
- [x] Procedimientos `sp_listar_`, `sp_consultar_`, `sp_asignar_`,
  `sp_retirar_` (JOIN de nombres dentro del procedimiento). Las cuatro puentes
  puras siguen **sin PUT/PATCH**.
- [x] Servicio: pareja duplicada → 409; FK inexistente → 409.
- **Verificar:** asignar → repetir (409) → retirar (200) → repetir retiro (404).
- [x] **Verificar Felipe:** ejecutar asignar → duplicar (409) → consultar →
  retirar → segundo retiro (404) para las cuatro puentes en una BD temporal
  aislada; validar también padres inexistentes → 409.

## Fase 5 — El disparador (Valentina)

- [ ] `trg_acreditacion_contador` `AFTER INSERT, UPDATE, DELETE` sobre
  `acreditacion`; recalcula `programa.num_acreditaciones`.
- [ ] Comprobar que escucha el **`UPDATE`** (retirar = `UPDATE activo = 0`).
- **Verificar:** asignar y **retirar** una acreditación cambia el contador; la API
  nunca lo escribe.

## Fase 6 — Frontend (los tres, sobre su parte)

- [ ] `models/catalogo.ts`: agregar las 15 tablas con sus columnas, marcando
  **FK** (→ desplegable), **fecha** (→ input de fecha) y **opcional** (→
  «(ninguna)»).
- [ ] `pages/catalogo`: pintar desplegables de FK (cargados en paralelo con
  `forkJoin`, sólo activos), enviar `null` en «(ninguna)», editar los atributos
  de `alianza`/`docente_departamento` con las claves bloqueadas y ocultar Editar
  en las cuatro puentes puras.
- [ ] `api.service.ts`: `asignar(tabla, pareja)`, `retirar(tabla, pk1, pk2)` y
  PUT/PATCH por pareja para las relaciones con atributos.
- [ ] `pages/programa-detalle` (Valentina): maestro-detalle de `programa`
  ([9_frontend.md](9_frontend.md) §8).
- [ ] Mostrar el **409** con su mensaje castellano, sin jerga.
- [ ] `dashboard` y `nav-bar`: crecen solos desde `TABLAS`.
- **Verificar:** desde el navegador se crea/edita/retira con desplegables llenos
  de nombres; el disparador cambia el número al asignar y al retirar.

## Fase 7 — Pruebas (los tres)

- [ ] Extender `pruebas/Programa.cs` con dobles de las tablas de la v2 (prueba de
  capas sin BD).
- [ ] Completar la colección de peticiones (`api_innovacion.http` o equivalente)
  con casos **válidos** y casos que **deben fallar**: 409 (FK inexistente, pareja
  duplicada, padre con hijos), 422 (body inválido) y 404.
- **Verificar:** `dotnet run --project pruebas` pasa; la colección se ejecuta de
  corrido.

## Fase 8 — Cierre e integración (integrador)

- [ ] Ejecutar el smoke test completo de [7_quickstart.md](7_quickstart.md).
- [ ] **Regresión de la v1**: repetir los criterios de
  [`../v1_producto_sqlserver/2_spec.md`](../v1_producto_sqlserver/2_spec.md) §5 —
  deben seguir en verde.
- [ ] Aplicar el **ajuste a la constitución** (Art. 2 → procs; Art. 3 → 409;
  Art. 8 → envoltura) — [2_spec.md](2_spec.md) §4.
- [ ] Actualizar `../0_mapa_versiones.md` (fila de la v2 → "En curso", con enlace
  a esta spec).
- [ ] Marcar [9_checklist.md](9_checklist.md) **a mano**, con todo en verde.
- [ ] Tag `v2` sobre el commit que cumple los 12 criterios.

---

## Resumen por integrante (alineado con `issues_v2.md`)

### Emmanuel — Issue 1
- [ ] Spec kit completo (docs 2–9).
- [ ] `facultad`, `pasantia`, `premio` (procs + capas + frontend).
- [ ] Criterios 2, 3, 5, 7, 10.
- [ ] Completar `GUIA_IA2_EMMANUEL.md` (prompt usado + registro de correcciones).

### Valentina — Issue 2
- [ ] `programa` (maestro-detalle) + `acreditacion`, `registro_calificado`,
  `activ_academica`, `enfoque_rc`, `aa_rc`.
- [ ] **El disparador** + comprobación desde la interfaz.
- [ ] Criterios 2, 3, 4, 5, 6, 7, 8, 9, 10, 11.
- [ ] Completar `GUIA_IA2_VALENTINA.md`.

### Felipe — Issue 3
- [x] `docente_departamento`, `alianza`, `programa_ac`, `programa_pe`,
  `programa_ci`, `an_programa`.
- [x] Validación de fechas coherentes.
- [ ] Criterios 2, 3, 5, 6, 7, 8, 10, 11.
- [x] Completar `GUIA_IA2_FELIPE.md`.

> Issue 3: `dotnet build` pasa sin advertencias; 96 pruebas de capas y el build
> Angular pasan; 46 respuestas HTTP de CRUD en BD temporal aislada y siete
> respuestas 409 por padres inexistentes pasan. La revisión visual del navegador
> queda como validación manual. El maestro-detalle de `programa` pertenece al
> módulo responsable y no está en la rama de Felipe.

> **Recordatorio de ramas:** nadie trabaja en `main`; cada quien en
> `rama-emmanuel-v2`, `rama-valentina-v2`, `felipe-correa`; todo entra por PR y
> sólo el encargado del main integra.
