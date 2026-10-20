// =============================================================================
// QUÉ ES: contratos TypeScript y metadatos de los catálogos de la aplicación.
// PAPEL: permite que una sola página genere tablas y formularios configurables,
//       mientras mantiene tipadas las respuestas y opciones de las claves.
// La API responde con sus nombres de contrato; las interfaces de dominio usan
// camelCase y la configuración conserva el nombre de cada columna SQL.
// =============================================================================

// --- Contratos auxiliares (9_frontend.md §4) -------------------------------
export interface RespuestaLista<T> {
  tabla: string;
  limite: number;
  total: number;
  datos: T[];
}

// Envoltorio del POST de la v2: el registro creado vuelve en `datos`
// (9_frontend.md §4). Fuente: 6_contracts.md §4.
export interface RespuestaCreacion<T> {
  estado: number;
  mensaje: string;
  datos: T;
}

export interface ApiError {
  estado: number;
  mensaje: string;
  detalle?: string;
  errores?: string[];
}

// --- Entidades en camelCase (mapeo snake_case -> camelCase) ------------------
export interface AreaConocimiento {
  id: number;
  granArea: string;
  area: string;
  disciplina: string;
  activo: boolean;
}

export interface Universidad {
  id: number;
  nombre: string;
  tipo: string;
  ciudad: string;
  activo: boolean;
}

export interface AspectoNormativo {
  id: number;
  tipo: string;
  descripcion: string;
  fuente: string;
  activo: boolean;
}

export interface PracticaEstrategia {
  id: number;
  tipo: string;
  nombre: string;
  descripcion: string;
  activo: boolean;
}

export interface Enfoque {
  id: number;
  nombre: string;
  descripcion: string;
  activo: boolean;
}

export interface CarInnovacion {
  id: number;
  nombre: string;
  descripcion: string;
  tipo: string;
  activo: boolean;
}

// `aliado` es la única tabla cuya llave es `nit` y no `id` (9_frontend.md §4).
export interface Aliado {
  nit: number;
  razonSocial: string;
  nombreContacto: string;
  correo: string;
  telefono: string;
  ciudad: string;
  activo: boolean;
}

export type CualquierRegistro = Record<string, any>;

// --- Entidades de la v2 (9_frontend.md §4) ----------------------------------
// Cada fila trae ademas el nombre de la FK (`universidadNombre`,
// `programaNombre`), que es lo que se muestra en el listado (6_contracts.md §0.7).

export interface Facultad {
  id: number;
  nombre: string;
  tipo: string;
  fechaFun: string;          // DATE -> "yyyy-MM-dd"
  universidad: number;
  universidadNombre?: string;
  activo: boolean;
}

export interface Pasantia {
  id: number;
  nombre: string;
  pais: string;
  empresa: string;
  descripcion: string;
  programa: number;
  programaNombre?: string;
  activo: boolean;
}

export interface Premio {
  id: number;
  nombre: string;
  descripcion: string;
  fecha: string;             // DATE -> "yyyy-MM-dd"
  entidadOtorgante: string;
  pais: string;
  programa: number;
  programaNombre?: string;
  activo: boolean;
}

// Relaciones de Felipe (v2): la API usa snake_case; estas interfaces son el
// contrato tipado que consumen otros componentes Angular.
export interface DocenteDepartamento {
  docente: number;
  departamento: number;
  departamentoNombre?: string;
  dedicacion: string;
  modalidad: string;
  fechaIngreso: string;
  fechaSalida: string | null;
  activo: boolean;
}

export interface Alianza {
  aliado: number;
  aliadoNombre?: string;
  departamento: number;
  departamentoNombre?: string;
  fechaInicio: string;
  fechaFin: string | null;
  docente: number | null;
  activo: boolean;
}

export interface ProgramaAc {
  programa: number;
  programaNombre?: string;
  areaConocimiento: number;
  areaConocimientoNombre?: string;
  activo: boolean;
}

export interface ProgramaPe {
  programa: number;
  programaNombre?: string;
  practicaEstrategia: number;
  practicaEstrategiaNombre?: string;
  activo: boolean;
}

export interface ProgramaCi {
  programa: number;
  programaNombre?: string;
  carInnovacion: number;
  carInnovacionNombre?: string;
  activo: boolean;
}

export interface AnPrograma {
  aspectoNormativo: number;
  aspectoNormativoNombre?: string;
  programa: number;
  programaNombre?: string;
  activo: boolean;
}

// Opcion de un desplegable de clave foranea (9_frontend.md §8): se muestra la
// etiqueta de la maestra y se envia su clave.
export interface OpcionFk {
  valor: number | string;
  etiqueta: string;
}

// --- Configuracion generica de la pagina catalogo ---------------------------
export interface CampoConfig {
  campo: string;        // clave en snake_case (la misma que usa la API)
  etiqueta: string;     // nombre legible para la interfaz (español)
  // 'correo' se comporta como 'texto' pero ademas valida formato de correo
  // antes de enviar (lo exige el formulario de `aliado`, issue #3 punto 8).
  // 'desplegable' es una clave foranea (select); 'fecha' es un DATE (input date).
  tipo: 'texto' | 'entero' | 'correo' | 'desplegable' | 'fecha';
  requerido: boolean;
  editable: boolean;    // indica si el campo puede modificarse al editar un registro
  maximo: number | null;
  esLlave?: boolean;
  generado?: boolean;          // la base genera la clave; nunca se captura en formularios
  opcionesFijas?: string[];     // valores permitidos por el formulario, sin consultar otro catálogo
  // Metadatos de las claves foraneas (solo 'desplegable', 9_frontend.md §8):
  origen?: string;              // tabla maestra de la que salen las opciones
  origenEtiqueta?: string;      // columna a mostrar (por defecto 'nombre')
  origenSubEtiqueta?: string;   // segundo dato de contexto para evitar opciones ambiguas
  campoMostrar?: string;        // columna con el nombre resuelto (<fk>_nombre) para el listado
}

export interface TablaConfig {
  nombre: string;
  nombreLegible: string;
  llave: string;                 // llave simple de las tablas de v1
  llaves?: [string, string];     // PK compuesta: ruta en el orden del script
  esRelacion?: boolean;          // la identidad se forma con dos claves compuestas
  permiteEdicion?: boolean;      // relaciones con atributos propios: PUT/PATCH conservan las claves
  campos: CampoConfig[];
}

// Las tablas disponibles (valores válidos de :tabla, 9_frontend.md §3). En la v2
// se suman facultad, pasantia y premio (issue #1); las demás tablas de la v2 las
// integran los otros issues.
export const TABLAS: string[] = [
  'area_conocimiento',
  'universidad',
  'aspecto_normativo',
  'practica_estrategia',
  'enfoque',
  'car_innovacion',
  'aliado',
  'facultad',
  'pasantia',
  'premio',
  'docente',
  'docente_departamento',
  'alianza',
  'programa_ac',
  'programa_pe',
  'programa_ci',
  'an_programa',
];

// Configuracion por tabla. Los catalogos de Felipe siguen el mismo formulario
// generico y los contratos de 6_contracts.md §8.3–8.5. Los de Valentina
// (car_innovacion y aliado) usan el mismo formulario con los contratos §8.6–8.7.
export const CATALOGOS: Record<string, TablaConfig> = {
  docente: {
    nombre: 'docente',
    nombreLegible: 'Docentes',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'documento', etiqueta: 'Documento de identidad', tipo: 'texto', requerido: true, editable: true, maximo: 20 },
      { campo: 'nombre', etiqueta: 'Nombre completo', tipo: 'texto', requerido: true, editable: true, maximo: 120 },
    ],
  },
  area_conocimiento: {
    nombre: 'area_conocimiento',
    nombreLegible: 'Área de conocimiento',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'gran_area', etiqueta: 'Gran área', tipo: 'texto', requerido: true, editable: true, maximo: 60 },
      { campo: 'area', etiqueta: 'Área', tipo: 'texto', requerido: true, editable: true, maximo: 60 },
      { campo: 'disciplina', etiqueta: 'Disciplina', tipo: 'texto', requerido: true, editable: true, maximo: 60 },
    ],
  },
  universidad: {
    nombre: 'universidad',
    nombreLegible: 'Universidad',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 60 },
      { campo: 'tipo', etiqueta: 'Tipo', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'ciudad', etiqueta: 'Ciudad', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
    ],
  },
  aspecto_normativo: {
    nombre: 'aspecto_normativo',
    nombreLegible: 'Aspecto normativo',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'tipo', etiqueta: 'Tipo', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'fuente', etiqueta: 'Fuente', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
    ],
  },
  practica_estrategia: {
    nombre: 'practica_estrategia',
    nombreLegible: 'Práctica o estrategia',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'tipo', etiqueta: 'Tipo', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
    ],
  },
  enfoque: {
    nombre: 'enfoque',
    nombreLegible: 'Enfoque',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
    ],
  },
  car_innovacion: {
    nombre: 'car_innovacion',
    nombreLegible: 'Característica de innovación',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      // En la base es VARCHAR(MAX): obligatoria pero sin máximo por la API
      // (6_contracts.md §8.6), por eso maximo va en null.
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'texto', requerido: true, editable: true, maximo: null },
      { campo: 'tipo', etiqueta: 'Tipo', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
    ],
  },
  aliado: {
    nombre: 'aliado',
    nombreLegible: 'Aliado',
    // Única tabla de la v1 cuya llave es `nit` y no `id` (6_contracts.md §0.2).
    llave: 'nit',
    campos: [
      { campo: 'nit', etiqueta: 'NIT', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
      { campo: 'razon_social', etiqueta: 'Razón social', tipo: 'texto', requerido: true, editable: true, maximo: 60 },
      { campo: 'nombre_contacto', etiqueta: 'Nombre de contacto', tipo: 'texto', requerido: true, editable: true, maximo: 60 },
      { campo: 'correo', etiqueta: 'Correo', tipo: 'correo', requerido: true, editable: true, maximo: 70 },
      { campo: 'telefono', etiqueta: 'Teléfono', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'ciudad', etiqueta: 'Ciudad', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
    ],
  },
  // --- v2 (issue #1) --------------------------------------------------------
  facultad: {
    nombre: 'facultad',
    nombreLegible: 'Facultad',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 60 },
      { campo: 'tipo', etiqueta: 'Tipo', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'fecha_fun', etiqueta: 'Fecha de fundación', tipo: 'fecha', requerido: true, editable: true, maximo: null },
      {
        campo: 'universidad',
        etiqueta: 'Universidad',
        tipo: 'desplegable',
        requerido: true,
        editable: true,
        maximo: null,
        origen: 'universidad',
        origenEtiqueta: 'nombre',
        campoMostrar: 'universidad_nombre',
      },
    ],
  },
  pasantia: {
    nombre: 'pasantia',
    nombreLegible: 'Pasantía',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'pais', etiqueta: 'País', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'empresa', etiqueta: 'Empresa', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      {
        campo: 'programa',
        etiqueta: 'Programa',
        tipo: 'desplegable',
        requerido: true,
        editable: true,
        maximo: null,
        origen: 'programa',
        origenEtiqueta: 'nombre',
        campoMostrar: 'programa_nombre',
      },
    ],
  },
  premio: {
    nombre: 'premio',
    nombreLegible: 'Premio',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: false, editable: false, maximo: null, esLlave: true, generado: true },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'fecha', etiqueta: 'Fecha', tipo: 'fecha', requerido: true, editable: true, maximo: null },
      { campo: 'entidad_otorgante', etiqueta: 'Entidad otorgante', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'pais', etiqueta: 'País', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      {
        campo: 'programa',
        etiqueta: 'Programa',
        tipo: 'desplegable',
        requerido: true,
        editable: true,
        maximo: null,
        origen: 'programa',
        origenEtiqueta: 'nombre',
        campoMostrar: 'programa_nombre',
      },
    ],
  },
  // --- v2, issue de Felipe: relaciones con atributos y tablas puente puras --
  docente_departamento: {
    nombre: 'docente_departamento',
    nombreLegible: 'Docentes por programa',
    llave: 'docente',
    llaves: ['docente', 'departamento'],
    esRelacion: true,
    permiteEdicion: true,
    campos: [
      { campo: 'docente', etiqueta: 'Docente', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'docente', origenEtiqueta: 'nombre', campoMostrar: 'docente_nombre' },
      { campo: 'departamento', etiqueta: 'Programa', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'programa', origenEtiqueta: 'nombre', campoMostrar: 'departamento_nombre' },
      { campo: 'dedicacion', etiqueta: 'Dedicación', tipo: 'texto', requerido: true, editable: true, maximo: 15 },
      {
        campo: 'modalidad',
        etiqueta: 'Modalidad',
        tipo: 'desplegable',
        requerido: true,
        editable: true,
        maximo: 45,
        opcionesFijas: ['Presencial', 'Virtual-Remoto', 'Hibrido'],
      },
      { campo: 'fecha_ingreso', etiqueta: 'Fecha de ingreso', tipo: 'fecha', requerido: true, editable: true, maximo: null },
      { campo: 'fecha_salida', etiqueta: 'Fecha de salida', tipo: 'fecha', requerido: false, editable: true, maximo: null },
    ],
  },
  alianza: {
    nombre: 'alianza',
    nombreLegible: 'Alianzas',
    llave: 'aliado',
    llaves: ['aliado', 'departamento'],
    esRelacion: true,
    permiteEdicion: true,
    campos: [
      { campo: 'aliado', etiqueta: 'Aliado', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'aliado', origenEtiqueta: 'razon_social', campoMostrar: 'aliado_nombre' },
      { campo: 'departamento', etiqueta: 'Programa', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'programa', origenEtiqueta: 'nombre', campoMostrar: 'departamento_nombre' },
      { campo: 'fecha_inicio', etiqueta: 'Fecha de inicio', tipo: 'fecha', requerido: true, editable: true, maximo: null },
      { campo: 'fecha_fin', etiqueta: 'Fecha de fin', tipo: 'fecha', requerido: false, editable: true, maximo: null },
      { campo: 'docente', etiqueta: 'Docente responsable', tipo: 'desplegable', requerido: false, editable: true, maximo: null, origen: 'docente', origenEtiqueta: 'nombre', campoMostrar: 'docente_nombre' },
    ],
  },
  programa_ac: {
    nombre: 'programa_ac',
    nombreLegible: 'Áreas del programa',
    llave: 'programa',
    llaves: ['programa', 'area_conocimiento'],
    esRelacion: true,
    campos: [
      { campo: 'programa', etiqueta: 'Programa', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'programa', origenEtiqueta: 'nombre', campoMostrar: 'programa_nombre' },
      { campo: 'area_conocimiento', etiqueta: 'Área de conocimiento', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'area_conocimiento', origenEtiqueta: 'area', origenSubEtiqueta: 'disciplina', campoMostrar: 'area_conocimiento_nombre' },
    ],
  },
  programa_pe: {
    nombre: 'programa_pe',
    nombreLegible: 'Prácticas del programa',
    llave: 'programa',
    llaves: ['programa', 'practica_estrategia'],
    esRelacion: true,
    campos: [
      { campo: 'programa', etiqueta: 'Programa', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'programa', origenEtiqueta: 'nombre', campoMostrar: 'programa_nombre' },
      { campo: 'practica_estrategia', etiqueta: 'Práctica o estrategia', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'practica_estrategia', origenEtiqueta: 'nombre', campoMostrar: 'practica_estrategia_nombre' },
    ],
  },
  programa_ci: {
    nombre: 'programa_ci',
    nombreLegible: 'Innovación del programa',
    llave: 'programa',
    llaves: ['programa', 'car_innovacion'],
    esRelacion: true,
    campos: [
      { campo: 'programa', etiqueta: 'Programa', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'programa', origenEtiqueta: 'nombre', campoMostrar: 'programa_nombre' },
      { campo: 'car_innovacion', etiqueta: 'Característica de innovación', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'car_innovacion', origenEtiqueta: 'nombre', campoMostrar: 'car_innovacion_nombre' },
    ],
  },
  an_programa: {
    nombre: 'an_programa',
    nombreLegible: 'Normativa del programa',
    llave: 'aspecto_normativo',
    llaves: ['aspecto_normativo', 'programa'],
    esRelacion: true,
    campos: [
      { campo: 'aspecto_normativo', etiqueta: 'Aspecto normativo', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'aspecto_normativo', origenEtiqueta: 'descripcion', campoMostrar: 'aspecto_normativo_nombre' },
      { campo: 'programa', etiqueta: 'Programa', tipo: 'desplegable', requerido: true, editable: false, maximo: null, esLlave: true, origen: 'programa', origenEtiqueta: 'nombre', campoMostrar: 'programa_nombre' },
    ],
  },
};

export function obtenerConfig(tabla: string): TablaConfig | undefined {
  return CATALOGOS[tabla];
}

export function nombreLegible(tabla: string): string {
  return CATALOGOS[tabla]?.nombreLegible ?? tabla;
}
