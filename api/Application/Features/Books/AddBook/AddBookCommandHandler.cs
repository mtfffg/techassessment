using Application.Interfaces;
using Application.Models;
using Application.Validation;
using System.Text.RegularExpressions;

namespace Application.Features.Books.AddBook;

public class AddBookCommandHandler(IBookRepository books)
{
    public Task<BookVM?> Handle(AddBookModel request)
    {
        //check book details
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 500)
        {
            throw new ArgumentException("A book title between 1 and 500 characters is required.");
        }

        request.Title = request.Title.Trim();
        request.Author = (request.Author ?? string.Empty).Trim();
        request.Isbn = (request.Isbn ?? string.Empty).Replace("-", "").Replace(" ", "").ToUpperInvariant();
        request.OpenLibraryKey = (request.OpenLibraryKey ?? string.Empty).Trim();

        if (request.Author.Length > 500)
        {
            throw new ArgumentException("The author name must be 500 characters or less.");
        }
        if (!Regex.IsMatch(request.Isbn, @"^([0-9]{13}|[0-9]{9}[0-9X])$"))
        {
            throw new ArgumentException("Choose a catalog result with a valid ISBN.");
        }
        if (!Regex.IsMatch(request.OpenLibraryKey, @"^/(works|books)/OL[0-9]+[WM]$"))
        {
            throw new ArgumentException("Choose a book from Open Library.");
        }

        //check store details and save
        BookValidation.ValidateStoreDetails(request.Genre, request.Price, request.Stock);
        return books.Add(request);
    }
}
