using Models;
using Enums;
using Services;
using Data;
using Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Globalization;

namespace Tests;

public class BookQueryTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private BookStoreDbContext db = null!;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new BookStoreDbContext(new DbContextOptionsBuilder<BookStoreDbContext>()
            .UseSqlite(connection).Options);
        await DatabaseInitializer.Initialize(db);

        //add books for these tests only
        db.Books.AddRange(
            TestBook("Pride and Prejudice", "Jane Austen", "9780141439518", 1813, EGenre.Fiction, 12.99m),
            TestBook("The Hobbit", "J. R. R. Tolkien", "9780547928227", 1937, EGenre.Fantasy, 15.50m),
            TestBook("Frankenstein", "Mary Shelley", "9780141439471", 1818, EGenre.SciFi, 10.00m),
            TestBook("The Adventures of Sherlock Holmes", "Arthur Conan Doyle", "9780140437713", 1892, EGenre.Mystery, 11.25m));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task FiltersBeforeCountingAndPaging()
    {
        var service = new BookService(new BookRepository(db));
        var page = await service.GetBooks(new SearchBookModel { Genre = EGenre.Fantasy, PageSize = 1 });

        Assert.Equal(1, page.TotalCount);
        Assert.Equal("The Hobbit", Assert.Single(page.Items).Title);
    }

    [Fact]
    public async Task SortsPricesAndPreservesExactCentsInSqlite()
    {
        db.ChangeTracker.Clear();
        var result = await new BookRepository(db).List(new SearchBookModel { Sort = EBookSort.Price });

        Assert.Equal(new[] { 10.00m, 11.25m, 12.99m, 15.50m }, result.Items.Select(book => book.Price));
    }

    [Fact]
    public async Task ReturnsTheRequestedPageAndTotalCount()
    {
        var result = await new BookRepository(db).List(new SearchBookModel { Page = 2, PageSize = 1 });

        Assert.Equal(4, result.TotalCount);
        Assert.Equal("Pride and Prejudice", Assert.Single(result.Items).Title);
    }

    [Theory]
    [InlineData(EBookSort.Title, "The Hobbit")]
    [InlineData(EBookSort.Author, "Frankenstein")]
    [InlineData(EBookSort.Price, "The Hobbit")]
    [InlineData(EBookSort.PublishedYear, "The Hobbit")]
    public async Task SortsInDescendingOrder(EBookSort sort, string expectedFirstTitle)
    {
        var result = await new BookRepository(db).List(new SearchBookModel { Sort = sort, Descending = true });

        Assert.Equal(expectedFirstTitle, result.Items[0].Title);
    }

    [Fact]
    public async Task GroupsGenresInSqlite()
    {
        var counts = await new BookRepository(db).CountByGenre();
        Assert.Equal(4, counts.Sum(item => item.Count));
        Assert.Contains(counts, item => item.Genre == EGenre.Fantasy && item.Count == 1);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public async Task RejectsInvalidPaging(int page, int pageSize)
    {
        var service = new BookService(new BookRepository(db));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetBooks(new SearchBookModel { Page = page, PageSize = pageSize }));
    }

    [Fact]
    public async Task StartupDoesNotAddDefaultBooks()
    {
        await db.Books.ExecuteDeleteAsync();
        await DatabaseInitializer.Initialize(db);
        Assert.Equal(0, await db.Books.CountAsync());
    }

    [Fact]
    public async Task SavesSelectedBookDetails()
    {
        var service = new BookService(new BookRepository(db));
        var saved = await service.AddBook(NewBook());

        Assert.NotNull(saved);
        db.ChangeTracker.Clear();
        var book = await db.Books.SingleAsync(book => book.Id == saved.Id);
        Assert.Equal("The Witcher", book.Title);
        Assert.Equal(12.99m, book.Price);
        Assert.Equal(EGenre.Fantasy, book.Genre);
        Assert.Equal("https://covers.openlibrary.org/b/id/123-M.jpg", book.CoverUrl);
    }

    [Fact]
    public async Task RejectsDuplicateIsbn()
    {
        var service = new BookService(new BookRepository(db));
        await service.AddBook(NewBook());
        var duplicate = NewBook();
        duplicate.OpenLibraryKey = "/works/OL456W";

        Assert.Null(await service.AddBook(duplicate));
        Assert.Equal(5, await db.Books.CountAsync());
    }

    [Fact]
    public async Task RejectsDuplicateOpenLibraryKey()
    {
        var service = new BookService(new BookRepository(db));
        await service.AddBook(NewBook());
        var duplicate = NewBook();
        duplicate.Isbn = "9780316029193";

        Assert.Null(await service.AddBook(duplicate));
        Assert.Equal(5, await db.Books.CountAsync());
    }

    [Fact]
    public async Task RequiresGenreAndPrice()
    {
        var service = new BookService(new BookRepository(db));
        var book = NewBook();
        book.Genre = null;
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddBook(book));

        book.Genre = EGenre.Fantasy;
        book.Price = null;
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddBook(book));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("1.001")]
    [InlineData("100000000")]
    public async Task RejectsInvalidPrices(string price)
    {
        var service = new BookService(new BookRepository(db));
        var book = NewBook();
        book.Price = decimal.Parse(price, CultureInfo.InvariantCulture);

        await Assert.ThrowsAsync<ArgumentException>(() => service.AddBook(book));
    }

    [Fact]
    public async Task DeletesSavedBook()
    {
        var service = new BookService(new BookRepository(db));
        var saved = await service.AddBook(NewBook());
        Assert.NotNull(saved);

        Assert.True(await service.DeleteBook(saved.Id));
        Assert.False(await db.Books.AnyAsync(book => book.Id == saved.Id));
        Assert.False(await service.DeleteBook(saved.Id));
    }

    [Fact]
    public async Task GetsOneSavedBook()
    {
        var service = new BookService(new BookRepository(db));
        var saved = await service.AddBook(NewBook());
        Assert.NotNull(saved);

        var book = await service.GetBook(saved.Id);

        Assert.NotNull(book);
        Assert.Equal(saved.Id, book.Id);
        Assert.Equal("The Witcher", book.Title);
        Assert.Equal(12.99m, book.Price);
    }

    [Fact]
    public async Task ReturnsNullWhenBookDoesNotExist()
    {
        var service = new BookService(new BookRepository(db));
        var id = Guid.NewGuid();

        Assert.Null(await service.GetBook(id));
        Assert.Null(await service.UpdateBook(id, new UpdateBookModel { Genre = EGenre.History, Price = 10m, Stock = 2 }));
    }

    [Fact]
    public async Task UpdatesStoreDetailsAndKeepsCatalogDetails()
    {
        var service = new BookService(new BookRepository(db));
        var saved = await service.AddBook(NewBook());
        Assert.NotNull(saved);

        await service.UpdateBook(saved.Id, new UpdateBookModel { Genre = EGenre.History, Price = 15.50m, Stock = 8 });
        db.ChangeTracker.Clear();
        var book = await service.GetBook(saved.Id);

        Assert.NotNull(book);
        Assert.Equal(EGenre.History, book.Genre);
        Assert.Equal(15.50m, book.Price);
        Assert.Equal(8, book.Stock);
        Assert.Equal(saved.Title, book.Title);
        Assert.Equal(saved.Author, book.Author);
        Assert.Equal(saved.Isbn, book.Isbn);
        Assert.Equal(saved.PublishedYear, book.PublishedYear);
        Assert.Equal(saved.CoverUrl, book.CoverUrl);
        Assert.Equal(saved.OpenLibraryKey, book.OpenLibraryKey);
        Assert.Equal(5, await db.Books.CountAsync());
    }

    [Fact]
    public async Task RejectsInvalidUpdateDetails()
    {
        var service = new BookService(new BookRepository(db));
        var saved = await service.AddBook(NewBook());
        Assert.NotNull(saved);
        var update = new UpdateBookModel { Genre = EGenre.Fantasy, Price = -1m, Stock = 2 };
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateBook(saved.Id, update));

        update.Price = 10m;
        update.Stock = -1;
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateBook(saved.Id, update));

        update.Stock = null;
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateBook(saved.Id, update));
        var unchanged = await service.GetBook(saved.Id);
        Assert.NotNull(unchanged);
        Assert.Equal(12.99m, unchanged.Price);
        Assert.Equal(0, unchanged.Stock);
    }

    [Fact]
    public async Task AllowsZeroPriceAndStockWhenUpdating()
    {
        var service = new BookService(new BookRepository(db));
        var saved = await service.AddBook(NewBook());
        Assert.NotNull(saved);

        var updated = await service.UpdateBook(saved.Id, new UpdateBookModel { Genre = EGenre.Fiction, Price = 0m, Stock = 0 });

        Assert.NotNull(updated);
        Assert.Equal(EGenre.Fiction, updated.Genre);
        Assert.Equal(0m, updated.Price);
        Assert.Equal(0, updated.Stock);
    }

    private static AddBookModel NewBook()
    {
        return new AddBookModel
        {
            Title = "The Witcher",
            Author = "Andrzej Sapkowski",
            Isbn = "9780316029186",
            PublishedYear = 1993,
            CoverUrl = "https://covers.openlibrary.org/b/id/123-M.jpg",
            OpenLibraryKey = "/works/OL123W",
            Genre = EGenre.Fantasy,
            Price = 12.99m
        };
    }

    private static BookModel TestBook(string title, string author, string isbn, int year, EGenre genre, decimal price)
    {
        return new BookModel
        {
            Id = Guid.NewGuid(),
            Title = title,
            Author = author,
            Isbn = isbn,
            PublishedYear = year,
            OpenLibraryKey = "/works/" + isbn,
            Genre = genre,
            Price = price
        };
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }
}
