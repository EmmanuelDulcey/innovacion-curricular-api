// =============================================================================
// icono.ts
// Componente de íconos SVG de línea, estilo Lucide (trazo fino, sin relleno).
// Recibe el NOMBRE del ícono y el tamaño en píxeles; el color lo hereda del
// texto del contenedor (currentColor), así cada pantalla lo adapta con CSS.
// No usa CDN ni librerías: los trazos viven aquí como datos.
// =============================================================================
import { Component, computed, input } from '@angular/core';

// Trazos de cada ícono dentro de un viewBox 24x24. Cada cadena es el atributo
// `d` de un <path>; los círculos se dibujan con arcos dentro del mismo path.
const TRAZOS: Record<string, string[]> = {
  // Navegación y acciones
  grid: ['M3 3h7v7H3z', 'M14 3h7v7h-7z', 'M14 14h7v7h-7z', 'M3 14h7v7H3z'],
  'chevron-down': ['m6 9 6 6 6-6'],
  search: ['M11 19a8 8 0 1 0 0-16 8 8 0 0 0 0 16Z', 'm21 21-4.3-4.3'],
  download: ['M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4', 'm7 10 5 5 5-5', 'M12 15V3'],
  menu: ['M4 6h16', 'M4 12h16', 'M4 18h16'],
  x: ['M18 6 6 18', 'm6 6 12 12'],

  // Secciones
  sparkles: [
    'm12 3-1.9 5.8a2 2 0 0 1-1.3 1.3L3 12l5.8 1.9a2 2 0 0 1 1.3 1.3L12 21l1.9-5.8a2 2 0 0 1 1.3-1.3L21 12l-5.8-1.9a2 2 0 0 1-1.3-1.3Z',
  ],
  graduation: ['M22 10 12 5 2 10l10 5 10-5Z', 'M6 12v5c0 1.7 2.7 3 6 3s6-1.3 6-3v-5', 'M22 10v6'],
  building: ['M3 21h18', 'M5 21V7l7-4 7 4v14', 'M9 21v-6h6v6'],
  network: [
    'M18 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z',
    'M6 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z',
    'M18 22a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z',
    'm8.6 13.5 6.8 4',
    'm15.4 6.5-6.8 4',
  ],

  // Catálogos
  layers: ['m12 2 10 5-10 5L2 7l10-5Z', 'm2 12 10 5 10-5', 'm2 17 10 5 10-5'],
  scale: [
    'M7 21h10',
    'M12 3v18',
    'M3 7h2c2 0 5-1 7-2 2 1 5 2 7 2h2',
    'M16 16l3-8 3 8c-.9.7-1.9 1-3 1s-2.1-.3-3-1Z',
    'M2 16l3-8 3 8c-.9.7-1.9 1-3 1s-2.1-.3-3-1Z',
  ],
  compass: ['M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20Z', 'm16.2 7.8-2.9 6.3-6.3 2.9 2.9-6.3 6.3-2.9Z'],
  target: [
    'M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20Z',
    'M12 18a6 6 0 1 0 0-12 6 6 0 0 0 0 12Z',
    'M12 14a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z',
  ],
  link: [
    'M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.7 1.7',
    'M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.7-1.7',
  ],
  award: ['M12 15a6 6 0 1 0 0-12 6 6 0 0 0 0 12Z', 'm8.2 13.9-1.7 7.1 5.5-3 5.5 3-1.7-7.1'],
  'file-text': ['M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z', 'M14 2v5h5', 'M9 13h6', 'M9 17h6'],
  book: ['M4 19.5A2.5 2.5 0 0 1 6.5 17H20', 'M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2Z'],
  briefcase: ['M9 7V5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v2', 'M4 7h16v13H4Z', 'M4 12h16'],
  trophy: [
    'M8 21h8',
    'M12 17v4',
    'M7 4h10v5a5 5 0 0 1-10 0Z',
    'M7 6H4v2a3 3 0 0 0 3 3',
    'M17 6h3v2a3 3 0 0 1-3 3',
  ],
  users: ['M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z', 'M2 21a7 7 0 0 1 14 0', 'M16.5 3.5a4 4 0 0 1 0 7', 'M18 14.5a7 7 0 0 1 4 6.5'],
  user: ['M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2', 'M12 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z'],
  'user-check': ['M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2', 'M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z', 'm16 11 2 2 4-4'],
};

@Component({
  selector: 'app-icono',
  template: `
    <svg
      [attr.width]="tamano()"
      [attr.height]="tamano()"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.7"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      @for (trazo of trazos(); track $index) {
        <path [attr.d]="trazo" />
      }
    </svg>
  `,
  styleUrl: './icono.css',
})
export class Icono {
  // Nombre del ícono; si no existe, se dibuja un respaldo neutro (capas).
  readonly nombre = input.required<string>();
  readonly tamano = input<number>(20);

  readonly trazos = computed(() => TRAZOS[this.nombre()] ?? TRAZOS['layers']);
}
