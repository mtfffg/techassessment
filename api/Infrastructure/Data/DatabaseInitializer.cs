using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Domain.Enums;

namespace Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task Initialize(BookStoreDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        //add sample books if the store is empty
        if (await db.Books.AnyAsync()) return;

        var books = new[]
        {
            new BookModel
            {
                Id = Guid.NewGuid(),
                Title = "Pride and Prejudice",
                Author = "Jane Austen",
                Isbn = "9780141439518",
                PublishedYear = 1813,
                OpenLibraryKey = "/works/OL66554W",
                Genre = EGenre.Fiction,
                Price = 12.99m,
                Stock = 8
            },
            new BookModel
            {
                Id = Guid.NewGuid(),
                Title = "The Hobbit",
                Author = "J. R. R. Tolkien",
                Isbn = "9780547928227",
                PublishedYear = 1937,
                OpenLibraryKey = "/works/OL27448W",
                Genre = EGenre.Fantasy,
                Price = 15.50m,
                Stock = 5
            },
            new BookModel
            {
                Id = Guid.NewGuid(),
                Title = "Frankenstein",
                Author = "Mary Shelley",
                Isbn = "9780141439471",
                PublishedYear = 1818,
                OpenLibraryKey = "/works/OL450063W",
                Genre = EGenre.SciFi,
                Price = 10.00m,
                Stock = 12
            },
            new BookModel
            {
                Id = Guid.NewGuid(),
                Title = "The Adventures of Sherlock Holmes",
                Author = "Arthur Conan Doyle",
                Isbn = "9780140437713",
                PublishedYear = 1892,
                OpenLibraryKey = "/works/OL262421W",
                Genre = EGenre.Mystery,
                Price = 11.25m,
                Stock = 4
            },
            new BookModel
            {
                Id = Guid.NewGuid(),
                Title = "The Great Gatsby",
                Author = "F. Scott Fitzgerald",
                Isbn = "9780743273565",
                PublishedYear = 1925,
                OpenLibraryKey = "/works/OL468431W",
                Genre = EGenre.Fiction,
                Price = 13.50m,
                Stock = 6
            }
        };

        //set cover urls and save
        foreach (var book in books)
        {
            book.CoverUrl = $"https://covers.openlibrary.org/b/isbn/{book.Isbn}-M.jpg";
        }
        db.Books.AddRange(books);
        await db.SaveChangesAsync();
    }
}
