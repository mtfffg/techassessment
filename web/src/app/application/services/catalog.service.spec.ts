import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CATALOG_REPOSITORY } from '../bookstore.tokens';
import { CatalogRepository } from '../../infrastructure/repositories/catalog.repository';
import { TestBed } from '@angular/core/testing';
import { CatalogService } from './catalog.service';

describe('CatalogService', () => {
  it('searches through the api and returns search errors', () => {
    //replace real http calls for this test
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), { provide: CATALOG_REPOSITORY, useClass: CatalogRepository }] });
    const service = TestBed.inject(CatalogService);
    const http = TestBed.inject(HttpTestingController);
    const books = [{ title: 'The Hobbit', author: 'J. R. R. Tolkien', isbn: '9780547928227',
      publishedYear: 1937, coverUrl: null, openLibraryKey: '/works/OL27448W' }];

    //search and check the returned books
    service.searchBooks('hobbit').subscribe(result => expect(result).toEqual(books));
    const search = http.expectOne('/api/catalog/search?q=hobbit');
    expect(search.request.method).toBe('GET');
    search.flush(books);

    //pass search errors back to the component
    let errorStatus = 0;
    service.searchBooks('hobbit').subscribe({ error: error => { errorStatus = error.status; } });
    http.expectOne('/api/catalog/search?q=hobbit').flush(
      { title: 'Open Library is unavailable.' }, { status: 503, statusText: 'Service Unavailable' });
    expect(errorStatus).toBe(503);
    http.verify();
  });
});
