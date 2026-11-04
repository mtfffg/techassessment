using Models;
using Enums;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class BookRepository(BookStoreDbContext db) : IBookRepository
{
    public async Task<BookPageVM> List(SearchBookModel search)
    {
        //get books from db
        var books = db.Books.AsNoTracking();

        //filter by genre if needed
        if (search.Genre.HasValue)
        {
            books = books.Where(book => book.Genre == search.Genre.Value);
        }

        //count, sort, skip, take
        var totalCount = await books.CountAsync();
        var sortedBooks = SortBooks(books, search);
        var skip = (search.Page - 1) * search.PageSize;

        //get the items sorted by id and paged
        var items = await sortedBooks
            .ThenBy(book => book.Id)
            .Skip(skip)
            .Take(search.PageSize)
            .Select(book => new BookVM
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Isbn = book.Isbn,
                PublishedYear = book.PublishedYear,
                CoverUrl = book.CoverUrl,
                OpenLibraryKey = book.OpenLibraryKey,
                Description = book.Description,
                Genre = book.Genre,
                Price = book.Price,
                Stock = book.Stock
            })
            .ToListAsync();

        //return as viewmodel
        return new BookPageVM
        {
            Items = items,
            TotalCount = totalCount,
            Page = search.Page,
            PageSize = search.PageSize
        };
    }

    private static IOrderedQueryable<BookModel> SortBooks(IQueryable<BookModel> books, SearchBookModel search)
    {
        //sort by property and direction
        switch (search.Sort)
        {
            case EBookSort.Author:
                return search.Descending
                    ? books.OrderByDescending(book => book.Author)
                    : books.OrderBy(book => book.Author);

            case EBookSort.Price:
                return search.Descending
                    ? books.OrderByDescending(book => book.Price)
                    : books.OrderBy(book => book.Price);

            case EBookSort.PublishedYear:
                return search.Descending
                    ? books.OrderByDescending(book => book.PublishedYear)
                    : books.OrderBy(book => book.PublishedYear);

            default:
                return search.Descending
                    ? books.OrderByDescending(book => book.Title)
                    : books.OrderBy(book => book.Title);
        }
    }

    public async Task<IReadOnlyList<GenreCountVM>> CountByGenre()
    {
        //get genre counts from db
        return await db.Books
            .AsNoTracking()
            .GroupBy(book => book.Genre)
            .OrderBy(group => group.Key)
            .Select(group => new GenreCountVM
            {
                Genre = group.Key,
                Count = group.Count()
            })
            .ToListAsync();
    }
}
