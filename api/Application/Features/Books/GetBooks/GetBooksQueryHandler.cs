using Application.Interfaces;
using Application.Models;

namespace Application.Features.Books.GetBooks;

public class GetBooksQueryHandler(IBookRepository books)
{
    public Task<BookPageVM> Handle(SearchBookModel request)
    {
        //if less than 1 or greater than 100
        if (request.Page < 1 || request.PageSize is < 1 or > 100)
            throw new ArgumentException("Page must be positive and pageSize must be between 1 and 100.");
        //if page too large
        if ((long)(request.Page - 1) * request.PageSize > int.MaxValue)
            throw new ArgumentException("Requested page is too large.");
        //if genre is null
        if (request.Genre is { } genre && !Enum.IsDefined(genre))
            throw new ArgumentException("Unknown genre.");
        //if sort is null
        if (!Enum.IsDefined(request.Sort))
            throw new ArgumentException("Unknown sort field.");
        //call repo
        return books.List(request);
    }
}
