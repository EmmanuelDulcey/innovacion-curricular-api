import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

// Componente raíz: solo hospeda el router. El layout privado (menú + barra)
// vive en Layout y el login se renderiza a pantalla completa sin ese layout.
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
})
export class App {}
