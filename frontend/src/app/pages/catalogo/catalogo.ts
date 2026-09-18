import { Component, computed, effect, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { lastValueFrom } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { MensajeError } from '../../components/mensaje-error/mensaje-error';
import {
  ApiError,
  CampoConfig,
  CualquierRegistro,
  obtenerConfig,
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
    for (const campo of cfg.campos) fila[campo.campo] = '';
    this.formaSeleccionada(fila, false);
  }

  abrirEditar(fila: CualquierRegistro) {
    this.formaSeleccionada({ ...fila }, true);
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
