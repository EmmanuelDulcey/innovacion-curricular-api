# Quickstart — Versión 2: arranque y smoke test

Este documento explica cómo levantar la **v2** de Innovación Curricular (API +
Frontend) y ejecutar un *smoke test*, haciendo énfasis en **lo nuevo**: los
**409**, las **tablas puente** y el **disparador**.

La v2 usa el mismo entorno que la v1: C# / ASP.NET Core, SQL Server, Dapper,
Docker Compose y Swagger. Lo nuevo es que el SQL vive en **procedimientos
almacenados** y que hay **15 tablas con clave foránea**.

> **Antes de empezar:** la v1 debe estar cerrada con tag `v1` y sus datos base
> cargados (la v2 necesita catálogos en las 7 tablas sin FK para los
> desplegables).

---

## 1. Requisitos previos

- Git, Docker Desktop, .NET SDK, navegador o `curl`.
- Comprobar:

```bash
docker --version
docker compose version
git --version
```

## 2. Levantar la v2

Desde la raíz del proyecto:

```bash
docker compose up -d --build
```

El inicializador corre, **en orden e idempotentemente**:

1. `db/innovacion_curricular.ss.sql` (25 tablas, provisto — intacto)
2. `db/01_activar_borrado_logico.sql` (v1: `activo` en 7 tablas)
3. `db/02_datos_iniciales.sql` (v1: catálogos base)
4. **`db/03_activar_borrado_logico_v2.sql`** (`activo` en las 15 tablas + `num_acreditaciones`)
5. **`db/04_procedimientos_v2_*.sql`** (procedimientos + disparador)

## 3. Verificar los contenedores

```bash
docker compose ps
docker compose logs api-innovacion
```

## 4. Verificar la API (diagnóstico)

```bash
curl -i http://localhost:8036/
```

```json
{
  "mensaje": "API Innovación Curricular funcionando",
  "version": "v2",
  "contratos": "docs/spec_kit/versiones/v2/6_contracts.md"
}
```

Swagger: <http://localhost:8036/swagger> (incluye las 15 tablas de la v2).

## 5. Smoke test de lectura con nombres de FK

```bash
curl -i http://localhost:8036/api/facultad
```

Debe responder 200 con la envoltura `{tabla, limite, total, datos}` y, en cada
fila, el nombre de la universidad (`universidad_nombre`). Una tabla vacía → 204.

## 6. Smoke test del CRUD de `facultad` (tabla de Emmanuel)

### 6.1 Crear (POST) — devuelve la fila guardada

```bash
curl -X POST http://localhost:8036/api/facultad \
  -H "Content-Type: application/json" \
  -d '{
    "id": 100,
    "nombre": "Facultad de Prueba",
    "tipo": "Pública",
    "fecha_fun": "1990-01-15",
    "universidad": 1
  }'
```

Respuesta esperada: 200 con `{estado, mensaje, datos:{…}}` (la fila recién
creada).

### 6.2 Consultar / PUT / PATCH

```bash
curl -i http://localhost:8036/api/facultad/100
curl -X PUT http://localhost:8036/api/facultad/100 \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Facultad de Prueba 2","tipo":"Privada","fecha_fun":"1990-01-15","universidad":1}'
curl -X PATCH http://localhost:8036/api/facultad/100 \
  -H "Content-Type: application/json" -d '{"tipo":"Pública"}'
```

### 6.3 Eliminación lógica (DELETE)

```bash
curl -X DELETE http://localhost:8036/api/facultad/100
```

200 con `filasEliminadas: 1`. Un **segundo** `DELETE` → **404**.

## 7. Smoke test de los 409 (lo nuevo)

### 7.1 FK inexistente → 409

```bash
curl -X POST http://localhost:8036/api/facultad \
  -H "Content-Type: application/json" \
  -d '{"id":101,"nombre":"X","tipo":"Pública","fecha_fun":"1990-01-15","universidad":99999}'
```

```json
{ "estado": 409, "mensaje": "Conflicto con los datos.",
  "detalle": "La universidad indicada no existe o está inactiva." }
```

### 7.2 Pareja duplicada en puente → 409

```bash
# Asignar (200)
curl -X POST http://localhost:8036/api/programa_ac \
  -H "Content-Type: application/json" -d '{"programa":10,"area_conocimiento":3}'
# Repetir (409)
curl -X POST http://localhost:8036/api/programa_ac \
  -H "Content-Type: application/json" -d '{"programa":10,"area_conocimiento":3}'
```

### 7.3 Padre con hijos activos → 409

Con un `programa` que tenga, por ejemplo, una `pasantia` activa:

```bash
curl -X DELETE http://localhost:8036/api/programa/10
```

```json
{ "estado": 409, "mensaje": "No se puede retirar el registro.",
  "detalle": "El programa tiene registros asociados activos; primero retírelos." }
```

## 8. Smoke test de tablas puente (asignar / retirar)

```bash
curl -i http://localhost:8036/api/programa_ac
curl -X POST   http://localhost:8036/api/programa_ac -H "Content-Type: application/json" \
  -d '{"programa":10,"area_conocimiento":3}'      # asignar → 200
curl -X DELETE http://localhost:8036/api/programa_ac/10/3   # retirar → 200
curl -X DELETE http://localhost:8036/api/programa_ac/10/3   # repetir → 404
```

## 9. Smoke test del disparador (contador de acreditaciones)

El disparador se prueba **por la interfaz** (ver [9_frontend.md](9_frontend.md)),
pero en la API se comprueba así, sin que la API escriba el contador:

```bash
# 1) Ver el contador actual del programa
curl -s http://localhost:8036/api/programa/10        # fijarse en num_acreditaciones

# 2) Crear una acreditación del programa 10 (POST /api/acreditacion)

# 3) Volver a leer el programa: num_acreditaciones subió EN 1 (lo puso el disparador)

# 4) Retirar la acreditación (DELETE /api/acreditacion/{resolucion})

# 5) Leer el programa: num_acreditaciones bajó EN 1 (el disparador escucha el UPDATE)
```

El paso 5 es el que prueba que el disparador **no es sordo al `UPDATE`**
([4_research.md](4_research.md) D3, tropiezo 6).

## 10. Verificar validaciones (422) y recurso inexistente (404)

```bash
curl -X POST http://localhost:8036/api/facultad -H "Content-Type: application/json" \
  -d '{"id":102,"nombre":"","tipo":"Pública","fecha_fun":"1990-01-15","universidad":1}'   # 422
curl -i http://localhost:8036/api/facultad/999999    # 404
```

## 11. Probar desde el Frontend

<http://localhost:8037>

- **Dashboard**: una tarjeta por tabla operable (7 de v1 + 15 de v2).
- **`/tablas/facultad`**: crear/editar/retirar, con el desplegable de
  universidad cargado desde la API.
- **`/tablas/pasantia`**: desplegable de `programa`; probar «(ninguna)» donde
  aplique (`activ_academica.disenio`).
- **Maestro-detalle de `programa`**: abrir un programa y agregar su detalle sin
  salir de la pantalla.
- **Tablas puente**: sólo **asignar** y **retirar** (sin botón editar).

## 12. Detener / reiniciar

```bash
docker compose down          # detiene (conserva datos)
docker compose down -v       # detiene y borra la BD
docker compose up -d --build # reconstruye desde cero
```

## 13. Problemas comunes

- **Docker no está corriendo**: abrir Docker Desktop.
- **Puerto 8036/8037 ocupado**: revisar `docker compose ps` y cerrar el proceso.
- **La API no responde**: `docker compose logs api-innovacion` (¿compiló?).
- **`sqlserver-init` falló**: revisar `docker compose logs sqlserver-init`
  (algún script de `db/` con error).
- **Un desplegable sale vacío**: la tabla maestra no tiene activos (cargar datos
  base o crear primero el maestro).

## 14. Checklist final del smoke test de la v2

- [ ] Docker corriendo; `docker compose up -d --build` termina bien.
- [ ] `docker compose ps` muestra todo activo.
- [ ] `GET /` responde `"version":"v2"`.
- [ ] Swagger disponible con las 15 tablas.
- [ ] Listar `facultad`/`programa` con `*_nombre` (nombres de FK).
- [ ] CRUD de `facultad` completo (POST devuelve la fila; DELETE lógico).
- [ ] **FK inexistente → 409.**
- [ ] **Pareja duplicada en puente → 409.**
- [ ] **Padre con hijos activos → 409.**
- [ ] Asignar / retirar en una tabla puente (sin editar).
- [ ] **El disparador cambia `num_acreditaciones` al asignar y al retirar.**
- [ ] Body inválido → 422; recurso inexistente → 404.
- [ ] La regresión de la v1 sigue en verde ([9_checklist.md](9_checklist.md)).
