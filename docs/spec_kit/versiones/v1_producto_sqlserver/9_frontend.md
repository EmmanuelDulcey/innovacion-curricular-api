# Especificación del Frontend — Versión 1: Angular (dashboard + CRUD)

> **Documento 9 de la v1.** Describe la interfaz de usuario: qué páginas
> existen, qué muestran, qué contratos consumen y cómo se ven los errores.
> Rige la constitución ([../../1_constitution.md](../../1_constitution.md)),
> se detalla en [3_plan.md](3_plan.md) §6 y consume exactamente los
> contratos de [6_contracts.md](6_contracts.md).

---

## 1. Propósito

El Frontend de la v1 es una **SPA en Angular** que le da cara a la API:
muestra un **dashboard** con las métricas de los 7 catálogos y permite
**crear, listar, editar y eliminar** registros de cada una de las 7 tablas
sin llave foránea desde el navegador.

- Vive en la carpeta `frontend/` y corre en el puerto **8037**.
- Consume la API en `http://localhost:8036` (contratos de `6_contracts.md`);
  la API habilitó CORS para el origen `http://localhost:8037`.
- **Sin login en v1**: todas las operaciones son públicas. Los roles y
  guardias llegan en v3.

## 2. Stack y configuración

| Elemento | Elección |
|---|---|
| Framework | Angular (TypeScript), componentes *standalone* |
| CLI | Angular CLI (`ng serve --host 0.0.0.0 --port 8037`) |
| HTTP | `HttpClient` con `apiUrl` desde `environments/environment.ts` |
| Estilos | CSS plano por componente + `styles.css` global; layout responsive |
| Pruebas | (Opcional en v1) consola del navegador contra la API levantada |

Configuración mínima:

```ts
// src/environments/environment.ts
export const environment = {
  apiUrl: 'http://localhost:8036'
};
```

## 3. Rutas y páginas

| Ruta | Página | Qué hace |
|---|---|---|
| `/` | Dashboard | Tarjetas con el total de registros activos de cada tabla + menú de navegación |
| `/tablas/:tabla` | Catálogo | Lista los registros activos de la tabla y ofrece Crear / Editar / Eliminar |
| `**` | No encontrada | Mensaje "página no encontrada" con enlace al dashboard |

Las 7 tablas disponibles (valores válidos de `:tabla`):

```text
area_conocimiento
universidad
aspecto_normativo
practica_estrategia
enfoque
car_innovacion
aliado
```

## 4. Modelos de datos (mapeo snake_case → camelCase)

La API responde en snake_case (`6_contracts.md`). El Frontend define una
interfaz en camelCase por tabla:

| Tabla | Interfaz TypeScript | Tipo de la llave |
|---|---|---|
| `area_conocimiento` | `id`, `granArea`, `area`, `disciplina`, `activo` | `number` (`id`) |
| `universidad` | `id`, `nombre`, `tipo`, `ciudad`, `activo` | `number` (`id`) |
| `aspecto_normativo` | `id`, `tipo`, `descripcion`, `fuente`, `activo` | `number` (`id`) |
| `practica_estrategia` | `id`, `tipo`, `nombre`, `descripcion`, `activo` | `number` (`id`) |
| `enfoque` | `id`, `nombre`, `descripcion`, `activo` | `number` (`id`) |
| `car_innovacion` | `id`, `nombre`, `descripcion`, `tipo`, `activo` | `number` (`id`) |
| `aliado` | `nit`, `razonSocial`, `nombreContacto`, `correo`, `telefono`, `ciudad`, `activo` | `number` (`nit`) |

Contratos auxiliares para todas las tablas:

```ts
export interface RespuestaLista<T> {
  tabla: string;
  limite: number;
  total: number;
  datos: T[];
}

export interface ApiError {
  estado: number;
  mensaje: string;
  detalle?: string;
  errores?: string[];
}
```

> Reglas de formulario (obligatoriedad, longitud máxima, formato de correo)
> = las de `6_contracts.md` §8. La llave primaria (`id`/`nit`) la asigna el
> cliente al crear; no se edita después.

## 5. Componentes

- **`nav-bar`**: marca del módulo, enlace "Dashboard" y un enlace por tabla
  (generado a partir de la lista de 7). Marca la página activa.
- **`mensaje-error`**: muestra `ApiError` de forma uniforme (banner general
  para 400/500 y lista por campo para 422).
- **`pages/dashboard`**: consulta las 7 tablas en paralelo
  (`Promise.all`) y pinta una tarjeta por tabla con su `total`; indica si
  la API no responde.
- **`pages/catalogo`**: página genérica de CRUD que recibe la tabla por
  ruta y usa `6_contracts.md` §8 para configurar columnas y campos.

## 6. Consumo de la API (servicio)

`api.service.ts` implementa un método por verbo y devuelve datos tipados:

| Método | Llamada HTTP | Éxito | Uso |
|---|---|---|---|
| `listar(tabla, limite = 1000)` | `GET /api/{tabla}?limite={limite}` | 200 → `RespuestaLista<T>`; 204 → lista vacía | Listado y dashboard |
| `obtener(tabla, id)` | `GET /api/{tabla}/{id}` | 200 → `T` | Detalle/fila a editar |
| `crear(tabla, body)` | `POST /api/{tabla}` | 200 → `{estado, mensaje}` | Nuevo registro |
| `reemplazar(tabla, id, body)` | `PUT /api/{tabla}/{id}` | 200 → `{estado, mensaje, filasAfectadas}` | Edición (formulario completo) |
| `actualizar(tabla, id, patch)` | `PATCH /api/{tabla}/{id}` | 200 → `{estado, mensaje, filasAfectadas}` | Cambios puntuales |
| `eliminar(tabla, id)` | `DELETE /api/{tabla}/{id}` | 200 → `{estado, mensaje, filasEliminadas}` | Borrado lógico |

## 7. Manejo de errores (RF10)

| Situación | HTTP | Comportamiento del Frontend |
|---|---|---|
| Campos inválidos (crear/editar) | 422 | El formulario marca cada campo con el mensaje de `errores[]` y muestra el banner genérico con `mensaje` |
| Registro inexistente | 404 | Mensaje "registro no encontrado" en la página, sin romper el resto de la vista |
| Parámetros inválidos (ej. `limite <= 0`) | 400 | Banner con `detalle` |
| Tabla sin registros | 204 | Estado vacío en el listado ("no hay registros") |
| Error del servidor / SQL | 500 | Banner "error del servidor" con `detalle` y log en consola |
| API no disponible | conexión | Mensaje de conexión en el dashboard y en los catálogos |

El listado siempre filtra lo que muestra la API (la API ya excluye
`activo = false`); el Frontend **no reintegra** registros eliminados.

## 8. Dashboard (RF8) — detalle

- Tarjetas en cuadrícula ([responsive → 1 col en móvil, 4+ en escritorio]).
- Cada tarjeta: nombre legible de la tabla, total de registros activos
  (campo `total` de `GET /api/{tabla}`), enlace a `/tablas/{tabla}`.
- Encabezado con el nombre del módulo y el estado de conexión con la API.
- Valores esperados al primer arranque: `area_conocimiento` **218**,
  `universidad` **6**, resto en **0**.

## 9. Página catálogo (RF9) — detalle

- Cabecera con el nombre de la tabla y botón **+ Crear**.
- Tabla de datos: una columna por campo de la entidad (`activo` siempre
  visible como estado), y acciones por fila: **Editar**, **Eliminar**.
  En `aliado`, los encabezados usan el nombre legible de cada campo
  (`razón social`, `nombre de contacto`, etc.).
- **Crear/Editar**: formulario con los campos de la entidad; la llave
  (`id`/`nit`) es obligatoria solo al crear y se muestra de solo lectura al
  editar. Se envía `PUT` al editar (cuerpo completo) cumpliendo las
  obligatoriedad de `6_contracts.md` §8.
- **Eliminar**: confirmación ("¿Eliminar este registro?") y `DELETE`
  (borrado lógico); la fila desaparece del listado al refrescar.
- Filtro por texto por encima del listado (client-side sobre los datos
  cargados) y control de `limite`.
- Después de una operación exitosa, se refresca el listado.

## 10. Diseño y UX

- Layout con `nav-bar` fijo y contenido responsive.
- Convención de estado: `activo = true` (verde), `activo = false` (gris/
  inactivo) — aunque la API no devuelve inactivos en listados normales.
- Textos de la interfaz en español (definidos arriba), mientras que los
  nombres de campo de la API conservan el snake_case de los contratos.

## 11. Criterios de aceptación del Frontend

Corresponden a los criterios 6–9 de `2_spec.md`:

1. El dashboard de `http://localhost:8037` muestra las 7 tarjetas con sus
   totales (218, 6 y ceros).
2. Desde el navegador se crea, edita y elimina un `aliado`, y el cambio
   persiste al recargar.
3. Un `POST`/`PUT` con campos inválidos muestra los errores 422 por campo
   sin romper la interfaz.
4. Consultar un registro inexistente muestra "registro no encontrado" sin
   errores en la consola.