using Data;
using Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using Services;
using Xunit;

namespace Tests;

public class BookTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private BookStoreDbContext db = null!;
    private BookService service = null!;

    public async Task InitializeAsync()
    {
        //use a temporary database for each test
        await connection.OpenAsync();
        db = new BookStoreDbContext(new DbContextOptionsBuilder<BookStoreDbContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        service = new BookService(new BookRepository(db));
    }

    [Fact]
    public async Task CreateBook()
    {
        //save a book and check the database
        var saved = await service.AddBook(NewBook());
        Assert.NotNull(saved);
        db.ChangeTracker.Clear();
        var book = await db.Books.SingleAsync();
        Assert.Equal(saved.Id, book.Id);
        Assert.Equal("The Hobbit", book.Title);
        Assert.Equal(15.50m, book.Price);
        Assert.Equal(5, book.Stock);

        //reject duplicate isbn and open library key
        var duplicate = NewBook();
        duplicate.OpenLibraryKey = "/works/OL123W";
        Assert.Null(await service.AddBook(duplicate));
        duplicate = NewBook();
        duplicate.Isbn = "9780316029186";
        Assert.Null(await service.AddBook(duplicate));

        //reject missing genre and negative price
        var invalid = NewBook();
        invalid.Genre = null;
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddBook(invalid));
        invalid.Genre = EGenre.Fantasy;
        invalid.Price = -1;
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddBook(invalid));
        Assert.Equal(1, await db.Books.CountAsync());
    }

    [Fact]
    public async Task ReadBooks()
    {
        var saved = await service.AddBook(NewBook());
        Assert.NotNull(saved);

        //get one book and the filtered list
        var book = await service.GetBook(saved.Id);
        Assert.NotNull(book);
        Assert.Equal("The Hobbit", book.Title);
        var page = await service.GetBooks(new SearchBookModel { Genre = EGenre.Fantasy });
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(saved.Id, Assert.Single(page.Items).Id);

        //check missing books and invalid pages
        Assert.Null(await service.GetBook(Guid.NewGuid()));
        var empty = await service.GetBooks(new SearchBookModel { Genre = EGenre.History });
        Assert.Empty(empty.Items);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetBooks(new SearchBookModel { Page = 0 }));
    }

    [Fact]
    public async Task UpdateBook()
    {
        var saved = await service.AddBook(NewBook());
        Assert.NotNull(saved);
        var update = new UpdateBookModel { Genre = EGenre.Fiction, Price = 12.99m, Stock = 8 };

        //save store details and read them from the database
        await service.UpdateBook(saved.Id, update);
        db.ChangeTracker.Clear();
        var book = await service.GetBook(saved.Id);
        Assert.NotNull(book);
        Assert.Equal(EGenre.Fiction, book.Genre);
        Assert.Equal(12.99m, book.Price);
        Assert.Equal(8, book.Stock);
        Assert.Equal(saved.Title, book.Title);
        Assert.Equal(saved.Isbn, book.Isbn);
        Assert.Equal(saved.OpenLibraryKey, book.OpenLibraryKey);

        //check missing books and invalid stock or price
        Assert.Null(await service.UpdateBook(Guid.NewGuid(), update));
        update.Stock = -1;
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateBook(saved.Id, update));
        update.Stock = 8;
        update.Price = -1;
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateBook(saved.Id, update));
        Assert.Equal(12.99m, (await service.GetBook(saved.Id))!.Price);
    }

    [Fact]
    public async Task DeleteBook()
    {
        var saved = await service.AddBook(NewBook());
        Assert.NotNull(saved);

        //delete book and check it is gone
        Assert.True(await service.DeleteBook(saved.Id));
        Assert.False(await db.Books.AnyAsync());
        Assert.Null(await service.GetBook(saved.Id));

        //deleting a missing book returns false
        Assert.False(await service.DeleteBook(saved.Id));
    }

    private static AddBookModel NewBook()
    {
        return new AddBookModel
        {
            Title = "The Hobbit",
            Author = "J. R. R. Tolkien",
            Isbn = "9780547928227",
            PublishedYear = 1937,
            OpenLibraryKey = "/works/OL27448W",
            Genre = EGenre.Fantasy,
            Price = 15.50m,
            Stock = 5
        };
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }
}
