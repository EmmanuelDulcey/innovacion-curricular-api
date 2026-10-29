# Checklist de aceptación — Versión 2

> **Documento 9 (checklist) de la v2.** Marcar **a mano**. Todo en verde =
> la versión está lista para el tag `v2`.
> Base: [2_spec.md](2_spec.md) §8 · operación:
> [7_quickstart.md](7_quickstart.md) · tareas: [8_tasks.md](8_tasks.md).

---

## A. Los 12 criterios de aceptación

### 1. El proyecto levanta completo
- [ ] `docker compose down -v && docker compose up -d --build` levanta **todo**
  (BD + `sqlserver-init` + API + Frontend) sin errores.
- [ ] `docker compose ps` muestra todos los servicios activos.
- [ ] `GET http://localhost:8036/` responde y trae `"version": "v2"`.

### 2. CRUD completo por procedimientos
- [ ] Las tablas con PK simple hacen Crear / Listar / Consultar / Reemplazar /
  Actualizar / Eliminar; las de PK compuesta hacen Listar / Consultar pareja /
  Asignar / Retirar, todo **a través de procedimientos almacenados**.
- [ ] **Cero SQL** en el C# (los repositorios sólo usan
  `CommandType.StoredProcedure`).
- [ ] El `POST` **devuelve la fila guardada** (`datos`).

### 3. Las claves foráneas son desplegables
- [ ] Todo campo FK se ofrece como **desplegable** con el **nombre** de la
  maestra; se envía su **clave**.
- [ ] Sólo se ofrecen maestras **activas**.
- [ ] El listado muestra el **nombre** de la FK (`programa_nombre`, …), no la
  clave cruda.

### 4. Pantalla maestro-detalle de `programa`
- [ ] Desde `/programas/:id` se agrega detalle (hijas de `programa`) **sin salir
  de la pantalla**.

### 5. Borrado lógico
- [ ] Al retirar, el registro **desaparece del listado** y **sigue en la base**
  con `activo = 0`.
- [ ] Un **segundo** `DELETE` (o de un inexistente) → **404**.

### 6. Relaciones compuestas: claves estables y acciones según sus datos
- [ ] En `docente_departamento` y `alianza`, Editar permite cambiar atributos,
  conserva las claves compuestas y ofrece PUT/PATCH por la pareja original.
- [ ] Las cuatro puentes puras de Felipe (`programa_ac`, `programa_pe`,
  `programa_ci`, `an_programa`) no tienen Editar; sólo **Asignar** y **Retirar**.

### 7. FK inexistente → 409
- [ ] `POST` con una FK que no existe → **409** (no 500), con mensaje en
  castellano.

### 8. Pareja duplicada en puente → 409
- [ ] Asignar una pareja ya existente en una relación compuesta → **409**.

### 9. El disparador se ve desde la interfaz
- [ ] Al **asignar** una acreditación sube `programa.num_acreditaciones`.
- [ ] Al **retirar** (UPDATE lógico) **baja** el contador — prueba de que el
  disparador **no es sordo al UPDATE**.

### 10. Interfaz en castellano, sin jerga
- [ ] Textos, mensajes y errores en **castellano**; no aparecen `id`, `PK`, `FK`.
- [ ] Campo opcional con opción **«(ninguna)»** que envía **`null`**.

### 11. La v1 sigue funcionando
- [ ] La regresión de la v1 pasa (sección D).

### 12. Cierre de la versión
- [ ] Todo lo anterior en verde, en el commit que recibe el **tag `v2`**.

---

## B. Reglas de la versión

- [ ] La regla del **maestro con hijos activos** bloquea el borrado → **409**
  ([2_spec.md](2_spec.md) §3.1).
- [ ] Las 15 tablas tienen `activo`; los listados filtran `activo = 1`.
- [ ] `num_acreditaciones` **sólo** lo escribe el disparador (la API nunca).
- [x] Los IDs internos se generan en SQL Server (secuencia por defecto en v1 y `IDENTITY` en el catálogo local docente); el documento de identidad y las claves naturales no se usan como PK.
- [ ] Ajuste a la constitución aplicado (Art. 2 procs / Art. 3 409 / Art. 8
  envoltura) ([2_spec.md](2_spec.md) §4).

---

## C. Códigos HTTP por operación

- [ ] GET lista → **200** (con datos) / **204** (vacío) / **400** (`limite <= 0`).
- [ ] GET por llave → **200** / **404**.
- [ ] POST → **200** / **409** (FK o duplicado) / **422** (body).
- [ ] PUT → **200** / **404** / **409** / **422**.
- [ ] PATCH → **200** / **400** (sin campos) / **404** / **409**.
- [ ] DELETE → **200** / **404** / **409** (padre con hijos).
- [ ] POST relación compuesta (asignar) → **200** / **409** / **422**.
- [ ] DELETE relación compuesta (retirar) → **200** / **404**.
- [ ] PUT/PATCH por pareja para `docente_departamento` y `alianza` → **200**;
  PATCH vacío/fechas incoherentes → **400**; pareja inexistente → **404**.

---

## D. Regresión de la v1 (no romper lo que ya funcionaba)

Repetir los criterios de
[`../v1_producto_sqlserver/2_spec.md`](../v1_producto_sqlserver/2_spec.md) §5:

- [ ] Las **7 tablas de la v1** siguen con su CRUD completo.
- [ ] El borrado lógico de la v1 sigue igual.
- [ ] La envoltura y los mensajes de la v1 **no cambiaron**.
- [ ] La colección de pruebas de la v1 sigue en verde.
- [ ] La interfaz de la v1 sigue operando (dashboard + catálogo).

---

## E. Entrega y documentación

- [ ] Guías `GUIA_IA2_*.md` completas (prompt usado + registro de correcciones)
  por cada integrante.
- [ ] `../0_mapa_versiones.md` actualizado (fila de la v2 → "Completada", con
  enlace a esta spec).
- [ ] Commits en castellano, pequeños y descriptivos, en la rama de cada quien;
  integración por PR (nadie en `main`).
- [ ] **Tag `v2`** sobre el commit que cumple el checklist.

> **Dependencias externas**: el smoke test de `pasantia`/`premio` depende de
> `GET /api/programa`; si aún no existe, usar una fila `programa` insertada por
> SQL como *fixture* temporal.
