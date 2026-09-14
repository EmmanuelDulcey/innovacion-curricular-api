import { Component, computed, input } from '@angular/core';
import { ApiError } from '../../models/catalogo';

// Muestra ApiError de forma uniforme (9_frontend.md §5 y §7):
// banner general para errores (400/404/500/0) y lista por campo para 422.
@Component({
  selector: 'app-mensaje-error',
  imports: [],
  templateUrl: './mensaje-error.html',
  styleUrl: './mensaje-error.css',
})
export class MensajeError {
  readonly error = input<ApiError | null>(null);

  readonly erroresPorCampo = computed(() => this.error()?.errores ?? []);
  readonly detalle = computed(() => {
    const err = this.error();
    return err?.detalle ? `${err.estado ? `[${err.estado}] ` : ''}${err.detalle}` : null;
  });
}