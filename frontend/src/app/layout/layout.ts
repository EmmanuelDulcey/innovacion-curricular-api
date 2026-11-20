import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BarraSuperior } from '../components/barra-superior/barra-superior';
import { NavBar } from '../components/nav-bar/nav-bar';

// Layout privado: menú lateral fijo + barra superior global + contenido de
// rutas. La pantalla de login vive FUERA de este layout (sin menú ni barra).
@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, NavBar, BarraSuperior],
  templateUrl: './layout.html',
  styleUrl: './layout.css',
})
export class Layout {}
