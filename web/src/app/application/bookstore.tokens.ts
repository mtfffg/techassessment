import { InjectionToken } from '@angular/core';
import { IBookRepository } from '../core/repositories/IBookRepository';
import { ICatalogRepository } from '../core/repositories/ICatalogRepository';

//angular uses these tokens to find the repository implementations
export const BOOKS_REPOSITORY = new InjectionToken<IBookRepository>('BookRepository');
export const CATALOG_REPOSITORY = new InjectionToken<ICatalogRepository>('CatalogRepository');
