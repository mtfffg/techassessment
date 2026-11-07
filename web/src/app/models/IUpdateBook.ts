import { EGenre } from '../enums/EGenre';

export interface IUpdateBook {
  genre: EGenre;
  price: number;
  stock: number;
}
