// =============================================================================
// secciones.ts
// ÚNICA fuente de la agrupación y del aspecto visual de los catálogos:
//  - SECCIONES: las 4 secciones del módulo, en el orden del dashboard.
//  - CATALOGO_VISUAL: categoría, descripción e ícono de cada tabla.
// La usan el menú lateral y el dashboard, para que ambos muestren siempre las
// mismas secciones y las mismas tablas.
//
// No es una regla de negocio ni cambia rutas: es presentación. Una tabla que
// no aparezca en ninguna sección cae sola en "Otros" (seccionesDe), así que
// agregar una tabla nueva a TABLAS nunca la deja por fuera de la interfaz.
// =============================================================================
import { TABLAS } from './catalogo';

export interface Seccion {
  id: string;
  titulo: string;
  descripcion: string; // línea corta que explica la sección
  tablas: string[];
  punto: 'azul' | 'cian' | 'verde'; // color del punto en el menú lateral
  icono: string;                    // nombre del ícono (app-icono)
  ordenMenu: number;                // orden con que el menú lista los grupos
}

export const SECCIONES: Seccion[] = [
  {
    id: 'innovacion',
    titulo: 'Innovación Curricular y Conocimiento',
    descripcion: 'Áreas maestras, estrategias de innovación y normativa curricular',
    tablas: [
      'area_conocimiento',
      'car_innovacion',
      'aspecto_normativo',
      'practica_estrategia',
      'enfoque',
      // Tablas puente relacionadas con la innovación
      'enfoque_rc',
      'programa_pe',
      'programa_ci',
      'an_programa',
    ],
    punto: 'cian',
    icono: 'sparkles',
    ordenMenu: 3,
  },
  {
    id: 'programas',
    titulo: 'Programas Académicos y Registro',
    descripcion: 'Acreditaciones oficiales, registros calificados, pasantías y reconocimientos',
    tablas: [
      'programa',
      'acreditacion',
      'registro_calificado',
      'activ_academica',
      'pasantia',
      'premio',
      'aa_rc',
      'programa_ac',
    ],
    punto: 'azul',
    icono: 'graduation',
    ordenMenu: 2,
  },
  {
    id: 'institucion',
    titulo: 'Institución',
    descripcion: 'Universidades, facultades y talento docente de la institución',
    tablas: ['universidad', 'facultad', 'docente', 'docente_departamento'],
    punto: 'azul',
    icono: 'building',
    ordenMenu: 1,
  },
  {
    id: 'alianzas',
    titulo: 'Alianzas',
    descripcion: 'Aliados y convenios con el sector externo',
    tablas: ['aliado', 'alianza'],
    punto: 'verde',
    icono: 'network',
    ordenMenu: 4,
  },
];

// Aspecto de cada catálogo en las tarjetas del dashboard. La categoría es la
// etiqueta pequeña superior, la descripción se usa en la tarjeta destacada y
// el ícono es el nombre que entiende app-icono.
export interface VisualCatalogo {
  categoria: string;
  descripcion: string;
  icono: string;
}

export const CATALOGO_VISUAL: Record<string, VisualCatalogo> = {
  // --- Innovación Curricular y Conocimiento ---
  area_conocimiento: {
    categoria: 'Innovación',
    descripcion: 'Gran área, área y disciplina que clasifican la producción académica del programa.',
    icono: 'layers',
  },
  car_innovacion: {
    categoria: 'Innovación',
    descripcion: 'Características que evidencian la innovación curricular en cada programa.',
    icono: 'sparkles',
  },
  aspecto_normativo: {
    categoria: 'Legal',
    descripcion: 'Aspectos normativos y fuentes legales que enmarcan el currículo.',
    icono: 'scale',
  },
  practica_estrategia: {
    categoria: 'Metodología',
    descripcion: 'Prácticas y estrategias didácticas empleadas en la formación.',
    icono: 'compass',
  },
  enfoque: {
    categoria: 'Pedagogía',
    descripcion: 'Enfoques pedagógicos que orientan el diseño curricular.',
    icono: 'target',
  },
  enfoque_rc: {
    categoria: 'Vinculación',
    descripcion: 'Enfoques asociados a cada registro calificado.',
    icono: 'link',
  },
  programa_pe: {
    categoria: 'Vinculación',
    descripcion: 'Prácticas y estrategias asignadas a los programas.',
    icono: 'link',
  },
  programa_ci: {
    categoria: 'Vinculación',
    descripcion: 'Características de innovación asociadas a los programas.',
    icono: 'link',
  },
  an_programa: {
    categoria: 'Vinculación',
    descripcion: 'Aspectos normativos asociados a los programas.',
    icono: 'link',
  },

  // --- Programas Académicos y Registro ---
  programa: {
    categoria: 'Formación',
    descripcion: 'Oferta académica de la institución y su información general.',
    icono: 'graduation',
  },
  acreditacion: {
    categoria: 'Calidad',
    descripcion: 'Acreditaciones oficiales que respaldan la calidad de los programas.',
    icono: 'award',
  },
  registro_calificado: {
    categoria: 'Regulación',
    descripcion: 'Registros calificados y condiciones de operación de los programas.',
    icono: 'file-text',
  },
  activ_academica: {
    categoria: 'Pedagogía',
    descripcion: 'Actividades académicas que componen los planes de estudio.',
    icono: 'book',
  },
  pasantia: {
    categoria: 'Práctica',
    descripcion: 'Pasantías y experiencias externas de los estudiantes.',
    icono: 'briefcase',
  },
  premio: {
    categoria: 'Reconocimiento',
    descripcion: 'Premios y reconocimientos obtenidos por los programas.',
    icono: 'trophy',
  },
  aa_rc: {
    categoria: 'Vinculación',
    descripcion: 'Actividades académicas vinculadas a cada registro calificado.',
    icono: 'link',
  },
  programa_ac: {
    categoria: 'Vinculación',
    descripcion: 'Áreas de conocimiento asociadas a los programas.',
    icono: 'link',
  },

  // --- Institución ---
  universidad: {
    categoria: 'Institución',
    descripcion: 'Universidades registradas en el sistema.',
    icono: 'building',
  },
  facultad: {
    categoria: 'Institución',
    descripcion: 'Facultades que agrupan la oferta académica.',
    icono: 'building',
  },
  docente: {
    categoria: 'Talento',
    descripcion: 'Docentes vinculados a la institución.',
    icono: 'user',
  },
  docente_departamento: {
    categoria: 'Vinculación',
    descripcion: 'Docentes asignados a cada programa o departamento.',
    icono: 'user-check',
  },

  // --- Alianzas ---
  aliado: {
    categoria: 'Alianzas',
    descripcion: 'Organizaciones aliadas del sector externo.',
    icono: 'users',
  },
  alianza: {
    categoria: 'Alianzas',
    descripcion: 'Convenios vigentes entre aliados y programas.',
    icono: 'network',
  },
};

// Devuelve las secciones con solo las tablas que existen en TABLAS y agrega
// al final "Otros" con las que ninguna sección reclama. `tablasBase` permite
// reutilizar la función en pruebas con otra lista; por defecto es TABLAS.
export function seccionesDe(tablasBase: string[] = TABLAS): Seccion[] {
  const asignadas = new Set<string>();
  const secciones = SECCIONES.map((seccion) => {
    const tablas = seccion.tablas.filter((tabla) => tablasBase.includes(tabla));
    tablas.forEach((tabla) => asignadas.add(tabla));
    return { ...seccion, tablas };
  });

  const restantes = tablasBase.filter((tabla) => !asignadas.has(tabla));
  if (restantes.length > 0) {
    secciones.push({
      id: 'otros',
      titulo: 'Otros',
      descripcion: 'Catálogos que aún no pertenecen a una sección',
      tablas: restantes,
      punto: 'azul',
      icono: 'layers',
      ordenMenu: 99,
    });
  }
  return secciones;
}
