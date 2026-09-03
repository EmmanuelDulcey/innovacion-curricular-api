# Mapa de versiones — Innovación Curricular

> La ruta completa del proyecto. Cada versión se especifica SOLO cuando la
> anterior está cerrada (commit + tag). Este mapa da la dirección; las
> specs de cada versión dan el detalle.

| Versión | Qué agrega | Estado |
|---|---|---|
| **v1** | CRUD completo de las **7 tablas sin FK**: `area_conocimiento`, `universidad`, `aspecto_normativo`, `practica_estrategia`, `enfoque`, `car_innovacion`, `aliado`. Catálogos del Excel cargados. | **En curso** ([spec](versiones/v1_innovacion_curricular/2_spec.md)) |
| **v2** | CRUD de las **15 tablas con FK**: `facultad`, `programa`, `acreditacion`, `registro_calificado`, `activ_academica`, `pasantia`, `premio`, `docente_departamento`, `alianza`, y las tablas puente (`programa_ac`, `programa_pe`, `programa_ci`, `an_programa`, `enfoque_rc`, `aa_rc`). | Sin especificar |
| **v3** | Gestión de usuarios: `POST /api/login` con JWT; middleware de autenticación y autorización; menú por roles; CRUD de `usuario`, `rol`, `rol_usuario` solo para administradores; logout. | Sin especificar |
| **v4** | Aplicativo completo: 10 consultas multitabla (mínimo 4 tablas cada una), dashboard con gráficos, páginas corporativas con imagen propia, responsive/PWA, publicación en servidor gratuito. | Sin especificar |

> **El destino del proyecto:** el módulo Innovación Curricular queda COMPLETO y publicado; cada versión intermedia es un paso deliberado de ese camino.

**Reglas del mapa** (constitución, Artículo 1): no se anticipa nada de una
versión futura; una versión cerrada no se reabre (los ajustes van en la
siguiente); el repositorio siempre muestra la versión en curso funcionando.
