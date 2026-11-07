import { Component, EventEmitter, Input, OnChanges, OnDestroy, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { EGenre } from '../../core/enums/EGenre';
import { IBook } from '../../core/models/IBook';
import { IUpdateBook } from '../../core/models/IUpdateBook';
import { BooksService } from '../../application/services/books.service';

@Component({
  selector: 'app-edit-book',
  imports: [FormsModule, ButtonModule, DialogModule, InputNumberModule, MessageModule, SelectModule],
  templateUrl: './edit-book.component.html',
})
export class EditBookComponent implements OnChanges, OnDestroy {
  @Input() bookId: string | null = null;
  @Output() bookIdChange = new EventEmitter<string | null>();
  @Output() bookSaved = new EventEmitter<IBook>();

  book = signal<IBook | null>(null);
  loading = signal(false);
  saving = signal(false);
  error = signal('');
  genre: EGenre | null = null;
  price: number | null = null;
  stock: number | null = null;
  genres = Object.values(EGenre);
  private request?: Subscription;

  constructor(private booksService: BooksService) {}

  ngOnChanges() {
    //load book when dialog opens
    if (this.bookId) this.loadBook();
  }

  loadBook() {
    if (!this.bookId) return;
    //clear previous request and get saved book
    this.request?.unsubscribe();
    this.book.set(null);
    this.loading.set(true);
    this.error.set('');
    this.request = this.booksService.getBook(this.bookId).subscribe({
      next: book => {
        //set current store details
        this.book.set(book);
        this.genre = book.genre;
        this.price = book.price;
        this.stock = book.stock;
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.error.set(error.error?.detail || error.error?.title || 'Could not load book. Please try again.');
      },
    });
  }

  saveBook() {
    //check store details
    if (!this.bookId || !this.book() || this.saving()) return;
    if (!this.genre || this.price === null || this.price < 0 || this.price > 99999999.99 ||
        this.stock === null || this.stock < 0 || !Number.isInteger(this.stock)) {
      this.error.set('Select a genre, enter a valid price and a whole stock quantity.');
      return;
    }

    const request: IUpdateBook = { genre: this.genre, price: this.price, stock: this.stock };
    this.saving.set(true);
    this.error.set('');
    //save changes and refresh home
    this.request = this.booksService.updateBook(this.bookId, request).subscribe({
      next: book => {
        this.saving.set(false);
        this.bookSaved.emit(book);
        this.close();
      },
      error: (error) => {
        this.saving.set(false);
        this.error.set(error.error?.detail || error.error?.title || 'Could not update book. Please try again.');
      },
    });
  }

  close() {
    if (this.saving()) return;
    //stop request and reset dialog
    this.request?.unsubscribe();
    this.bookId = null;
    this.book.set(null);
    this.genre = null;
    this.price = null;
    this.stock = null;
    this.loading.set(false);
    this.error.set('');
    this.bookIdChange.emit(null);
  }

  ngOnDestroy() {
    //stop request when leaving page
    this.request?.unsubscribe();
  }
}
