import { CurrencyPipe } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { DialogModule } from 'primeng/dialog';
import { BooksService } from '../../application/services/books.service';
import { IBook } from '../../core/models/IBook';
import { EBookSort } from '../../core/enums/EBookSort';
import { AddBookComponent } from '../add-book/add-book.component';
import { EditBookComponent } from '../edit-book/edit-book.component';

@Component({
  selector: 'app-home',
  imports: [CurrencyPipe, ButtonModule, MessageModule, TableModule, TagModule, PaginatorModule, DialogModule, AddBookComponent, EditBookComponent],
  templateUrl: './home.component.html',
  host: { class: 'block flex-1' },
})
export class HomeComponent implements OnInit, OnDestroy {
  editBookId: string | null = null;
  bookToDelete: IBook | null = null;
  deleting = signal(false);
  deleteError = signal('');
  booksService = inject(BooksService);
  books = this.booksService.books;
  totalCount = this.booksService.totalCount;
  loading = this.booksService.loading;
  error = this.booksService.error;
  search = this.booksService.search;
  //badge colors for each genre
  genreColors: Record<string, { background: string; color: string }> = {
    Fiction: { background: '#1e3a5f', color: '#bfdbfe' },
    SciFi: { background: '#164e63', color: '#a5f3fc' },
    Fantasy: { background: '#4c1d95', color: '#ddd6fe' },
    Mystery: { background: '#701a75', color: '#f5d0fe' },
    Biography: { background: '#7c2d12', color: '#fed7aa' },
    NonFiction: { background: '#365314', color: '#d9f99d' },
    Children: { background: '#831843', color: '#fbcfe8' },
    History: { background: '#713f12', color: '#fef08a' },
  };
  private deleteRequest?: Subscription;

  ngOnInit() {
    //get books on page load
    this.loadBooks();
  }

  loadBooks() {
    //load the list through the application service
    this.booksService.loadBooks();
  }

  changeSort(event: TableLazyLoadEvent) {
    //get sort property and direction
    const sort = typeof event.sortField === 'string' ? event.sortField as EBookSort : EBookSort.Title;
    const descending = event.sortOrder === -1;
    this.booksService.sortBooks(sort, descending);
  }

  changePage(event: PaginatorState) {
    this.booksService.pageBooks(event.first ?? 0, event.rows ?? this.search().pageSize);
  }

  hideCover(event: Event) {
    //hide cover if it fails to load
    (event.target as HTMLImageElement).hidden = true;
  }

  onBookSaved(book: IBook) {
    //refresh table after adding
    this.booksService.bookAdded(book);
  }

  onBookUpdated() {
    //refresh table after editing
    this.booksService.refreshBooks();
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
      error: (error) => {
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
    this.booksService.bookDeleted();
  }

  ngOnDestroy() {
    //stop request when leaving page
    this.booksService.stopLoading();
    this.deleteRequest?.unsubscribe();
  }
}
