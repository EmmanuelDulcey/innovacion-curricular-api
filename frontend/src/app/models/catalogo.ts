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

// --- Configuracion generica de la pagina catalogo ---------------------------
export interface CampoConfig {
  campo: string;        // clave en snake_case (la misma que usa la API)
  etiqueta: string;     // nombre legible para la interfaz (español)
  // 'correo' se comporta como 'texto' pero ademas valida formato de correo
  // antes de enviar (lo exige el formulario de `aliado`, issue #3 punto 8).
  tipo: 'texto' | 'entero' | 'correo';
  requerido: boolean;
  editable: boolean;    // false para la llave: obligatoria al crear, solo lectura al editar
  maximo: number | null;
  esLlave?: boolean;
}

export interface TablaConfig {
  nombre: string;
  nombreLegible: string;
  llave: string;                 // 'id' (cualquiera de las dos tablas del issue 1)
  campos: CampoConfig[];
}

// Las 7 tablas disponibles en v1 (valores válidos de :tabla, 9_frontend.md §3).
export const TABLAS: string[] = [
  'area_conocimiento',
  'universidad',
  'aspecto_normativo',
  'practica_estrategia',
  'enfoque',
  'car_innovacion',
  'aliado',
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
};

export function obtenerConfig(tabla: string): TablaConfig | undefined {
  return CATALOGOS[tabla];
}

export function nombreLegible(tabla: string): string {
  return CATALOGOS[tabla]?.nombreLegible ?? tabla;
}
