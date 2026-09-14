import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { nombreLegible, TABLAS } from '../../models/catalogo';

// Barra de navegacion (9_frontend.md §5): marca, "Dashboard" y un enlace por tabla.
@Component({
  selector: 'app-nav-bar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.css',
})
export class NavBar {
  readonly tablas = TABLAS;
  readonly nombreLegible = nombreLegible;
}