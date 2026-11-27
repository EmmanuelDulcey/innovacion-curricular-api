# Quickstart — Versión 3: arranque y smoke test de control de acceso

Este documento explica cómo levantar la **v3** (API + Frontend) y comprobar
lo nuevo: **login, sesión, 401, 403 y 429**. Las 22 tablas de v1/v2 siguen
igual (ver [`../v2/7_quickstart.md`](../v2/7_quickstart.md)); aquí solo se
cubre el delta.

> **Antes de empezar:** v1 y v2 deben estar funcionando (sus catálogos
> cargados). La v3 no necesita datos adicionales de dominio: solo los 5
> usuarios de demostración que siembra `db/08_seeders.sql`.

---

## 1. Levantar la v3

```bash
docker compose up -d --build
```

El inicializador corre, en orden e idempotentemente, los scripts de v1 y v2
y, al final, el de esta versión:

```
...
8. db/08_seeders.sql           (5 roles + 5 usuarios de demostración)
9. db/09_seguridad_v3.sql      (procedimientos de login + hash de las contraseñas demo)
```

```bash
docker compose ps
docker compose logs api-innovacion
```

## 2. Usuarios de demostración

Los 5 usuarios sembrados, todos con contraseña `Demo123!`:

| Username | Rol | ¿Escribe? |
|---|---|---|
| `admin` | Administrador | Sí |
| `coordinador` | Coordinador académico | Sí |
| `secretaria` | Secretaría académica | Sí |
| `docente.demo` | Docente | No (solo lectura) |
| `consulta` | Consulta | No (solo lectura) |

## 3. Smoke test — login

```bash
curl -i -X POST http://localhost:8036/api/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Demo123!"}'
```

200 esperado:

```json
{
  "estado": 200,
  "mensaje": "Inicio de sesión exitoso.",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "tipo_token": "Bearer",
  "expira": "2026-11-27T20:00:00+00:00",
  "usuario": {
    "id": 1, "username": "admin", "email": "admin@innovacion.edu.co",
    "nombre_completo": "Administrador del sistema", "roles": ["Administrador"]
  }
}
```

Guarda el token en una variable para los siguientes pasos:

```bash
TOKEN_ADMIN=$(curl -s -X POST http://localhost:8036/api/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Demo123!"}' | python3 -c "import sys,json;print(json.load(sys.stdin)['token'])")
```

Repite con `docente.demo`/`Demo123!` y guarda `TOKEN_DOCENTE`.

### Credenciales inválidas → 401

```bash
curl -i -X POST http://localhost:8036/api/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"incorrecta"}'
```

```json
{ "estado": 401, "mensaje": "Usuario o contraseña incorrectos.", "detalle": "" }
```

## 4. Smoke test — sesión

```bash
curl -i http://localhost:8036/api/auth/me \
  -H "Authorization: Bearer $TOKEN_ADMIN"
```

200 con el usuario reconstruido desde el token. Sin header `Authorization`:

```bash
curl -i http://localhost:8036/api/auth/me
```

→ **401**, sin cuerpo (ver [6_contracts.md](6_contracts.md) §5).

```bash
curl -i -X POST http://localhost:8036/api/auth/logout \
  -H "Authorization: Bearer $TOKEN_ADMIN"
```

→ 200 `{"estado":200,"mensaje":"Sesión cerrada."}`.

## 5. Smoke test — autorización por rol (403)

Con el token de `admin`, el CRUD de v1/v2 funciona igual que antes de la v3:

```bash
curl -i -X POST http://localhost:8036/api/facultad \
  -H "Authorization: Bearer $TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Facultad de Prueba","tipo":"Pública","fecha_fun":"2024-01-01","universidad":1}'
```

→ 200, igual que en v2. Con el token de `docente.demo` (solo lectura), el
**mismo** cuerpo:

```bash
curl -i -X POST http://localhost:8036/api/facultad \
  -H "Authorization: Bearer $TOKEN_DOCENTE" \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Facultad de Prueba","tipo":"Pública","fecha_fun":"2024-01-01","universidad":1}'
```

```json
{ "estado": 403, "mensaje": "No tiene permisos para modificar registros.", "detalle": "Su rol es de solo lectura." }
```

`GET` con ese mismo token sigue en 200:

```bash
curl -i http://localhost:8036/api/facultad -H "Authorization: Bearer $TOKEN_DOCENTE"
```

## 6. Smoke test — límite de intentos (429)

```bash
for i in $(seq 1 11); do
  curl -s -o /dev/null -w "%{http_code}\n" -X POST http://localhost:8036/api/login \
    -H "Content-Type: application/json" \
    -d '{"username":"admin","password":"incorrecta"}'
done
```

Las primeras 10 líneas → `401`; la línea 11 → `429`. Esperar un minuto antes
de repetir el smoke test de login (la ventana se reinicia).

## 7. Smoke test — Frontend

1. Abrir `http://localhost:8037` sin sesión → redirige a `/login`.
2. Iniciar sesión con `docente.demo` / `Demo123!` → entra al dashboard, pero
   en `/tablas/facultad` **no** se ven los botones de crear/editar/eliminar.
3. Cerrar sesión, volver a entrar con `admin` / `Demo123!` → los mismos
   botones **sí** se ven y funcionan.
4. Con sesión activa, ir manualmente a `/login` → redirige de vuelta al
   dashboard.

## 8. Swagger

```http
GET /swagger
```

Incluye el candado **Bearer**: pegar un token obtenido del paso 3 para
probar cualquier ruta protegida desde la UI de Swagger.
