import { of, Subject, throwError } from 'rxjs';
import { vi } from 'vitest';
import { AddBookComponent } from './add-book.component';
import { ICatalogBook } from '../models/ICatalogBook';
import { CatalogService } from '../services/catalog.service';
import { BooksService } from '../services/books.service';
import { IBook } from '../models/IBook';
import { EGenre } from '../enums/EGenre';
import { HttpErrorResponse } from '@angular/common/http';

describe('AddBookComponent', () => {
  const book: ICatalogBook = {
    title: 'The Hobbit',
    author: 'J. R. R. Tolkien',
    isbn: '9780547928227',
    publishedYear: 1937,
    coverUrl: null,
    openLibraryKey: '/works/OL27448W',
  };
  let dialog: AddBookComponent;
  let searchBooks: ReturnType<typeof vi.fn>;
  let addBook: ReturnType<typeof vi.fn>;
  const savedBook: IBook = { ...book, id: 'saved-book', description: null, genre: EGenre.Fantasy, price: 12.99, stock: 0 };

  beforeEach(() => {
    searchBooks = vi.fn().mockReturnValue(of([book]));
    addBook = vi.fn().mockReturnValue(of(savedBook));
    //use a fake service instead of calling the api
    dialog = new AddBookComponent({ searchBooks } as unknown as CatalogService, { addBook } as unknown as BooksService);
  });

  it('searches and shows the returned books', () => {
    dialog.searchTerm = '  hobbit  ';

    dialog.searchBooks();

    expect(searchBooks).toHaveBeenCalledWith('hobbit');
    expect(dialog.books()).toEqual([book]);
    expect(dialog.loading()).toBe(false);
  });

  it('saves the selected book and closes the dialog', () => {
    let result: IBook | null = null;
    dialog.bookSaved.subscribe(book => { result = book; });
    dialog.visible = true;
    dialog.selectedBook.set(book);
    dialog.genre = EGenre.Fantasy;
    dialog.price = 12.99;

    dialog.saveBook();

    expect(addBook).toHaveBeenCalledWith({ ...book, genre: EGenre.Fantasy, price: 12.99, stock: 0 });
    expect(result).toBe(savedBook);
    expect(dialog.visible).toBe(false);
  });

  it('requires genre and price before saving', () => {
    dialog.selectedBook.set(book);

    dialog.saveBook();

    expect(addBook).not.toHaveBeenCalled();
    expect(dialog.error()).toBe('Select a genre and enter a valid price.');
  });

  it('keeps the dialog open when the book is already saved', () => {
    addBook.mockReturnValueOnce(throwError(() => new HttpErrorResponse({
      status: 409, error: { title: 'This book is already in your store.' },
    })));
    dialog.visible = true;
    dialog.selectedBook.set(book);
    dialog.genre = EGenre.Fantasy;
    dialog.price = 12.99;

    dialog.saveBook();

    expect(dialog.visible).toBe(true);
    expect(dialog.saving()).toBe(false);
    expect(dialog.error()).toBe('This book is already in your store.');
  });

  it('shows a search error and clears it when trying again', () => {
    searchBooks.mockReturnValueOnce(throwError(() => new Error('API unavailable')));
    dialog.searchTerm = 'hobbit';

    dialog.searchBooks();
    expect(dialog.error()).toBe('Could not search Open Library. Please try again.');

    searchBooks.mockReturnValueOnce(of([]));
    dialog.searchBooks();
    expect(dialog.error()).toBe('');
    expect(dialog.books()).toEqual([]);
    expect(dialog.searched()).toBe(true);
  });

  it('stops an unfinished search when the dialog closes', () => {
    const response = new Subject<ICatalogBook[]>();
    searchBooks.mockReturnValue(response);
    dialog.searchTerm = 'hobbit';
    dialog.searchBooks();

    dialog.close();
    response.next([book]);

    expect(dialog.books()).toEqual([]);
    expect(dialog.loading()).toBe(false);
    expect(dialog.searchTerm).toBe('');
  });
});
