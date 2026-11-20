// =============================================================================
// numero-animado.ts
// Muestra un número que "cuenta" desde 0 hasta su valor en ~600 ms usando
// requestAnimationFrame. El dashboard lo reutiliza en indicadores, totales,
// tarjetas y barras. Si el sistema pide movimiento reducido
// (prefers-reduced-motion: reduce), muestra el valor final sin animación.
// =============================================================================
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, input, signal } from '@angular/core';

const DURACION_MS = 600;

@Component({
  selector: 'app-numero-animado',
  template: '{{ mostrado() }}',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NumeroAnimado implements OnInit, OnDestroy {
  readonly valor = input.required<number>();

  // Valor que se pinta en pantalla; empieza en 0 y avanza hasta `valor`.
  readonly mostrado = signal(0);

  private frame = 0;

  ngOnInit() {
    const destino = this.valor();

    // Accesibilidad: sin animación si el usuario la desactivó en su sistema.
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      this.mostrado.set(destino);
      return;
    }

    const inicio = performance.now();
    const paso = (ahora: number) => {
      // Progreso 0..1 y suavizado "ease out cubic": rápido al inicio y
      // desacelera al final, para que el número no se sienta mecánico.
      const progreso = Math.min((ahora - inicio) / DURACION_MS, 1);
      const suave = 1 - Math.pow(1 - progreso, 3);
      this.mostrado.set(Math.round(destino * suave));
      if (progreso < 1) this.frame = requestAnimationFrame(paso);
    };
    this.frame = requestAnimationFrame(paso);
  }

  ngOnDestroy() {
    cancelAnimationFrame(this.frame);
  }
}
