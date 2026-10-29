import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { nombreLegible, TABLAS } from '../../models/catalogo';

// Menú lateral de navegación (antes barra horizontal; ahora columna a la
// izquierda). Las opciones SIGUEN generándose solas desde la lista TABLAS: al
// agregar una tabla nueva aparece sola, sin tocar este componente.
@Component({
  selector: 'app-nav-bar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.css',
})
export class NavBar {
  readonly tablas = TABLAS;
  readonly nombreLegible = nombreLegible;

  // Estado del menú en PANTALLAS ANGOSTAS: arranca escondido y el botón de
  // hamburguesa (☰) lo abre y lo cierra. En escritorio este estado no hace
  // nada: el CSS mantiene el menú siempre visible (ver nav-bar.css).
  readonly abierto = signal(false);

  alternar() {
    this.abierto.update((valor) => !valor);
  }

  // Al elegir una opción se cierra el menú: en el celular el menú tapa el
  // contenido y debe desaparecer al navegar (en escritorio no tiene efecto).
  cerrar() {
    this.abierto.set(false);
  }
}
