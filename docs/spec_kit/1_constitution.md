# Constitución del proyecto — Innovación Curricular

> **Documento permanente.** Estas reglas rigen TODAS las versiones del
> proyecto de aula. Cada versión tiene además su propia especificación en
> [versiones/](versiones/0_mapa_versiones.md); ante conflicto, la
> constitución prevalece.

---

## Artículo 1 — El curso es POR VERSIONES y la especificación manda

- El sistema se construye por **versiones incrementales** (v1, v2, v3, v4),
  cada una con su spec kit propio (documentos 2 a 8).  
- Una versión está TERMINADA solo cuando pasa sus criterios de aceptación;
  entonces se hace commit, **tag** (`v1`, `v2`, …) y solo después se escribe
  la spec siguiente.  
- **No se anticipa**: nada de una versión futura se construye antes de tiempo
  (ej. JWT en v1, dashboard en v2).  
- El repositorio siempre contiene la **versión en curso funcionando**.

---

## Artículo 2 — Stack: C# y ASP.NET Core con SQL Server

- Lenguaje **C#** sobre **ASP.NET Core**.  
- Acceso a datos con **Dapper** (SQL escrito a mano, parametrizado).  
- Paquetes externos permitidos desde la v1:  
  - `Microsoft.Data.SqlClient`  
  - `Dapper`  
  - `Swashbuckle.AspNetCore` (Swagger).  
- La base de datos es **SQL Server** con el script provisto en
  `db_scripts/sqlserver/innovacion_curricular.ss.sql`.

---

## Artículo 3 — Arquitectura en capas con interfaces

- El controlador no toca SQL.  
- El servicio no conoce HTTP ni el motor.  
- El repositorio no conoce HTTP.  
- Los contratos son `interface` de C#.  
- El ensamblador (`Program.cs`) registra dependencias.  
- Errores traducidos a HTTP:  
  - `ArgumentException` → 400  
  - `NoEncontradoExcepcion` → 404  
  - `SqlException` → 500  
  - Body inválido → 422  

---

## Artículo 4 — Un solo comando

`docker compose up -d --build` deja TODO el sistema de la versión funcionando.  
El código corre con `dotnet watch`: guardar un `.cs` recompila y reinicia.

---

## Artículo 5 — La base de datos viene DADA

La BD `innovacion_curricular` se crea **completa** desde la v1 con los scripts provistos.  
Lo que crece por versiones es la API.  
El código de cada versión solo puede nombrar las tablas que su spec le permite.

---

## Artículo 6 — Idioma y documentación

- Nombres, rutas, mensajes y documentación: **en inglés** (código y API).  
- Comentarios explicativos en el código para claridad académica.  
- El repositorio es material de estudio, no solo software.

---

## Artículo 7 — Contratos exactos

- Los endpoints, formatos y códigos de estado de cada versión están en su
  `6_contracts.md` y se cumplen al pie de la letra.  
- CRUD por tabla:  
  - `GET /api/{tabla}`  
  - `GET /api/{tabla}/{id}`  
  - `POST /api/{tabla}`  
  - `PUT /api/{tabla}/{id}`  
  - `DELETE /api/{tabla}/{id}` (borrado lógico).  

---

## Artículo 8 — Convenciones fijas

| Cosa | Convención |
|---|---|
| Puertos del proyecto | API **8036** · SQL Server **11467** |
| Rutas | `/swagger` (documentación interactiva) · `/api/{tabla}` |
| Nombres | PascalCase en inglés; interfaces con prefijo `I`; carpetas `Controllers/ Models/ Requests/ Services/ Repositories/ Exceptions/ Tests/` |
| Respuesta | Lecturas: `{table, limit, total, data}` · Errores: `{status, message, detail}` (+ `errors:[…]` en 422) |
| Errores | Body inválido → **422** · ArgumentException → **400** · NotFound → **404** · SqlException → **500** |
| Secretos | Variables de entorno (`DB_CONNECTION`, `JWT_SECRET`) en `.env` (ignorado); `.env.example` con valores ficticios en el repo |

---

## Artículo 9 — Git y GitHub

- Repositorios privados (API y Frontend).  
- Profesor `ccastro2050` invitado como colaborador desde el primer día.  
- Nadie trabaja en `main`.  
- Cada estudiante en su rama (`rama-emmanuel`, `rama-jorge`, etc.).  
- Solo el encargado del main integra PRs.  
- Commits pequeños, frecuentes y descriptivos (en español).  
- Cada versión se cierra con tag `vN`.

---

## Artículo 10 — Seguridad y secretos

- Cero secretos en el código.  
- Contraseñas de usuarios **hasheadas** (bcrypt o equivalente).  
- JWT y roles implementados en la v3.  
- En la v4, secretos configurados en el servidor de publicación.  

---

# Mapa de versiones — Innovación Curricular

| Versión | Qué agrega | Cierre |
|---|---|---|
| **v1** | CRUD completo (API + Front) de las **7 tablas sin FK**: `area_conocimiento`, `universidad`, `aspecto_normativo`, `practica_estrategia`, `enfoque`, `car_innovacion`, `aliado`. Catálogos del Excel cargados. | Los 7 CRUD funcionan de punta a punta; borrado lógico filtrando inactivos; tag `v1`. |
| **v2** | CRUD de las **15 tablas con FK**: facultad, programa, acreditacion, registro_calificado, activ_academica, pasantia, premio, docente_departamento, alianza, y las tablas puente (`programa_ac`, `programa_pe`, `programa_ci`, `an_programa`, `enfoque_rc`, `aa_rc`). | Los 22 CRUD funcionan; validación de integridad referencial; tag `v2`. |
| **v3** | Gestión de usuarios: `POST /api/login` con JWT; middleware de autenticación y autorización; menú por roles; CRUD de usuario/rol/rol_usuario solo para administradores; logout. | Un usuario "consulta" no puede escribir; solo el admin ve usuarios/roles; contraseñas hasheadas; tag `v3`. |
| **v4** | Aplicativo completo: 10 consultas multitabla (mínimo 4 tablas cada una), dashboard con gráficos, páginas corporativas con imagen propia, responsive/PWA, publicación en servidor gratuito. | Sitio publicado y funcional con secretos en variables de entorno del servidor; tag `v4`. |

