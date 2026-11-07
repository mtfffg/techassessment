export interface ICatalogBook {
  title: string;
  author: string;
  isbn: string;
  publishedYear: number | null;
  coverUrl: string | null;
  openLibraryKey: string;
}
