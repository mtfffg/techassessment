import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { HomeComponent } from './home.component';
import { BooksService } from '../services/books.service';
import { EGenre } from '../enums/EGenre';
import { IBook } from '../models/IBook';

describe('HomeComponent', () => {
  const emptyPage = { items: [], totalCount: 0, page: 1, pageSize: 10 };
  let home: HomeComponent;
  let getBooks: ReturnType<typeof vi.fn>;
  let deleteBook: ReturnType<typeof vi.fn>;
  const book: IBook = {
    id: 'saved-book', title: 'The Witcher', author: 'Andrzej Sapkowski', isbn: '9780316029186',
    publishedYear: 1993, coverUrl: null, openLibraryKey: '/works/OL123W', description: null,
    genre: EGenre.Fantasy, price: 12.99, stock: 0,
  };

  beforeEach(() => {
    getBooks = vi.fn().mockReturnValue(of(emptyPage));
    deleteBook = vi.fn().mockReturnValue(of(undefined));
    // Use a fake service so these tests do not call the API.
    home = new HomeComponent({ getBooks, deleteBook } as unknown as BooksService);
  });

  it('starts at page one when the genre changes', () => {
    home.search.page = 3;

    home.selectGenre(EGenre.Fantasy);

    expect(home.search.genre).toBe('Fantasy');
    expect(home.search.page).toBe(1);
    expect(getBooks).toHaveBeenCalledOnce();
  });

  it('keeps the chosen page size when sorting', () => {
    home.changePage({ first: 5, rows: 5 });
    expect(home.search.page).toBe(2);

    home.changeSort({ sortField: 'Price', sortOrder: -1 });

    expect(home.search.page).toBe(1);
    expect(home.search.pageSize).toBe(5);
    expect(home.search.sort).toBe('Price');
    expect(home.search.descending).toBe(true);
  });

  it('shows an error when loading fails and clears it on retry', () => {
    getBooks.mockReturnValueOnce(throwError(() => new Error('API unavailable')));

    home.loadBooks();
    expect(home.error()).toBe('Could not load books. Please try again.');
    expect(home.loading()).toBe(false);

    home.loadBooks();
    expect(home.error()).toBe('');
  });

  it('refreshes the table after saving a book', () => {
    home.search.genre = EGenre.History;
    home.search.page = 3;

    home.onBookSaved(book);

    expect(home.search.genre).toBeNull();
    expect(home.search.page).toBe(1);
    expect(getBooks).toHaveBeenCalledOnce();
  });

  it('deletes a confirmed book and returns to the previous page if needed', () => {
    home.books.set([book]);
    home.search.page = 2;
    home.confirmDelete(book);

    home.deleteBook();

    expect(deleteBook).toHaveBeenCalledWith(book.id);
    expect(home.bookToDelete).toBeNull();
    expect(home.search.page).toBe(1);
    expect(getBooks).toHaveBeenCalledOnce();
  });
});
