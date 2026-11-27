import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Rutas privadas: sin sesión vigente se vuelve al login y se conserva la URL
// original para regresar después de autenticarse.
export const authGuard: CanActivateFn = (_ruta, estado) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.estaAutenticado()) return true;

  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: estado.url },
  });
};
