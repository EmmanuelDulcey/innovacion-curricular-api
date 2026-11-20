// =============================================================================
// estado-ui.ts
// Estado SOLO de interfaz que comparten tres piezas del layout:
//  - el dashboard lo actualiza después de cargar (conexión, hora de la última
//    sincronización y filas del CSV);
//  - la barra superior lo lee para buscar y exportar;
//  - el menú lateral lo lee para mostrar "Servidor conectado" o "Sin conexión".
// No hace peticiones ni conoce la API: no reemplaza ni modifica ApiService.
// =============================================================================
import { Injectable, signal } from '@angular/core';

// Fila del CSV de exportación: una por catálogo, con su sección y su total.
export interface FilaExportable {
  seccion: string;
  catalogo: string;
  total: number;
}

@Injectable({ providedIn: 'root' })
export class EstadoUI {
  // Término del buscador de la barra superior. El dashboard lo usa para
  // filtrar las tarjetas por nombre (solo frontend).
  readonly busqueda = signal('');

  // ¿Falló alguna petición del dashboard por falta de conexión con la API?
  // El menú lateral lo muestra como punto rojo en el pie.
  readonly sinConexion = signal(false);

  // Hora real en que terminó la última carga del dashboard.
  readonly ultimaSincronizacion = signal<Date | null>(null);

  // Datos ya cargados, listos para armar el CSV desde el navegador.
  readonly filasExportables = signal<FilaExportable[]>([]);

  actualizarBusqueda(valor: string) {
    this.busqueda.set(valor);
  }

  // Llamado una sola vez por el dashboard al terminar forkJoin.
  registrarCargue(filas: FilaExportable[], sinConexion: boolean) {
    this.filasExportables.set(filas);
    this.sinConexion.set(sinConexion);
    this.ultimaSincronizacion.set(new Date());
  }
}
