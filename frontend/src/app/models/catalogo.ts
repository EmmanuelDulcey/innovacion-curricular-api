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

// --- Entidades de la v2 (issue #2, Valentina) --------------------------------
// programa es el maestro central; le cuelgan acreditaciones, registros
// calificados y actividades academicas, y las puentes enfoque_rc y aa_rc.

export interface Programa {
  id: number;
  nombre: string;
  tipo: string;
  nivel: string;
  fechaCreacion: string;     // es TEXTO en la base (VARCHAR), no fecha
  fechaCierre: string | null;
  numeroCohortes: string;
  cantGraduados: string;
  fechaActualizacion: string;
  ciudad: string;
  facultad: number;
  facultadNombre?: string;
  // Solo lectura: lo mantiene el disparador al asignar/retirar acreditaciones.
  numAcreditaciones: number;
  activo: boolean;
}

export interface Acreditacion {
  resolucion: number;
  tipo: string;
  calificacion: string;
  fechaInicio: string;       // TEXTO en la base
  fechaFin: string;          // TEXTO en la base
  programa: number;
  programaNombre?: string;
  activo: boolean;
}

export interface RegistroCalificado {
  codigo: number;
  cantCreditos: string;
  horaAcom: string;
  horaInd: string;
  metodologia: string;
  fechaInicio: string;       // DATE -> "yyyy-MM-dd"
  fechaFin: string;          // DATE -> "yyyy-MM-dd"
  duracionAnios: string;
  duracionSemestres: string;
  tipoTitulacion: string;
  programa: number;
  programaNombre?: string;
  activo: boolean;
}

export interface ActivAcademica {
  id: number;
  nombre: string;
  numCreditos: number;
  tipo: string;
  areaFormacion: string;
  hAcom: number;
  hIndep: number;
  idioma: string;
  espejo: number;            // TINYINT 0/1
  entidadEspejo: string;
  paisEspejo: string;
  disenio: number | null;    // la unica clave foranea OPCIONAL (→ null)
  disenioNombre?: string;
  activo: boolean;
}

export interface EnfoqueRc {
  enfoque: number;
  registroCalificado: number;
  enfoqueNombre?: string;
  registroCalificadoNombre?: string;
  activo: boolean;
}

export interface AaRc {
  activAcademicasIdcurso: number;
  registroCalificadoCodigo: number;
  componente: string;
  semestre: string;
  activAcademicaNombre?: string;
  registroCalificadoNombre?: string;
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
  // 'booleano' es un 0/1 de la base (p. ej. activ_academica.espejo): select Sí/No.
  tipo: 'texto' | 'entero' | 'correo' | 'desplegable' | 'fecha' | 'booleano';
  requerido: boolean;
  editable: boolean;    // false para la llave: obligatoria al crear, solo lectura al editar
  maximo: number | null;
  esLlave?: boolean;
  // Columna que la API mantiene sola (p. ej. programa.num_acreditaciones, que lo
  // escribe el disparador): se muestra en el listado pero NUNCA viaja en el body.
  soloLectura?: boolean;
  // Metadatos de las claves foraneas (solo 'desplegable', 9_frontend.md §8):
  origen?: string;              // tabla maestra de la que salen las opciones
  origenEtiqueta?: string;      // columna(s) a mostrar; varias se unen con '+' (por defecto 'nombre')
  campoMostrar?: string;        // columna con el nombre resuelto (<fk>_nombre) para el listado
}

export interface TablaConfig {
  nombre: string;
  nombreLegible: string;
  llave: string;                 // 'id' (cualquiera de las dos tablas del issue 1)
  campos: CampoConfig[];
  // Tabla puente (v2): la pareja se asigna y se retira; no hay boton Editar ni
  // PUT/PATCH (9_frontend.md §9 y tropiezo 9). La PK es la union de los campos
  // marcados con esLlave, en su orden.
  esPuente?: boolean;
}

// Las tablas disponibles (valores válidos de :tabla, 9_frontend.md §3). En la v2
// se suman facultad, pasantia y premio (issue #1) y el bloque de Valentina
// (issue #2); las tablas restantes de la v2 las integran los otros issues.
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
  // Issue #2 (Valentina)
  'programa',
  'acreditacion',
  'registro_calificado',
  'activ_academica',
  'enfoque_rc',
  'aa_rc',
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
  // --- v2 (issue #2, Valentina) ---------------------------------------------
  programa: {
    nombre: 'programa',
    nombreLegible: 'Programa',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 60 },
      { campo: 'tipo', etiqueta: 'Tipo', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'nivel', etiqueta: 'Nivel', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      // Las fechas de programa son TEXTO en la base (4_research.md D13): se
      // capturan como texto, no con el control de fecha.
      { campo: 'fecha_creacion', etiqueta: 'Fecha de creación', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'fecha_cierre', etiqueta: 'Fecha de cierre', tipo: 'texto', requerido: false, editable: true, maximo: 45 },
      { campo: 'numero_cohortes', etiqueta: 'Número de cohortes', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'cant_graduados', etiqueta: 'Cantidad de graduados', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'fecha_actualizacion', etiqueta: 'Fecha de actualización', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'ciudad', etiqueta: 'Ciudad', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      {
        campo: 'facultad',
        etiqueta: 'Facultad',
        tipo: 'desplegable',
        requerido: true,
        editable: true,
        maximo: null,
        origen: 'facultad',
        origenEtiqueta: 'nombre',
        campoMostrar: 'facultad_nombre',
      },
      // Lo mantiene el disparador: se ve en el listado, nunca viaja en el body.
      { campo: 'num_acreditaciones', etiqueta: 'Acreditaciones', tipo: 'entero', requerido: false, editable: false, maximo: null, soloLectura: true },
    ],
  },
  acreditacion: {
    nombre: 'acreditacion',
    nombreLegible: 'Acreditación',
    llave: 'resolucion',
    campos: [
      { campo: 'resolucion', etiqueta: 'Resolución', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
      { campo: 'tipo', etiqueta: 'Tipo', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'calificacion', etiqueta: 'Calificación', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      // TEXTO en la base (a diferencia de registro_calificado, que usa DATE).
      { campo: 'fecha_inicio', etiqueta: 'Fecha de inicio', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'fecha_fin', etiqueta: 'Fecha de fin', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
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
  registro_calificado: {
    nombre: 'registro_calificado',
    nombreLegible: 'Registro calificado',
    llave: 'codigo',
    campos: [
      { campo: 'codigo', etiqueta: 'Código', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
      { campo: 'cant_creditos', etiqueta: 'Cantidad de créditos', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'hora_acom', etiqueta: 'Horas acompañadas', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'hora_ind', etiqueta: 'Horas independientes', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'metodologia', etiqueta: 'Metodología', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'fecha_inicio', etiqueta: 'Fecha de inicio', tipo: 'fecha', requerido: true, editable: true, maximo: null },
      { campo: 'fecha_fin', etiqueta: 'Fecha de fin', tipo: 'fecha', requerido: true, editable: true, maximo: null },
      { campo: 'duracion_anios', etiqueta: 'Duración en años', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'duracion_semestres', etiqueta: 'Duración en semestres', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'tipo_titulacion', etiqueta: 'Tipo de titulación', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
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
  activ_academica: {
    nombre: 'activ_academica',
    nombreLegible: 'Actividad académica',
    llave: 'id',
    campos: [
      { campo: 'id', etiqueta: 'ID', tipo: 'entero', requerido: true, editable: false, maximo: null, esLlave: true },
      { campo: 'nombre', etiqueta: 'Nombre', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'num_creditos', etiqueta: 'Número de créditos', tipo: 'entero', requerido: true, editable: true, maximo: null },
      { campo: 'tipo', etiqueta: 'Tipo', tipo: 'texto', requerido: true, editable: true, maximo: 20 },
      { campo: 'area_formacion', etiqueta: 'Área de formación', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'h_acom', etiqueta: 'Horas acompañadas', tipo: 'entero', requerido: true, editable: true, maximo: null },
      { campo: 'h_indep', etiqueta: 'Horas independientes', tipo: 'entero', requerido: true, editable: true, maximo: null },
      { campo: 'idioma', etiqueta: 'Idioma', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      // TINYINT 0/1: se captura con Sí/No (el API espera 1 o 0, nunca texto).
      { campo: 'espejo', etiqueta: '¿Es espejo?', tipo: 'booleano', requerido: true, editable: true, maximo: null },
      { campo: 'entidad_espejo', etiqueta: 'Entidad espejo', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'pais_espejo', etiqueta: 'País espejo', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      // La UNICA FK opcional: sin requerido, el select ofrece «(ninguna)» → null.
      {
        campo: 'disenio',
        etiqueta: 'Programa (diseño)',
        tipo: 'desplegable',
        requerido: false,
        editable: true,
        maximo: null,
        origen: 'programa',
        origenEtiqueta: 'nombre',
        campoMostrar: 'disenio_nombre',
      },
    ],
  },
  enfoque_rc: {
    nombre: 'enfoque_rc',
    nombreLegible: 'Enfoques del registro',
    llave: 'enfoque',
    esPuente: true,
    campos: [
      // Las dos claves de la pareja, en el orden de la PK (contrato §0.2).
      {
        campo: 'enfoque',
        etiqueta: 'Enfoque',
        tipo: 'desplegable',
        requerido: true,
        editable: false,
        maximo: null,
        esLlave: true,
        origen: 'enfoque',
        origenEtiqueta: 'nombre',
        campoMostrar: 'enfoque_nombre',
      },
      {
        campo: 'registro_calificado',
        etiqueta: 'Registro calificado',
        tipo: 'desplegable',
        requerido: true,
        editable: false,
        maximo: null,
        esLlave: true,
        origen: 'registro_calificado',
        // registro_calificado no tiene columna `nombre`: la etiqueta combina
        // codigo y tipo_titulacion (separadas con '+').
        origenEtiqueta: 'codigo+tipo_titulacion',
        campoMostrar: 'registro_calificado_nombre',
      },
    ],
  },
  aa_rc: {
    nombre: 'aa_rc',
    nombreLegible: 'Actividades del registro',
    llave: 'activ_academicas_idcurso',
    esPuente: true,
    campos: [
      {
        campo: 'activ_academicas_idcurso',
        etiqueta: 'Actividad académica',
        tipo: 'desplegable',
        requerido: true,
        editable: false,
        maximo: null,
        esLlave: true,
        origen: 'activ_academica',
        origenEtiqueta: 'nombre',
        campoMostrar: 'activ_academica_nombre',
      },
      {
        campo: 'registro_calificado_codigo',
        etiqueta: 'Registro calificado',
        tipo: 'desplegable',
        requerido: true,
        editable: false,
        maximo: null,
        esLlave: true,
        origen: 'registro_calificado',
        origenEtiqueta: 'codigo+tipo_titulacion',
        campoMostrar: 'registro_calificado_nombre',
      },
      // Atributos propios de la pareja: viajan en la asignación, sin edición.
      { campo: 'componente', etiqueta: 'Componente', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
      { campo: 'semestre', etiqueta: 'Semestre', tipo: 'texto', requerido: true, editable: true, maximo: 45 },
    ],
  },
};

export function obtenerConfig(tabla: string): TablaConfig | undefined {
  return CATALOGOS[tabla];
}

export function nombreLegible(tabla: string): string {
  return CATALOGOS[tabla]?.nombreLegible ?? tabla;
}
