import { EGenre } from '../enums/EGenre';

export interface IBook {
  id: string;
  title: string;
  author: string;
  isbn: string;
  publishedYear: number | null;
  coverUrl: string | null;
  openLibraryKey: string;
  description: string | null;
  genre: EGenre;
  price: number;
  stock: number;
}
