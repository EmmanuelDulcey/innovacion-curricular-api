// =============================================================================
// sesion-storage.ts
// Única puerta al almacenamiento local de la sesión. No conoce HTTP ni Angular
// Router: la usan AuthService y el interceptor para evitar dependencias
// circulares. Una sesión vencida o corrupta se descarta sola.
// =============================================================================
import { Injectable } from '@angular/core';
import { SesionGuardada } from '../models/auth';

const CLAVE = 'innovacion-curricular.sesion';

@Injectable({ providedIn: 'root' })
export class SesionStorage {
  leerSesion(): SesionGuardada | null {
    if (typeof window === 'undefined') return null;

    try {
      const bruto = window.localStorage.getItem(CLAVE);
      if (!bruto) return null;

      const sesion = JSON.parse(bruto) as Partial<SesionGuardada> | null;
      const { token, expira, usuario } = sesion ?? {};
      if (!token || !expira || !usuario) {
        this.limpiarSesion();
        return null;
      }

      const vencimiento = new Date(expira).getTime();
      if (!Number.isFinite(vencimiento) || vencimiento <= Date.now()) {
        this.limpiarSesion();
        return null;
      }

      return { token, expira, usuario };
    } catch {
      this.limpiarSesion();
      return null;
    }
  }

  guardarSesion(sesion: SesionGuardada): void {
    if (typeof window === 'undefined') return;

    try {
      window.localStorage.setItem(CLAVE, JSON.stringify(sesion));
    } catch {
      // Si el navegador bloquea el almacenamiento, la sesión vive solo en
      // memoria hasta recargar; no se rompe el login.
    }
  }

  limpiarSesion(): void {
    if (typeof window === 'undefined') return;
    window.localStorage.removeItem(CLAVE);
  }

  token(): string | null {
    return this.leerSesion()?.token ?? null;
  }
}
