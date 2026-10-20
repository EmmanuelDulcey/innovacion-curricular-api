import { Routes } from '@angular/router';
import { Dashboard } from './pages/dashboard/dashboard';
import { Catalogo } from './pages/catalogo/catalogo';
import { ProgramaDetalle } from './pages/programa-detalle/programa-detalle';
import { PaginaNoEncontrada } from './pages/pagina-no-encontrada/pagina-no-encontrada';

// Rutas base (9_frontend.md §3): / dashboard, /tablas/:tabla catalogo,
// /programas/:id maestro-detalle de programa (v2) y ** no encontrada.
export const routes: Routes = [
  { path: '', component: Dashboard },
  { path: 'tablas/:tabla', component: Catalogo },
  { path: 'programas/:id', component: ProgramaDetalle },
  { path: '**', component: PaginaNoEncontrada },
];