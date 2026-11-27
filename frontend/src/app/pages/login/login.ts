// =============================================================================
// login.ts
// Pantalla pública de inicio de sesión. Envía usuario y contraseña a
// /api/login, guarda el JWT en la sesión y vuelve a la URL que el usuario
// intentaba abrir (returnUrl). No muestra códigos HTTP ni detalles internos.
// =============================================================================
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Icono } from '../../components/icono/icono';
import { MensajeError } from '../../components/mensaje-error/mensaje-error';
import { ApiError } from '../../models/catalogo';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, Icono, MensajeError],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly ruta = inject(ActivatedRoute);

  readonly username = signal('');
  readonly password = signal('');
  readonly verPassword = signal(false);
  readonly cargando = signal(false);
  readonly error = signal<ApiError | null>(null);
  readonly anio = new Date().getFullYear();

  readonly puedeEnviar = computed(
    () =>
      this.username().trim().length > 0 &&
      this.password().length > 0 &&
      !this.cargando(),
  );

  actualizarUsername(evento: Event) {
    const entrada = evento.target;
    if (entrada instanceof HTMLInputElement) this.username.set(entrada.value);
  }

  actualizarPassword(evento: Event) {
    const entrada = evento.target;
    if (entrada instanceof HTMLInputElement) this.password.set(entrada.value);
  }

  alternarPassword() {
    this.verPassword.update((valor) => !valor);
  }

  async enviar(): Promise<void> {
    if (!this.puedeEnviar()) return;

    this.cargando.set(true);
    this.error.set(null);

    try {
      await firstValueFrom(this.auth.iniciarSesion(this.username().trim(), this.password()));
      await this.router.navigateByUrl(this.urlDeRetorno());
    } catch (fallo) {
      const apiError = fallo as Partial<ApiError> | null;
      this.error.set({
        estado: apiError?.estado ?? 0,
        mensaje:
          apiError?.estado === 0
            ? 'No se pudo conectar con la API.'
            : (apiError?.mensaje ?? 'No se pudo iniciar sesión.'),
        detalle: apiError?.detalle,
        errores: apiError?.errores,
      });
    } finally {
      this.cargando.set(false);
    }
  }

  // Solo se aceptan rutas internas: evita que un returnUrl externo convierta
  // el login en un redirect abierto.
  private urlDeRetorno(): string {
    const returnUrl = this.ruta.snapshot.queryParamMap.get('returnUrl');
    if (!returnUrl || !returnUrl.startsWith('/') || returnUrl.startsWith('//')) return '/';
    return returnUrl === '/login' ? '/' : returnUrl;
  }
}
