import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavBar } from './components/nav-bar/nav-bar';
import { BarraSuperior } from './components/barra-superior/barra-superior';

// Layout raíz: menú lateral fijo + barra superior global + contenido de rutas.
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavBar, BarraSuperior],
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {}
