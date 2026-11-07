import { ICatalogRepository } from '../../core/repositories/ICatalogRepository';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ICatalogBook } from '../../core/models/ICatalogBook';

@Injectable({ providedIn: 'root' })
export class CatalogRepository implements ICatalogRepository {
  constructor(private http: HttpClient) {}

  //service call to search books
  searchBooks(search: string) {
    const params = new HttpParams().set('q', search);
    return this.http.get<ICatalogBook[]>('/api/catalog/search', { params });
  }
}
