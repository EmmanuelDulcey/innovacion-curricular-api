// =============================================================================
// catalogo.ts
// Modelos del Frontend de la v1. La API responde en snake_case (6_contracts.md);
// aqui se definen las interfaces en camelCase (9_frontend.md §4) y la
// configuracion generica que usa la pagina /tablas/:tabla para armar columnas
// y formularios con las reglas de 6_contracts.md §8.
// Issue 1 (Emmanuel): area_conocimiento y universidad.
// Issue 2 (Felipe): aspecto_normativo, practica_estrategia y enfoque.
// Issue 3 (Valentina): car_innovacion y aliado.
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

// Opcion de un desplegable de clave foranea (9_frontend.md §8): se muestra la
// etiqueta de la maestra y se envia su clave.
export interface OpcionFk {
  valor: number;
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
  editable: boolean;    // false para la llave: obligatoria al crear, solo lectura al editar
  maximo: number | null;
  esLlave?: boolean;
  // Metadatos de las claves foraneas (solo 'desplegable', 9_frontend.md §8):
  origen?: string;              // tabla maestra de la que salen las opciones
  origenEtiqueta?: string;      // columna a mostrar (por defecto 'nombre')
  campoMostrar?: string;        // columna con el nombre resuelto (<fk>_nombre) para el listado
}

export interface TablaConfig {
  nombre: string;
  nombreLegible: string;
  llave: string;                 // 'id' (cualquiera de las dos tablas del issue 1)
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
];

// Configuracion por tabla. Los catalogos de Felipe siguen el mismo formulario
// generico y los contratos de 6_contracts.md §8.3–8.5. Los de Valentina
// (car_innovacion y aliado) usan el mismo formulario con los contratos §8.6–8.7.
export const CATALOGOS: Record<string, TablaConfig> = {
  area_conocimiento: {
    nombre: 'area_conocimiento',
    nombreLegible: 'Área de conocimiento',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
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
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
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
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
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
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
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
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'descripcion', etiqueta: 'Descripción', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
    ],
  },
  car_innovacion: {
    nombre: 'car_innovacion',
    nombreLegible: 'Característica de innovación',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
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
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
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
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
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
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
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
};

export function obtenerConfig(tabla: string): TablaConfig | undefined {
  return CATALOGOS[tabla];
}

export function nombreLegible(tabla: string): string {
  return CATALOGOS[tabla]?.nombreLegible ?? tabla;
}
