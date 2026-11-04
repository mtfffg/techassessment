import { IBook } from './IBook';

export interface IBookPage {
  items: IBook[];
  totalCount: number;
  page: number;
  pageSize: number;
}
