# Especificación del Frontend — Versión 2: 15 tablas, claves foráneas y paralelos

> **Documento 9 de la v2.** Describe la interfaz de usuario de la versión.
> Rige la constitución ([../../1_constitution.md](../../1_constitution.md)),
> se detalla en [3_plan.md](3_plan.md) §7 y consume exactamente los contratos de
> [6_contracts.md](6_contracts.md). Es un **delta** de
> [`../v1_producto_sqlserver/9_frontend.md`](../v1_producto_sqlserver/9_frontend.md):
> todo lo que la v1 especifica y no se contradice aquí sigue vigente.

---

## 1. Propósito

El Frontend de la v2 **reutiliza** el de la v1 (SPA Angular con dashboard y
catálogo genérico) y lo extiende para:

1. Operar las **15 tablas con clave foránea** además de los 7 catálogos de la v1, con un catálogo auxiliar de docentes para resolver sus relaciones.
2. **Elegir las FK con desplegables** (nunca escribir una clave a mano), sin
   mostrar la jerga `id`/clave al usuario.
3. Manejar **fechas** con un control de fecha real, respetando el tipo
   (`DATE` → fecha; los `VARCHAR(45)` de fecha se mantienen como texto).
4. Editar los atributos de `docente_departamento` y `alianza` sin permitir
   cambiar su pareja de claves; las cuatro tablas puente puras de Felipe sólo
   permiten **asignar / retirar**.
5. Ofrecer la pantalla **maestro-detalle de `programa`** para cargar sus tablas
   hijas sin salir de la vista.
6. Mostrar el **409** con un mensaje castellano claro.

Sin login en v2 (llega en v3). Corre en `frontend/`, puerto **8037**, consume la
API en `http://localhost:8036`.

## 2. Stack y configuración

Sin cambios respecto de la v1 (Angular, componentes *standalone*, `HttpClient`,
`environment.apiUrl = 'http://localhost:8036'`, CSS plano). Lo único nuevo es el
**registro de columnas por tabla** en `models/catalogo.ts`, que ahora describe
las 15 tablas adicionales.

## 3. Rutas y páginas

| Ruta | Página | Qué hace |
|---|---|---|
| `/` | Dashboard | Una tarjeta por tabla operable (7 de v1 + 15 de v2) |
| `/tablas/:tabla` | Catálogo | Lista los activos; Crear / Editar / Eliminar (o Asignar / Retirar si es puente) |
| `/programas/:id` | **Maestro-detalle** | Programa + sus tablas hijas sin salir de la pantalla |
| `**` | No encontrada | Enlace al dashboard |

Las 15 tablas nuevas (valores válidos de `:tabla`):

```text
facultad            programa            acreditacion        registro_calificado
activ_academica     pasantia            premio              docente_departamento
alianza             programa_ac         programa_pe         programa_ci
an_programa         enfoque_rc          aa_rc
docente (catálogo auxiliar)
```

## 4. Modelos de datos

Igual que la v1: la API responde en `snake_case` y el Frontend mapea a
`camelCase`. En la v2 cada fila trae además el **nombre de la FK** (`programa_nombre`,
`universidad_nombre`, …), que es lo que se muestra en el listado:

Los campos `id` que representan claves internas se ocultan al crear y se ven en
los listados después de guardar. En **Áreas del programa**, cada opción se lee
como `área — disciplina` para evitar asignaciones ambiguas. El documento de una
persona y las claves de negocio (como el NIT del aliado) no se sustituyen por IDs.

| Tabla | Interfaz (camelCase) | Tipo de llave |
|---|---|---|
| `docente` | `id`, `documento`, `nombre`, `activo` | `number` (`id`, automático); `documento` es dato único, no PK |
| `facultad` | `id`, `nombre`, `tipo`, `fechaFun`, `universidad`, `universidadNombre`, `activo` | `number` (`id`) |
| `programa` | `id`, `nombre`, `tipo`, `nivel`, `fechaCreacion`, `fechaCierre`, `numeroCohortes`, `cantGraduados`, `fechaActualizacion`, `ciudad`, `facultad`, `facultadNombre`, `numAcreditaciones`, `activo` | `number` (`id`) |
| `pasantia` | `id`, `nombre`, `pais`, `empresa`, `descripcion`, `programa`, `programaNombre`, `activo` | `number` (`id`) |
| `premio` | `id`, `nombre`, `descripcion`, `fecha`, `entidadOtorgante`, `pais`, `programa`, `programaNombre`, `activo` | `number` (`id`) |
| `acreditacion` | `resolucion`, `tipo`, `calificacion`, `fechaInicio`, `fechaFin`, `programa`, `programaNombre`, `activo` | `number` (`resolucion`) |
| `registro_calificado` | `codigo`, `cantCreditos`, `horaAcom`, `horaInd`, `fechaInicio`, `fechaFin`, `duracionAnios`, `duracionSemestres`, `tipoTitulacion`, `programa`, `programaNombre`, `activo` | `number` (`codigo`) |
| `activ_academica` | `id`, `nombre`, … , `disenio`, `disenioNombre`, `programa`, `programaNombre`, `activo` | `number` (`id`) |
| `docente_departamento` | `docente`, `departamento`, `departamentoNombre`, `dedicacion`, `modalidad`, `fechaIngreso`, `fechaSalida`, `activo` | **compuesta** |
| `alianza` | `aliado`, `aliadoNombre`, `departamento`, `departamentoNombre`, `docente`, `fechaInicio`, `fechaFin`, `activo` | **compuesta** |
| `programa_ac` | `programa`, `programaNombre`, `areaConocimiento`, `areaConocimientoNombre`, `activo` | **compuesta** |
| `programa_pe` | `programa`, `programaNombre`, `practicaEstrategia`, `practicaEstrategiaNombre`, `activo` | **compuesta** |
| `programa_ci` | `programa`, `programaNombre`, `carInnovacion`, `carInnovacionNombre`, `activo` | **compuesta** |
| `an_programa` | `aspectoNormativo`, `aspectoNormativoNombre`, `programa`, `programaNombre`, `activo` | **compuesta** |
| `enfoque_rc` | `enfoque`, `enfoqueNombre`, `registroCalificado`, `registroCalificadoNombre`, `activo` | **compuesta** |
| `aa_rc` | `activAcademicasIdcurso`, `registroCalificadoCodigo`, `componente`, `semestre`, `activo` | **compuesta** |

Contratos auxiliares (los mismos de la v1, más el de creación de la v2):

```ts
export interface RespuestaCreacion<T> {
  estado: number;
  mensaje: string;
  datos: T;
}
```

## 5. Componentes

- **`nav-bar`**: crece solo — recorre la lista de tablas operables (v1 + v2).
- **`mensaje-error`**: se reutiliza; ahora también pinta el **409**.
- **`pages/dashboard`**: `Promise.all` sobre **22** tablas (7 + 15).
- **`pages/catalogo`**: página genérica; en la v2 la configuración por tabla
  incluye **metadatos de columna**: si es FK (desplegable), si es fecha
  (control de fecha), si es opcional (opción «(ninguna)») y si el ID lo genera
  SQL Server (no se pide al crear).
- **Navegación:** cambiar de sección restaura el scroll arriba para que cada
  listado empiece desde su encabezado, aunque se llegue desde abajo de otra página.
- **`pages/programa-detalle`**: pantalla nueva de maestro-detalle (§8).

## 6. Servicio (`api.service.ts`)

Se conservan los métodos de la v1 y se agregan los de las parejas:

| Método | Llamada | Éxito | Uso |
|---|---|---|---|
| `listar(tabla, limite)` | `GET /api/{tabla}` | 200 → `RespuestaLista<T>`; 204 → lista vacía | Listado y dashboard |
| `obtener(tabla, id)` | `GET /api/{tabla}/{id}` | 200 → `T` | Fila a editar |
| `crear(tabla, body)` | `POST /api/{tabla}` | 200 → `RespuestaCreacion<T>` | Nuevo registro (ahora devuelve la fila) |
| `reemplazar(tabla, id, body)` | `PUT /api/{tabla}/{id}` | 200 → `{estado, mensaje, filasAfectadas}` | Edición (formulario completo) |
| `actualizar(tabla, id, patch)` | `PATCH /api/{tabla}/{id}` | 200 → `{estado, mensaje, filasAfectadas}` | Cambios puntuales |
| `eliminar(tabla, id)` | `DELETE /api/{tabla}/{id}` | 200 → `{estado, mensaje, filasEliminadas}` | Borrado lógico |
| **`asignar(puente, pareja)`** | `POST /api/{puente}` | 200 → `RespuestaCreacion<T>` | Asignar pareja |
| **`retirar(puente, pk1, pk2)`** | `DELETE /api/{puente}/{pk1}/{pk2}` | 200 → `{estado, mensaje, filasEliminadas}` | Retirar pareja |

## 7. Manejo de errores

Igual que en la v1, más la fila nueva:

| Situación | HTTP | Comportamiento del Frontend |
|---|---|---|
| Campos inválidos | 422 | Pinta cada mensaje de `errores[]` en su campo |
| Registro inexistente | 404 | "registro no encontrado" |
| Parámetros inválidos | 400 | Banner con `detalle` |
| Tabla vacía | 204 | Estado vacío "no hay registros" |
| **Conflicto (FK inexistente, pareja duplicada, padre con hijos)** | **409** | **Banner con `detalle`** ("La universidad indicada no existe…", "Esa pareja ya está registrada", "…tiene registros asociados activos…") |
| Error del servidor / SQL | 500 | Banner "error del servidor" + log |
| API no disponible | conexión | Mensaje de conexión en dashboard y catálogos |

## 8. Desplegables de FK (lo central de la v2)

- Cada campo FK se pinta como un **`<select>`** cuyas opciones se cargan con el
  `GET /api/{tablaMaestra}` correspondiente (utilizando `listar`).
- Para `programa`, los formularios de Felipe usan temporalmente la ruta de
  solo lectura `GET /api/opciones/programa`. Mantiene la selección funcional
  sin asumir el CRUD de `programa` de otro módulo. Un fallo al cargar se
  muestra junto al desplegable; una lista vacía se distingue del fallo.
- **Se muestra el nombre** de la maestra y **se envía su clave** en el body.
- Si una pantalla tiene **varias FK** (ej. `alianza`), las opciones se cargan
  **en paralelo** con `forkJoin` para no encadenar peticiones.
- Sólo se ofrecen **maestras activas** (la API ya excluye inactivas).
- **Campo opcional** (`activ_academica.disenio`): la primera opción es
  **«(ninguna)»** con valor `null`; el Frontend envía `null`, nunca `""`.
- **`docente`** sí es desplegable: se crea primero desde el catálogo auxiliar,
  SQL Server genera su ID interno y el documento de identidad se conserva como
  dato único separado. En `docente_departamento` y `alianza` se elige al docente
  por nombre; el formulario envía el ID interno ([4_research.md](4_research.md)
  D6).
- **Fechas**: los campos `DATE` usan `<input type="date">`; los `VARCHAR(45)` de
  fecha (ej. `programa.fecha_creacion`) se mantienen como texto.

## 9. Página catálogo (delta)

- En las tablas normales: Crear / Editar / Eliminar como en la v1, con los
  desplegables de §8.
- En `docente_departamento` y `alianza`: **Editar** abre un formulario con sus
  atributos. Las dos claves que identifican la relación quedan bloqueadas; al
  guardar se envía PUT por la pareja original.
- En las cuatro **tablas puente puras** de Felipe (`programa_ac`, `programa_pe`,
  `programa_ci`, `an_programa`) no hay botón Editar: se puede **Asignar** una
  pareja y **Retirarla**. Cada selector muestra el nombre de su maestra.
- Textos en castellano, sin jerga técnica (`id`, `PK`, `FK`).

## 10. Pantalla maestro-detalle de `programa`

- Ruta `/programas/:id`: muestra los datos del **programa** (maestro) y, debajo,
  las tablas **hijas** (`pasantia`, `premio`, `acreditacion`,
  `registro_calificado`, `activ_academica`, `programa_ac`, `programa_pe`,
  `programa_ci`, `an_programa`).
- Permite **agregar filas de detalle sin salir de la pantalla**. Como cada
  detalle es una tabla distinta, «un solo envío» significa **una petición por
  tipo de detalle** ([4_research.md](4_research.md) D11); documentarlo y
  confirmarlo con la profesora.
- Muestra el **contador `num_acreditaciones`** del programa, que cambia solo
  cuando se asigna o retira una acreditación (el disparador).

## 11. Diseño y UX

Igual que la v1 (layout `nav-bar` fijo, responsive, estado `activo` en
verde/gris), con el agregado de:

- Los **409** salen como banner claro y accionable (no como error genérico).
- En tablas con muchas columnas, el listado es desplazable horizontalmente.
- Los nombres de las FK se muestran legibles; las claves no se muestran al
  usuario.

## 12. Criterios de aceptación del Frontend

1. El dashboard muestra las tarjetas de las 22 tablas originales más el catálogo auxiliar de docentes (23 en total) con sus totales.
2. Desde el navegador se crea/edita/retira un `facultad` y un `pasantia`, y el
   cambio persiste al recargar.
3. Los campos FK salen como **desplegables con nombres** (no texto libre).
4. Las relaciones `docente_departamento` y `alianza` se editan sin cambiar sus
   claves; las cuatro puentes puras solo ofrecen asignar/retirar.
5. La pantalla **maestro-detalle de `programa`** permite agregar detalle sin
   salir.
6. Las cuatro **puentes puras** sólo tienen Asignar / Retirar (sin Editar).
7. Un `POST` inválido muestra los errores 422 por campo; un conflicto muestra el
   **409** con su mensaje; no rompe la interfaz.
8. El contador `num_acreditaciones` cambia en la interfaz al asignar y al
   **retirar** una acreditación.

> Corresponden a los criterios 2, 3, 4, 5, 6, 7, 9 y 10 de
> [2_spec.md](2_spec.md) §8.
