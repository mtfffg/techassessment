import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { BooksService } from './books.service';
import { EGenre } from '../enums/EGenre';
import { EBookSort } from '../enums/EBookSort';

describe('BooksService', () => {
  it('sends the filter, page and sort options to the API', () => {
    // Angular replaces real HTTP calls with a test controller.
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    const service = TestBed.inject(BooksService);
    const http = TestBed.inject(HttpTestingController);

    service.getBooks({ genre: EGenre.Fantasy, page: 2, pageSize: 5, sort: EBookSort.Price, descending: true })
      .subscribe();

    const request = http.expectOne(request => request.url === '/api/books');
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('genre')).toBe('Fantasy');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('5');
    expect(request.request.params.get('sort')).toBe('Price');
    expect(request.request.params.get('descending')).toBe('true');

    request.flush({ items: [], totalCount: 0, page: 2, pageSize: 5 });
    http.verify();
  });
});
