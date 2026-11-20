// =============================================================================
// auth.interceptor.ts
// Adjunta el JWT a cada petición y, ante un 401, limpia la sesión y vuelve al
// login. Se usa `Injector` para resolver AuthService dentro del manejador y
// evitar un ciclo de dependencias HttpClient -> interceptor -> AuthService.
// =============================================================================
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { SesionStorage } from './sesion-storage';

export const authInterceptor: HttpInterceptorFn = (peticion, siguiente) => {
  const almacen = inject(SesionStorage);
  const router = inject(Router);
  const injector = inject(Injector);

  const esApi = peticion.url.startsWith(environment.apiUrl);
  const esLogin = peticion.url.endsWith('/api/login');
  const token = almacen.token();
  const conToken =
    token && esApi && !esLogin
      ? peticion.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : peticion;

  return siguiente(conToken).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && esApi && !esLogin) {
        injector.get(AuthService).cerrarSesion(false);
        if (!router.url.startsWith('/login')) {
          const returnUrl = router.url;
          void router.navigate(['/login'], {
            queryParams: returnUrl && returnUrl !== '/' ? { returnUrl } : {},
          });
        }
      }

      return throwError(() => error);
    }),
  );
};
