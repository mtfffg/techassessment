import { EGenre } from '../enums/EGenre';
import { ICatalogBook } from './ICatalogBook';

export interface IAddBook extends ICatalogBook {
  genre: EGenre;
  price: number;
  stock: number;
}
