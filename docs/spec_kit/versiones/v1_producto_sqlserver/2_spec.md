# Especificación — Versión 1 del proyecto: Innovación Curricular + SQL Server

> **Versión 1** del desarrollo incremental ([mapa de versiones](../0_mapa_versiones.md)).  
> Rige la constitución del proyecto: [../../1_constitution.md](../../1_constitution.md).  
> En v1 el sistema completo ES esto: **no existe frontend, y la API solo conoce las 7 tablas sin claves foráneas y un motor (SQL Server).**  
> La BD `innovacion_curricular` sí se crea completa desde el inicio — es infraestructura dada, ver [5_data_model.md]; lo que crece por versiones es la API.

| Documento de esta versión | Contenido |
|---|---|
| **2_spec.md** (este) | QUÉ construir en v1 y sus criterios de aceptación |
| [3_plan.md](3_plan.md) | CÓMO: stack, estructura y diseño de las capas |
| [4_research.md](4_research.md) | Decisiones y alternativas *(lectura opcional)* |
| [5_data_model.md](5_data_model.md) | La BD completa (dada) y las 7 tablas sin FK |
| [6_contracts.md](6_contracts.md) | Los endpoints con formatos exactos |
| [7_quickstart.md](7_quickstart.md) | Arranque y smoke test |
| [8_tasks.md](8_tasks.md) | Orden de construcción por fases verificables |

---

## 1. Propósito de la v1

Construir la **primera rebanada vertical** de la API del módulo Innovación Curricular en **C# / ASP.NET Core**: el CRUD completo de las **7 tablas sin claves foráneas** contra **SQL Server**, con la **arquitectura en capas completa desde el primer día**: controlador → servicio → repositorio, comunicados por **interfaces de C#**.

Tablas incluidas en v1:
- `area_conocimiento` (con catálogo del Excel cargado: 218 registros).  
- `universidad` (6 registros del Excel).  
- `aspecto_normativo`.  
- `practica_estrategia`.  
- `enfoque`.  
- `car_innovacion`.  
- `aliado`.

La v1 es pequeña a propósito: su valor está en dejar el **esqueleto arquitectónico correcto** sobre el que las versiones siguientes agregan tablas con FK (v2), seguridad y roles (v3), y consultas/dashboard/publicación (v4).

---

## 2. Alcance

**Incluye:**
- CRUD completo de las 7 tablas sin FK.  
- Modelos entidad (`AreaConocimiento`, `Universidad`, etc.) con propiedades tipadas.  
- Peticiones por verbo (`Crear`, `Reemplazo`, `Actualizar`) con validaciones (`[Required]`, `[StringLength]`, `[Range]`).  
- Capas con interfaces: `IRepositorioEntidad` implementada por `RepositorioEntidadSqlServer` (Dapper: SQL a mano).  
- Configuración por `appsettings.json`, sobrescribible por variables de entorno (`ConnectionStrings__SqlServer`).  
- **Un solo comando** (Artículo 4): `docker-compose.yml` con SQL Server + inicializador + API, de modo que `docker compose up -d --build` deja todo funcionando.  
- Endpoint `/` de diagnóstico y documentación interactiva Swagger en `/swagger`.

**No incluye (y es deliberado — ver [mapa de versiones](../0_mapa_versiones.md)):**
- Ningún frontend (se aborda en v4).  
- Endpoints para tablas con FK (v2).  
- Gestión de usuarios y roles (v3).  
- Consultas multitabla, dashboard y publicación (v4).  
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

---

## 4. Requisitos no funcionales

- **Capas estrictas:** controlador no toca SQL; servicio no conoce HTTP ni motor; repositorio no conoce HTTP.  
- **SQL a la vista:** escrito a mano y parametrizado (`@parametro`).  
- **Asíncrona:** todo acceso a datos con `async/await`.  
- **Errores uniformes:** `{estado, mensaje, detalle}` (+ `errores:[…]` en 422).  
- **Sin anticipación:** nada de FK, JWT o dashboard en v1.  

---

## 5. Criterios de aceptación

1. `docker compose up -d --build` deja corriendo SQL Server (inicializado con el script provisto: 25 tablas) y la API; `GET http://localhost:8036/` responde el JSON de diagnóstico.  
2. `GET /api/area_conocimiento` devuelve los 218 registros del Excel con `{tabla:"area_conocimiento", total:218, datos:[…]}`; `GET /api/universidad` devuelve los 6 registros iniciales.  
3. CRUD completo probado en al menos una tabla (`aliado`): `POST` crea → `PUT` reemplaza → `PATCH` actualiza parcialmente → `GET` confirma → `DELETE` marca inactivo → un segundo `DELETE` responde 404.  
4. Validación de peticiones: `POST` con datos inválidos (ej. `nombre` vacío, `stock:-5`) → 422 con `errores:[…]`.  
5. Prueba de capas: ejecución con repositorio falso en memoria pasa todas las verificaciones sin SQL Server.  
6. Tag `v1` puesto en `main` cuando todos los criterios están en verde.

---
