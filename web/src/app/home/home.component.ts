import { CurrencyPipe } from '@angular/common';
import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { DialogModule } from 'primeng/dialog';
import { HttpErrorResponse } from '@angular/common/http';
import { BooksService } from '../services/books.service';
import { IBook } from '../models/IBook';
import { ISearchBook } from '../models/ISearchBook';
import { EBookSort } from '../enums/EBookSort';
import { EGenre } from '../enums/EGenre';
import { AddBookComponent } from '../add-book/add-book.component';

@Component({
  selector: 'app-home',
  imports: [CurrencyPipe, FormsModule, ButtonModule, MessageModule, TableModule, TagModule, SelectModule, PaginatorModule, DialogModule, AddBookComponent],
  templateUrl: './home.component.html',
  host: { class: 'block flex-1' },
})
export class HomeComponent implements OnInit, OnDestroy {
  addBookVisible = false;
  bookToDelete: IBook | null = null;
  deleting = signal(false);
  deleteError = signal('');
  books = signal<IBook[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal('');
  search: ISearchBook = { genre: null, page: 1, pageSize: 10, sort: EBookSort.Title, descending: false };
  genres = [
    { label: 'All genres', value: null },
    ...Object.values(EGenre).map(value => ({ label: value === EGenre.SciFi ? 'Science fiction' : value === EGenre.NonFiction ? 'Non-fiction' : value, value })),
  ];
  private request?: Subscription;
  private deleteRequest?: Subscription;

  constructor(private booksService: BooksService) {}

  ngOnInit() {
    //get books on page load
    this.loadBooks();
  }

  loadBooks() {
    //stop previous request
    this.request?.unsubscribe();
    this.loading.set(true);
    this.error.set('');

    //call service
    this.request = this.booksService.getBooks(this.search).subscribe({
      next: result => {
        //set books and count
        this.books.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        //clear books and show error
        this.books.set([]);
        this.totalCount.set(0);
        this.error.set('Could not load books. Please try again.');
        this.loading.set(false);
      },
    });
  }

  selectGenre(genre: EGenre | null) {
    //set genre and reset page
    this.search.genre = genre;
    this.search.page = 1;
    this.loadBooks();
  }

  changeSort(event: TableLazyLoadEvent) {
    //get sort property and direction
    const sort = typeof event.sortField === 'string' ? event.sortField as EBookSort : EBookSort.Title;
    const descending = event.sortOrder === -1;
    //skip if sort has not changed
    if (sort === this.search.sort && descending === this.search.descending) return;

    //set sort and reset page
    this.search.sort = sort;
    this.search.descending = descending;
    this.search.page = 1;
    this.loadBooks();
  }

  changePage(event: PaginatorState) {
    //set page size and page
    this.search.pageSize = event.rows ?? this.search.pageSize;
    this.search.page = Math.floor((event.first ?? 0) / this.search.pageSize) + 1;
    this.loadBooks();
  }

  hideCover(event: Event) {
    //hide cover if it fails to load
    (event.target as HTMLImageElement).hidden = true;
  }

  onBookSaved(book: IBook) {
    //clear a different genre filter and refresh table
    if (this.search.genre !== book.genre) this.search.genre = null;
    this.search.page = 1;
    this.loadBooks();
  }

  confirmDelete(book: IBook) {
    //ask before deleting book
    this.bookToDelete = book;
    this.deleteError.set('');
  }

  cancelDelete() {
    if (this.deleting()) return;
    this.bookToDelete = null;
    this.deleteError.set('');
  }

  deleteBook() {
    //delete selected book
    if (!this.bookToDelete || this.deleting()) return;
    this.deleting.set(true);
    this.deleteError.set('');
    this.deleteRequest = this.booksService.deleteBook(this.bookToDelete.id).subscribe({
      next: () => {
        this.onBookDeleted();
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 404) {
          this.onBookDeleted();
          return;
        }
        this.deleting.set(false);
        this.deleteError.set('Could not delete book. Please try again.');
      },
    });
  }

  onBookDeleted() {
    //close dialog and refresh table
    this.deleting.set(false);
    this.bookToDelete = null;
    if (this.books().length === 1 && this.search.page > 1) this.search.page--;
    this.loadBooks();
  }

  ngOnDestroy() {
    //stop request when leaving page
    this.request?.unsubscribe();
    this.deleteRequest?.unsubscribe();
  }
}
