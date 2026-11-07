using Application.Models;
using Application.Features.Books.AddBook;
using Application.Features.Books.GetBooks;
using Application.Features.Books.GetBook;
using Application.Features.Books.UpdateBook;
using Application.Features.Books.DeleteBook;
using Application.Features.Books.GetGenreCounts;
using Application.Features.Catalog.SearchCatalog;

namespace Application.Interfaces;

public interface IRequestService
{
    Task<BookVM?> Send(AddBookModel request);
    Task<BookPageVM> Send(SearchBookModel request);
    Task<BookVM?> Send(GetBookModel request);
    Task<BookVM?> Send(UpdateBookModel request);
    Task<bool> Send(DeleteBookModel request);
    Task<List<CatalogBookVM>> Send(SearchCatalogModel request);
    Task<IReadOnlyList<GenreCountVM>> GetGenreCounts();
}
