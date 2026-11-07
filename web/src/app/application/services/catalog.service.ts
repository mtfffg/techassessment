import { Inject, Injectable } from '@angular/core';
import { ICatalogRepository } from '../../core/repositories/ICatalogRepository';
import { CATALOG_REPOSITORY } from '../bookstore.tokens';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  constructor(@Inject(CATALOG_REPOSITORY) private catalog: ICatalogRepository) {}

  searchBooks(search: string) {
    //search through the catalog repository
    return this.catalog.searchBooks(search.trim());
  }
}
