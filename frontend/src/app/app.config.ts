// =============================================================================
// QUÉ ES: configuración global con la que Angular inicia la aplicación.
// PAPEL: registra el router, el cliente HTTP y la restauración del scroll para
//       que cada pantalla nueva abra desde arriba.
// =============================================================================
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, withComponentInputBinding, withInMemoryScrolling } from '@angular/router';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // Cada sección es una pantalla nueva; empezar arriba evita heredar el
    // desplazamiento de la sección anterior y mostrar un espacio vacío.
    provideRouter(routes, withComponentInputBinding(), withInMemoryScrolling({ scrollPositionRestoration: 'top' })),
    provideHttpClient(),
  ]
};
