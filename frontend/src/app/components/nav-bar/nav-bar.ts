import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { nombreLegible, TABLAS } from '../../models/catalogo';
import { Seccion, seccionesDe } from '../../models/secciones';
import { EstadoUI } from '../../services/estado-ui';
import { Icono } from '../icono/icono';

// Menú lateral de navegación. Las opciones siguen generándose solas desde
// TABLAS; la agrupación y los colores de los puntos vienen de
// models/secciones.ts, compartido con el dashboard.
@Component({
  selector: 'app-nav-bar',
  imports: [RouterLink, RouterLinkActive, Icono],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.css',
})
export class NavBar {
  readonly nombreLegible = nombreLegible;
  readonly estado = inject(EstadoUI);

  // Grupos ordenados como en el diseño: Institución, Programas, Innovación,
  // Alianzas (ordenMenu), y "Otros" al final si apareciera una tabla suelta.
  readonly grupos: Seccion[] = seccionesDe(TABLAS).sort((a, b) => a.ordenMenu - b.ordenMenu);

  // Versión visible en el pie del menú.
  readonly version = 'v2';

  // Estado del menú en PANTALLAS ANGOSTAS: arranca escondido y el botón de
  // hamburguesa lo abre y lo cierra. En escritorio el CSS lo mantiene fijo.
  readonly abierto = signal(false);

  // Grupos plegables: empiezan desplegados; el chevron los abre y cierra.
  readonly gruposAbiertos = signal<Set<string>>(new Set(this.grupos.map((grupo) => grupo.id)));

  alternar() {
    this.abierto.update((valor) => !valor);
  }

  // Al elegir una opción se cierra el menú móvil (en escritorio no tiene efecto).
  cerrar() {
    this.abierto.set(false);
  }

  grupoAbierto(id: string): boolean {
    return this.gruposAbiertos().has(id);
  }

  alternarGrupo(id: string) {
    const actuales = new Set(this.gruposAbiertos());
    if (actuales.has(id)) {
      actuales.delete(id);
    } else {
      actuales.add(id);
    }
    this.gruposAbiertos.set(actuales);
  }
}
