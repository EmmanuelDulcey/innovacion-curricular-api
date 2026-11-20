// =============================================================================
// auth.service.ts
// Estado de sesión de la aplicación: inicia sesión contra /api/login, persiste
// el JWT y expone usuario/roles como señales. La UI decide qué mostrar con
// `puedeEscribir`; la API vuelve a validar el rol en cada petición de escritura.
// =============================================================================
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import {
  puedeEscribirPorRoles,
  RespuestaLogin,
  SesionGuardada,
} from '../models/auth';
import { ApiService } from './api.service';
import { SesionStorage } from './sesion-storage';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly almacen = inject(SesionStorage);

  readonly sesion = signal<SesionGuardada | null>(this.almacen.leerSesion());
  readonly usuario = computed(() => this.sesion()?.usuario ?? null);
  readonly roles = computed(() => this.usuario()?.roles ?? []);
  readonly estaAutenticado = computed(() => this.sesion() !== null);
  readonly puedeEscribir = computed(() => puedeEscribirPorRoles(this.roles()));
  readonly esAdministrador = computed(() =>
    this.roles().some((rol) => rol.toLocaleLowerCase() === 'administrador'),
  );

  iniciarSesion(username: string, password: string): Observable<RespuestaLogin> {
    return this.api
      .iniciarSesion(username, password)
      .pipe(
        tap((respuesta) => {
          const sesion: SesionGuardada = {
            token: respuesta.token,
            expira: respuesta.expira,
            usuario: respuesta.usuario,
          };
          this.almacen.guardarSesion(sesion);
          this.sesion.set(sesion);
        }),
      );
  }

  cerrarSesion(navegar: boolean = true): void {
    this.almacen.limpiarSesion();
    this.sesion.set(null);
    if (navegar) void this.router.navigate(['/login']);
  }
}
