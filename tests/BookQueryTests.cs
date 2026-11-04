using Models;
using Enums;
using Services;
using Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

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
    public async Task RepeatedStartupDoesNotDuplicateSeeds()
    {
        await DatabaseInitializer.Initialize(db);
        Assert.Equal(4, await db.Books.CountAsync());
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }
}
