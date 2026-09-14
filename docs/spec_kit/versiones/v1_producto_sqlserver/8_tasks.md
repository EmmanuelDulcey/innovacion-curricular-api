# Tareas — Versión 1: catálogos base (7 tablas sin FK) + SQL Server (C#/ASP.NET Core)

> **Versión 1 (API + Frontend)** · El orden de construcción, partiendo de CERO. Cada fase termina en algo verificable.
> Requisitos: `2_spec.md` · técnica: `3_plan.md` · contratos: `6_contracts.md` · interfaz: `9_frontend.md` · validación final: `7_quickstart.md`.

## Fase 0 — Base de datos y esqueleto

* Copiar a `db/` los archivos provistos para esta versión: `innovacion_curricular.ss.sql` (la BD completa en dialecto SQL Server), `01_activar_borrado_logico.sql` (agrega la columna `activo`), `02_datos_iniciales.sql` (carga los registros base) e `init.sh` (el inicializador)[cite: 1].
* Crear el `docker-compose.yml` con los servicios `sqlserver` (imagen 2022, volumen `mssqldata`, puerto 11467, healthcheck con sqlcmd) y `sqlserver-init` (corre `init.sh` y termina)[cite: 1].
* Crear la estructura de carpetas de la API dentro de `api_innovacion/` con subcarpetas `Modelos/`, `Peticiones/`, `Controllers/`, `Servicios/`, `Repositorios/`, `Excepciones/` y `pruebas/`[cite: 1].
* **Verificar:** `docker compose ps -a` muestra `sqlserver (healthy)` y `sqlserver-init` en `Exited (0)`[cite: 1]; un cliente SQL conectado a `localhost:11467` (usuario `sa`) ve las tablas y `SELECT count(*) FROM area_conocimiento` da 218[cite: 1].

## Fase 1 — El proyecto .NET y los modelos entidad

* `ApiInnovacion.csproj`: proyecto Web de .NET, paquete `Microsoft.Data.SqlClient`, `Dapper`, `Swashbuckle.AspNetCore` (Swagger), y la exclusión de `pruebas/**`[cite: 1].
* `appsettings.json` con la cadena de conexión (por defecto `localhost,11467` para correr sin Docker)[cite: 1].
* `Modelos/`: clases entidad correspondientes a las 7 tablas sin FK (`AreaConocimiento`, `Universidad`, `AspectoNormativo`, `PracticaEstrategia`, `Enfoque`, `CarInnovacion`, `Aliado`) con sus propiedades tipadas `{ get; set; }` y la propiedad booleana `Activo` por defecto en `true`[cite: 1].
* **Verificar:** `dotnet build` compila sin errores.

## Fase 2 — Las peticiones por verbo y la excepción

* `Peticiones/`: tres clases por cada entidad: `<Tabla>Crear.cs` (todo obligatorio, incluida la PK), `<Tabla>Reemplazo.cs` (todo obligatorio sin la PK), y `<Tabla>Actualizar.cs` (todo opcional)[cite: 1].
* `Excepciones/NoEncontradoExcepcion.cs`: la excepción de negocio que el controlador traducirá a un código HTTP 404[cite: 1].
* **Verificar:** `dotnet build` compila sin errores.

## Fase 3 — Contratos (interfaces) y repositorio SQL Server

* `Repositorios/IRepositorio<Tabla>.cs`: interfaces con los 5 métodos asíncronos estándar[cite: 1].
* `Servicios/IServicio<Tabla>.cs`: interfaces de los servicios de negocio[cite: 1].
* `Repositorios/Repositorio<Tabla>SqlServer.cs`: implementación mediante Dapper (`QueryAsync` / `ExecuteAsync`) utilizando los comandos SQL parametrizados (`@`), el filtrado estricto de `activo = 1`, y el UPDATE de borrado lógico con `activo = 0`[cite: 1].
* **Verificar:** `dotnet build` compila sin errores.

## Fase 4 — Servicio (y la prueba de capas)

* `Servicios/Servicio<Tabla>.cs`: validan reglas de negocio (ej. `limite > 0`, PATCH sin campos lanza `ArgumentException` para devolver HTTP 400) y traducen registros inexistentes a `NoEncontradoExcepcion`[cite: 1].
* `pruebas/PruebaCapas.csproj` y `pruebas/Programa.cs`: implementa un servicio conectado a un repositorio falso en memoria para validar la lógica sin tocar SQL Server[cite: 1].
* **Verificar:** `dotnet run --project pruebas` termina con éxito confirmando el aislamiento de las capas.

## Fase 5 — Controller y Program.cs

* `Controllers/<Tabla>Controller.cs`: ruta base `[Route("api/{tabla}")]`, los 6 métodos HTTP con sus atributos de verbo, try/catch estricto traduciendo excepciones a códigos HTTP, y respuesta 204 para listas vacías[cite: 1].
* `Program.cs`: el ensamblador de dependencias (`AddScoped`), la personalización de la respuesta 422 para fallos de modelo (`InvalidModelStateResponseFactory` → `{estado, mensaje, errores}`), Swagger (`AddSwaggerGen` + `UseSwagger` + `UseSwaggerUI`), **la política CORS para el origen `http://localhost:8037`**, y el endpoint `GET /` de diagnóstico[cite: 1].
* **Verificar:** con la BD arriba probar listar `area_conocimiento` (200 con 218 registros y con `?limite=3`), consultar por ID (200 o 404), POST inválido (422 con array de errores), y el contraste PUT vs PATCH[cite: 1].

## Fase 6 — Docker: un solo comando

* `Dockerfile` (en la carpeta de la API): imagen basada en el SDK de .NET, ejecutando `dotnet watch`, con la variable `ASPNETCORE_URLS` configurada en el puerto 8036[cite: 1].
* Actualizar el `docker-compose.yml` incorporando el servicio `api-innovacion`: puerto expuesto 8036, variable de entorno `ConnectionStrings__SqlServer` apuntando al host interno `sqlserver,1433`, y el `depends_on` configurado sobre `sqlserver-init` con `condition: service_completed_successfully`. *(El servicio `frontend` se incorpora en la Fase 11.)*[cite: 1].
* **Verificar:** `docker compose down` seguido de `docker compose up -d --build` deja la BD y la API operativas con un solo comando[cite: 1].

## Fase 7 — Frontend: andamio Angular

* Crear el proyecto con `ng new frontend` (routing incluido, estilos CSS plano, componentes standalone) y eliminar el contenido de ejemplo.
* En `package.json`, configurar `start` como `ng serve --host 0.0.0.0 --port 8037` (puerto del Frontend, Artículo 8).
* `src/environments/environment.ts` con `apiUrl: 'http://localhost:8036'`; `app.routes.ts` con las rutas base (`/` → dashboard, `/tablas/:tabla` → catálogo)[cite: 1].
* **Verificar:** `npm start` compila sin errores y `http://localhost:8037` responde HTML.

## Fase 8 — Frontend: modelos y servicio de la API

* `src/app/models/catalogo.ts`: interfaces en camelCase por tabla (mapeando el snake_case de `6_contracts.md`) más `RespuestaLista` con `{ tabla, limite, total, datos }` y `ApiError` con `{ estado, mensaje, detalle, errores? }`[cite: 1].
* `src/app/services/api.service.ts`: métodos `listar(tabla, limite?)`, `obtener(tabla, id)`, `crear`, `reemplazar`, `actualizar`, `eliminar` usando `HttpClient` contra `apiUrl`; manejo de 200/204/400/404/422/500[cite: 1].
* **Verificar:** `ng build` compila y, con la API levantada, `apiService.listar('area_conocimiento')` responde los 218 registros.

## Fase 9 — Frontend: dashboard y navegación (RF8)

* `components/nav-bar`: menú con "Dashboard" y un enlace por cada una de las 7 tablas.
* `pages/dashboard`: consulta las 7 tablas en paralelo con `Promise.all` y muestra una tarjeta por tabla con su `total`[cite: 1].
* **Verificar:** `http://localhost:8037/` muestra las 7 tarjetas; `area_conocimiento` = 218, `universidad` = 6 (API y CORS activos).

## Fase 10 — Frontend: páginas CRUD (RF9 y RF10)

* `pages/catalogo`: listado con columnas por tabla, botones **Crear**/**Editar**/**Eliminar** (con confirmación de borrado) y la tabla que se muestra obtenida de la ruta.
* Formulario reutilizado que sigue las reglas de `6_contracts.md` §8; errores 422 mostrados por campo, 404 → "registro no encontrado", 500 → error del servidor, 204 → tabla vacía[cite: 1].
* **Verificar:** desde `http://localhost:8037/tablas/aliado` se crea, edita y elimina un registro y el cambio persiste al recargar; un formulario inválido muestra los errores sin romper la interfaz.

## Fase 11 — Frontend en Docker y un solo comando

* `frontend/Dockerfile` (imagen node LTS) ejecutando `npm ci && npm start`; el `apiUrl` apunta a `http://localhost:8036`.
* Actualizar `docker-compose.yml` con el servicio `frontend`: puerto 8037, `depends_on: api-innovacion`[cite: 1].
* **Verificar:** `docker compose down` seguido de `docker compose up -d --build` deja los 4 servicios operativos y `http://localhost:8037` funciona de punta a punta (API + Frontend).

## Fase 12 — Cierre de la versión

* Ejecutar el *smoke test* completo descrito en `7_quickstart.md`: API (§8–12) y Frontend (§7, §16)[cite: 1].
* Asegurar las reglas de control de versiones con `.gitignore` (excluyendo `bin/`, `obj/`, `frontend/node_modules/`) y `.gitattributes`[cite: 1].
* Realizar el commit final y etiquetar el repositorio con el tag `v1`
  (monorepo: backend y Frontend quedan en el mismo commit y comparten el tag)[cite: 1].