# Mapa de versiones — Innovación Curricular

> La ruta completa del proyecto. Cada versión se especifica SOLO cuando la
> anterior está cerrada (commit + tag). Este mapa da la dirección; las
> specs de cada versión dan el detalle. **Cada versión entrega API + Frontend
> funcionando de punta a punta.**

| Versión | API (backend) | Frontend (Angular) | Estado |
|---|---|---|---|
| **v1** | CRUD de las **7 tablas sin FK**: `area_conocimiento`, `universidad`, `aspecto_normativo`, `practica_estrategia`, `enfoque`, `car_innovacion`, `aliado`. Catálogos del Excel cargados. | Angular: dashboard con métricas de los 7 catálogos; CRUD visual (listar, crear, editar, eliminar) de cada tabla. | **En curso** ([spec](v1_producto_sqlserver/2_spec.md)) |
| **v2** | CRUD de las **15 tablas con FK**: `facultad`, `programa`, `acreditacion`, `registro_calificado`, `activ_academica`, `pasantia`, `premio`, `docente_departamento`, `alianza` y tablas puente (`programa_ac`, `programa_pe`, `programa_ci`, `an_programa`, `enfoque_rc`, `aa_rc`). | CRUD visual de las 15 tablas con selects que resuelven las FK; dashboard ampliado. | Sin especificar |
| **v3** | `POST /api/login` con JWT; middleware de autenticación/autorización; CRUD de `usuario`, `rol`, `rol_usuario` solo para administradores. | Login Angular; menú por roles; guardias de ruta; vistas admin de usuarios/roles. | Sin especificar |
| **v4** | 10 consultas multitabla (mínimo 4 tablas cada una). | Dashboard con gráficos; páginas corporativas; responsive/PWA; deploy. | Sin especificar |

> **El destino del proyecto:** el módulo Innovación Curricular queda COMPLETO y publicado; cada versión intermedia es un paso deliberado de ese camino.

**Reglas del mapa** (constitución, Artículo 1):
- No se anticipa nada de una versión futura; una versión cerrada no se reabre
  (los ajustes van en la siguiente).
- El repositorio siempre muestra la versión en curso funcionando (API + Front).
- Cada versión se cierra con un único tag `vN` en el repositorio (monorepo:
  backend en `api_innovacion/`, Frontend en `frontend/`).
