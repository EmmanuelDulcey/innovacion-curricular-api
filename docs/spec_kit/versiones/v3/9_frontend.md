# Especificación del Frontend — Versión 3: control de acceso

> **Documento 9 de la v3.** Describe la interfaz de usuario de la versión.
> Rige la constitución ([../../1_constitution.md](../../1_constitution.md)),
> se detalla en [3_plan.md](3_plan.md) §6 y consume exactamente los contratos
> de [6_contracts.md](6_contracts.md). Es un **delta** de
> [`../v2/9_frontend.md`](../v2/9_frontend.md): todo lo que v1/v2 especifican
> y no se contradice aquí sigue vigente.

---

## 1. Propósito

Hasta la v2, cualquiera podía abrir el Frontend y operar las 22 tablas. La
v3 agrega:

1. Una pantalla de **login**, pública, fuera del layout privado.
2. Un **layout privado** que envuelve el resto de la app y exige sesión.
3. **Guardias de ruta**: `/login` rechaza a quien ya tiene sesión;
   todo lo demás rechaza a quien no la tiene.
4. Un **interceptor HTTP** que adjunta el JWT a cada petición a la API y
   reacciona a un 401 cerrando la sesión.
5. **Ocultar** (no solo deshabilitar) los botones de crear/editar/eliminar
   cuando el rol del usuario es de solo lectura.
6. Mostrar el usuario real (nombre, iniciales, rol) en la barra superior.

Corre en `frontend/`, puerto **8037**, consume la API en
`http://localhost:8036`, igual que v1/v2.

## 2. Rutas

```ts
// app.routes.ts
{ path: 'login', component: Login, canActivate: [invitadoGuard] },
{
  path: '',
  component: Layout,
  canActivate: [authGuard],
  children: [
    { path: '', component: Dashboard },
    { path: 'tablas/:tabla', component: Catalogo },
    { path: 'programas/:id', component: ProgramaDetalle },
    { path: '**', component: PaginaNoEncontrada },
  ],
},
```

| Ruta | Guardia | Comportamiento |
|---|---|---|
| `/login` | `invitadoGuard` | Con sesión activa, redirige a `/` |
| `/`, `/tablas/:tabla`, `/programas/:id`, `**` | `authGuard` | Sin sesión, redirige a `/login?returnUrl=<ruta>` |

El login **no** usa `Layout` (sin barra superior ni menú); todo lo demás
cuelga de `Layout`.

## 3. `AuthService` — estado de sesión

```ts
readonly sesion = signal<SesionGuardada | null>(this.almacen.leerSesion());
readonly usuario = computed(() => this.sesion()?.usuario ?? null);
readonly roles = computed(() => this.usuario()?.roles ?? []);
readonly estaAutenticado = computed(() => this.sesion() !== null);
readonly puedeEscribir = computed(() => puedeEscribirPorRoles(this.roles()));
readonly esAdministrador = computed(() => this.roles().some(r => r.toLocaleLowerCase() === 'administrador'));
```

- `iniciarSesion(username, password)`: llama a `POST /api/login`, guarda el
  resultado en `SesionStorage` y actualiza la señal.
- `cerrarSesion(navegar = true)`: limpia el almacenamiento, vacía la señal
  y, si `navegar` es verdadero, redirige a `/login`. El interceptor llama a
  esta función con `navegar = false` para no duplicar la navegación.

`puedeEscribir` **replica** `PoliticaRoles.PuedeEscribir` del backend
(`ROLES_ESCRITURA` en `models/auth.ts` debe coincidir con
`PoliticaRoles.RolesEscritura` — ver [4_research.md](4_research.md) D6). Es
una ayuda de interfaz: la API vuelve a decidir con el **403** real.

## 4. `SesionStorage` — persistencia

Única puerta a `localStorage` (clave `innovacion-curricular.sesion`). Una
sesión sin `token`/`expira`/`usuario`, o con `expira` ya pasado, se
descarta sola al leerla — así la app nunca arranca con una sesión vencida
"viva" en la señal.

## 5. `authInterceptor` — JWT y expiración

```ts
export const authInterceptor: HttpInterceptorFn = (peticion, siguiente) => {
  // adjunta Authorization: Bearer <token> a toda petición a la API,
  // EXCEPTO /api/login (que todavía no tiene token que enviar)
  ...
  // ante un 401 de la API (no de /api/login): cierra sesión y redirige a
  // /login, conservando la URL actual como returnUrl
};
```

Usa `Injector` (no `inject(AuthService)` directo) para resolver
`AuthService` dentro del manejador de errores y evitar un ciclo
`HttpClient → interceptor → AuthService → HttpClient`.

No necesitó cambios para la revocación de sesión del backend (ver
[2_spec.md](2_spec.md) §3.4): un token revocado por logout responde 401
igual que uno vencido, así que el mismo manejo de 401 de arriba ya cubre
ambos casos sin lógica adicional en el Frontend.

## 6. Guardias de ruta

```ts
// auth.guard.ts — rutas privadas
export const authGuard: CanActivateFn = (_ruta, estado) =>
  auth.estaAutenticado() || router.createUrlTree(['/login'], { queryParams: { returnUrl: estado.url } });

// invitado.guard.ts — la pantalla de login
export const invitadoGuard: CanActivateFn = () =>
  auth.estaAutenticado() ? router.createUrlTree(['/']) : true;
```

## 7. Pantalla de login

`pages/login/`: formulario de usuario/contraseña con `FormsModule`, botón
para mostrar/ocultar la contraseña, estado `cargando` mientras espera la
API, y un mensaje de error (componente `MensajeError` reutilizado de v1/v2)
que nunca muestra código HTTP ni jerga — solo el `mensaje` del 401/400/429.

Al enviar con éxito, navega a `returnUrl` si es una ruta **interna**
(empieza por `/` y no por `//`) o a `/` en cualquier otro caso — evita que
un `returnUrl` externo convierta el login en un *open redirect*.

## 8. Ocultar acciones según el rol

En `pages/catalogo/catalogo.ts` y `pages/programa-detalle/programa-detalle.ts`:

```ts
readonly puedeEscribir = this.auth.puedeEscribir;
```

```html
@if (puedeEscribir()) {
  <!-- botones de crear / editar / retirar -->
}
```

Los manejadores de clic (`crear()`, `editar()`, `eliminar()`) también
verifican `if (!this.puedeEscribir()) return;` al principio, como segunda
barrera en la propia pantalla — aunque la única barrera que importa de
verdad es el 403 del backend.

## 9. Barra superior — usuario real

`barra-superior.ts` deja de mostrar datos fijos y calcula, desde
`AuthService.usuario()`:

- **Iniciales**: primera letra del primer y del último nombre.
- **Nombre**: `nombre_completo` o, si falta, `username`.
- **Rol**: `rolPrincipal(roles)` — los roles unidos con " · " si hay más de
  uno, o `"Sin rol"` si no tiene ninguno.

Incluye el botón de **cerrar sesión**, que llama a
`AuthService.cerrarSesion()`.

