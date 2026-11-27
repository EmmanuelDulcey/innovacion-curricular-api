import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { invitadoGuard } from './guards/invitado.guard';
import { Layout } from './layout/layout';
import { Catalogo } from './pages/catalogo/catalogo';
import { Dashboard } from './pages/dashboard/dashboard';
import { Login } from './pages/login/login';
import { PaginaNoEncontrada } from './pages/pagina-no-encontrada/pagina-no-encontrada';
import { ProgramaDetalle } from './pages/programa-detalle/programa-detalle';

// Rutas de la v3:
//  * /login es pública y no usa el layout privado.
//  * Todo lo demás cuelga de Layout y exige sesión (authGuard).
//  * invitadoGuard evita que un usuario ya autenticado vuelva al login.
export const routes: Routes = [
  { path: 'login', component: Login, canActivate: [invitadoGuard] },
  {
    path: '',
    component: Layout,
    canActivate: [authGuard],
    children: [
      { path: '', component: Dashboard },
      { path: 'tablas/:tabla', component: Catalogo },
      { path: 'programas/:id', component: ProgramaDetalle },
      { path: '**', component: PaginaNoEncontrada },
    ],
  },
];
