# Contratos HTTP — Versión 1: las 7 tablas sin llave foránea

> **Versión 1** · Base: `http://localhost:8036`.
> Estos contratos definen los verbos HTTP, rutas, códigos de respuesta y formatos exactos que debe cumplir la API de Innovación Curricular en esta versión.
>
> La v1 expone el CRUD completo de las siete tablas sin llave foránea y un endpoint de diagnóstico. La documentación interactiva está disponible mediante Swagger.

---

## 0. Convenciones globales

### 0.1 Tablas disponibles

La API v1 permite realizar operaciones CRUD sobre las siguientes siete tablas:

```text
area_conocimiento
universidad
aspecto_normativo
practica_estrategia
enfoque
car_innovacion
aliado
```

Las seis primeras tablas utilizan `id` como llave primaria.

La tabla `aliado` utiliza `nit` como llave primaria.

### 0.2 Rutas

Para las tablas cuya llave primaria es `id`:

```text
/api/{tabla}
/api/{tabla}/{id}
```

Para `aliado`:

```text
/api/aliado
/api/aliado/{nit}
```

### 0.3 Lecturas con envoltura

Las operaciones de listado utilizan el siguiente formato:

```json
{
  "tabla": "nombre_tabla",
  "limite": 1000,
  "total": 10,
  "datos": [
    {}
  ]
}
```

`limite` es un entero mayor que cero y su valor predeterminado es `1000`.

### 0.4 Lectura individual

La consulta de un registro individual devuelve directamente el objeto correspondiente, sin envoltura:

```json
{
  "id": 1
}
```

En `aliado`, la llave es `nit`:

```json
{
  "nit": 900123456
}
```

### 0.5 Formato de errores

Los errores generales utilizan:

```json
{
  "estado": 400,
  "mensaje": "Parámetros inválidos.",
  "detalle": "Descripción del error."
}
```

Los errores de validación de las peticiones utilizan `422` y además incluyen `errores`:

```json
{
  "estado": 422,
  "mensaje": "Datos inválidos.",
  "errores": [
    "Descripción del error de validación."
  ]
}
```

### 0.6 Códigos HTTP

| Situación                       |    HTTP |
| ------------------------------- | ------: |
| Operación exitosa               | **200** |
| Tabla sin registros             | **204** |
| Regla de negocio inválida       | **400** |
| Registro inexistente            | **404** |
| Body inválido según la petición | **422** |
| Error de SQL Server             | **500** |

---

# 1. `GET /` — Diagnóstico

Permite verificar que la API se encuentra funcionando y que corresponde a la versión 1.

### Petición

```http
GET /
```

### Respuesta

```http
HTTP/1.1 200 OK
```

```json
{
  "mensaje": "API Innovación Curricular funcionando",
  "version": "v1",
  "contratos": "docs/spec_kit/versiones/v1_producto_sqlserver/6_contracts.md"
}
```

---

# 2. `GET /api/{tabla}` — Listar registros

Permite obtener los registros de cualquiera de las siete tablas sin llave foránea.

### Endpoints

```http
GET /api/area_conocimiento
GET /api/universidad
GET /api/aspecto_normativo
GET /api/practica_estrategia
GET /api/enfoque
GET /api/car_innovacion
GET /api/aliado
```

### Query string

El parámetro `limite` es opcional:

```text
?limite=N
```

Debe ser un entero mayor que cero.

Si no se especifica:

```text
limite = 1000
```

### Ejemplo — `area_conocimiento`

```http
GET /api/area_conocimiento?limite=3
```

### Respuesta

```http
HTTP/1.1 200 OK
```

```json
{
  "tabla": "area_conocimiento",
  "limite": 3,
  "total": 3,
  "datos": [
    {
      "id": 1,
      "gran_area": "Ciencias Naturales",
      "area": "Matemáticas",
      "disciplina": "Matemática Aplicada",
      "activo": true
    },
    {
      "id": 2,
      "gran_area": "Ingeniería",
      "area": "Ingeniería de Sistemas",
      "disciplina": "Computación",
      "activo": true
    },
    {
      "id": 3,
      "gran_area": "Ciencias Sociales",
      "area": "Educación",
      "disciplina": "Pedagogía",
      "activo": true
    }
  ]
}
```

### Ejemplo — `universidad`

```http
GET /api/universidad?limite=3
```

```json
{
  "tabla": "universidad",
  "limite": 3,
  "total": 3,
  "datos": [
    {
      "id": 1,
      "nombre": "Universidad Ejemplo",
      "tipo": "Pública",
      "ciudad": "Medellín",
      "activo": true
    }
  ]
}
```

### Ejemplo — `aspecto_normativo`

```http
GET /api/aspecto_normativo?limite=3
```

```json
{
  "tabla": "aspecto_normativo",
  "limite": 3,
  "total": 3,
  "datos": [
    {
      "id": 1,
      "tipo": "Ley",
      "descripcion": "Descripción del aspecto normativo",
      "fuente": "Ministerio de Educación",
      "activo": true
    }
  ]
}
```

### Ejemplo — `practica_estrategia`

```http
GET /api/practica_estrategia?limite=3
```

```json
{
  "tabla": "practica_estrategia",
  "limite": 3,
  "total": 3,
  "datos": [
    {
      "id": 1,
      "tipo": "Práctica",
      "nombre": "Aprendizaje basado en proyectos",
      "descripcion": "Descripción de la práctica",
      "activo": true
    }
  ]
}
```

### Ejemplo — `enfoque`

```http
GET /api/enfoque?limite=3
```

```json
{
  "tabla": "enfoque",
  "limite": 3,
  "total": 3,
  "datos": [
    {
      "id": 1,
      "nombre": "Enfoque pedagógico",
      "descripcion": "Descripción del enfoque",
      "activo": true
    }
  ]
}
```

### Ejemplo — `car_innovacion`

```http
GET /api/car_innovacion?limite=3
```

```json
{
  "tabla": "car_innovacion",
  "limite": 3,
  "total": 3,
  "datos": [
    {
      "id": 1,
      "nombre": "Característica de innovación",
      "descripcion": "Descripción de la característica",
      "tipo": "Pedagógica",
      "activo": true
    }
  ]
}
```

### Ejemplo — `aliado`

```http
GET /api/aliado?limite=3
```

```json
{
  "tabla": "aliado",
  "limite": 3,
  "total": 3,
  "datos": [
    {
      "nit": 900123456,
      "razon_social": "Empresa Ejemplo",
      "nombre_contacto": "Juan Pérez",
      "correo": "contacto@empresa.com",
      "telefono": "3001234567",
      "ciudad": "Medellín",
      "activo": true
    }
  ]
}
```

### Tabla vacía

Si la tabla no contiene registros activos:

```http
HTTP/1.1 204 No Content
```

La respuesta no contiene body.

### `limite <= 0`

Ejemplo:

```http
GET /api/aliado?limite=0
```

Respuesta:

```http
HTTP/1.1 400 Bad Request
```

```json
{
  "estado": 400,
  "mensaje": "Parámetros inválidos.",
  "detalle": "El parámetro limite debe ser mayor que 0."
}
```

### Consideración sobre `activo`

Los registros con:

```text
activo = 1
```

son los registros visibles para las operaciones normales de lectura.

Los registros con:

```text
activo = 0
```

corresponden a registros eliminados lógicamente y no deben aparecer en las consultas normales.

---

# 3. `GET /api/{tabla}/{id}` — Obtener un registro

Permite consultar un registro mediante su llave primaria.

Para las siguientes tablas la llave es `id`:

```text
area_conocimiento
universidad
aspecto_normativo
practica_estrategia
enfoque
car_innovacion
```

Para `aliado`, la llave es `nit`.

---

## 3.1 `GET /api/area_conocimiento/{id}`

### Ejemplo

```http
GET /api/area_conocimiento/1
```

### Respuesta exitosa

```http
HTTP/1.1 200 OK
```

```json
{
  "id": 1,
  "gran_area": "Ciencias Naturales",
  "area": "Matemáticas",
  "disciplina": "Matemática Aplicada",
  "activo": true
}
```

---

## 3.2 `GET /api/universidad/{id}`

```http
GET /api/universidad/1
```

```json
{
  "id": 1,
  "nombre": "Universidad Ejemplo",
  "tipo": "Pública",
  "ciudad": "Medellín",
  "activo": true
}
```

---

## 3.3 `GET /api/aspecto_normativo/{id}`

```http
GET /api/aspecto_normativo/1
```

```json
{
  "id": 1,
  "tipo": "Ley",
  "descripcion": "Descripción del aspecto normativo",
  "fuente": "Ministerio de Educación",
  "activo": true
}
```

---

## 3.4 `GET /api/practica_estrategia/{id}`

```http
GET /api/practica_estrategia/1
```

```json
{
  "id": 1,
  "tipo": "Práctica",
  "nombre": "Aprendizaje basado en proyectos",
  "descripcion": "Descripción de la práctica",
  "activo": true
}
```

---

## 3.5 `GET /api/enfoque/{id}`

```http
GET /api/enfoque/1
```

```json
{
  "id": 1,
  "nombre": "Enfoque pedagógico",
  "descripcion": "Descripción del enfoque",
  "activo": true
}
```

---

## 3.6 `GET /api/car_innovacion/{id}`

```http
GET /api/car_innovacion/1
```

```json
{
  "id": 1,
  "nombre": "Característica de innovación",
  "descripcion": "Descripción de la característica",
  "tipo": "Pedagógica",
  "activo": true
}
```

---

## 3.7 `GET /api/aliado/{nit}`

```http
GET /api/aliado/900123456
```

```json
{
  "nit": 900123456,
  "razon_social": "Empresa Ejemplo",
  "nombre_contacto": "Juan Pérez",
  "correo": "contacto@empresa.com",
  "telefono": "3001234567",
  "ciudad": "Medellín",
  "activo": true
}
```

### Registro inexistente

Ejemplo:

```http
GET /api/aliado/999999999
```

Respuesta:

```http
HTTP/1.1 404 Not Found
```

```json
{
  "estado": 404,
  "mensaje": "Registro no encontrado.",
  "detalle": "No existe un registro activo con nit = 999999999."
}
```

Para las tablas cuya llave es `id`, el formato será:

```json
{
  "estado": 404,
  "mensaje": "Registro no encontrado.",
  "detalle": "No existe un registro activo con id = 999999."
}
```

---

# 4. `POST /api/{tabla}` — Crear registro

Permite crear un nuevo registro.

El identificador **no utiliza `IDENTITY`**, por lo que debe ser proporcionado por el cliente.

Para las seis tablas correspondientes se utiliza `id`.

Para `aliado` se utiliza `nit`.

El campo `activo` no necesita enviarse para crear el registro; la API debe crearlo inicialmente como activo.

---

## 4.1 `POST /api/area_conocimiento`

### Petición

```http
POST /api/area_conocimiento
Content-Type: application/json
```

```json
{
  "id": 219,
  "gran_area": "Ingeniería",
  "area": "Ingeniería de Sistemas",
  "disciplina": "Desarrollo de Software"
}
```

### Respuesta

```http
HTTP/1.1 200 OK
```

```json
{
  "estado": 200,
  "mensaje": "Registro creado exitosamente."
}
```

### Body inválido

```json
{
  "id": 219,
  "gran_area": "",
  "area": "Ingeniería de Sistemas"
}
```

```http
HTTP/1.1 422 Unprocessable Entity
```

```json
{
  "estado": 422,
  "mensaje": "Datos inválidos.",
  "errores": [
    "El campo gran_area es obligatorio.",
    "El campo disciplina es obligatorio."
  ]
}
```

---
## 4.2 `POST /api/universidad`

```http
POST /api/universidad
Content-Type: application/json
```

```json
{
  "id": 7,
  "nombre": "Universidad Ejemplo",
  "tipo": "Pública",
  "ciudad": "Medellín"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro creado exitosamente."
}
```

---

## 4.3 `POST /api/aspecto_normativo`

```http
POST /api/aspecto_normativo
Content-Type: application/json
```

```json
{
  "id": 1,
  "tipo": "Ley",
  "descripcion": "Descripción normativa",
  "fuente": "Ministerio de Educación"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro creado exitosamente."
}
```

---

## 4.4 `POST /api/practica_estrategia`

```http
POST /api/practica_estrategia
Content-Type: application/json
```

```json
{
  "id": 1,
  "tipo": "Práctica",
  "nombre": "Aprendizaje basado en proyectos",
  "descripcion": "Descripción de la práctica"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro creado exitosamente."
}
```

---

## 4.5 `POST /api/enfoque`

```http
POST /api/enfoque
Content-Type: application/json
```

```json
{
  "id": 1,
  "nombre": "Enfoque pedagógico",
  "descripcion": "Descripción del enfoque"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro creado exitosamente."
}
```

---

## 4.6 `POST /api/car_innovacion`

```http
POST /api/car_innovacion
Content-Type: application/json
```

```json
{
  "id": 1,
  "nombre": "Innovación pedagógica",
  "descripcion": "Descripción de la característica de innovación",
  "tipo": "Pedagógica"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro creado exitosamente."
}
```

---

## 4.7 `POST /api/aliado`

`aliado` utiliza `nit` como llave primaria.

```http
POST /api/aliado
Content-Type: application/json
```

```json
{
  "nit": 900123456,
  "razon_social": "Empresa Ejemplo",
  "nombre_contacto": "Juan Pérez",
  "correo": "contacto@empresa.com",
  "telefono": "3001234567",
  "ciudad": "Medellín"
}
```

### Respuesta

```http
HTTP/1.1 200 OK
```

```json
{
  "estado": 200,
  "mensaje": "Registro creado exitosamente."
}
```

### Validación de correo

El campo `correo` debe cumplir el formato de correo electrónico.

Ejemplo inválido:

```json
{
  "nit": 900123456,
  "razon_social": "Empresa Ejemplo",
  "nombre_contacto": "Juan Pérez",
  "correo": "correo-invalido",
  "telefono": "3001234567",
  "ciudad": "Medellín"
}
```

Respuesta:

```http
HTTP/1.1 422 Unprocessable Entity
```

```json
{
  "estado": 422,
  "mensaje": "Datos inválidos.",
  "errores": [
    "El campo correo debe tener un formato válido."
  ]
}
```

### Llave primaria duplicada

Si se intenta crear un registro utilizando un `id` o `nit` que ya existe:

```http
HTTP/1.1 500 Internal Server Error
```

```json
{
  "estado": 500,
  "mensaje": "Error al crear el registro.",
  "detalle": "Error devuelto por el motor de SQL Server."
}
```

El detalle debe contener el error proporcionado por SQL Server.

---

# 5. `PUT /api/{tabla}/{id}` — Reemplazo completo

`PUT` realiza un reemplazo completo del registro.

Todos los campos modificables definidos para la entidad son obligatorios.

El identificador se proporciona en la URL y no debe modificarse mediante el body.

El campo `activo` no forma parte del body de reemplazo.

---

## 5.1 `PUT /api/area_conocimiento/{id}`

```http
PUT /api/area_conocimiento/219
Content-Type: application/json
```

```json
{
  "gran_area": "Ingeniería",
  "area": "Ingeniería de Sistemas",
  "disciplina": "Ingeniería de Software"
}
```

### Respuesta

```http
HTTP/1.1 200 OK
```

```json
{
  "estado": 200,
  "mensaje": "Registro reemplazado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 5.2 `PUT /api/universidad/{id}`

```http
PUT /api/universidad/7
Content-Type: application/json
```

```json
{
  "nombre": "Universidad Actualizada",
  "tipo": "Pública",
  "ciudad": "Medellín"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro reemplazado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 5.3 `PUT /api/aspecto_normativo/{id}`

```http
PUT /api/aspecto_normativo/1
Content-Type: application/json
```

```json
{
  "tipo": "Ley",
  "descripcion": "Descripción actualizada",
  "fuente": "Fuente actualizada"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro reemplazado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 5.4 `PUT /api/practica_estrategia/{id}`

```http
PUT /api/practica_estrategia/1
Content-Type: application/json
```

```json
{
  "tipo": "Estrategia",
  "nombre": "Nueva estrategia",
  "descripcion": "Descripción actualizada"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro reemplazado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 5.5 `PUT /api/enfoque/{id}`

```http
PUT /api/enfoque/1
Content-Type: application/json
```

```json
{
  "nombre": "Nuevo enfoque",
  "descripcion": "Descripción actualizada"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro reemplazado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 5.6 `PUT /api/car_innovacion/{id}`

```http
PUT /api/car_innovacion/1
Content-Type: application/json
```

```json
{
  "nombre": "Innovación curricular",
  "descripcion": "Descripción completa actualizada",
  "tipo": "Curricular"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro reemplazado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 5.7 `PUT /api/aliado/{nit}`

```http
PUT /api/aliado/900123456
Content-Type: application/json
```

```json
{
  "razon_social": "Empresa Actualizada",
  "nombre_contacto": "Carlos Pérez",
  "correo": "carlos@empresa.com",
  "telefono": "3009876543",
  "ciudad": "Medellín"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro reemplazado exitosamente.",
  "filasAfectadas": 1
}
```

### Campos faltantes

Por ejemplo:

```http
PUT /api/aliado/900123456
Content-Type: application/json
```

```json
{
  "razon_social": "Empresa Actualizada"
}
```

Respuesta:

```http
HTTP/1.1 422 Unprocessable Entity
```

```json
{
  "estado": 422,
  "mensaje": "Datos inválidos.",
  "errores": [
    "El campo nombre_contacto es obligatorio.",
    "El campo correo es obligatorio.",
    "El campo telefono es obligatorio.",
    "El campo ciudad es obligatorio."
  ]
}
```

### Registro inexistente

```http
HTTP/1.1 404 Not Found
```

```json
{
  "estado": 404,
  "mensaje": "Registro no encontrado.",
  "detalle": "No existe un registro activo con id = 999999."
}
```

Para `aliado`:

```json
{
  "estado": 404,
  "mensaje": "Registro no encontrado.",
  "detalle": "No existe un registro activo con nit = 999999999."
}
```

---

# 6. `PATCH /api/{tabla}/{id}` — Actualización parcial

`PATCH` permite actualizar únicamente los campos enviados en el body.

Todos los campos de la petición de actualización son opcionales.

El identificador se proporciona en la URL.

El campo `activo` no se actualiza mediante este endpoint.

---

## 6.1 `PATCH /api/area_conocimiento/{id}`

```http
PATCH /api/area_conocimiento/219
Content-Type: application/json
```

```json
{
  "disciplina": "Desarrollo de Software"
}
```

### Respuesta

```http
HTTP/1.1 200 OK
```

```json
{
  "estado": 200,
  "mensaje": "Registro actualizado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 6.2 `PATCH /api/universidad/{id}`

```http
PATCH /api/universidad/7
Content-Type: application/json
```

```json
{
  "ciudad": "Bogotá"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro actualizado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 6.3 `PATCH /api/aspecto_normativo/{id}`

```http
PATCH /api/aspecto_normativo/1
Content-Type: application/json
```

```json
{
  "fuente": "Ministerio de Educación Nacional"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro actualizado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 6.4 `PATCH /api/practica_estrategia/{id}`

```http
PATCH /api/practica_estrategia/1
Content-Type: application/json
```

```json
{
  "nombre": "Estrategia actualizada"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro actualizado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 6.5 `PATCH /api/enfoque/{id}`

```http
PATCH /api/enfoque/1
Content-Type: application/json
```

```json
{
  "descripcion": "Descripción actualizada"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro actualizado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 6.6 `PATCH /api/car_innovacion/{id}`

```http
PATCH /api/car_innovacion/1
Content-Type: application/json
```

```json
{
  "tipo": "Tecnológica"
}
```

### Respuesta

```json
{
  "estado": 200,
  "mensaje": "Registro actualizado exitosamente.",
  "filasAfectadas": 1
}
```

---

## 6.7 `PATCH /api/aliado/{nit}`

```http
PATCH /api/aliado/900123456
Content-Type: application/json
```

```json
{
  "correo": "nuevo.correo@empresa.com",
  "telefono": "3001112233"
}
```

### Respuesta

```http
HTTP/1.1 200 OK
```

```json
{
  "estado": 200,
  "mensaje": "Registro actualizado exitosamente.",
  "filasAfectadas": 1
}
```

### Body vacío

Ejemplo:

```http
PATCH /api/aliado/900123456
Content-Type: application/json
```

```json
{}
```

Respuesta:

```http
HTTP/1.1 400 Bad Request
```

```json
{
  "estado": 400,
  "mensaje": "Parámetros inválidos.",
  "detalle": "No se envió ningún campo para actualizar."
}
```

### Registro inexistente

```http
HTTP/1.1 404 Not Found
```

```json
{
  "estado": 404,
  "mensaje": "Registro no encontrado.",
  "detalle": "No existe un registro activo con nit = 999999999."
}
```

---

# 7. `DELETE /api/{tabla}/{id}` — Eliminación lógica

La operación `DELETE` realiza un **borrado lógico**.

No se elimina físicamente el registro de SQL Server.

La API modifica:

```text
activo = 0
```

Por lo tanto, el registro permanece almacenado en la base de datos, pero deja de estar disponible para las consultas normales.

---

## 7.1 `DELETE /api/area_conocimiento/{id}`

```http
DELETE /api/area_conocimiento/219
```

Respuesta:

```http
HTTP/1.1 200 OK
```

```json
{
  "estado": 200,
  "mensaje": "Registro eliminado exitosamente.",
  "filasEliminadas": 1
}
```

---

## 7.2 `DELETE /api/universidad/{id}`

```http
DELETE /api/universidad/7
```

```json
{
  "estado": 200,
  "mensaje": "Registro eliminado exitosamente.",
  "filasEliminadas": 1
}
```

---

## 7.3 `DELETE /api/aspecto_normativo/{id}`

```http
DELETE /api/aspecto_normativo/1
```

```json
{
  "estado": 200,
  "mensaje": "Registro eliminado exitosamente.",
  "filasEliminadas": 1
}
```

---

## 7.4 `DELETE /api/practica_estrategia/{id}`

```http
DELETE /api/practica_estrategia/1
```

```json
{
  "estado": 200,
  "mensaje": "Registro eliminado exitosamente.",
  "filasEliminadas": 1
}
```

---

## 7.5 `DELETE /api/enfoque/{id}`

```http
DELETE /api/enfoque/1
```

```json
{
  "estado": 200,
  "mensaje": "Registro eliminado exitosamente.",
  "filasEliminadas": 1
}
```

---

## 7.6 `DELETE /api/car_innovacion/{id}`

```http
DELETE /api/car_innovacion/1
```

```json
{
  "estado": 200,
  "mensaje": "Registro eliminado exitosamente.",
  "filasEliminadas": 1
}
```

---

## 7.7 `DELETE /api/aliado/{nit}`

```http
DELETE /api/aliado/900123456
```

Respuesta:

```http
HTTP/1.1 200 OK
```

```json
{
  "estado": 200,
  "mensaje": "Registro eliminado exitosamente.",
  "filasEliminadas": 1
}
```

### Segundo DELETE

Si se intenta eliminar nuevamente un registro que ya tiene:

```text
activo = 0
```

la API debe responder:

```http
HTTP/1.1 404 Not Found
```

```json
{
  "estado": 404,
  "mensaje": "Registro no encontrado.",
  "detalle": "No existe un registro activo con nit = 900123456."
}
```

---

# 8. Validaciones de los cuerpos de petición

Las peticiones de creación, reemplazo y actualización deben validar los tipos, obligatoriedad y longitud de los campos antes de ejecutar SQL.

## 8.1 `area_conocimiento`
| Campo | Crear | PUT | PATCH | Regla |
|---|---|---|---|---|
| `id` | ✓ | URL | URL | Entero positivo |
| `gran_area` | ✓ | ✓ | Opcional | Obligatorio, máximo 60 caracteres |
| `area` | ✓ | ✓ | Opcional | Obligatorio, máximo 60 caracteres |
| `disciplina` | ✓ | ✓ | Opcional | Obligatorio, máximo 60 caracteres |

## 8.2 `universidad`

| Campo    | Crear | PUT |   PATCH  | Regla                             |
|---|---|---|---|---|
| `id`     |   ✓   | URL |    URL   | Entero positivo                   |
| `nombre` |   ✓   |  ✓  | Opcional | Obligatorio, máximo 60 caracteres |
| `tipo`   |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |
| `ciudad` |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |

## 8.3 `aspecto_normativo`

| Campo         | Crear | PUT |   PATCH  | Regla                             |
|---|---|---|---|---|
| `id`          |   ✓   | URL |    URL   | Entero positivo                   |
| `tipo`        |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |
| `descripcion` |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |
| `fuente`      |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |

## 8.4 `practica_estrategia`

| Campo         | Crear | PUT |   PATCH  | Regla                             |
|---|---|---|---|---|
| `id`          |   ✓   | URL |    URL   | Entero positivo                   |
| `tipo`        |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |
| `nombre`      |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |
| `descripcion` |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |

## 8.5 `enfoque`

| Campo         | Crear | PUT |   PATCH  | Regla                             |
|---|---|---|---|-----------------------------------|
| `id`          |   ✓   | URL |    URL   | Entero positivo                   |
| `nombre`      |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |
| `descripcion` |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres |

## 8.6 `car_innovacion`

| Campo         | Crear | PUT |   PATCH  | Regla                                                |
|---|---|---|---|---||---|---|---|---|---||---|---|---|---|---||---|---|---|---|---||---|---|---|
| `id`          |   ✓   | URL |    URL   | Entero positivo                                      |
| `nombre`      |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres                    |
| `descripcion` |   ✓   |  ✓  | Opcional | Obligatorio, sin límite práctico definido por la API |
| `tipo`        |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres                    |

## 8.7 `aliado`

| Campo             | Crear | PUT |   PATCH  | Regla                                                 |
|---|---|---|---|---||---|---|---|---|---||---|---|---|---|---||---|---|---|---|---||---|---|---|---|
| `nit`             |   ✓   | URL |    URL   | Entero positivo                                       |
| `razon_social`    |   ✓   |  ✓  | Opcional | Obligatorio, máximo 60 caracteres                     |
| `nombre_contacto` |   ✓   |  ✓  | Opcional | Obligatorio, máximo 60 caracteres                     |
| `correo`          |   ✓   |  ✓  | Opcional | Obligatorio, máximo 70 caracteres y formato de correo |
| `telefono`        |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres                     |
| `ciudad`          |   ✓   |  ✓  | Opcional | Obligatorio, máximo 45 caracteres                     |

---

# 9. Contraste entre `PUT` y `PATCH`

La diferencia entre ambos verbos es contractual:

### PUT

Requiere todos los campos modificables:

```http
PUT /api/aliado/900123456
```

```json
{
  "razon_social": "Empresa Nueva",
  "nombre_contacto": "Juan Pérez",
  "correo": "juan@empresa.com",
  "telefono": "3001234567",
  "ciudad": "Medellín"
}
```

Si falta un campo obligatorio:

```text
422
```

### PATCH

Solo requiere los campos que se quieran modificar:

```http
PATCH /api/aliado/900123456
```

```json
{
  "telefono": "3009998877"
}
```

Resultado:

```text
200
```

El resto de los campos permanece sin cambios.

### Body vacío en PATCH

```json
{}
```

Resultado:

```text
400
```

```json
{
  "estado": 400,
  "mensaje": "Parámetros inválidos.",
  "detalle": "No se envió ningún campo para actualizar."
}
```

---

# 10. Swagger

La API proporciona documentación interactiva mediante Swagger UI.

```http
GET /swagger
```

Swagger debe permitir visualizar y probar los endpoints definidos para las siete tablas de la versión 1.

---

# 11. Resumen de endpoints

Cada una de las siete tablas dispone de seis operaciones CRUD:

| Verbo  | Ruta                | Operación               | Éxito |
| ------ | ------------------- | ----------------------- | ----: |
| GET    | `/api/{tabla}`      | Listar                  |   200 |
| GET    | `/api/{tabla}/{id}` | Obtener uno             |   200 |
| POST   | `/api/{tabla}`      | Crear                   |   200 |
| PUT    | `/api/{tabla}/{id}` | Reemplazar completo     |   200 |
| PATCH  | `/api/{tabla}/{id}` | Actualizar parcialmente |   200 |
| DELETE | `/api/{tabla}/{id}` | Eliminación lógica      |   200 |

En `aliado`, `{id}` se sustituye por `{nit}`:

| Verbo  | Ruta                |
| ------ | ------------------- |
| GET    | `/api/aliado`       |
| GET    | `/api/aliado/{nit}` |
| POST   | `/api/aliado`       |
| PUT    | `/api/aliado/{nit}` |
| PATCH  | `/api/aliado/{nit}` |
| DELETE | `/api/aliado/{nit}` |

Además:

```text
GET /
GET /swagger
```

corresponden al diagnóstico y documentación interactiva de la API.

---

# 12. Matriz de comportamiento HTTP

| Operación     | 200 | 204 | 400 | 404 | 422 | 500 |
|---|---|---|---|---|
| GET lista     |  ✓  |  ✓  |  ✓  |     |     |  ✓  |
| GET por llave |  ✓  |     |     |  ✓  |     |  ✓  |
| POST          |  ✓  |     |     |     |  ✓  |  ✓  |
| PUT           |  ✓  |     |     |  ✓  |  ✓  |  ✓  |
| PATCH         |  ✓  |     |  ✓  |  ✓  |  ✓  |  ✓  |
| DELETE        |  ✓  |     |     |  ✓  |     |  ✓  |

---

# 13. Reglas fundamentales de la v1

1. Solo se exponen las siete tablas sin llave foránea.
2. `area_conocimiento` inicia con 218 registros.
3. `universidad` inicia con 6 registros.
4. `aspecto_normativo`, `practica_estrategia`, `enfoque`, `car_innovacion` y `aliado` inician vacías.
5. Las llaves primarias son enteros positivos y no utilizan `IDENTITY`.
6. El cliente proporciona `id` al crear registros de las seis tablas correspondientes.
7. El cliente proporciona `nit` al crear un `aliado`.
8. Los registros nuevos comienzan con `activo = 1`.
9. Las consultas normales solo muestran registros activos.
10. `DELETE` realiza borrado lógico mediante `activo = 0`.
11. `POST` utiliza una petición completa de creación.
12. `PUT` exige todos los campos modificables.
13. `PATCH` acepta únicamente los campos que se desean modificar.
14. Un `PATCH` sin campos produce `400`.
15. Un body inválido produce `422` antes de acceder a SQL Server.
16. Una llave primaria duplicada es rechazada por SQL Server y se reporta como `500`.
17. Un registro inexistente produce `404`.
18. Una tabla sin registros activos produce `204` en el listado.
19. Todos los accesos a datos son asíncronos.
20. El SQL utilizado por los repositorios es parametrizado.
21. La lógica de negocio pertenece a la API y no a triggers o procedimientos almacenados.
22. La API utiliza SQL Server mediante Dapper.
23. Swagger está disponible en `/swagger`.
24. El endpoint de diagnóstico está disponible en `/`.
25. La versión de estos contratos corresponde exclusivamente a la v1.
