import { Observable } from 'rxjs';
import { ICatalogBook } from '../models/ICatalogBook';

export interface ICatalogRepository {
  searchBooks(search: string): Observable<ICatalogBook[]>;
}
