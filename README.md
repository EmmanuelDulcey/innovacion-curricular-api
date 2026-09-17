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
