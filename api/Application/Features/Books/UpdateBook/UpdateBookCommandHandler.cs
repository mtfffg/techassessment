using Application.Interfaces;
using Application.Models;
using Application.Validation;

namespace Application.Features.Books.UpdateBook;

public class UpdateBookCommandHandler(IBookRepository books)
{
    public Task<BookVM?> Handle(UpdateBookModel request)
    {
        //check store details and update
        BookValidation.ValidateStoreDetails(request.Genre, request.Price, request.Stock);
        return books.Update(request.Id, request);
    }
}
