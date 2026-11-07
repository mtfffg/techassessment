using Models;
using Data;
using System.Text.RegularExpressions;
using Enums;

namespace Services;

public class BookService(IBookRepository books)
{
    public Task<BookPageVM> GetBooks(SearchBookModel request)
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

    public Task<IReadOnlyList<GenreCountVM>> GetGenreCounts()
    {
        //get genre counts
        return books.CountByGenre();
    }

    public Task<BookVM?> AddBook(AddBookModel request)
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
        ValidateStoreDetails(request.Genre, request.Price, request.Stock);
        return books.Add(request);
    }

    public Task<BookVM?> GetBook(Guid id)
    {
        //get saved book
        return books.Get(id);
    }

    public Task<BookVM?> UpdateBook(Guid id, UpdateBookModel request)
    {
        //check store details and update
        ValidateStoreDetails(request.Genre, request.Price, request.Stock);
        return books.Update(id, request);
    }

    private static void ValidateStoreDetails(EGenre? genre, decimal? price, int? stock)
    {
        //check genre and price
        if (!genre.HasValue || !Enum.IsDefined(genre.Value))
        {
            throw new ArgumentException("Select a genre.");
        }
        if (!price.HasValue || price < 0 || price > 99999999.99m || decimal.Round(price.Value, 2) != price.Value)
        {
            throw new ArgumentException("Enter a price between 0 and 99999999.99 with up to two decimal places.");
        }
        if (!stock.HasValue || stock < 0)
        {
            throw new ArgumentException("Enter a stock quantity of zero or more.");
        }

    }

    public Task<bool> DeleteBook(Guid id)
    {
        //delete book from db
        return books.Delete(id);
    }
}
