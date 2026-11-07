using Infrastructure.Data;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Application.Models;
using Application.Features.Books.AddBook;
using Application.Features.Books.GetBooks;
using Application.Features.Books.UpdateBook;
using Application.Messaging;
using Application.Features.Books.GetBook;
using Application.Features.Books.DeleteBook;
using Application.Features.Books.GetGenreCounts;
using Application.Features.Catalog.SearchCatalog;
using Infrastructure.Catalog;
using Xunit;

namespace Tests;

public class BookTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private BookStoreDbContext db = null!;
    private RequestService service = null!;
    private readonly HttpClient http = new();

    public async Task InitializeAsync()
    {
        //use a temporary database for each test
        await connection.OpenAsync();
        db = new BookStoreDbContext(new DbContextOptionsBuilder<BookStoreDbContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        //use the same mediator and handlers as the api
        var repository = new BookRepository(db);
        service = new RequestService(
            new AddBookCommandHandler(repository),
            new GetBooksQueryHandler(repository),
            new GetBookQueryHandler(repository),
            new UpdateBookCommandHandler(repository),
            new DeleteBookCommandHandler(repository),
            new SearchCatalogQueryHandler(new CatalogService(http)),
            new GetGenreCountsQueryHandler(repository));
    }

    [Fact]
    public async Task CreateBook()
    {
        //save a book and check the database
        var saved = await service.Send(NewBook());
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
        Assert.Null(await service.Send(duplicate));
        duplicate = NewBook();
        duplicate.Isbn = "9780316029186";
        Assert.Null(await service.Send(duplicate));

        //reject missing genre and negative price
        var invalid = NewBook();
        invalid.Genre = null;
        await Assert.ThrowsAsync<ArgumentException>(() => service.Send(invalid));
        invalid.Genre = EGenre.Fantasy;
        invalid.Price = -1;
        await Assert.ThrowsAsync<ArgumentException>(() => service.Send(invalid));
        Assert.Equal(1, await db.Books.CountAsync());
    }

    [Fact]
    public async Task ReadBooks()
    {
        var saved = await service.Send(NewBook());
        Assert.NotNull(saved);

        //get one book and the filtered list
        var book = await service.Send(new GetBookModel { Id = saved.Id });
        Assert.NotNull(book);
        Assert.Equal("The Hobbit", book.Title);
        var page = await service.Send(new SearchBookModel { Genre = EGenre.Fantasy });
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(saved.Id, Assert.Single(page.Items).Id);

        //check missing books and invalid pages
        Assert.Null(await service.Send(new GetBookModel { Id = Guid.NewGuid() }));
        var empty = await service.Send(new SearchBookModel { Genre = EGenre.History });
        Assert.Empty(empty.Items);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.Send(new SearchBookModel { Page = 0 }));
    }

    [Fact]
    public async Task UpdateBook()
    {
        var saved = await service.Send(NewBook());
        Assert.NotNull(saved);
        var update = new UpdateBookModel { Id = saved.Id, Genre = EGenre.Fiction, Price = 12.99m, Stock = 8 };

        //save store details and read them from the database
        await service.Send(update);
        db.ChangeTracker.Clear();
        var book = await service.Send(new GetBookModel { Id = saved.Id });
        Assert.NotNull(book);
        Assert.Equal(EGenre.Fiction, book.Genre);
        Assert.Equal(12.99m, book.Price);
        Assert.Equal(8, book.Stock);
        Assert.Equal(saved.Title, book.Title);
        Assert.Equal(saved.Isbn, book.Isbn);
        Assert.Equal(saved.OpenLibraryKey, book.OpenLibraryKey);

        //check missing books and invalid stock or price
        update.Id = Guid.NewGuid();
        Assert.Null(await service.Send(update));
        update.Id = saved.Id;
        update.Stock = -1;
        await Assert.ThrowsAsync<ArgumentException>(() => service.Send(update));
        update.Stock = 8;
        update.Price = -1;
        await Assert.ThrowsAsync<ArgumentException>(() => service.Send(update));
        Assert.Equal(12.99m, (await service.Send(new GetBookModel { Id = saved.Id }))!.Price);
    }

    [Fact]
    public async Task DeleteBook()
    {
        var saved = await service.Send(NewBook());
        Assert.NotNull(saved);

        //delete book and check it is gone
        Assert.True(await service.Send(new DeleteBookModel { Id = saved.Id }));
        Assert.False(await db.Books.AnyAsync());
        Assert.Null(await service.Send(new GetBookModel { Id = saved.Id }));

        //deleting a missing book returns false
        Assert.False(await service.Send(new DeleteBookModel { Id = saved.Id }));
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
        http.Dispose();
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }
}
