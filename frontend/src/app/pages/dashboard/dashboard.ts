import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiService } from '../../services/api.service';
import { TABLAS, nombreLegible, ApiError } from '../../models/catalogo';

interface Tarjeta {
  tabla: string;
  nombre: string;
  total: number;
  sinApi: boolean;
}

// Tarjetas en cuadricula por tabla con su `total` (9_frontend.md §8).
// Consulta las 7 tablas en paralelo con forkJoin (equivalente a Promise.all).
@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  readonly TABLAS = TABLAS;
  readonly nombreLegible = nombreLegible;

  readonly tarjetas = signal<Tarjeta[]>([]);
  readonly cargando = signal(true);

  constructor(private readonly api: ApiService) {}

  ngOnInit() {
    const consultas = TABLAS.map((tabla) =>
      this.api.listar<never>(tabla).pipe(
        map((data) => ({
          tabla,
          nombre: nombreLegible(tabla),
          total: data?.total ?? 0,
          sinApi: false,
        })),
        catchError((error: ApiError) =>
          of({
            tabla,
            nombre: nombreLegible(tabla),
            total: 0,
            sinApi: error.estado === 0, // sin conexion con la API
          }),
        ),
      ),
    );

    forkJoin(consultas).subscribe((resultados) => {
      this.tarjetas.set(resultados);
      this.cargando.set(false);
    });
  }
}