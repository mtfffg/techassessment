import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { EditBookComponent } from './edit-book.component';
import { EGenre } from '../enums/EGenre';
import { IBook } from '../models/IBook';
import { BooksService } from '../services/books.service';

describe('EditBookComponent', () => {
  const book: IBook = {
    id: 'saved-book', title: 'The Witcher', author: 'Andrzej Sapkowski', isbn: '9780316029186',
    publishedYear: 1993, coverUrl: null, openLibraryKey: '/works/OL123W', description: null,
    genre: EGenre.Fantasy, price: 12.99, stock: 2,
  };
  let dialog: EditBookComponent;
  let getBook: ReturnType<typeof vi.fn>;
  let updateBook: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    getBook = vi.fn().mockReturnValue(of(book));
    updateBook = vi.fn().mockReturnValue(of({ ...book, genre: EGenre.History, price: 15.50, stock: 8 }));
    //use a fake service instead of calling the api
    dialog = new EditBookComponent({ getBook, updateBook } as unknown as BooksService);
    dialog.bookId = book.id;
  });

  it('loads the saved genre, price and stock', () => {
    dialog.loadBook();

    expect(getBook).toHaveBeenCalledWith(book.id);
    expect(dialog.genre).toBe(EGenre.Fantasy);
    expect(dialog.price).toBe(12.99);
    expect(dialog.stock).toBe(2);
    expect(dialog.loading()).toBe(false);
  });

  it('saves only the store details and closes the dialog', () => {
    dialog.loadBook();
    dialog.genre = EGenre.History;
    dialog.price = 15.50;
    dialog.stock = 8;
    let savedBook: IBook | null = null;
    dialog.bookSaved.subscribe(book => { savedBook = book; });

    dialog.saveBook();

    expect(updateBook).toHaveBeenCalledWith(book.id, { genre: EGenre.History, price: 15.50, stock: 8 });
    expect(savedBook).toEqual({ ...book, genre: EGenre.History, price: 15.50, stock: 8 });
    expect(dialog.bookId).toBeNull();
  });

  it('rejects negative stock before calling the api', () => {
    dialog.loadBook();
    dialog.stock = -1;

    dialog.saveBook();

    expect(updateBook).not.toHaveBeenCalled();
    expect(dialog.error()).toBe('Select a genre, enter a valid price and a whole stock quantity.');
  });

  it('keeps the entered details when saving fails', () => {
    dialog.loadBook();
    dialog.stock = 8;
    updateBook.mockReturnValueOnce(throwError(() => new HttpErrorResponse({
      status: 404, error: { title: 'Book not found.' },
    })));

    dialog.saveBook();

    expect(dialog.error()).toBe('Book not found.');
    expect(dialog.stock).toBe(8);
    expect(dialog.bookId).toBe(book.id);
    expect(dialog.saving()).toBe(false);
  });

  it('shows a load error and lets the user retry', () => {
    getBook.mockReturnValueOnce(throwError(() => new Error('API unavailable')));
    dialog.loadBook();
    expect(dialog.loading()).toBe(false);
    expect(dialog.error()).toBe('Could not load book. Please try again.');

    dialog.loadBook();
    expect(dialog.error()).toBe('');
    expect(dialog.book()).toBe(book);
  });
});
