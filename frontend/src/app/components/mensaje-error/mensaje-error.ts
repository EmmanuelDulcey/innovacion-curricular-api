// =============================================================================
// QUÉ ES: componente reutilizable para presentar errores de la API.
// PAPEL: muestra mensajes y errores de validación en español sin códigos HTTP.
// =============================================================================
import { Component, computed, input } from '@angular/core';
import { ApiError } from '../../models/catalogo';

// Muestra errores con palabras comprensibles, sin exponer códigos HTTP en la
// interfaz; las validaciones siguen apareciendo junto al campo correspondiente.
@Component({
  selector: 'app-mensaje-error',
  imports: [],
  templateUrl: './mensaje-error.html',
  styleUrl: './mensaje-error.css',
})
export class MensajeError {
  readonly error = input<ApiError | null>(null);

  readonly erroresPorCampo = computed(() => this.error()?.errores ?? []);
  readonly detalle = computed(() => this.error()?.detalle ?? null);
}
