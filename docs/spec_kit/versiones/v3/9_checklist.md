# Checklist de aceptación — Versión 3

> **Documento 9 (checklist) de la v3.** Marcar **a mano**.
> Base: [2_spec.md](2_spec.md) §6 · contratos: [6_contracts.md](6_contracts.md).
> **Nota:** esta versión, tal como está mergeada, no incluye el CRUD admin de
> `usuario`/`rol`/`rol_usuario` (ver [2_spec.md](2_spec.md) §2). No marcar el
> tag `v3` hasta que esa parte también esté lista.

---

## A. Autenticación

- [ ] `docker compose up -d --build` levanta todo; `db/09_seguridad_v3.sql`
  corre sin error tras los seeders.
- [ ] `SELECT LEFT(password, 4) FROM usuario` empieza por `$2a$` en los 5
  usuarios de demostración (contraseñas migradas a bcrypt).
- [ ] `POST /api/login` con `admin`/`Demo123!` → 200, token con 3 partes,
  `usuario.roles` incluye `Administrador`.
- [ ] Lo mismo para `coordinador`, `docente.demo`, `secretaria`, `consulta`.
- [ ] Contraseña incorrecta, usuario inexistente y usuario inactivo → **401**
  con el mismo mensaje genérico en los tres casos.
- [ ] Body vacío (`{}` o campos en blanco) → **400**.
- [ ] 11 intentos de login en menos de un minuto desde la misma IP → el 11.º
  → **429**.

## B. Sesión y rutas protegidas

- [ ] `GET /api/auth/me` con el token de login → 200 con el mismo usuario y
  roles.
- [ ] `GET /api/auth/me` sin token, o con token expirado/alterado → **401**.
- [ ] `POST /api/auth/logout` con token → 200.
- [ ] Cualquier ruta de las 22 tablas, sin header `Authorization` → **401**.
- [ ] `GET /` (diagnóstico) sigue respondiendo **sin** token.

## C. Autorización por rol en escritura

- [ ] Con token de `admin`, `coordinador` o `secretaria`: `POST`, `PUT`,
  `PATCH` y `DELETE` sobre tablas de v1/v2 funcionan igual que antes de la
  v3.
- [ ] Con token de `docente.demo` o `consulta`: el mismo `POST`/`PUT`/
  `PATCH`/`DELETE` → **403**, con `{estado, mensaje, detalle}`.
- [ ] Con esos mismos tokens de solo lectura, `GET` sigue en 200/204 (no se
  bloquea lectura por rol).
- [ ] Un usuario autenticado **sin ningún rol** asignado → solo lectura
  (403 en escritura), no un error distinto.

## D. Frontend

- [ ] `/login` pide usuario/contraseña y, con credenciales válidas, redirige
  al dashboard (o a `returnUrl` si venía de una ruta protegida).
- [ ] Con sesión activa, navegar a `/login` redirige al dashboard
  (`invitadoGuard`).
- [ ] Sin sesión, cualquier ruta privada redirige a
  `/login?returnUrl=...` (`authGuard`).
- [ ] Iniciar sesión como `docente.demo` o `consulta`: no se ven botones de
  crear/editar/eliminar en catálogo ni en el maestro-detalle de `programa`.
- [ ] Iniciar sesión como `admin`: los botones de escritura se ven y
  funcionan.
- [ ] Forzar un 401 (token vencido o manipulado en `localStorage`) → la app
  limpia la sesión y redirige a `/login`.
- [ ] La barra superior muestra nombre, iniciales y rol del usuario real de
  la sesión (no un dato fijo).
- [ ] Cerrar sesión limpia el almacenamiento y vuelve a `/login`.

## E. Regresión de v1 y v2 (no romper lo que ya funcionaba)

- [ ] Las 7 tablas de la v1 siguen con su CRUD completo (con un token de rol
  habilitado).
- [ ] Las 15 tablas de la v2 siguen con su CRUD/asignar-retirar, incluido el
  disparador de `programa.num_acreditaciones`.
- [ ] Ninguna ruta, verbo ni formato de respuesta de v1/v2 cambió.
- [ ] El dashboard y el catálogo siguen mostrando datos reales con un token
  válido.

## F. Pendiente para el tag `v3` (fuera de este merge)

- [ ] CRUD de `usuario` para administradores (crear, listar, editar,
  desactivar).
- [ ] CRUD de `rol` y de `rol_usuario` (asignar/retirar rol a un usuario)
  para administradores.
- [ ] Pantalla admin en el Frontend para gestionar usuarios y roles,
  visible solo para `Administrador`.
- [ ] Una vez lo anterior esté en verde: actualizar
  [`../0_mapa_versiones.md`](../0_mapa_versiones.md) (fila v3 → "Completada")
  y crear el tag `v3`.
