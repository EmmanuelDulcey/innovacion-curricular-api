import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiService } from '../../services/api.service';
import { TABLAS, nombreLegible, ApiError } from '../../models/catalogo';
import { CATALOGO_VISUAL, Seccion, VisualCatalogo, seccionesDe } from '../../models/secciones';
import { EstadoUI, FilaExportable } from '../../services/estado-ui';
import { NumeroAnimado } from './numero-animado';
import { Icono } from '../../components/icono/icono';

// Una tabla con su total y el aspecto visual que le toca en el dashboard.
interface Tarjeta {
  tabla: string;
  nombre: string;
  total: number;
  sinApi: boolean;
  error: boolean;
  visual: VisualCatalogo;
}

// Una sección ya resumida: sus tarjetas, la principal (la de más registros)
// y los totales que muestran los chips del encabezado.
interface ResumenSeccion extends Seccion {
  tarjetas: Tarjeta[];
  totalCatalogos: number;
  totalRegistros: number;
  principal?: Tarjeta;
  resto: Tarjeta[];
}

// Aspecto de respaldo si una tabla no está en CATALOGO_VISUAL.
const VISUAL_POR_DEFECTO: VisualCatalogo = {
  categoria: 'Catálogo',
  descripcion: 'Catálogo de apoyo del módulo.',
  icono: 'layers',
};

// Dashboard (9_frontend.md §8). Mantiene EXACTAMENTE la misma carga de datos
// que antes: un api.listar(tabla) por tabla de TABLAS en paralelo (forkJoin),
// con el total de cada una. La agrupación, los totales por sección y el CSV
// se calculan aquí, en el componente, sin datos inventados.
@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, NumeroAnimado, Icono],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private readonly api = inject(ApiService);
  readonly estado = inject(EstadoUI);

  // Secciones compartidas con el menú lateral (models/secciones.ts).
  private readonly seccionesBase: Seccion[] = seccionesDe(TABLAS);

  readonly tarjetas = signal<Tarjeta[]>([]);
  readonly cargando = signal(true);

  // Cantidad de bloques del esqueleto de carga.
  readonly esqueletos = [1, 2, 3, 4, 5, 6, 7, 8];

  // ---- Totales del resumen ------------------------------------------------

  // Catálogos que respondieron bien (sin error de red ni de servidor).
  readonly catalogosActivos = computed(
    () => this.tarjetas().filter((tarjeta) => !tarjeta.sinApi && !tarjeta.error).length,
  );

  // Suma real de todos los totales válidos.
  readonly totalRegistros = computed(() =>
    this.tarjetas().reduce(
      (suma, tarjeta) => suma + (tarjeta.sinApi || tarjeta.error ? 0 : tarjeta.total),
      0,
    ),
  );

  // "Hoy, HH:MM AM/PM" con la hora en que terminó la última carga. Null
  // mientras carga (la plantilla muestra un esqueleto).
  readonly ultimaSincronizacion = computed(() => {
    const fecha = this.estado.ultimaSincronizacion();
    if (!fecha) return null;
    const hora = new Intl.DateTimeFormat('es-CO', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    }).format(fecha);
    return `Hoy, ${hora}`;
  });

  // ---- Secciones ----------------------------------------------------------

  // Agrupa las tarjetas y calcula principal, resto y sumas de cada sección.
  readonly resumenSecciones = computed<ResumenSeccion[]>(() => {
    const porTabla = new Map(this.tarjetas().map((tarjeta) => [tarjeta.tabla, tarjeta]));
    return this.seccionesBase
      .map((seccion) => {
        const tarjetas = seccion.tablas
          .map((tabla) => porTabla.get(tabla))
          .filter((tarjeta): tarjeta is Tarjeta => tarjeta !== undefined);

        const totalRegistros = tarjetas.reduce(
          (suma, tarjeta) => suma + (tarjeta.sinApi || tarjeta.error ? 0 : tarjeta.total),
          0,
        );

        // La tarjeta destacada es la tabla con MÁS registros de la sección
        // (ante un empate se queda la primera del orden configurado).
        const principal = tarjetas.length
          ? tarjetas.reduce((mayor, tarjeta) => (tarjeta.total > mayor.total ? tarjeta : mayor))
          : undefined;

        return {
          ...seccion,
          tarjetas,
          totalCatalogos: tarjetas.length,
          totalRegistros,
          principal,
          resto: tarjetas.filter((tarjeta) => tarjeta !== principal),
        };
      })
      .filter((seccion) => seccion.tarjetas.length > 0);
  });

  // Secciones visibles: si hay texto en el buscador de la barra superior, se
  // filtran las tarjetas por nombre y se ocultan las secciones sin coincidencias.
  readonly vista = computed<ResumenSeccion[]>(() => {
    const termino = this.estado.busqueda().trim().toLowerCase();
    if (!termino) return this.resumenSecciones();

    return this.resumenSecciones()
      .map((seccion) => {
        const coincide = (tarjeta: Tarjeta) => tarjeta.nombre.toLowerCase().includes(termino);
        return {
          ...seccion,
          tarjetas: seccion.tarjetas.filter(coincide),
          principal:
            seccion.principal && coincide(seccion.principal) ? seccion.principal : undefined,
          resto: seccion.resto.filter(coincide),
        };
      })
      .filter((seccion) => seccion.tarjetas.length > 0);
  });

  ngOnInit() {
    const consultas = TABLAS.map((tabla) =>
      this.api.listar<never>(tabla).pipe(
        map((data) => this.crearTarjeta(tabla, data?.total ?? 0, false, false)),
        catchError((error: ApiError) =>
          // sin conexion con la API (estado 0) vs. error del servidor
          of(this.crearTarjeta(tabla, 0, error.estado === 0, error.estado !== 0)),
        ),
      ),
    );

    forkJoin(consultas).subscribe((resultados) => {
      this.tarjetas.set(resultados);
      this.cargando.set(false);

      // Filas reales para el CSV de la barra superior (sección, catálogo, total)
      const porTabla = new Map(resultados.map((tarjeta) => [tarjeta.tabla, tarjeta]));
      const filas: FilaExportable[] = this.seccionesBase.flatMap((seccion) =>
        seccion.tablas.map((tabla) => ({
          seccion: seccion.titulo,
          catalogo: nombreLegible(tabla),
          total: porTabla.get(tabla)?.total ?? 0,
        })),
      );
      this.estado.registrarCargue(
        filas,
        resultados.some((tarjeta) => tarjeta.sinApi),
      );
    });
  }

  private crearTarjeta(tabla: string, total: number, sinApi: boolean, error: boolean): Tarjeta {
    return {
      tabla,
      nombre: nombreLegible(tabla),
      total,
      sinApi,
      error,
      visual: CATALOGO_VISUAL[tabla] ?? VISUAL_POR_DEFECTO,
    };
  }
}
