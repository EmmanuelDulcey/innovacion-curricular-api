# Tareas — Versión 1: catálogos base (7 tablas sin FK) + SQL Server (C#/ASP.NET Core)

> **Versión 1** · El orden de construcción, partiendo de CERO. Cada fase termina en algo verificable.
> Requisitos: `2_spec.md` · técnica: `3_plan.md` · contratos: `6_contracts.md` · validación final: `7_quickstart.md`.

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
* `Program.cs`: el ensamblador de dependencias (`AddScoped`), la personalización de la respuesta 422 para fallos de modelo (`InvalidModelStateResponseFactory` → `{estado, mensaje, errores}`), Swagger (`AddSwaggerGen` + `UseSwagger` + `UseSwaggerUI`), y el endpoint `GET /` de diagnóstico[cite: 1].
* **Verificar:** con la BD arriba probar listar `area_conocimiento` (200 con 218 registros y con `?limite=3`), consultar por ID (200 o 404), POST inválido (422 con array de errores), y el contraste PUT vs PATCH[cite: 1].

## Fase 6 — Docker: un solo comando

* `Dockerfile` (en la carpeta de la API): imagen basada en el SDK de .NET, ejecutando `dotnet watch`, con la variable `ASPNETCORE_URLS` configurada en el puerto 8036[cite: 1].
* Actualizar el `docker-compose.yml` incorporando el servicio `api-innovacion`: puerto expuesto 8036, variable de entorno `ConnectionStrings__SqlServer` apuntando al host interno `sqlserver,1433`, y el `depends_on` configurado sobre `sqlserver-init` con `condition: service_completed_successfully`[cite: 1].
* **Verificar:** `docker compose down` seguido de `docker compose up -d --build` deja la BD y la API operativas con un solo comando[cite: 1].

## Fase 7 — Cierre de la versión

* Ejecutar el *smoke test* completo descrito en `7_quickstart.md` §2[cite: 1].
* Asegurar las reglas de control de versiones con `.gitignore` (excluyendo `bin/`, `obj/`) y `.gitattributes`[cite: 1].
* Realizar el commit final y etiquetar el repositorio con el tag `v1`[cite: 1].