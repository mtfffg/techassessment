import { IBookRepository } from '../../core/repositories/IBookRepository';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IBookPage } from '../../core/models/IBookPage';
import { ISearchBook } from '../../core/models/ISearchBook';
import { IAddBook } from '../../core/models/IAddBook';
import { IBook } from '../../core/models/IBook';
import { IUpdateBook } from '../../core/models/IUpdateBook';

@Injectable({ providedIn: 'root' })
export class BookRepository implements IBookRepository {
  constructor(private http: HttpClient) {}

  //service call to get books
  getBooks(search: ISearchBook) {
    let params = new HttpParams()
      .set('page', search.page)
      .set('pageSize', search.pageSize)
      .set('sort', search.sort)
      .set('descending', search.descending);

    if (search.genre) params = params.set('genre', search.genre);

    return this.http.get<IBookPage>('/api/books', { params });
  }

  //service call to save book
  addBook(book: IAddBook) {
    return this.http.post<IBook>('/api/books', book);
  }

  //service call to delete book
  deleteBook(id: string) {
    return this.http.delete<void>(`/api/books/${id}`);
  }

  //service call to get one book
  getBook(id: string) {
    return this.http.get<IBook>(`/api/books/${id}`);
  }

  //service call to update book
  updateBook(id: string, book: IUpdateBook) {
    return this.http.put<IBook>(`/api/books/${id}`, book);
  }
}
