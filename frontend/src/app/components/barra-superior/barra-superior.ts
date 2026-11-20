// =============================================================================
// barra-superior.ts
// Barra blanca global (todas las páginas): migas de pan según la ruta,
// buscador de catálogos, exportación CSV armada en el navegador y bloque de
// usuario. No hace peticiones: usa el estado de interfaz compartido.
// =============================================================================
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter } from 'rxjs/operators';
import { nombreLegible } from '../../models/catalogo';
import { EstadoUI } from '../../services/estado-ui';
import { Icono } from '../icono/icono';

interface Miga {
  texto: string;
  enlace: string | null; // null = tramo actual (no clickeable)
}

@Component({
  selector: 'app-barra-superior',
  imports: [RouterLink, Icono],
  templateUrl: './barra-superior.html',
  styleUrl: './barra-superior.css',
})
export class BarraSuperior {
  readonly estado = inject(EstadoUI);
  private readonly router = inject(Router);

  // URL actual como señal, para que las migas reaccionen a cada navegación.
  private readonly urlActual = signal(this.router.url);

  // En móvil el buscador queda escondido detrás del ícono de lupa.
  readonly buscadorAbierto = signal(false);

  // TODO v3: tomar del token cuando exista login.
  readonly usuario = {
    iniciales: 'US',
    nombre: 'Usuario',
    rol: 'Innovación Curricular',
  };

  // Migas de pan: en el dashboard las tres del diseño; en un catálogo, el
  // nombre legible de la tabla; en el detalle, la ruta de programas.
  readonly migas = computed<Miga[]>(() => {
    const url = this.urlActual().split('?')[0];

    if (url === '/' || url === '') {
      return [
        { texto: 'Inicio', enlace: '/' },
        { texto: 'Dashboard', enlace: null },
        { texto: 'Resumen de catálogos activos', enlace: null },
      ];
    }

    const partes = url.split('/').filter(Boolean);
    if (partes[0] === 'tablas' && partes[1]) {
      return [
        { texto: 'Inicio', enlace: '/' },
        { texto: nombreLegible(decodeURIComponent(partes[1])), enlace: null },
      ];
    }
    if (partes[0] === 'programas') {
      return [
        { texto: 'Inicio', enlace: '/' },
        { texto: 'Programas', enlace: '/tablas/programa' },
        { texto: 'Detalle del programa', enlace: null },
      ];
    }
    return [{ texto: 'Inicio', enlace: '/' }];
  });

  constructor() {
    this.router.events
      .pipe(
        filter((evento) => evento instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.urlActual.set(this.router.url));
  }

  actualizarBusqueda(evento: Event) {
    const entrada = evento.target;
    if (entrada instanceof HTMLInputElement) this.estado.actualizarBusqueda(entrada.value);
  }

  alternarBuscador() {
    this.buscadorAbierto.update((valor) => !valor);
  }

  // CSV generado en el navegador con los datos REALES ya cargados por el
  // dashboard: una fila por catálogo con su sección y su total. Se agrega el
  // BOM (﻿) para que Excel reconozca los acentos.
  exportarCsv() {
    const filas = this.estado.filasExportables();
    const escapar = (valor: string | number) => `"${String(valor).replace(/"/g, '""')}"`;
    const lineas = [
      ['seccion', 'catalogo', 'total'].map(escapar).join(','),
      ...filas.map((fila) =>
        [fila.seccion, fila.catalogo, fila.total].map(escapar).join(','),
      ),
    ];

    const blob = new Blob(['﻿' + lineas.join('\r\n')], {
      type: 'text/csv;charset=utf-8;',
    });
    const url = URL.createObjectURL(blob);
    const enlace = document.createElement('a');
    enlace.href = url;
    enlace.download = 'catalogos-innovacion-curricular.csv';
    enlace.click();
    URL.revokeObjectURL(url);
  }
}
