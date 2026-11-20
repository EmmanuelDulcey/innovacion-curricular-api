# innovacion-curricular
Proyecto de aula — Módulo Innovación Curricular. **Monorepo** (backend + frontend) con metodología SDD por versiones.

# Proyecto de Aula — Monorepo (API + Frontend)

Este repositorio contiene todo el sistema del módulo Innovación Curricular bajo la metodología **Spec-Driven Development (SDD)**: el **backend** en **C# con SQL Server** y el **frontend** en **Angular**, separados por carpetas.

## 📘 Estructura
- `docs/spec_kit/` → documentación y especificaciones por versión.
- `api_innovacion/` → backend (API REST en C# / ASP.NET Core).
  - `Controllers/` → controladores de la API.
  - `Services/` → lógica de negocio.
  - `Repositories/` → acceso a datos.
  - `Models/` → modelos de dominio.
  - `Requests/` → objetos de petición y validaciones.
- `frontend/` → frontend (Angular): dashboard y CRUD visual.
- `db/` → scripts de la base de datos.
- `.env.example` → variables de entorno ficticias (ejemplo).

## ⚙️ Reglas del repositorio
- Repositorio **privado** con acceso al profesor `ccastro2050`.
- Secretos gestionados mediante **variables de entorno** (nunca en el código).
- Cada versión se cierra con un **tag único** (`v1`, `v2`, etc.) que cubre backend y frontend.
- Flujo de trabajo con ramas: cada estudiante trabaja en su rama, el encargado hace el merge a `main`.

## 🚀 Objetivo
Implementar el módulo Innovación Curricular completo (API + Frontend) siguiendo buenas prácticas de arquitectura y control de versiones.

## 🔐 Autenticación (v3)

- `POST /api/login` recibe `{ "username", "password" }` y devuelve un **JWT** (`token`, `expira`, `usuario`).
- El resto de endpoints exige `Authorization: Bearer <token>`. La única excepción es `GET /` (diagnóstico).
- Las contraseñas se almacenan como **hash bcrypt** (work factor 12); nunca en texto plano.
- **Roles y permisos**:
  - `Administrador`, `Coordinador académico` y `Secretaría académica` pueden crear, editar y retirar.
  - `Docente` y `Consulta` son de solo lectura (la API responde `403` ante cualquier escritura).
- El secreto JWT se configura con `Jwt:Secret` (`appsettings.json` para desarrollo) o con la variable de entorno `Jwt__Secret` / `JWT_SECRET` en Docker.
- El login tiene un límite de **10 intentos por minuto por IP**.

### Credenciales de demostración

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Demo123!` | Administrador |
| `coordinador` | `Demo123!` | Coordinador académico |
| `secretaria` | `Demo123!` | Secretaría académica |
| `docente.demo` | `Demo123!` | Docente (solo lectura) |
| `consulta` | `Demo123!` | Consulta (solo lectura) |

> Cambie estas credenciales y el `JWT_SECRET` antes de cualquier despliegue real.
