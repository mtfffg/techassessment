using Application.Models;
using Application.Features.Books.AddBook;
using Application.Features.Books.GetBooks;
using Application.Features.Books.UpdateBook;

namespace Application.Interfaces;

public interface IBookRepository
{
    Task<BookPageVM> List(SearchBookModel search);

    Task<IReadOnlyList<GenreCountVM>> CountByGenre();

    Task<BookVM?> Add(AddBookModel book);

    Task<BookVM?> Get(Guid id);

    Task<BookVM?> Update(Guid id, UpdateBookModel book);

    Task<bool> Delete(Guid id);
}
