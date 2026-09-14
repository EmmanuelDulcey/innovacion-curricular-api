# Quickstart — Versión 1: arranque y smoke test (API + Frontend)

Este documento explica cómo levantar localmente la Versión 1 de Innovación
Curricular y ejecutar una prueba rápida de funcionamiento (smoke test).

La V1 utiliza:
* C# / ASP.NET Core para la API.
* SQL Server como motor de base de datos.
* Dapper para acceso a datos.
* **Angular para el Frontend (dashboard + CRUD visual).**
* Docker Compose para levantar los servicios.
* Swagger para explorar y probar los endpoints de la API.

---

## 1. Requisitos previos

Antes de iniciar el proyecto se debe tener instalado:
* Git.
* Docker Desktop.
* .NET SDK compatible con el proyecto.
* En opcional, Node.js LTS + Angular CLI para correr el Frontend fuera de Docker.
* Un navegador web o una consola para peticiones HTTP (como `curl`).

Se recomienda comprobar Docker con:

```bash
docker --version
docker compose version
```

Y Git con:

```bash
git --version
```

## 2. Clonar el repositorio

Clonar el repositorio:

```bash
git clone <URL_DEL_REPOSITORIO>
```

Entrar a la carpeta:

```bash
cd innovacion-curricular-api
```

## 3. Levantar el proyecto

Desde la raíz del proyecto ejecutar:

```bash
docker compose up -d --build
```

Este comando construye las imágenes necesarias y levanta los servicios
definidos en `docker-compose.yml`. La V1 inicia **cuatro** componentes:

1. SQL Server.
2. Inicializador de base de datos.
3. API ASP.NET Core.
4. Frontend Angular.

> No es necesario crear manualmente la base de datos; el contenedor
> inicializador se encarga de ejecutar los scripts y cargar los datos base
> cuando el entorno Docker está correctamente configurado.

## 4. Verificar los contenedores

Comprobar que los contenedores estén ejecutándose:

```bash
docker compose ps
```

Los servicios deben aparecer en estado activo (`sqlserver` healthy,
`api-innovacion` y `frontend` up). También se pueden consultar los logs:

```bash
docker compose logs
```

Para revisar únicamente los logs de un servicio (por ejemplo, la API):

```bash
docker compose logs api-innovacion
```

Si un contenedor no inicia correctamente, revisar primero sus logs.

## 5. Verificar la API

La API de la V1 está disponible en: `http://localhost:8036`

```bash
curl -i http://localhost:8036/
```

La respuesta esperada es un JSON de diagnóstico que identifica la versión
de la API y la ubicación de los contratos:

```json
{
  "mensaje": "API Innovación Curricular funcionando",
  "version": "v1",
  "contratos": "docs/spec_kit/versiones/v1_producto_sqlserver/6_contracts.md"
}
```

## 6. Verificar Swagger

Swagger está disponible en:

```text
http://localhost:8036/swagger
```

Desde allí se pueden consultar visualmente los endpoints disponibles y
ejecutar las operaciones HTTP de la V1 para las siete tablas sin llaves
foráneas (`area_conocimiento`, `universidad`, `aspecto_normativo`,
`practica_estrategia`, `enfoque`, `car_innovacion`, `aliado`).

## 7. Verificar el Frontend

El Frontend Angular está disponible en: `http://localhost:8037`

1. **Dashboard:** al abrir la raíz `/` deben verse las tarjetas de los 7
   catálogos con sus totales (`area_conocimiento` = 218, `universidad` = 6
   y el resto en 0 si no han cargado datos).
2. **Navegación:** el menú debe permitir entrar a cualquier tabla.
3. **CRUD visual:** entrar a `http://localhost:8037/tablas/aliado`,
   crear un registro con el formulario, editarlo y eliminarlo; al recargar
   la página el cambio debe persistir.

## 8. Smoke test de lectura (API)

8.1 Listar áreas de conocimiento

```bash
curl -i http://localhost:8036/api/area_conocimiento
```

La V1 debe iniciar con 218 registros cargados. La respuesta debe ser
`200 OK` con la estructura:

```json
{
  "tabla": "area_conocimiento",
  "limite": 1000,
  "total": 218,
  "datos": []
}
```

8.2 Listar universidades

```bash
curl -i http://localhost:8036/api/universidad
```

Debe responder `200 OK` con los 6 registros iniciales de universidad.

8.3 Consultar una entidad por ID

```bash
curl -i http://localhost:8036/api/area_conocimiento/1
```

Si el registro existe, debe responder `200 OK`; si no, `404 Not Found`.

## 9. Smoke test del CRUD de aliado (API)

La tabla `aliado` se utiliza como entidad principal para comprobar el ciclo
completo de CRUD.

9.1 Crear un aliado (POST)

```bash
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

La respuesta esperada es `200 OK`.

9.2 Consultar el aliado creado (GET)

```bash
curl -i http://localhost:8036/api/aliado/900123456
```

Debe responder `200 OK` y devolver el registro recién creado.

9.3 Actualizar el aliado con PUT (cuerpo completo)

```bash
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

La respuesta debe ser `200 OK` e incluir `filasAfectadas: 1`.

9.4 Actualizar parcialmente con PATCH

```bash
curl -X PATCH http://localhost:8036/api/aliado/900123456 \
  -H "Content-Type: application/json" \
  -d '{
    "telefono": "3019876543"
  }'
```

La respuesta debe ser `200 OK` e incluir `filasAfectadas: 1`.

9.5 Eliminación lógica (DELETE)

La eliminación es lógica: el registro no se elimina físicamente de SQL
Server, sino que se marca como inactivo mediante `activo = 0`.

```bash
curl -X DELETE http://localhost:8036/api/aliado/900123456
```

La respuesta debe ser `200 OK` e incluir `filasEliminadas: 1`.

## 10. Verificar validaciones

Por ejemplo, enviar un POST sin un campo obligatorio:

```bash
curl -X POST http://localhost:8036/api/aliado \
  -H "Content-Type: application/json" \
  -d '{
    "nit": 900123457,
    "razon_social": "Empresa de Prueba"
  }'
```

La respuesta esperada es `422 Unprocessable Entity` con la lista `errores`.

Desde el Frontend, enviar el mismo formulario incompleto debe mostrar los
errores de validación junto a los campos, sin romper la interfaz.

## 11. Verificar recurso inexistente

```bash
curl -i http://localhost:8036/api/aliado/999999999
```

La respuesta esperada es `404 Not Found`. El mismo comportamiento aplica
para PUT, PATCH y DELETE cuando el registro no existe o ya fue eliminado
lógicamente. El Frontend debe mostrar un mensaje de "registro no encontrado".

## 12. Probar límite de resultados

```bash
curl -i "http://localhost:8036/api/area_conocimiento?limite=10"
```

La respuesta debe contener como máximo 10 registros manteniendo la
estructura de envoltura esperada. Un valor menor o igual a cero debe
producir `400 Bad Request`:

```bash
curl -i "http://localhost:8036/api/area_conocimiento?limite=0"
```

## 13. Detener el entorno

Para detener los contenedores sin eliminar los datos persistidos:

```bash
docker compose down
```

Para detenerlos y eliminar los volúmenes de Docker (borra la BD):

```bash
docker compose down -v
```

## 14. Reiniciar desde cero

Para reconstruir el entorno completo con base de datos limpia:

```bash
docker compose down -v
docker compose up -d --build
```

Después del reinicio se debe repetir el smoke test para comprobar que los
218 registros y 6 registros volvieron a cargarse, la API está disponible y
el dashboard del Frontend muestra los totales esperados.

## 15. Problemas comunes

* **Docker no está ejecutándose:** si Docker Desktop no está iniciado,
  `docker compose up` no podrá levantar los servicios. Abre Docker Desktop
  y vuelve a intentarlo.
* **El puerto 8036 o 8037 está ocupado:** si otro proceso utiliza el
  puerto, el contenedor no iniciará. Usa `docker compose ps` para revisar
  el estado y cierra los programas que interfieran.
* **La API no responde / el Frontend no carga:** revisa `docker compose logs`
  para ver si hay un error de compilación de la API o del Frontend.
* **El Frontend muestra datos vacíos:** verifica que la API esté arriba y que
  el CORS esté habilitado (`http://localhost:8037` en `appsettings.json`).
* **SQL Server no está listo:** SQL Server puede tardar algunos segundos en
  iniciar. Espera y revisa `docker compose logs sqlserver`.

## 16. Checklist final del smoke test

La V1 se considera correctamente levantada cuando se cumple:

- [ ] Docker está ejecutándose.
- [ ] `docker compose up -d --build` finaliza correctamente.
- [ ] Los 4 servicios aparecen activos en `docker compose ps`.
- [ ] `GET /` (API) responde `200 OK`.
- [ ] Swagger está disponible.
- [ ] `GET /api/area_conocimiento` devuelve los 218 registros iniciales.
- [ ] `GET /api/universidad` devuelve los 6 registros iniciales.
- [ ] Se puede crear un aliado (POST), consultarlo (GET), actualizarlo (PUT),
      actualizarlo parcialmente (PATCH) y eliminarlo lógicamente (DELETE).
- [ ] Las validaciones inválidas devuelven 422.
- [ ] Los recursos inexistentes devuelven 404.
- [ ] `limite <= 0` devuelve 400.
- [ ] **El dashboard carga en `http://localhost:8037`** con las métricas de
      los 7 catálogos.
- [ ] **Desde el navegador** se puede crear, editar y eliminar un `aliado`
      y el cambio persiste al recargar (criterios 6–9 de `2_spec.md`).