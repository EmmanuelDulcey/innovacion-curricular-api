// =============================================================================
// QUÉ ES: página genérica de listado y formulario para /tablas/:tabla.
// PAPEL: configura el catálogo desde catalogo.ts y concentra el filtrado en vivo,
//       las opciones de claves y las operaciones sobre cada registro o relación.
// =============================================================================
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
  obtenerConfig,
  OpcionFk,
  RespuestaLista,
  TablaConfig,
} from '../../models/catalogo';

// Pagina generica de CRUD /tablas/:tabla (RF9, 9_frontend.md §9). Recibe la
// tabla por ruta (withComponentInputBinding), configura columnas y formulario a
// partir de CATALOGOS y usa api.service para los verbos de cada tabla.
//
// Dos familias de tablas comparten esta pagina:
//  * PK simple: crear / editar / eliminar como en la v1. Si la llave es
//    interna (`generado`), SQL Server la asigna y el formulario nunca la pide.
//  * identidad compuesta (relaciones/puentes): la pareja viaja en la ruta.
//    Las puentes puras solo se asignan y retiran; docente_departamento y
//    alianza ademas permiten editar sus atributos (PUT por pareja), sin
//    cambiar las claves (6_contracts.md §8).
@Component({
  selector: 'app-catalogo',
  imports: [FormsModule, MensajeError, RouterLink],
  templateUrl: './catalogo.html',
  styleUrl: './catalogo.css',
})
export class Catalogo {
  readonly tabla = input.required<string>();

  readonly config = computed<TablaConfig | undefined>(() => obtenerConfig(this.tabla()));

  // Estado del listado.
  readonly registros = signal<CualquierRegistro[]>([]);
  readonly cargando = signal(false);
  readonly error = signal<ApiError | null>(null);
  readonly filtro = signal('');
  readonly limite = signal(1000);

  // Opciones de los desplegables de clave foranea, por tabla maestra de origen
  // (9_frontend.md §8). Se cargan en paralelo al listado.
  readonly opciones = signal<Record<string, OpcionFk[]>>({});
  readonly erroresOpciones = signal<Record<string, boolean>>({});
  readonly opcionesPendientes = signal(false);

  // Estado del formulario (crear/editar).
  readonly editando = signal<{ fila: CualquierRegistro; esEdicion: boolean } | null>(null);
  readonly mostrarFormulario = computed(() => this.editando() !== null);
  readonly erroresPorCampo = signal<Record<string, string>>({});
  readonly errorForm = signal<ApiError | null>(null);

  // Modelo del formulario: objeto plano con las claves snake_case del campo.
  readonly forma: Record<string, any> = {};
  private cargaVersion = 0;

  // La misma instancia de la página se reutiliza al cambiar :tabla. El efecto
  // reinicia el estado y carga el catálogo nuevo sin dejar datos de la ruta
  // anterior ni permitir que una respuesta tardía lo sobrescriba.
  private readonly recargarAlCambiarTabla = effect(() => {
    const tabla = this.tabla();
    if (!tabla) return;

    const version = ++this.cargaVersion;
    this.cerrarFormulario();
    this.filtro.set('');
    void this.cargar(version);
  });

  readonly registrosFiltrados = computed(() => {
    const termino = this.filtro().trim().toLowerCase();
    if (!termino) return this.registros();

    return this.registros().filter((fila) =>
      Object.values(fila).some((valor) => String(valor).toLowerCase().includes(termino)),
    );
  });

  // Escuchar el evento `input` actualiza el filtro en cada tecla y evita
  // depender de que el usuario confirme una búsqueda con Enter o un botón.
  actualizarFiltro(evento: Event): void {
    const entrada = evento.target;
    if (entrada instanceof HTMLInputElement) this.filtro.set(entrada.value);
  }

  constructor(private readonly api: ApiService) {}

  async cargar(version = this.cargaVersion) {
    const cfg = this.config();
    if (!cfg) return;

    this.cargando.set(true);
    this.error.set(null);

    // Las opciones de los desplegables se piden en paralelo al listado (§8).
    const cargandoOpciones = this.cargarOpciones(cfg, version);
    try {
      const data = await lastValueFrom(this.api.listar<CualquierRegistro>(cfg.nombre, this.limite()));
      if (version === this.cargaVersion) this.registros.set(data?.datos ?? []);
    } catch (error) {
      if (version === this.cargaVersion) {
        this.error.set(error as ApiError);
        this.registros.set([]);
      }
    } finally {
      if (version === this.cargaVersion) this.cargando.set(false);
    }

    await cargandoOpciones;
  }

  // Carga las opciones de cada clave foranea con un forkJoin (una peticion por
  // tabla maestra, en paralelo). Si una maestra falla o esta vacia, su lista
  // queda vacia y el <select> solo muestra el texto de relleno.
  private async cargarOpciones(cfg: TablaConfig, version: number) {
    this.opcionesPendientes.set(true);
    this.erroresOpciones.set({});
    const origenes = [
      ...new Set(
        cfg.campos
          .filter((campo) => campo.tipo === 'desplegable' && campo.origen)
          .map((campo) => campo.origen as string),
      ),
    ];

    if (origenes.length === 0) {
      this.opciones.set({});
      this.opcionesPendientes.set(false);
      return;
    }

    type ResultadoOpcion = { respuesta: RespuestaLista<CualquierRegistro> | null; fallo: boolean };
    const peticiones: Record<string, Observable<ResultadoOpcion>> = {};
    for (const origen of origenes) {
      // El CRUD de programa pertenece a otro módulo; esta ruta de lectura
      // (6_contracts.md §2) provee sus opciones activas sin ocupar /api/programa.
      const ruta = origen === 'programa' ? 'opciones/programa' : origen;
      peticiones[origen] = this.api
        .listar<CualquierRegistro>(ruta, 1000)
        .pipe(
          map((respuesta) => ({ respuesta, fallo: false })),
          catchError(() => of({ respuesta: null, fallo: true })),
        );
    }

    try {
      const resultados = await lastValueFrom(forkJoin(peticiones));
      if (version !== this.cargaVersion) return;

      const mapa: Record<string, OpcionFk[]> = {};
      const fallos: Record<string, boolean> = {};
      for (const origen of origenes) {
        const llave = CATALOGOS[origen]?.llave ?? 'id';
        const campoFk = cfg.campos.find((campo) => campo.origen === origen);
        const etiqueta = campoFk?.origenEtiqueta ?? 'nombre';
        const subEtiqueta = campoFk?.origenSubEtiqueta;
        fallos[origen] = resultados[origen]?.fallo ?? false;
        mapa[origen] = (resultados[origen]?.respuesta?.datos ?? []).map((item) => ({
          valor: item[llave],
          // Cada maestra aporta su etiqueta legible. Con `origenSubEtiqueta` se
          // agrega un segundo dato para desambiguar (área — disciplina); si no,
          // `etiquetaDe` resuelve etiquetas compuestas con '+' y cae a la clave.
          etiqueta: subEtiqueta
            ? `${item[etiqueta] ?? ''} — ${item[subEtiqueta] ?? ''}`
            : this.etiquetaDe(item, etiqueta, llave),
        }));
      }
      this.opciones.set(mapa);
      this.erroresOpciones.set(fallos);
    } catch {
      if (version === this.cargaVersion) {
        this.opciones.set({});
        this.erroresOpciones.set(Object.fromEntries(origenes.map((origen) => [origen, true])));
      }
    } finally {
      if (version === this.cargaVersion) this.opcionesPendientes.set(false);
    }
  }

  opcionesDe(campo: CampoConfig): OpcionFk[] {
    if (campo.opcionesFijas) {
      return campo.opcionesFijas.map((valor) => ({ valor, etiqueta: valor }));
    }
    return campo.origen ? this.opciones()[campo.origen] ?? [] : [];
  }

  // Etiqueta visible de una opcion de desplegable. `etiqueta` puede nombrar
  // varias columnas separadas con '+' (registro_calificado no tiene `nombre`:
  // se muestra como "codigo - tipo_titulacion"); si no hay ningun valor, cae a
  // la clave para que la opcion nunca quede en blanco.
  private etiquetaDe(item: CualquierRegistro, etiqueta: string, llave: string): string {
    const partes = etiqueta
      .split('+')
      .map((clave) => item[clave.trim()])
      .filter((valor) => valor !== undefined && valor !== null && String(valor).trim() !== '');
    if (partes.length === 0) return String(item[llave] ?? '');
    return partes.map((valor) => String(valor)).join(' - ');
  }

  // En el listado se muestra el nombre resuelto de la FK (<fk>_nombre) cuando el
  // campo lo declara; si no, el valor crudo (9_frontend.md §4). Los 0/1 de la
  // base (espejo) se leen como Sí/No: la interfaz va sin jerga.
  valorVisible(fila: CualquierRegistro, campo: CampoConfig): unknown {
    if (campo.tipo === 'booleano') return fila[campo.campo] === 1 ? 'Sí' : 'No';
    return campo.campoMostrar ? fila[campo.campoMostrar] : fila[campo.campo];
  }

  cambiarLimite(valor: unknown) {
    const limite = Number(valor);
    if (Number.isInteger(limite) && limite > 0) {
      this.limite.set(limite);
      void this.cargar();
    }
  }

  abrirCrear() {
    const cfg = this.config()!;
    const fila: Record<string, any> = {};
    // Los desplegables, los Sí/No y los campos opcionales arrancan en null (la
    // opción de relleno); el resto, en cadena vacía. Las columnas generadas
    // por SQL Server y las de solo lectura no entran al formulario.
    for (const campo of cfg.campos) {
      if (campo.soloLectura || campo.generado) continue;
      fila[campo.campo] =
        campo.tipo === 'desplegable' || campo.tipo === 'booleano' || !campo.requerido ? null : '';
    }
    this.formaSeleccionada(fila, false);
  }

  abrirEditar(fila: CualquierRegistro) {
    const cfg = this.config()!;
    const copia: Record<string, any> = { ...fila };
    // El <input type="date"> solo acepta "yyyy-MM-dd"; se recorta la hora si vino.
    for (const campo of cfg.campos) {
      if (campo.tipo === 'fecha' && copia[campo.campo]) {
        copia[campo.campo] = String(copia[campo.campo]).slice(0, 10);
      }
    }
    this.formaSeleccionada(copia, true);
  }

  private formaSeleccionada(fila: Record<string, any>, esEdicion: boolean) {
    this.editando.set({ fila, esEdicion });
    this.erroresPorCampo.set({});
    this.errorForm.set(null);
    Object.keys(this.forma).forEach((clave) => delete this.forma[clave]);
    Object.assign(this.forma, fila);
  }

  cerrarFormulario() {
    this.editando.set(null);
    this.erroresPorCampo.set({});
    this.errorForm.set(null);
  }

  // Una relación tiene identidad compuesta: sus DOS claves viajan en la ruta
  // (orden del script) y la fila se maneja como pareja, no como registro suelto.
  esCompuesta(): boolean {
    const cfg = this.config();
    return cfg !== undefined && (cfg.llaves !== undefined || cfg.esRelacion === true || cfg.esPuente === true);
  }

  // Solo las relaciones con atributos propios (permiteEdicion) tienen botón
  // Editar: su PUT conserva las claves y cambia únicamente los atributos.
  // Las cuatro puentes puras se quedan en asignar/retirar (tropiezo 9).
  puedeEditar(): boolean {
    const cfg = this.config();
    return cfg !== undefined && (!this.esCompuesta() || cfg.permiteEdicion === true);
  }

  // Clave estable de una fila para el @for y las acciones: la pareja completa
  // en las relaciones, la llave simple en el resto.
  claveDe(fila: CualquierRegistro): string {
    const cfg = this.config()!;
    return cfg.llaves
      ? cfg.llaves.map((llave) => String(fila[llave])).join('|')
      : String(fila[cfg.llave]);
  }

  async eliminar(fila: CualquierRegistro) {
    const cfg = this.config()!;
    const pregunta = this.esCompuesta() ? '¿Retirar esta relación?' : '¿Eliminar este registro?';
    if (!window.confirm(pregunta)) return;

    try {
      if (cfg.llaves) {
        // Retirar una relación lleva las DOS claves de la PK, en su orden (§8.2).
        await lastValueFrom(this.api.retirar(cfg.nombre, fila[cfg.llaves[0]], fila[cfg.llaves[1]]));
      } else {
        await lastValueFrom(this.api.eliminar(cfg.nombre, fila[cfg.llave]));
      }
      this.error.set(null);
      await this.cargar();
    } catch (error) {
      this.error.set(error as ApiError);
    }
  }

  async guardar() {
    const cfg = this.config()!;
    const esEdicion = this.editando()?.esEdicion ?? false;

    const errores = this.validarFormulario(cfg);
    if (Object.keys(errores).length > 0) {
      this.erroresPorCampo.set(errores);
      return;
    }
    this.erroresPorCampo.set({});

    const body: Record<string, any> = {};
    for (const campo of cfg.campos) {
      // Las claves que genera SQL Server y las columnas derivadas (el contador
      // del disparador) nunca viajan en el body.
      if (campo.generado || campo.soloLectura) continue;
      if (!esEdicion || campo.editable) {
        const valor = this.forma[campo.campo];
        // Los opcionales vacíos viajan como null, nunca como cadena vacía.
        body[campo.campo] = !campo.requerido && (valor === '' || valor === undefined) ? null : valor;
      }
    }

    try {
      if (cfg.llaves) {
        if (esEdicion) {
          // PUT por pareja: las claves identifican la fila y no cambian; el
          // body lleva solo los atributos editables.
          const original = this.editando()!.fila;
          await lastValueFrom(
            this.api.reemplazarRelacion(
              cfg.nombre,
              original[cfg.llaves[0]],
              original[cfg.llaves[1]],
              body,
            ),
          );
        } else {
          // Asignar la pareja (POST de la relación).
          await lastValueFrom(this.api.asignar(cfg.nombre, body));
        }
      } else if (esEdicion) {
        await lastValueFrom(this.api.reemplazar(cfg.nombre, this.forma[cfg.llave], body));
      } else {
        await lastValueFrom(this.api.crear(cfg.nombre, body));
      }
      this.cerrarFormulario();
      await this.cargar();
    } catch (error) {
      const apiError = error as ApiError;
      this.errorForm.set(apiError);
      if (apiError.estado === 422) this.asignarErroresPorCampo(apiError.errores ?? []);
    }
  }

  // ---- Helpers del formulario ---------------------------------------------

  esLlave(campo: CampoConfig): boolean {
    return campo.esLlave === true;
  }

  esSoloLectura(campo: CampoConfig): boolean {
    return this.esLlave(campo) && (this.editando()?.esEdicion ?? false);
  }

  private validarFormulario(cfg: TablaConfig): Record<string, string> {
    const errores: Record<string, string> = {};
    for (const campo of cfg.campos) {
      if (campo.soloLectura || campo.generado || this.esSoloLectura(campo)) continue;

      const valor = this.forma[campo.campo];
      const vacio = valor === undefined || valor === null || String(valor).trim() === '';

      if (campo.requerido && vacio) {
        errores[campo.campo] = `El campo ${campo.campo} es obligatorio.`;
        continue;
      }
      if (!vacio) {
        const texto = String(valor).trim();
        if (campo.tipo === 'entero') {
          const numero = Number(texto);
          // Las llaves naturales deben ser positivas; los contadores
          // (num_creditos, h_acom, h_indep) admiten 0, igual que la API.
          if (campo.esLlave && (!Number.isInteger(numero) || numero <= 0)) {
            errores[campo.campo] = `El campo ${campo.campo} debe ser un entero positivo.`;
            continue;
          }
          if (!campo.esLlave && (!Number.isInteger(numero) || numero < 0)) {
            errores[campo.campo] = `El campo ${campo.campo} no puede ser negativo.`;
            continue;
          }
        }
        // El mensaje es el mismo que devuelve la API en el 422, para que el
        // usuario vea el texto idéntico venga de donde venga la validación
        // (6_contracts.md §4.3). La API sigue siendo la autoridad final.
        if (campo.tipo === 'correo' && !this.esCorreo(texto)) {
          errores[campo.campo] = `El campo ${campo.campo} debe tener un formato válido.`;
          continue;
        }
        if (campo.opcionesFijas && !campo.opcionesFijas.includes(texto)) {
          errores[campo.campo] = `Seleccione una opción válida para ${campo.etiqueta}.`;
          continue;
        }
        if (campo.maximo !== null && texto.length > campo.maximo) {
          errores[campo.campo] = `El campo ${campo.campo} no puede superar los ${campo.maximo} caracteres.`;
        }
      }
    }
    // Coherencia de fechas propias de las relaciones de Felipe (6_contracts §8.3).
    if (cfg.nombre === 'alianza' && this.forma['fecha_fin'] && this.forma['fecha_inicio']
      && this.forma['fecha_fin'] < this.forma['fecha_inicio']) {
      errores['fecha_fin'] = 'La fecha de fin no puede ser anterior a la fecha de inicio.';
    }
    if (cfg.nombre === 'docente_departamento' && this.forma['fecha_salida'] && this.forma['fecha_ingreso']
      && this.forma['fecha_salida'] < this.forma['fecha_ingreso']) {
      errores['fecha_salida'] = 'La fecha de salida no puede ser anterior a la fecha de ingreso.';
    }
    return errores;
  }

  // Comprobacion de formato de correo previa al envio: exige algo@algo.algo
  // sin espacios. La validacion definitiva la hace la API con [EmailAddress];
  // esta solo evita el viaje innecesario al servidor.
  private esCorreo(texto: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(texto);
  }

  private asignarErroresPorCampo(mensajes: string[]) {
    const porCampo: Record<string, string> = {};
    for (const campo of this.config()?.campos ?? []) {
      const coincidencia = mensajes.find((mensaje) => mensaje.includes(campo.campo));
      if (coincidencia) porCampo[campo.campo] = coincidencia;
    }
    this.erroresPorCampo.set(porCampo);
  }
}
