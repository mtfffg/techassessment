import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { BOOKS_REPOSITORY, CATALOG_REPOSITORY } from './application/bookstore.tokens';
import { BookRepository } from './infrastructure/repositories/books.repository';
import { CatalogRepository } from './infrastructure/repositories/catalog.repository';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    //connect repository contracts to their http implementations
    { provide: BOOKS_REPOSITORY, useClass: BookRepository },
    { provide: CATALOG_REPOSITORY, useClass: CatalogRepository },
    provideRouter(routes),
    providePrimeNG({ theme: { preset: Aura, options: {
      darkModeSelector: '.app-dark',
      cssLayer: { name: 'primeng', order: 'theme, base, primeng, components, utilities' },
    } } }),
  ]
};
