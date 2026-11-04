import { EGenre } from '../enums/EGenre';
import { EBookSort } from '../enums/EBookSort';

export interface ISearchBook {
  genre: EGenre | null;
  page: number;
  pageSize: number;
  sort: EBookSort;
  descending: boolean;
}
