import { Component, EventEmitter, Input, OnDestroy, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { ICatalogBook } from '../../core/models/ICatalogBook';
import { CatalogService } from '../../application/services/catalog.service';
import { BooksService } from '../../application/services/books.service';
import { IAddBook } from '../../core/models/IAddBook';
import { IBook } from '../../core/models/IBook';
import { EGenre } from '../../core/enums/EGenre';

@Component({
  selector: 'app-add-book',
  imports: [FormsModule, ButtonModule, DialogModule, InputTextModule, MessageModule, TableModule, SelectModule, InputNumberModule],
  templateUrl: './add-book.component.html',
})
export class AddBookComponent implements OnDestroy {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() bookSaved = new EventEmitter<IBook>();

  searchTerm = '';
  books = signal<ICatalogBook[]>([]);
  selectedBook = signal<ICatalogBook | null>(null);
  loading = signal(false);
  searched = signal(false);
  error = signal('');
  saving = signal(false);
  genre: EGenre | null = null;
  price: number | null = null;
  stock: number | null = 0;
  genres = Object.values(EGenre);
  private request?: Subscription;

  constructor(private catalogService: CatalogService, private booksService: BooksService) {}

  searchBooks() {
    //check search term
    const search = this.searchTerm.trim();
    if (!search || search.length > 200) {
      this.error.set('Enter a title, author or ISBN between 1 and 200 characters.');
      return;
    }

    //clear previous search
    this.request?.unsubscribe();
    this.books.set([]);
    this.selectedBook.set(null);
    this.error.set('');
    this.loading.set(true);
    this.searched.set(true);

    //call service
    this.request = this.catalogService.searchBooks(search).subscribe({
      next: books => {
        //set search results
        this.books.set(books);
        this.loading.set(false);
      },
      error: () => {
        //show search error
        this.error.set('Could not search Open Library. Please try again.');
        this.loading.set(false);
      },
    });
  }

  selectBook(book: ICatalogBook | null) {
    //set selected book and clear store details
    this.selectedBook.set(book);
    this.genre = null;
    this.price = null;
    this.stock = 0;
    this.error.set('');
  }

  saveBook() {
    //check selected book and store details
    if (this.saving()) return;
    const book = this.selectedBook();
    if (!book) return;
    if (!this.genre || this.price === null || this.price < 0 || this.price > 99999999.99) {
      this.error.set('Select a genre and enter a valid price.');
      return;
    }
    if (this.stock === null || this.stock < 0 || this.stock > 2147483647 || !Number.isInteger(this.stock)) {
      this.error.set('Enter a whole stock quantity of zero or more.');
      return;
    }
    if (!book.isbn) {
      this.error.set('Choose a catalog result with an ISBN.');
      return;
    }

    //build the book to save
    const request: IAddBook = { ...book, genre: this.genre, price: this.price, stock: this.stock };
    this.saving.set(true);
    this.error.set('');

    //save book and refresh home
    this.request = this.booksService.addBook(request).subscribe({
      next: savedBook => {
        this.saving.set(false);
        this.bookSaved.emit(savedBook);
        this.close();
      },
      error: (error) => {
        this.saving.set(false);
        this.error.set(error.error?.detail || error.error?.title || 'Could not save book. Please try again.');
      },
    });
  }

  hideCover(event: Event) {
    //hide cover if it fails to load
    (event.target as HTMLImageElement).hidden = true;
  }

  close() {
    if (this.saving()) return;
    //stop search and reset dialog
    this.request?.unsubscribe();
    this.searchTerm = '';
    this.books.set([]);
    this.selectedBook.set(null);
    this.loading.set(false);
    this.searched.set(false);
    this.error.set('');
    this.genre = null;
    this.price = null;
    this.stock = 0;
    this.visible = false;
    this.visibleChange.emit(false);
  }

  ngOnDestroy() {
    //stop request when leaving page
    this.request?.unsubscribe();
  }
}
