# Quickstart — Versión 1: arranque y smoke test

Este documento explica cómo levantar localmente la Versión 1 de Innovación Curricular API y ejecutar una prueba rápida de funcionamiento (smoke test).

La V1 utiliza:
* C# / ASP.NET Core para la API.
* SQL Server como motor de base de datos.
* Dapper para acceso a datos.
* Docker Compose para levantar los servicios.
* Swagger para explorar y probar los endpoints.

---

## 1. Requisitos previos
Antes de iniciar el proyecto se debe tener instalado:
* Git.
* Docker Desktop.
* .NET SDK compatible con el proyecto.
* Un navegador web o consola para peticiones HTTP (como `curl`).

Se recomienda comprobar Docker con:
```bash
docker --version
docker compose version
```
Y Git con:
```Bash
git --version
```
2. Clonar el repositorio
Clonar el repositorio:

```Bash
git clone <URL_DEL_REPOSITORIO>
```
Entrar a la carpeta:
```Bash
cd innovacion-curricular-api
```
3. Levantar el proyecto
Desde la raíz del proyecto ejecutar:
```Bash
docker compose up -d --build
```
Este comando construye las imágenes necesarias y levanta los servicios definidos en docker-compose.yml.
La V1 debe iniciar:
SQL Server.
Inicializador de base de datos.
API ASP.NET Core.
Nota: No es necesario crear manualmente la base de datos; el contenedor inicializador se encarga de ejecutar los scripts y cargar los datos base cuando el entorno Docker está correctamente configurado.

4. Verificar los contenedores
Comprobar que los contenedores estén ejecutándose:
```bash
docker compose ps
```
Los servicios deben aparecer en estado activo.
También se pueden consultar los logs generales con:
```Bash
docker compose logs
```
Para revisar únicamente los logs de un servicio en específico (por ejemplo, la API):
Bash
docker compose logs api-innovacion
Si un contenedor no inicia correctamente, revisar primero sus logs.
5. Verificar la API
La API de la V1 está disponible en: http://localhost:8036
Para verificar la conectividad base, puedes usar el navegador o curl:
```Bash
curl -i http://localhost:8036/
```
La respuesta esperada es un JSON de diagnóstico que identifica la versión de la API y la ubicación de los contratos:
```JSON
{
  "mensaje": "API Innovación Curricular funcionando",
  "version": "v1",
  "contratos": "docs/spec_kit/versiones/v1_producto_sqlserver/6_contracts.md"
}
```
6. Verificar Swagger
Swagger está disponible en:
http://localhost:8036/swagger
Desde Swagger se pueden consultar visualmente los endpoints disponibles y ejecutar las operaciones HTTP de la V1 para las siete tablas sin llaves foráneas (area_conocimiento, universidad, aspecto_normativo, practica_estrategia, enfoque, car_innovacion, aliado).
7. Smoke test de lectura
El primer smoke test consiste en comprobar que la API responde correctamente a las operaciones de lectura con la base de datos.
7.1 Listar áreas de conocimiento
```Bash
curl -i http://localhost:8036/api/area_conocimiento
```
La V1 debe iniciar con 218 registros cargados. La respuesta debe ser 200 OK y tener la estructura:
```JSON
{
  "tabla": "area_conocimiento",
  "limite": 1000,
  "total": 218,
  "datos": []
}
```
7.2 Listar universidades
```Bash
curl -i http://localhost:8036/api/universidad
```
La respuesta debe ser 200 OK. La V1 debe iniciar con 6 registros de universidad.
7.3 Consultar una entidad por ID
```Bash
curl -i http://localhost:8036/api/area_conocimiento/1
```
Si el registro existe, debe responder 200 OK.
Si no existe, debe responder 404 Not Found.
8. Smoke test del CRUD de aliado
La tabla aliado se utiliza como entidad principal para comprobar el ciclo completo de CRUD.
8.1 Crear un aliado (POST)
```Bash
curl -X POST http://localhost:8036/api/aliado \
  -H "Content-Type: application/json" \
  -d '{
    "nit": 900123456,
    "razon_social": "Empresa de Prueba",
    "nombre_contacto": "Felipe Correa",
    "correo": "felipe@example.com",
    "telefono": "3001234567",
    "ciudad": "Medellín"
  }'
  ```
La respuesta esperada es 200 OK.
8.2 Consultar el aliado creado (GET)
```Bash
curl -i http://localhost:8036/api/aliado/900123456
```
Debe responder 200 OK y devolver el registro recién creado.
8.3 Actualizar el aliado con PUT
En PUT se debe enviar el cuerpo completo:
```Bash
curl -X PUT http://localhost:8036/api/aliado/900123456 \
  -H "Content-Type: application/json" \
  -d '{
    "razon_social": "Empresa de Prueba Actualizada",
    "nombre_contacto": "Felipe Correa",
    "correo": "felipe@example.com",
    "telefono": "3001234567",
    "ciudad": "Medellín"
  }'
  ```
La respuesta debe ser 200 OK e incluir filasAfectadas: 1.
8.4 Actualizar parcialmente con PATCH
Con PATCH solamente se modifican los campos enviados:
```Bash
curl -X PATCH http://localhost:8036/api/aliado/900123456 \
  -H "Content-Type: application/json" \
  -d '{
    "telefono": "3019876543"
  }'
  ```
La respuesta debe ser 200 OK e incluir filasAfectadas: 1.
8.5 Eliminación lógica (DELETE)
La eliminación es lógica: el registro no se elimina físicamente de SQL Server, sino que se marca como inactivo mediante activo = 0.
```Bash
curl -X DELETE http://localhost:8036/api/aliado/900123456
```
La respuesta debe ser 200 OK e incluir filasEliminadas: 1.
9. Verificar validaciones
También se debe comprobar que la API rechaza cuerpos inválidos. Por ejemplo, enviar un POST sin un campo obligatorio:
```Bash
curl -X POST http://localhost:8036/api/aliado \
  -H "Content-Type: application/json" \
  -d '{
    "nit": 900123457,
    "razon_social": "Empresa de Prueba"
  }'
  ```
La respuesta esperada es 422 Unprocessable Entity y debe incluir información sobre los campos que no cumplen las reglas de validación en la lista errores.
10. Verificar recurso inexistente
Consultar un registro que não exista:
```Bash
curl -i http://localhost:8036/api/aliado/999999999
```
La respuesta esperada es 404 Not Found.
El mismo comportamiento aplica para las operaciones PUT, PATCH y DELETE cuando el registro solicitado no existe o ya fue eliminado lógicamente.
11. Probar límite de resultados
El listado permite utilizar el parámetro limite.
```Bash
curl -i "http://localhost:8036/api/area_conocimiento?limite=10"
```
La respuesta debe contener como máximo 10 registros manteniendo la estructura de envoltura esperada.
Un valor menor o igual a cero debe producir 400 Bad Request:
```Bash
curl -i "http://localhost:8036/api/area_conocimiento?limite=0"
```
12. Detener el entorno
Para detener los contenedores sin eliminar los datos persistidos:
```Bash
docker compose down
```
Para detenerlos y eliminar los volúmenes de Docker (borra la BD):
```Bash
docker compose down -v
```
13. Reiniciar desde cero
Para reconstruir el entorno completo con base de datos limpia:
```Bash
docker compose down -v
docker compose up -d --build
```
Después del reinicio se debe repetir el smoke test para comprobar que los 218 registros y 6 registros volvieron a cargarse y la API está disponible.
14. Problemas comunes
Docker no está ejecutándose: Si Docker Desktop no está iniciado, docker compose up no podrá levantar los servicios. Abre Docker Desktop y vuelve a intentarlo.
El puerto 8036 está ocupado: Si otro proceso utiliza el puerto, el contenedor de la API no iniciará. Usa docker compose ps para revisar el estado y cierra los programas que interfieran.
La API no responde: Revisa docker compose logs para ver si hay un error en compilación.
SQL Server no está listo: SQL Server puede tardar algunos segundos en iniciar. Espera y revisa docker compose logs sqlserver para comprobar el estado.

15. Checklist final del smoke test
La V1 se considera correctamente levantada cuando se cumple:
[ ] Docker está ejecutándose.

[ ] docker compose up -d --build finaliza correctamente.

[ ] Los servicios aparecen activos en docker compose ps.

[ ] GET / responde 200 OK.

[ ] Swagger está disponible.

[ ] GET /api/area_conocimiento devuelve los 218 registros iniciales.

[ ] GET /api/universidad devuelve los 6 registros iniciales.

[ ] Se puede crear un aliado (POST).

[ ] Se puede consultar el aliado creado (GET).

[ ] Se puede actualizar mediante PUT.

[ ] Se puede actualizar parcialmente mediante PATCH.

[ ] Se puede realizar eliminación lógica mediante DELETE.

[ ] Las validaciones inválidas devuelven 422.

[ ] Los recursos inexistentes devuelven 404.

[ ] limite <= 0 devuelve 400.