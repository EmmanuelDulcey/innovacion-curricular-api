import { Component, computed, effect, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, lastValueFrom, Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiService } from '../../services/api.service';
import { MensajeError } from '../../components/mensaje-error/mensaje-error';
import {
  ApiError,
  CampoConfig,
  CATALOGOS,
  CualquierRegistro,
  OpcionFk,
  RespuestaCreacion,
  RespuestaLista,
} from '../../models/catalogo';

// Una sección del detalle: la tabla hija, su título legible, los campos del
// formulario de alta (reutilizados del catálogo) y las columnas especiales de
// las puentes de solo lectura.
interface Seccion {
  tabla: string;
  titulo: string;
  campos: CampoConfig[];
  pk: string;                 // columna llave de la fila (para retirar)
  columnaPrograma: string;    // columna con la que se filtra el detalle del programa
  columnasLectura?: { etiqueta: string; campo: string }[];   // puentes de Felipe
}

// Pantalla maestro-detalle de `programa` (RF12 y 9_frontend.md §10): muestra el
// programa y, debajo, sus tablas hijas sin salir de la vista. Los renglones
// nuevos se acumulan por tipo y se envían con UNA sola acción de guardado; como
// los contratos no tienen endpoint por lotes, cada renglón viaja en su propia
// petición (2_spec.md RF12) y, si alguno falla, lo recién creado en ese mismo
// envío se retira para que no quede nada a medias (guía de la v2, tropiezos §4:
// "si el tercero falla, no debe quedar ninguno").
@Component({
  selector: 'app-programa-detalle',
  imports: [FormsModule, MensajeError, RouterLink],
  templateUrl: './programa-detalle.html',
  styleUrl: './programa-detalle.css',
})
export class ProgramaDetalle {
  // Parámetro :id de la ruta /programas/:id (withComponentInputBinding).
  readonly id = input.required<string>();
  readonly idNumero = computed(() => Number(this.id()));

  readonly programa = signal<CualquierRegistro | null>(null);
  readonly cargando = signal(false);
  readonly errorCarga = signal<ApiError | null>(null);

  // Listado de cada sección (filtrado en memoria por el programa abierto: los
  // contratos no tienen parámetro de filtro, solo `limite`).
  readonly registros = signal<Record<string, CualquierRegistro[]>>({});
  // Secciones cuyo endpoint no existe todavía o no respondió: se avisa y la
  // pantalla NO se rompe (9_frontend.md §7).
  readonly avisos = signal<Record<string, string>>({});

  // Renglones en borrador por tabla (varios por envío).
  readonly borradores = signal<Record<string, CualquierRegistro[]>>({});
  readonly errorEnvio = signal<ApiError | null>(null);
  readonly guardando = signal<string | null>(null);

  // Opciones de los desplegables de la sección (el `disenio` de actividades),
  // cargadas en paralelo (forkJoin), como en la página de catálogo.
  readonly opciones = signal<Record<string, OpcionFk[]>>({});

  // Las cinco tablas de detalle que la pantalla permite AGREGAR (4_research.md
  // D11 y guía §1): las tres de Valentina y pasantía/premio de Emmanuel, cuyos
  // endpoints ya existen y aquí solo se consumen.
  readonly secciones: Seccion[] = [
    this.desdeCatalogo('acreditacion', 'Acreditaciones'),
    this.desdeCatalogo('registro_calificado', 'Registros calificados'),
    this.desdeCatalogo('activ_academica', 'Actividades académicas'),
    this.desdeCatalogo('pasantia', 'Pasantías'),
    this.desdeCatalogo('premio', 'Premios'),
  ];

  // Las puentes de Felipe (9_frontend.md §10 las lista como hijas del programa):
  // SOLO LECTURA aquí, porque se asignan y retiran en su propio catálogo. Si su
  // endpoint todavía no existe, la sección muestra un aviso y sigue.
  readonly puentes: Seccion[] = [
    {
      tabla: 'programa_ac',
      titulo: 'Áreas de conocimiento asociadas',
      campos: [],
      pk: 'programa',
      columnaPrograma: 'programa',
      columnasLectura: [{ etiqueta: 'Área de conocimiento', campo: 'area_conocimiento_nombre' }],
    },
    {
      tabla: 'programa_pe',
      titulo: 'Prácticas o estrategias asociadas',
      campos: [],
      pk: 'programa',
      columnaPrograma: 'programa',
      columnasLectura: [{ etiqueta: 'Práctica o estrategia', campo: 'practica_estrategia_nombre' }],
    },
    {
      tabla: 'programa_ci',
      titulo: 'Características de innovación asociadas',
      campos: [],
      pk: 'programa',
      columnaPrograma: 'programa',
      columnasLectura: [{ etiqueta: 'Característica de innovación', campo: 'car_innovacion_nombre' }],
    },
    {
      tabla: 'an_programa',
      titulo: 'Aspectos normativos asociados',
      campos: [],
      pk: 'programa',
      columnaPrograma: 'programa',
      columnasLectura: [{ etiqueta: 'Aspecto normativo', campo: 'aspecto_normativo_nombre' }],
    },
  ];

  private cargaVersion = 0;

  constructor(private readonly api: ApiService) {}

  // Al entrar a /programas/:id (o cambiar de id) se recarga todo el detalle.
  private readonly recargarAlEntrar = effect(() => {
    const id = this.id();
    if (!id) return;
    const version = ++this.cargaVersion;
    void this.cargar(version);
  });

  // Reutiliza la configuración de columnas de la página de catálogo: una sola
  // definición por campo (etiquetas, largos, desplegables, Sí/No). La FK al
  // programa abierto no se pide en el formulario: la fija la pantalla.
  private desdeCatalogo(tabla: string, titulo: string): Seccion {
    const cfg = CATALOGOS[tabla];
    return {
      tabla,
      titulo,
      campos: cfg.campos.filter((campo) => campo.campo !== 'programa'),
      pk: cfg.llave,
      columnaPrograma: tabla === 'activ_academica' ? 'disenio' : 'programa',
    };
  }

  private get todasLasSecciones(): Seccion[] {
    return [...this.secciones, ...this.puentes];
  }

  async cargar(version = this.cargaVersion) {
    this.cargando.set(true);
    this.errorCarga.set(null);
    this.errorEnvio.set(null);
    this.avisos.set({});
    const id = this.idNumero();

    // Primero el maestro: sin programa no tiene sentido pedir el detalle.
    try {
      const programa = await lastValueFrom(this.api.obtener<CualquierRegistro>('programa', id));
      if (version !== this.cargaVersion) return;
      this.programa.set(programa);
    } catch (error) {
      if (version === this.cargaVersion) {
        this.programa.set(null);
        this.errorCarga.set(error as ApiError);
        this.cargando.set(false);
      }
      return;
    }

    // Desplegables y listados, todo en paralelo: encadenar peticiones haría
    // lenta la pantalla (tropiezo 5).
    const cargandoOpciones = this.cargarOpciones(version);

    const peticiones: Record<
      string,
      Observable<{ disponible: boolean; respuesta: RespuestaLista<CualquierRegistro> | null }>
    > = {};
    for (const seccion of this.todasLasSecciones) {
      peticiones[seccion.tabla] = this.api.listar<CualquierRegistro>(seccion.tabla, 1000).pipe(
        // Un fallo (por ejemplo, un endpoint que aún no existe) NO rompe la
        // pantalla: se distingue de una tabla vacía y se muestra como aviso.
        map((respuesta) => ({ disponible: true, respuesta })),
        catchError(() => of({ disponible: false, respuesta: null })),
      );
    }

    try {
      const resultados = await lastValueFrom(forkJoin(peticiones));
      if (version !== this.cargaVersion) return;

      const mapaRegistros: Record<string, CualquierRegistro[]> = {};
      const mapaAvisos: Record<string, string> = {};
      for (const seccion of this.todasLasSecciones) {
        const resultado = resultados[seccion.tabla];
        if (!resultado.disponible) {
          mapaRegistros[seccion.tabla] = [];
          mapaAvisos[seccion.tabla] = 'Esta sección no está disponible todavía.';
          continue;
        }
        mapaRegistros[seccion.tabla] = (resultado.respuesta?.datos ?? []).filter(
          (fila) => fila[seccion.columnaPrograma] === id,
        );
      }
      this.registros.set(mapaRegistros);
      this.avisos.set(mapaAvisos);
      this.borradores.set({});
    } catch {
      // forkJoin no falla (cada petición se captura arriba); por si acaso.
    } finally {
      if (version === this.cargaVersion) this.cargando.set(false);
    }

    await cargandoOpciones;
  }

  private async cargarOpciones(version: number) {
    const origenes = [
      ...new Set(
        this.secciones
          .flatMap((seccion) => seccion.campos)
          .filter((campo) => campo.tipo === 'desplegable' && campo.origen)
          .map((campo) => campo.origen as string),
      ),
    ];

    if (origenes.length === 0) {
      this.opciones.set({});
      return;
    }

    const peticiones: Record<string, Observable<RespuestaLista<CualquierRegistro> | null>> = {};
    for (const origen of origenes) {
      // El desplegable de programas usa la ruta de apoyo de solo lectura
      // (6_contracts.md §2), igual que la página de catálogo.
      const ruta = origen === 'programa' ? 'opciones/programa' : origen;
      peticiones[origen] = this.api
        .listar<CualquierRegistro>(ruta, 1000)
        .pipe(catchError(() => of(null)));
    }

    try {
      const resultados = await lastValueFrom(forkJoin(peticiones));
      if (version !== this.cargaVersion) return;

      const mapa: Record<string, OpcionFk[]> = {};
      for (const origen of origenes) {
        const llave = CATALOGOS[origen]?.llave ?? 'id';
        const etiqueta = this.secciones
          .flatMap((seccion) => seccion.campos)
          .find((campo) => campo.origen === origen)?.origenEtiqueta ?? 'nombre';
        mapa[origen] = (resultados[origen]?.datos ?? []).map((item) => ({
          valor: item[llave],
          etiqueta: this.etiquetaDe(item, etiqueta, llave),
        }));
      }
      this.opciones.set(mapa);
    } catch {
      if (version === this.cargaVersion) this.opciones.set({});
    }
  }

  opcionesDe(campo: CampoConfig): OpcionFk[] {
    return campo.origen ? this.opciones()[campo.origen] ?? [] : [];
  }

  // Igual que en la página de catálogo: permite etiquetas compuestas con '+'
  // (registro_calificado = "codigo - tipo_titulacion") y cae a la clave si el
  // maestro no tiene ningún texto.
  private etiquetaDe(item: CualquierRegistro, etiqueta: string, llave: string): string {
    const partes = etiqueta
      .split('+')
      .map((clave) => item[clave.trim()])
      .filter((valor) => valor !== undefined && valor !== null && String(valor).trim() !== '');
    if (partes.length === 0) return String(item[llave] ?? '');
    return partes.map((valor) => String(valor)).join(' - ');
  }

  // Valor legible de una celda: nombre de la FK si el campo lo declara, Sí/No
  // para los 0/1 y la fecha recortada; los vacíos se muestran como «—».
  valorTexto(fila: CualquierRegistro, campo: CampoConfig): string {
    let valor: unknown;
    if (campo.tipo === 'booleano') valor = fila[campo.campo] === 1 ? 'Sí' : 'No';
    else valor = campo.campoMostrar ? fila[campo.campoMostrar] : fila[campo.campo];

    if (campo.tipo === 'fecha' && valor) valor = String(valor).slice(0, 10);
    return valor === null || valor === undefined || valor === '' ? '—' : String(valor);
  }

  // ---- Renglones del detalle (varios por envío) ----------------------------

  agregarRenglon(seccion: Seccion) {
    const nuevo: CualquierRegistro = {};
    for (const campo of seccion.campos) {
      // Las claves internas que genera SQL Server y las columnas derivadas no
      // entran al formulario de alta.
      if (campo.generado || campo.soloLectura) continue;
      nuevo[campo.campo] =
        campo.tipo === 'desplegable' || campo.tipo === 'booleano' || !campo.requerido ? null : '';
    }

    // En actividades académicas el vínculo con el programa abierto es `disenio`
    // (la única FK opcional): se preselecciona, y el usuario puede cambiarlo o
    // dejarlo en «(ninguna)» → null.
    if (seccion.tabla === 'activ_academica') nuevo['disenio'] = this.idNumero();

    const actuales = this.borradores()[seccion.tabla] ?? [];
    this.borradores.set({ ...this.borradores(), [seccion.tabla]: [...actuales, nuevo] });
  }

  quitarRenglon(seccion: Seccion, indice: number) {
    const actuales = this.borradores()[seccion.tabla] ?? [];
    this.borradores.set({
      ...this.borradores(),
      [seccion.tabla]: actuales.filter((_, posicion) => posicion !== indice),
    });
  }

  async guardarRenglones(seccion: Seccion) {
    const filas = this.borradores()[seccion.tabla] ?? [];
    if (filas.length === 0) return;

    // Validación local con los mismos mensajes de la API: si un renglón está
    // incompleto no se envía NADA (así tampoco queda nada a medias por un
    // error de digitación). La API sigue siendo la autoridad final (422).
    const errores = this.validarRenglones(seccion, filas);
    if (errores.length > 0) {
      this.errorEnvio.set({ estado: 422, mensaje: 'Datos inválidos.', errores });
      return;
    }

    this.errorEnvio.set(null);
    this.guardando.set(seccion.tabla);

    const cuerpos = filas.map((fila) => {
      const cuerpo: Record<string, any> = {};
      for (const campo of seccion.campos) {
        // Claves generadas por la base y columnas derivadas: nunca viajan.
        if (campo.generado || campo.soloLectura) continue;
        cuerpo[campo.campo] = fila[campo.campo];
      }
      // La pareja con el programa abierto la fija la pantalla (pasantía,
      // premio, acreditación y registro calificado no piden ese campo).
      if (seccion.tabla !== 'activ_academica') cuerpo['programa'] = this.idNumero();
      return cuerpo;
    });

    // UN solo envío: se disparan todas las peticiones juntas y se esperan TODAS
    // antes de decidir, para poder compensar si alguna falla.
    const resultados = await Promise.allSettled(
      cuerpos.map((cuerpo) => lastValueFrom(this.api.crear<CualquierRegistro>(seccion.tabla, cuerpo))),
    );

    const fallidos = resultados.filter(
      (resultado): resultado is PromiseRejectedResult => resultado.status === 'rejected',
    );
    const creados = resultados.filter(
      (resultado): resultado is PromiseFulfilledResult<RespuestaCreacion<CualquierRegistro>> =>
        resultado.status === 'fulfilled',
    );

    if (fallidos.length > 0) {
      // Compensación: lo que SÍ se creó en este mismo envío se retira aquí
      // mismo, para que el envío sea "todo o nada" sin endpoint por lotes.
      await Promise.allSettled(
        creados.map((resultado) =>
          lastValueFrom(this.api.eliminar(seccion.tabla, resultado.value.datos[seccion.pk])),
        ),
      );
      this.errorEnvio.set(fallidos[0].reason as ApiError);
    } else {
      this.borradores.set({ ...this.borradores(), [seccion.tabla]: [] });
    }

    await this.recargarSeccion(seccion);
    // El contador del programa cambia SOLO cuando se toca una acreditación
    // (lo mantiene el disparador): se relee para mostrarlo actualizado.
    if (seccion.tabla === 'acreditacion') await this.recargarPrograma();

    this.guardando.set(null);
  }

  async retirarDeSeccion(seccion: Seccion, fila: CualquierRegistro) {
    if (!window.confirm('¿Retirar este registro?')) return;

    try {
      await lastValueFrom(this.api.eliminar(seccion.tabla, fila[seccion.pk]));
      this.errorEnvio.set(null);
      await this.recargarSeccion(seccion);
      if (seccion.tabla === 'acreditacion') await this.recargarPrograma();
    } catch (error) {
      this.errorEnvio.set(error as ApiError);
    }
  }

  // Mensajes por renglón con el mismo texto de la API (6_contracts.md §4.3),
  // numerados para saber cuál renglón corregir.
  private validarRenglones(seccion: Seccion, filas: CualquierRegistro[]): string[] {
    const errores: string[] = [];
    filas.forEach((fila, indice) => {
      const renglon = filas.length > 1 ? ` (renglón ${indice + 1})` : '';
      for (const campo of seccion.campos) {
        if (campo.generado || campo.soloLectura) continue;
        const valor = fila[campo.campo];
        const vacio = valor === undefined || valor === null || String(valor).trim() === '';

        if (campo.requerido && vacio) {
          errores.push(`El campo ${campo.campo} es obligatorio${renglon}.`);
          continue;
        }
        if (vacio) continue;

        const texto = String(valor).trim();
        if (campo.tipo === 'entero' && (Number.isNaN(Number(texto)) || Number(texto) < 0)) {
          errores.push(`El campo ${campo.campo} no puede ser negativo${renglon}.`);
          continue;
        }
        if (campo.esLlave && Number(texto) <= 0) {
          errores.push(`El campo ${campo.campo} debe ser un entero positivo${renglon}.`);
          continue;
        }
        if (campo.maximo !== null && texto.length > campo.maximo) {
          errores.push(`El campo ${campo.campo} no puede superar los ${campo.maximo} caracteres${renglon}.`);
        }
      }
    });
    return errores;
  }

  private async recargarSeccion(seccion: Seccion) {
    try {
      const respuesta = await lastValueFrom(
        this.api.listar<CualquierRegistro>(seccion.tabla, 1000),
      );
      this.registros.set({
        ...this.registros(),
        [seccion.tabla]: (respuesta?.datos ?? []).filter(
          (fila) => fila[seccion.columnaPrograma] === this.idNumero(),
        ),
      });
    } catch {
      // Si la recarga falla se conserva lo que ya se mostraba; el aviso del
      // error de la operación ya quedó en errorEnvio.
    }
  }

  private async recargarPrograma() {
    try {
      this.programa.set(
        await lastValueFrom(this.api.obtener<CualquierRegistro>('programa', this.idNumero())),
      );
    } catch {
      // El contador se refrescará en la próxima carga de la pantalla.
    }
  }
}
