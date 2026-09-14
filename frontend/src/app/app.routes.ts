import { Routes } from '@angular/router';
import { Dashboard } from './pages/dashboard/dashboard';
import { Catalogo } from './pages/catalogo/catalogo';
import { PaginaNoEncontrada } from './pages/pagina-no-encontrada/pagina-no-encontrada';

// Rutas base (9_frontend.md §3): / dashboard, /tablas/:tabla catalogo, ** no encontrada.
export const routes: Routes = [
  { path: '', component: Dashboard },
  { path: 'tablas/:tabla', component: Catalogo },
  { path: '**', component: PaginaNoEncontrada },
];