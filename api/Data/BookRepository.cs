using Models;
using Enums;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

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

    public async Task<BookVM?> Add(AddBookModel request)
    {
        //check if book already exists
        if (await db.Books.AnyAsync(book => book.Isbn == request.Isbn || book.OpenLibraryKey == request.OpenLibraryKey))
        {
            return null;
        }

        //build the book
        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Author = request.Author,
            Isbn = request.Isbn,
            PublishedYear = request.PublishedYear,
            CoverUrl = request.CoverUrl,
            OpenLibraryKey = request.OpenLibraryKey,
            Genre = request.Genre!.Value,
            Price = request.Price!.Value,
            Stock = request.Stock
        };

        //save book to db
        db.Books.Add(book);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException sqlite &&
            sqlite.SqliteExtendedErrorCode == 2067)
        {
            //if another request saved the same book
            db.Entry(book).State = EntityState.Detached;
            return null;
        }

        //return saved book
        return ToViewModel(book);
    }

    public async Task<BookVM?> Get(Guid id)
    {
        //get book from db
        var book = await db.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == id);
        if (book == null) return null;

        return ToViewModel(book);
    }

    public async Task<BookVM?> Update(Guid id, UpdateBookModel request)
    {
        //find saved book
        var book = await db.Books.FirstOrDefaultAsync(book => book.Id == id);
        if (book == null) return null;

        //update store details only
        book.Genre = request.Genre!.Value;
        book.Price = request.Price!.Value;
        book.Stock = request.Stock!.Value;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            //if book was deleted while editing
            return null;
        }

        return ToViewModel(book);
    }

    private static BookVM ToViewModel(BookModel book)
    {
        //build book viewmodel
        return new BookVM
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
        };
    }

    public async Task<bool> Delete(Guid id)
    {
        //remove matching book from db
        var deleted = await db.Books.Where(book => book.Id == id).ExecuteDeleteAsync();
        return deleted > 0;
    }
}
