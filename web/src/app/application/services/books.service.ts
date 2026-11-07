import { Inject, Injectable, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { IBookRepository } from '../../core/repositories/IBookRepository';
import { IBook } from '../../core/models/IBook';
import { IAddBook } from '../../core/models/IAddBook';
import { ISearchBook } from '../../core/models/ISearchBook';
import { IUpdateBook } from '../../core/models/IUpdateBook';
import { EGenre } from '../../core/enums/EGenre';
import { EBookSort } from '../../core/enums/EBookSort';
import { BOOKS_REPOSITORY } from '../bookstore.tokens';

@Injectable({ providedIn: 'root' })
export class BooksService {
  books = signal<IBook[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal('');
  addBookVisible = signal(false);
  search = signal<ISearchBook>({ genre: null, page: 1, pageSize: 10, sort: EBookSort.Title, descending: false });
  private request?: Subscription;

  constructor(@Inject(BOOKS_REPOSITORY) private repository: IBookRepository) {}

  getBook(id: string) {
    return this.repository.getBook(id);
  }

  addBook(book: IAddBook) {
    return this.repository.addBook(book);
  }

  updateBook(id: string, book: IUpdateBook) {
    return this.repository.updateBook(id, book);
  }

  deleteBook(id: string) {
    return this.repository.deleteBook(id);
  }

  loadBooks() {
    //stop previous request and load the list
    this.stopLoading();
    this.loading.set(true);
    this.error.set('');
    this.request = this.repository.getBooks(this.search()).subscribe({
      next: page => {
        this.books.set(page.items);
        this.totalCount.set(page.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.books.set([]);
        this.totalCount.set(0);
        this.error.set('Could not load books. Please try again.');
        this.loading.set(false);
      },
    });
  }

  selectGenre(genre: EGenre | null) {
    //reset page when the filter changes
    this.search.update(search => ({ ...search, genre, page: 1 }));
    this.loadBooks();
  }

  sortBooks(sort: EBookSort, descending: boolean) {
    if (sort === this.search().sort && descending === this.search().descending) return;
    this.search.update(search => ({ ...search, sort, descending, page: 1 }));
    this.loadBooks();
  }

  pageBooks(first: number, rows: number) {
    //set page size and page
    this.search.update(search => ({ ...search, pageSize: rows, page: Math.floor(first / rows) + 1 }));
    this.loadBooks();
  }

  bookAdded(book: IBook) {
    //show the new book if another genre was selected
    if (this.search().genre !== book.genre) {
      this.search.update(search => ({ ...search, genre: null }));
    }
    this.refreshBooks();
  }

  refreshBooks() {
    this.search.update(search => ({ ...search, page: 1 }));
    this.loadBooks();
  }

  bookDeleted() {
    //return to the previous page if the last row was deleted
    if (this.books().length === 1 && this.search().page > 1) {
      this.search.update(search => ({ ...search, page: search.page - 1 }));
    }
    this.loadBooks();
  }

  stopLoading() {
    this.request?.unsubscribe();
    this.loading.set(false);
  }
}
