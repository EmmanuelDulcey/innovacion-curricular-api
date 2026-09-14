# Especificación — Versión 1: API + Frontend Angular (SQL Server)

> **Versión 1** del desarrollo incremental ([mapa de versiones](../0_mapa_versiones.md)).  
> Rige la constitución del proyecto: [../../1_constitution.md](../../1_constitution.md).  
> En v1 el sistema completo ES esto: la **API REST** (7 tablas sin FK sobre SQL Server) + un **frontend Angular** que consume esa API con dashboard y CRUD visual. La BD `innovacion_curricular` sí se crea completa desde el inicio — es infraestructura dada; lo que crece por versiones es la API y el Frontend.

| Documento de esta versión | Contenido |
|---|---|
| **2_spec.md** (este) | QUÉ construir en v1 (API + Frontend) y sus criterios de aceptación |
| [3_plan.md](3_plan.md) | CÓMO: stack, estructura y diseño de las capas (API y Frontend) |
| [4_research.md](4_research.md) | Decisiones y alternativas *(lectura opcional)* |
| [5_data_model.md](5_data_model.md) | La BD completa (dada) y las 7 tablas sin FK |
| [6_contracts.md](6_contracts.md) | Los endpoints con formatos exactos (consumidos por el Frontend) |
| [7_quickstart.md](7_quickstart.md) | Arranque y smoke test (API + Frontend) |
| [8_tasks.md](8_tasks.md) | Orden de construcción por fases verificables (API + Frontend) |
| [9_frontend.md](9_frontend.md) | La interfaz Angular: páginas, rutas, componentes y consumo de la API |

---

## 1. Propósito de la v1

Construir la **primera rebanada vertical** del sistema del módulo Innovación Curricular: la **API REST** en **C# / ASP.NET Core** — el CRUD completo de las **7 tablas sin claves foráneas** contra **SQL Server**, con la **arquitectura en capas completa desde el primer día** (controlador → servicio → repositorio, comunicados por **interfaces de C#**) — y un **frontend Angular** que la consume: un **dashboard** con las métricas de los catálogos y el **CRUD visual** de cada tabla.

Tablas incluidas en v1:
- `area_conocimiento` (con catálogo del Excel cargado: 218 registros).  
- `universidad` (6 registros del Excel).  
- `aspecto_normativo`.  
- `practica_estrategia`.  
- `enfoque`.  
- `car_innovacion`.  
- `aliado`.

La v1 es pequeña a propósito: su valor está en dejar el **esqueleto arquitectónico correcto** —en la API y en el Frontend— sobre el que las versiones siguientes agregan tablas con FK (v2), seguridad y roles (v3) y consultas/gráficos/publicación (v4).

---

## 2. Alcance

**Incluye:**
- CRUD completo de las 7 tablas sin FK.  
- Modelos entidad (`AreaConocimiento`, `Universidad`, etc.) con propiedades tipadas.  
- Peticiones por verbo (`Crear`, `Reemplazo`, `Actualizar`) con validaciones (`[Required]`, `[StringLength]`, `[Range]`).  
- Capas con interfaces: `IRepositorioEntidad` implementada por `RepositorioEntidadSqlServer` (Dapper: SQL a mano).  
- Configuración por `appsettings.json`, sobrescribible por variables de entorno (`ConnectionStrings__SqlServer`).  
- **Un solo comando** (Artículo 4): `docker-compose.yml` con SQL Server + inicializador + API + Frontend, de modo que `docker compose up -d --build` deja todo funcionando.  
- Endpoint `/` de diagnóstico y documentación interactiva Swagger en `/swagger`.  
- **Frontend Angular** (desde la v1, ver [9_frontend.md](9_frontend.md)):
  dashboard con métricas de los catálogos, navegación de acceso a cada tabla y CRUD visual (listar, crear, editar, eliminar) de las 7 tablas, consumiendo los contratos de [6_contracts.md](6_contracts.md).

**No incluye (y es deliberado — ver [mapa de versiones](../0_mapa_versiones.md)):**
- Endpoints para tablas con FK (v2).  
- Gestión de usuarios y roles (v3). En v1 el Frontend no tiene login: todas las operaciones son públicas.  
- Gráficos avanzados, páginas corporativas, PWA y publicación (v4).  
- ORM de entidades (Entity Framework) — se usa Dapper.  

---

## 3. Requisitos funcionales

La v1 usa los cinco verbos HTTP (GET, POST, PUT, PATCH, DELETE) y las tres vías de envío de datos: parámetro de ruta, query string y body JSON.

### RF1 — Listar registros
`GET /api/{tabla}` → 200 con envoltura `{tabla, limite, total, datos:[…]}`.  
- Query param opcional `limite` (entero > 0, por defecto 1000).  
- Tabla vacía → 204 sin cuerpo.

### RF2 — Obtener por id
`GET /api/{tabla}/{id}` → 200 con el registro; inexistente → 404.

### RF3 — Crear registro
`POST /api/{tabla}` con body validado por la petición `CrearEntidad`.  
Éxito → 200 `{estado, mensaje}`; body inválido → 422 con lista de errores; PK duplicada → 500 con detalle del motor.

### RF4 — Reemplazar registro (PUT)
`PUT /api/{tabla}/{id}` con body completo.  
Todos los campos obligatorios → omitir uno es 422.  
Devuelve `filasAfectadas`; inexistente → 404.

### RF5 — Actualizar parcialmente (PATCH)
`PATCH /api/{tabla}/{id}` con body parcial.  
Solo se modifican los enviados.  
Devuelve `filasAfectadas`; inexistente → 404; body vacío → 400.

### RF6 — Eliminar registro (DELETE)
`DELETE /api/{tabla}/{id}` → borrado lógico (`activo = 0`).  
Devuelve `filasEliminadas`; inexistente → 404.

### RF7 — Diagnóstico
`GET /` → JSON con mensaje, versión (`"v1"`) y la ruta de los contratos.

### RF8 — Dashboard (Frontend)
`/` en el Frontend (puerto 8037) muestra una tarjeta por cada una de las 7
tablas con el **total de registros activos** (obtenido de
`GET /api/{tabla}`), un menú de navegación y el estado de conexión con la
API.

### RF9 — CRUD visual por tabla (Frontend)
Ruta `/tablas/{tabla}`: tabla con los registros activos, búsqueda/filtro por
texto, y acciones **Crear** (formulario), **Editar** (mismo formulario) y
**Eliminar** (con confirmación). Los campos del formulario siguen las reglas
de [6_contracts.md](6_contracts.md) §8 (obligatoriedad y longitud).

### RF10 — Consumo de contratos y errores (Frontend)
El Frontend consume exactamente los contratos de [6_contracts.md](6_contracts.md):
listado con envoltura `{tabla, limite, total, datos}`, lectura individual,
`POST`/`PUT`/`PATCH`/`DELETE`. Muestra los errores 422 como mensajes legibles
por campo, 404 como "registro no encontrado" y 500 como error del servidor.

---

## 4. Requisitos no funcionales

- **Capas estrictas (API):** controlador no toca SQL; servicio no conoce HTTP ni motor; repositorio no conoce HTTP.  
- **SQL a la vista:** escrito a mano y parametrizado (`@parametro`).  
- **Asíncrona:** todo acceso a datos con `async/await`.  
- **Errores uniformes:** `{estado, mensaje, detalle}` (+ `errores:[…]` en 422).  
- **Sin anticipación:** nada de FK, JWT o login en v1.  
- **Frontend:** TypeScript estricto; componentes Angular standalone; la API base vive en `environments/environment.ts` (por defecto `http://localhost:8036`); CORS habilitado en la API para `http://localhost:8037`; sin secretos en el código del cliente.  

---

## 5. Criterios de aceptación

**API (backend):**
1. `docker compose up -d --build` deja corriendo SQL Server (inicializado con el script provisto: 25 tablas), la API y el Frontend; `GET http://localhost:8036/` responde el JSON de diagnóstico.  
2. `GET /api/area_conocimiento` devuelve los 218 registros del Excel con `{tabla:"area_conocimiento", total:218, datos:[…]}`; `GET /api/universidad` devuelve los 6 registros iniciales.  
3. CRUD completo probado en al menos una tabla (`aliado`): `POST` crea → `PUT` reemplaza → `PATCH` actualiza parcialmente → `GET` confirma → `DELETE` marca inactivo → un segundo `DELETE` responde 404.  
4. Validación de peticiones: `POST` con datos inválidos (ej. `nombre` vacío, `stock:-5`) → 422 con `errores:[…]`.  
5. Prueba de capas: ejecución con repositorio falso en memoria pasa todas las verificaciones sin SQL Server.

**Frontend (Angular):**
6. `http://localhost:8037` carga el **dashboard** con una tarjeta por cada tabla; la de `area_conocimiento` muestra **218** registros y la de `universidad` **6**.  
7. Desde el Frontend se puede **crear**, **editar** y **eliminar** (borrado lógico) un registro de `aliado`; al recargar la página el cambio persiste.  
8. Cuando el Frontend envía un `POST`/`PUT` con campos inválidos, la API devuelve 422 y el Frontend muestra un mensaje legible por campo sin romper la interfaz.  
9. Cuando se consulta un registro inexistente, el Frontend muestra un mensaje de "registro no encontrado" (404) sin errores en consola.

**Cierre:**
10. Tag `v1` puesto en `main` cuando todos los criterios están en verde.

---
