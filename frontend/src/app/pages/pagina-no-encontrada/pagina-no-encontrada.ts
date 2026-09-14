import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

// Pagina "no encontrada" (ruta **). Fuente: 9_frontend.md §3.
@Component({
  selector: 'app-pagina-no-encontrada',
  imports: [RouterLink],
  templateUrl: './pagina-no-encontrada.html',
  styleUrl: './pagina-no-encontrada.css',
})
export class PaginaNoEncontrada {}