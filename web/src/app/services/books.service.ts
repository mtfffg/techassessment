import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IBookPage } from '../models/IBookPage';
import { ISearchBook } from '../models/ISearchBook';

@Injectable({ providedIn: 'root' })
export class BooksService {
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
}
