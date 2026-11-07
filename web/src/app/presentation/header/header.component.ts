import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { BooksService } from '../../application/services/books.service';
import { EGenre } from '../../core/enums/EGenre';

@Component({
  selector: 'app-header',
  imports: [RouterLink, FormsModule, ButtonModule, SelectModule],
  templateUrl: './header.component.html',
})
export class HeaderComponent {
  booksService = inject(BooksService);
  genres = [
    { label: 'All genres', value: null },
    ...Object.values(EGenre).map(value => ({ label: value === EGenre.SciFi ? 'Science fiction' : value === EGenre.NonFiction ? 'Non-fiction' : value, value })),
  ];
}
