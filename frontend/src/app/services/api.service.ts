// =============================================================================
// api.service.ts
// Un metodo por verbo contra la API (6_contracts.md). Traduce las respuestas de
// error en ApiError tipado. `listar` devuelve null cuando la tabla no tiene
// registros (204) para que la UI muestre el estado vacio (9_frontend.md §6-7).
// =============================================================================
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { ApiError, RespuestaCreacion, RespuestaLista } from '../models/catalogo';

export interface RespuestaExitoBasica {
  estado: number;
  mensaje: string;
}

export interface RespuestaFilas {
  estado: number;
  mensaje: string;
  filasAfectadas?: number;
  filasEliminadas?: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  // GET /api/{tabla}?limite=N (RF1). 200 -> envoltura; 204 -> null (tabla vacia).
  listar<T>(tabla: string, limite = 1000): Observable<RespuestaLista<T> | null> {
    return this.http
      .get<RespuestaLista<T> | null>(`${this.apiUrl}/api/${tabla}`, {
        params: { limite },
        observe: 'response',
      })
      .pipe(
        map((respuesta: HttpResponse<RespuestaLista<T> | null>) =>
          respuesta.status === 204 || !respuesta.body ? null : respuesta.body,
        ),
        catchError((error: HttpErrorResponse) => this.manejarError(error)),
      );
  }

  // GET /api/{tabla}/{id} (RF2). 404 -> ApiError {estado: 404}.
  obtener<T>(tabla: string, id: number | string): Observable<T> {
    return this.http
      .get<T>(`${this.apiUrl}/api/${tabla}/${id}`)
      .pipe(catchError((error: HttpErrorResponse) => this.manejarError(error)));
  }

  // POST /api/{tabla} (RF3). Body invalido -> 422 con errores[].
  // En la v2 la respuesta trae la fila creada en `datos` (9_frontend.md §6).
  crear<T>(tabla: string, body: Partial<T>): Observable<RespuestaCreacion<T>> {
    return this.http
      .post<RespuestaCreacion<T>>(`${this.apiUrl}/api/${tabla}`, body)
      .pipe(catchError((error: HttpErrorResponse) => this.manejarError(error)));
  }

  // PUT /api/{tabla}/{id} (RF4): reemplazo completo.
  reemplazar<T>(tabla: string, id: number | string, body: Partial<T>): Observable<RespuestaFilas> {
    return this.http
      .put<RespuestaFilas>(`${this.apiUrl}/api/${tabla}/${id}`, body)
      .pipe(catchError((error: HttpErrorResponse) => this.manejarError(error)));
  }

  // PATCH /api/{tabla}/{id} (RF5): actualizacion parcial.
  actualizar<T>(tabla: string, id: number | string, patch: Partial<T>): Observable<RespuestaFilas> {
    return this.http
      .patch<RespuestaFilas>(`${this.apiUrl}/api/${tabla}/${id}`, patch)
      .pipe(catchError((error: HttpErrorResponse) => this.manejarError(error)));
  }

  // DELETE /api/{tabla}/{id} (RF6): borrado logico.
  eliminar(tabla: string, id: number | string): Observable<RespuestaFilas> {
    return this.http
      .delete<RespuestaFilas>(`${this.apiUrl}/api/${tabla}/${id}`)
      .pipe(catchError((error: HttpErrorResponse) => this.manejarError(error)));
  }

  // POST /api/{puente} (v2): asigna una pareja en una tabla puente. Repetir la
  // pareja -> 409. Fuente: 9_frontend.md §6.
  asignar<T>(puente: string, pareja: Partial<T>): Observable<RespuestaCreacion<T>> {
    return this.http
      .post<RespuestaCreacion<T>>(`${this.apiUrl}/api/${puente}`, pareja)
      .pipe(catchError((error: HttpErrorResponse) => this.manejarError(error)));
  }

  // DELETE /api/{puente}/{pk1}/{pk2} (v2): retira la pareja. Retirar una pareja
  // inexistente -> 404. Fuente: 9_frontend.md §6.
  retirar(puente: string, pk1: number | string, pk2: number | string): Observable<RespuestaFilas> {
    return this.http
      .delete<RespuestaFilas>(`${this.apiUrl}/api/${puente}/${pk1}/${pk2}`)
      .pipe(catchError((error: HttpErrorResponse) => this.manejarError(error)));
  }

  // PUT /api/{relacion}/{pk1}/{pk2}: reemplaza los atributos editables sin
  // permitir que cambie la pareja compuesta que identifica la fila.
  reemplazarRelacion<T>(tabla: string, pk1: number | string, pk2: number | string, body: Partial<T>): Observable<RespuestaFilas> {
    return this.http
      .put<RespuestaFilas>(`${this.apiUrl}/api/${tabla}/${pk1}/${pk2}`, body)
      .pipe(catchError((error: HttpErrorResponse) => this.manejarError(error)));
  }

  // PATCH /api/{relacion}/{pk1}/{pk2}: actualiza solo los atributos enviados.
  actualizarRelacion<T>(tabla: string, pk1: number | string, pk2: number | string, patch: Partial<T>): Observable<RespuestaFilas> {
    return this.http
      .patch<RespuestaFilas>(`${this.apiUrl}/api/${tabla}/${pk1}/${pk2}`, patch)
      .pipe(catchError((error: HttpErrorResponse) => this.manejarError(error)));
  }

  // Traduce el JSON de error {estado, mensaje, detalle, errores} en ApiError tipado.
  private manejarError(error: HttpErrorResponse): Observable<never> {
    if (error.status === 0) {
      return throwError(() => ({
        estado: 0,
        mensaje: 'No se pudo conectar con la API.',
        detalle: error.message,
      } satisfies ApiError));
    }

    const cuerpo = error.error as ApiError | null;
    return throwError(() => ({
      estado: error.status,
      mensaje: cuerpo?.mensaje ?? 'Error del servidor.',
      detalle: cuerpo?.detalle,
      errores: cuerpo?.errores,
    } satisfies ApiError));
  }
}
