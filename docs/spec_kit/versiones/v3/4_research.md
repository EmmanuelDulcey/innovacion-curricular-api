# Investigación y decisiones — Versión 3: control de acceso

> **Versión 3** · **Lectura opcional** (el porqué de cada decisión del plan,
> con las alternativas evaluadas y descartadas). Complementa a
> [3_plan.md](3_plan.md); el modelo de datos exacto está en
> [5_data_model.md](5_data_model.md).

---

## D1 — JWT sin estado vs. sesiones guardadas en la base

**Contexto.** ¿Cómo recuerda la API que un usuario ya inició sesión?

| Opción | Argumento | ¿Sirve? |
|---|---|---|
| Sesión en servidor (tabla `sesion` con un id de sesión) | Se puede invalidar en cualquier momento (logout real) | Descartada por ahora: agrega una tabla y una consulta extra en **cada** petición solo para confirmar que el token sigue vigente |
| **JWT firmado, sin estado** | El servidor valida la firma y la expiración sin tocar la base | **Sí.** Es lo que la guía del curso espera en v3 y es consistente con una API sin sesiones pegajosas (`docker compose` puede reiniciar la API sin perder sesiones activas) |

**Decisión: JWT sin estado (HS256), expiración de 480 minutos.**

**Consecuencia aceptada:** `POST /api/auth/logout` no invalida el token en
el servidor; solo le indica al cliente que lo descarte. Si un token robado
sigue siendo válido, funciona hasta que expire — no hay revocación. Esto se
documenta como limitación conocida (ver [2_spec.md](2_spec.md) §2
"No incluye").

## D2 — bcrypt para contraseñas

**Contexto.** El Artículo 10 de la constitución exige "contraseñas
hasheadas (bcrypt o equivalente)".

| Opción | Comentario |
|---|---|
| SHA-256 + sal manual | Hay que implementar la sal y el "costo" a mano; fácil de hacer mal |
| **bcrypt** (`BCrypt.Net-Next`) | Sal integrada en el propio hash, costo configurable (work factor), librería madura |

**Decisión: bcrypt, work factor 12.** Cada `Hashear()` genera una sal nueva
(dos hashes de la misma contraseña son distintos — se verifica en
`PruebasAutenticacion.cs`). `Verificar()` atrapa `SaltParseException` y
`ArgumentException` para que un hash corrupto o un dato mal migrado no
tumbe el login con un 500.

## D3 — Mensaje genérico de login y "hash de relleno"

**Contexto.** Si el login respondiera mensajes distintos para "usuario no
existe" y "contraseña incorrecta", cualquiera podría **enumerar** usuarios
válidos probando nombres. Si además el tiempo de respuesta fuera distinto
(verificar el hash solo cuando el usuario existe), la diferencia de tiempo
delataría lo mismo.

**Decisión:**
- Un único mensaje 401: `"Usuario o contraseña incorrectos."`, sin importar
  si el usuario no existe, está inactivo o la contraseña es incorrecta.
- Cuando el usuario no existe, el servicio verifica igual un **hash de
  relleno** (`HashDeRelleno`, una contraseña aleatoria que nunca se usa para
  autenticar) para que el costo de cómputo de bcrypt sea el mismo en los dos
  casos.

**Alternativa descartada:** devolver 404 para "usuario no existe" — más
"RESTful" en apariencia, pero revela directamente qué usuarios existen.

## D4 — 401 vs. 403: dos razones de rechazo distintas

**Contexto.** Dos situaciones parecen "no autorizado" pero son conceptualmente
distintas: no tener sesión, y tener sesión pero no el rol correcto.

| Código | Significa | Quién lo genera |
|---|---|---|
| **401** | No hay sesión válida (sin token, token vencido, firma inválida) | El pipeline de autenticación/autorización de ASP.NET, **antes** de llegar al controlador |
| **403** | Hay sesión válida, pero el rol no alcanza para escribir | `RestringirEscrituraMiddleware`, **después** de confirmar que sí hay sesión |

**Decisión:** mantener los dos códigos separados en vez de responder 401 en
ambos casos. Le permite al Frontend distinguir "inicia sesión" (401 →
redirigir a `/login`) de "no tienes permiso" (403 → mostrar el motivo sin
cerrar la sesión).

## D5 — Límite de intentos de login (rate limiting)

**Contexto.** Sin límite, el endpoint de login es vulnerable a fuerza bruta
de contraseñas.

| Opción | Comentario |
|---|---|
| Sin límite | Descartado: permite probar miles de contraseñas por minuto |
| Ventana global (todos los clientes comparten el cupo) | Descartado: un atacante agotaría el cupo de todos los usuarios legítimos (DoS fácil) |
| **Ventana fija de 10/minuto por IP** | Aísla el límite por origen; si no se puede resolver la IP, usa una partición `"desconocido"` compartida en vez de una global |

**Decisión:** `FixedWindowRateLimiter`, 10 intentos por minuto por IP,
`QueueLimit = 0` (rechaza de inmediato con 429, no hace esperar la petición
en una cola).

## D6 — Dónde vive la regla de "quién puede escribir"

**Contexto.** La regla de roles podría aplicarse con `[Authorize(Roles =
"...")]` en cada controlador, o de forma centralizada.

| Opción | Comentario |
|---|---|
| `[Authorize(Roles = "...")]` por controlador/acción | Hay que repetirlo en los ~22 controladores de v1/v2 y mantenerlo sincronizado si la lista de roles cambia |
| **Un middleware + `PoliticaRoles` centralizada** | Un solo lugar define "qué roles escriben"; se aplica igual a **todas** las tablas sin tocar los controladores existentes |

**Decisión:** middleware + clase estática `PoliticaRoles`. Ventaja directa
para esta versión: **ningún controlador de v1/v2 se modificó** para aplicar
la regla (cero riesgo de romper su regresión).

## D7 — Migración de contraseñas de demostración

**Contexto.** Los seeders (`08_seeders.sql`) insertan usuarios con la
contraseña `Demo123!`. `09_seguridad_v3.sql` necesita convertir eso a bcrypt
sin pisar una contraseña que alguien ya haya cambiado.

**Decisión:** el `UPDATE` de cada usuario de demostración lleva
`WHERE password = N'Demo123!'` — solo reemplaza si la contraseña **sigue**
siendo el literal de demostración. Si un usuario ya tiene un hash bcrypt (lo
cambió, o el script ya corrió antes), el `UPDATE` no afecta ninguna fila:
el script es seguro de repetir (`docker compose down -v && up` o un rerun
manual).

## D8 — `DateTimeOffset` para `expira`

**Gotcha.** La API tiene un `SoloFechaJsonConverter` que serializa
`DateTime` como `"yyyy-MM-dd"` (lo que esperan los campos de fecha de
v1/v2). Si `RespuestaLogin.Expira` fuera `DateTime`, ese converter global le
recortaría la hora y la zona, y el Frontend no podría calcular el
vencimiento real de la sesión. Por eso `Expira` es `DateTimeOffset`: evita el
converter global y conserva hora y zona completas.

## D9 — Alcance pendiente (explícito, no un olvido)

El CRUD admin de `usuario`/`rol`/`rol_usuario` **no se investigó ni se
planeó** en este documento porque no entró en este incremento (ver
[2_spec.md](2_spec.md) §2). Cuando se construya, su investigación (¿se
reasignan roles con PUT o con un endpoint de "asignar rol" como las tablas
puente de v2?) debe agregarse aquí, no reabrir D1-D8.
