using Application.Models;
using Application.Features.Books.AddBook;
using Application.Features.Books.GetBooks;
using Application.Features.Books.GetBook;
using Application.Features.Books.UpdateBook;
using Application.Features.Books.DeleteBook;
using Application.Features.Books.GetGenreCounts;
using Application.Features.Catalog.SearchCatalog;
using Application.Interfaces;

namespace Application.Messaging;

public class RequestService(
    AddBookCommandHandler addBook,
    GetBooksQueryHandler getBooks,
    GetBookQueryHandler getBook,
    UpdateBookCommandHandler updateBook,
    DeleteBookCommandHandler deleteBook,
    SearchCatalogQueryHandler searchCatalog,
    GetGenreCountsQueryHandler getGenreCounts) : IRequestService
{
    public Task<BookVM?> Send(AddBookModel request)
    {
        //send command to its handler
        return addBook.Handle(request);
    }

    public Task<BookPageVM> Send(SearchBookModel request)
    {
        //send query to its handler
        return getBooks.Handle(request);
    }

    public Task<BookVM?> Send(GetBookModel request)
    {
        //send query to its handler
        return getBook.Handle(request);
    }

    public Task<BookVM?> Send(UpdateBookModel request)
    {
        //send command to its handler
        return updateBook.Handle(request);
    }

    public Task<bool> Send(DeleteBookModel request)
    {
        //send command to its handler
        return deleteBook.Handle(request);
    }

    public Task<List<CatalogBookVM>> Send(SearchCatalogModel request)
    {
        //send query to its handler
        return searchCatalog.Handle(request);
    }

    public Task<IReadOnlyList<GenreCountVM>> GetGenreCounts()
    {
        //send query to its handler
        return getGenreCounts.Handle();
    }
}
