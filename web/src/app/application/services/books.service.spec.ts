import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { BOOKS_REPOSITORY } from '../bookstore.tokens';
import { BookRepository } from '../../infrastructure/repositories/books.repository';
import { TestBed } from '@angular/core/testing';
import { BooksService } from './books.service';
import { EGenre } from '../../core/enums/EGenre';

describe('BooksService', () => {
  it('calls the crud endpoints and returns api errors', () => {
    //replace real http calls for this test
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), { provide: BOOKS_REPOSITORY, useClass: BookRepository }] });
    const service = TestBed.inject(BooksService);
    const http = TestBed.inject(HttpTestingController);
    const book = {
      id: 'book-1', title: 'The Hobbit', author: 'J. R. R. Tolkien', isbn: '9780547928227',
      publishedYear: 1937, coverUrl: null, openLibraryKey: '/works/OL27448W', description: null,
      genre: EGenre.Fantasy, price: 15.50, stock: 5,
    };

    //create
    service.addBook(book).subscribe(result => expect(result).toEqual(book));
    const create = http.expectOne('/api/books');
    expect(create.request.method).toBe('POST');
    expect(create.request.body).toEqual(book);
    create.flush(book);

    //read
    service.getBook(book.id).subscribe(result => expect(result).toEqual(book));
    const read = http.expectOne('/api/books/book-1');
    expect(read.request.method).toBe('GET');
    read.flush(book);
    service.selectGenre(EGenre.Fantasy);
    const list = http.expectOne(request => request.url === '/api/books');
    expect(list.request.params.get('genre')).toBe('Fantasy');
    list.flush({ items: [book], totalCount: 1, page: 1, pageSize: 10 });
    expect(service.books()).toEqual([book]);
    expect(service.totalCount()).toBe(1);
    expect(service.loading()).toBe(false);

    //update
    const changes = { genre: EGenre.Fantasy, price: 12.99, stock: 8 };
    service.updateBook(book.id, changes).subscribe(result => expect(result.price).toBe(12.99));
    const update = http.expectOne('/api/books/book-1');
    expect(update.request.method).toBe('PUT');
    expect(update.request.body).toEqual(changes);
    update.flush({ ...book, ...changes });

    //delete
    service.deleteBook(book.id).subscribe();
    const remove = http.expectOne('/api/books/book-1');
    expect(remove.request.method).toBe('DELETE');
    remove.flush(null);

    //pass api errors back to the component
    let errorStatus = 0;
    service.getBook(book.id).subscribe({ error: error => { errorStatus = error.status; } });
    http.expectOne('/api/books/book-1').flush({ title: 'Book not found.' }, { status: 404, statusText: 'Not Found' });
    expect(errorStatus).toBe(404);

    //show an error if the list cannot load
    service.loadBooks();
    http.expectOne(request => request.url === '/api/books').flush(
      { title: 'API unavailable.' }, { status: 503, statusText: 'Service Unavailable' });
    expect(service.error()).toBe('Could not load books. Please try again.');
    expect(service.loading()).toBe(false);
    http.verify();
  });
});
