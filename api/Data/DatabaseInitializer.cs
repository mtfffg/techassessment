using Data.Models;
using Enums;
using Microsoft.EntityFrameworkCore;

namespace Data;

public static class DatabaseInitializer
{
    public static async Task Initialize(BookStoreDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Books.AnyAsync()) return;

        db.Books.AddRange(
            Sample("b1c0a1c1-8724-4b99-9843-0abc771e0801", "Pride and Prejudice", "Jane Austen",
                "9780141439518", 1813, "/works/OL66554W", EGenre.Fiction, 12.99m, 8),
            Sample("b1c0a1c1-8724-4b99-9843-0abc771e0802", "The Hobbit", "J. R. R. Tolkien",
                "9780547928227", 1937, "/works/OL27448W", EGenre.Fantasy, 15.50m, 5),
            Sample("b1c0a1c1-8724-4b99-9843-0abc771e0803", "Frankenstein", "Mary Shelley",
                "9780141439471", 1818, "/works/OL450063W", EGenre.SciFi, 10.00m, 12),
            Sample("b1c0a1c1-8724-4b99-9843-0abc771e0804", "The Adventures of Sherlock Holmes",
                "Arthur Conan Doyle", "9780140437713", 1892, "/works/OL262421W", EGenre.Mystery, 11.25m, 4));
        await db.SaveChangesAsync();
    }

    private static BookModel Sample(string id, string title, string author, string isbn, int year,
        string key, EGenre genre, decimal price, int stock)
    {
        return new BookModel
        {
            Id = Guid.Parse(id),
            Title = title,
            Author = author,
            Isbn = isbn,
            PublishedYear = year,
            OpenLibraryKey = key,
            Genre = genre,
            Price = price,
            Stock = stock
        };
    }
}
