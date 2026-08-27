# innovacion-curricular-api
Proyecto de aula — Módulo Innovación Curricular (API REST). Backend con metodología SDD por versiones.

# Proyecto de Aula — Backend API

Este repositorio contiene el **backend** del módulo Innovación Curricular, desarrollado en **C# con SQL Server** bajo la metodología **Spec-Driven Development (SDD)**.

## 📘 Estructura
- `docs/spec_kit/` → documentación y especificaciones por versión.
- `src/Controllers/` → controladores de la API.
- `src/Services/` → lógica de negocio.
- `src/Repositories/` → acceso a datos.
- `src/Models/` → modelos de dominio.
- `src/Requests/` → objetos de petición y validaciones.
- `.env.example` → variables de entorno ficticias (ejemplo).

## ⚙️ Reglas del repositorio
- Repositorio **privado** con acceso al profesor `ccastro2050`.
- Secretos gestionados mediante **variables de entorno** (nunca en el código).
- Cada versión se cierra con un **tag** (`v1`, `v2`, etc.).
- Flujo de trabajo con ramas: cada estudiante trabaja en su rama, el encargado hace el merge a `main`.

## 🚀 Objetivo
Implementar el backend del módulo Innovación Curricular siguiendo buenas prácticas de arquitectura y control de versiones.
