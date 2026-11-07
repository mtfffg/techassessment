import { Observable } from 'rxjs';
import { IAddBook } from '../models/IAddBook';
import { IBook } from '../models/IBook';
import { IBookPage } from '../models/IBookPage';
import { ISearchBook } from '../models/ISearchBook';
import { IUpdateBook } from '../models/IUpdateBook';

export interface IBookRepository {
  getBooks(search: ISearchBook): Observable<IBookPage>;
  getBook(id: string): Observable<IBook>;
  addBook(book: IAddBook): Observable<IBook>;
  updateBook(id: string, book: IUpdateBook): Observable<IBook>;
  deleteBook(id: string): Observable<void>;
}
