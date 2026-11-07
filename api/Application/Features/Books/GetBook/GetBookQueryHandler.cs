using Application.Interfaces;
using Application.Models;

namespace Application.Features.Books.GetBook;

public class GetBookQueryHandler(IBookRepository books)
{
    public Task<BookVM?> Handle(GetBookModel request)
    {
        //get saved book
        return books.Get(request.Id);
    }
}
