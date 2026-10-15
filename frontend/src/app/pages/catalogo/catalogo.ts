import { Component, computed, effect, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin, lastValueFrom, Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
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
// partir de CATALOGOS y usa api.service para los cinco verbos.
@Component({
  selector: 'app-catalogo',
  imports: [FormsModule, MensajeError],
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
    const origenes = [
      ...new Set(
        cfg.campos
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
      peticiones[origen] = this.api
        .listar<CualquierRegistro>(origen, 1000)
        .pipe(catchError(() => of(null)));
    }

    try {
      const resultados = await lastValueFrom(forkJoin(peticiones));
      if (version !== this.cargaVersion) return;

      const mapa: Record<string, OpcionFk[]> = {};
      for (const origen of origenes) {
        const llave = CATALOGOS[origen]?.llave ?? 'id';
        const etiqueta = cfg.campos.find((campo) => campo.origen === origen)?.origenEtiqueta ?? 'nombre';
        mapa[origen] = (resultados[origen]?.datos ?? []).map((item) => ({
          valor: item[llave],
          etiqueta: String(item[etiqueta] ?? item[llave]),
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

  // En el listado se muestra el nombre resuelto de la FK (<fk>_nombre) cuando el
  // campo lo declara; si no, el valor crudo (9_frontend.md §4).
  valorVisible(fila: CualquierRegistro, campo: CampoConfig): unknown {
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
    // Un desplegable arranca en null (su opcion de relleno); el resto en cadena
    // vacia. Asi el select no queda con un valor inexistente (9_frontend.md §8).
    for (const campo of cfg.campos) {
      fila[campo.campo] = campo.tipo === 'desplegable' ? null : '';
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

  async eliminar(fila: CualquierRegistro) {
    const cfg = this.config()!;
    const id = fila[cfg.llave];
    if (!window.confirm('¿Eliminar este registro?')) return;

    try {
      await lastValueFrom(this.api.eliminar(cfg.nombre, id));
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

    const id = this.forma[cfg.llave];
    const body: Record<string, any> = {};
    for (const campo of cfg.campos) {
      if (!esEdicion || campo.editable) body[campo.campo] = this.forma[campo.campo];
    }

    try {
      if (esEdicion) {
        await lastValueFrom(this.api.reemplazar(cfg.nombre, id, body));
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
      if (this.esSoloLectura(campo)) continue;

      const valor = this.forma[campo.campo];
      const vacio = valor === undefined || valor === null || String(valor).trim() === '';

      if (campo.requerido && vacio) {
        errores[campo.campo] = `El campo ${campo.campo} es obligatorio.`;
        continue;
      }
      if (!vacio) {
        const texto = String(valor).trim();
        if (campo.tipo === 'entero' && (Number.isNaN(Number(texto)) || Number(texto) <= 0)) {
          errores[campo.campo] = `El campo ${campo.campo} debe ser un entero positivo.`;
          continue;
        }
        // El mensaje es el mismo que devuelve la API en el 422, para que el
        // usuario vea el texto idéntico venga de donde venga la validación
        // (6_contracts.md §4.7). La API sigue siendo la autoridad final.
        if (campo.tipo === 'correo' && !this.esCorreo(texto)) {
          errores[campo.campo] = `El campo ${campo.campo} debe tener un formato válido.`;
          continue;
        }
        if (campo.maximo !== null && texto.length > campo.maximo) {
          errores[campo.campo] = `El campo ${campo.campo} no puede superar los ${campo.maximo} caracteres.`;
        }
      }
    }
    return errores;
  }

  // Comprobacion de formato de correo previa al envio: exige algo@algo.algo
  // sin espacios. La validacion definitiva la hace la API con [EmailAddress]
  // (6_contracts.md §8.7); esta solo evita el viaje innecesario al servidor.
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
