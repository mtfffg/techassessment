using Application.Interfaces;
using Application.Models;

namespace Application.Features.Books.DeleteBook;

public class DeleteBookCommandHandler(IBookRepository books)
{
    public Task<bool> Handle(DeleteBookModel request)
    {
        //delete saved book
        return books.Delete(request.Id);
    }
}
