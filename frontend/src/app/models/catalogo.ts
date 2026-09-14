// =============================================================================
// catalogo.ts
// Modelos del Frontend de la v1. La API responde en snake_case (6_contracts.md);
// aqui se definen las interfaces en camelCase (9_frontend.md §4) y la
// configuracion generica que usa la pagina /tablas/:tabla para armar columnas
// y formularios con las reglas de 6_contracts.md §8.
// Issue 1 (Emmanuel): area_conocimiento y universidad.
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

export type CualquierRegistro = Record<string, any>;

// --- Configuracion generica de la pagina catalogo ---------------------------
export interface CampoConfig {
  campo: string;        // clave en snake_case (la misma que usa la API)
  etiqueta: string;     // nombre legible para la interfaz (español)
  tipo: 'texto' | 'entero';
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

// Configuracion por tabla. Por ahora solo Area de conocimiento y Universidad
// (Issue 1); los demas catalogos se agregan en los issues 2 y 3.
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
};

export function obtenerConfig(tabla: string): TablaConfig | undefined {
  return CATALOGOS[tabla];
}

export function nombreLegible(tabla: string): string {
  return CATALOGOS[tabla]?.nombreLegible ?? tabla;
}