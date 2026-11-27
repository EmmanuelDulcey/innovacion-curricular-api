# Tareas — Versión 3: control de acceso

> **Versión 3** · Orden de construcción, partiendo de v1 y v2 funcionando.
> Requisitos: [2_spec.md](2_spec.md) · técnica: [3_plan.md](3_plan.md) ·
> contratos: [6_contracts.md](6_contracts.md) · validación final:
> [7_quickstart.md](7_quickstart.md) · cierre: [9_checklist.md](9_checklist.md).
>
> Este documento refleja el trabajo **ya hecho** en `rama-valentina`
> (Fases 0 a 4) y deja explícita la Fase 5, **pendiente**, antes de poder
> poner el tag `v3`.

---

## Fase 0 — Especificación y base de datos (Valentina)

- [x] Escribir el spec kit de la v3 (`2_spec.md` … `9_checklist.md`) en
  `docs/spec_kit/versiones/v3/`.
- [x] `db/09_seguridad_v3.sql`: `sp_consultar_usuario_por_username`,
  `sp_listar_roles_usuario` y la migración idempotente de las contraseñas
  de demostración a bcrypt.
- [x] Seeders de `rol` y `usuario` en `db/08_seeders.sql` (5 roles, 5
  usuarios, sus filas en `rol_usuario`).
- [x] Actualizar `db/init.sh` para correr `09_seguridad_v3.sql` después de
  `08_seeders.sql`.
- **Verificado:** `docker compose down -v && docker compose up -d --build`
  deja los 5 usuarios con `password` en formato `$2a$...`.

## Fase 1 — Autenticación en la API (Valentina)

- [x] `Modelos/Usuario.cs`, `Modelos/SesionUsuario.cs`,
  `Peticiones/LoginPeticion.cs`.
- [x] `Repositorios/IRepositorioUsuario.cs` +
  `RepositorioUsuarioSqlServer.cs` (solo `CommandType.StoredProcedure`).
- [x] `Servicios/IServicioHashContrasena.cs` + `ServicioHashContrasena.cs`
  (bcrypt).
- [x] `Configuracion/JwtOpciones.cs`, `Servicios/ITokenJwt.cs` +
  `ServicioTokenJwt.cs` (emisión HS256).
- [x] `Excepciones/NoAutorizadoExcepcion.cs` → 401.
- [x] `Servicios/IServicioAutenticacion.cs` + `ServicioAutenticacion.cs`
  (mensaje genérico + hash de relleno, ver [4_research.md](4_research.md) D3).
- [x] `Respuestas/RespuestasAuth.cs` (`RespuestaLogin`,
  `UsuarioAutenticadoRespuesta`).
- [x] `Controllers/AuthController.cs`: `POST /api/login`,
  `GET /api/auth/me`, `POST /api/auth/logout`.
- [x] `Program.cs`: configuración de `AddAuthentication().AddJwtBearer(...)`,
  política de respaldo `RequireAuthenticatedUser`, `AddRateLimiter` (10/min
  por IP en `/api/login`), DI de las piezas nuevas.
- **Verificado:** smoke test de login/sesión por `curl`
  ([7_quickstart.md](7_quickstart.md) §3-4).

## Fase 2 — Autorización por rol en escritura (Valentina)

- [x] `Seguridad/PoliticaRoles.cs` (lista de roles que escriben).
- [x] `Middlewares/RestringirEscrituraMiddleware.cs`: 403 si el verbo no es
  de lectura y el rol no alcanza.
- [x] Registrar el middleware en `Program.cs` **después** de
  `UseAuthorization` (ver [3_plan.md](3_plan.md) §3).
- **Verificado:** con token de `docente.demo`, un `POST`/`PUT`/`DELETE`
  sobre cualquier tabla de v1/v2 → 403; con `admin` → funciona igual que
  antes de la v3 (regresión).

## Fase 3 — Pruebas de capa sin SQL Server (Valentina)

- [x] `pruebas/PruebasAutenticacion.cs`: hash (sal distinta, verificación
  correcta/incorrecta, hash malformado no rompe), login correcto, los tres
  rechazos 401 (contraseña, usuario inexistente, usuario inactivo), los dos
  rechazos 400 (vacío, contraseña > 72 bytes), usuario sin rol (se autentica
  pero no escribe), la política de roles para los 5 roles, y la emisión real
  de un JWT (3 partes, claims, expiración futura).
- **Verificado:** `pruebas/Programa.cs` ejecuta
  `PruebasAutenticacion.EjecutarAsync()` junto con las pruebas de v1/v2 y
  reporta todo en verde.

## Fase 4 — Frontend (Valentina)

- [x] `models/auth.ts`: contratos de login/sesión y `ROLES_ESCRITURA`
  (espejo de `PoliticaRoles`).
- [x] `services/sesion-storage.ts`: única puerta a `localStorage`, descarta
  sola una sesión vencida o corrupta.
- [x] `services/auth.service.ts`: estado de sesión en `signal`, `usuario`,
  `roles`, `puedeEscribir`, `esAdministrador` como `computed`.
- [x] `services/auth.interceptor.ts`: adjunta el `Bearer`; ante un 401 limpia
  la sesión y redirige a `/login`.
- [x] `guards/auth.guard.ts` y `guards/invitado.guard.ts`.
- [x] `pages/login/` (componente + plantilla + estilos).
- [x] `layout/` (envoltura de las rutas privadas: barra superior + menú).
- [x] `app.routes.ts`: `/login` pública, todo lo demás bajo `Layout` con
  `authGuard`.
- [x] `app.config.ts`: registrar `authInterceptor`.
- [x] Ocultar crear/editar/eliminar en `pages/catalogo` y
  `pages/programa-detalle` cuando `puedeEscribir()` es falso.
- [x] `barra-superior.ts`: mostrar nombre, iniciales y rol de la sesión
  real; botón de cerrar sesión.
- **Verificado:** smoke test de Frontend
  ([7_quickstart.md](7_quickstart.md) §7).

## Fase 5 — CRUD admin de usuarios y roles (pendiente)

> **Bloquea el tag `v3`.** El mapa de versiones incluye esta fase dentro de
> la v3 (no es v4); sin ella, el control de acceso existe pero nadie puede
> gestionar usuarios desde la interfaz.

- [ ] `Controllers/UsuarioController.cs` + `Controllers/RolController.cs`:
  CRUD restringido a `Administrador` (reutilizar
  `RestringirEscrituraMiddleware` no sirve aquí porque la restricción es por
  **un** rol específico, no por los tres de escritura — definir la regla en
  [2_spec.md](2_spec.md) antes de construir).
- [ ] Procedimientos `sp_crear_usuario`, `sp_actualizar_usuario`,
  `sp_eliminar_usuario` (hashear la contraseña **antes** de llamarlos, nunca
  en SQL) y equivalentes para `rol`.
- [ ] Endpoint para asignar/retirar un rol a un usuario (mismo patrón
  asignar/retirar de las tablas puente de v2 — ver
  [`../v2/6_contracts.md`](../v2/6_contracts.md) §8).
- [ ] Pantalla admin en el Frontend (ruta nueva, visible solo si
  `auth.esAdministrador()`).
- [ ] Actualizar [2_spec.md](2_spec.md), [6_contracts.md](6_contracts.md),
  [9_checklist.md](9_checklist.md) y este documento con el resultado.
- **Verificar:** un administrador crea un usuario, le asigna un rol de
  escritura, y ese usuario puede escribir de inmediato (sin reiniciar la
  API); un no-administrador no puede acceder a estas rutas (403).

## Cierre

- [ ] Todo lo de la Fase 5 en verde.
- [ ] `9_checklist.md` marcado en verde por una persona.
- [ ] Actualizar [`../0_mapa_versiones.md`](../0_mapa_versiones.md) (fila v3
  → "Completada").
- [ ] Tag `v3` en `main`.
