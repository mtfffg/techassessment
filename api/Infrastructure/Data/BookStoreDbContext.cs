using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : DbContext(options)
{
    public DbSet<BookModel> Books
    {
        get
        {
            return Set<BookModel>();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var book = modelBuilder.Entity<BookModel>();
        book.ToTable("Books");
        book.HasKey(x => x.Id);
        book.Property(x => x.Title).HasMaxLength(500);
        book.Property(x => x.Author).HasMaxLength(500);
        book.Property(x => x.Isbn).HasMaxLength(13);
        book.Property(x => x.OpenLibraryKey).HasMaxLength(100);
        book.HasIndex(x => x.Isbn).IsUnique();
        book.HasIndex(x => x.OpenLibraryKey).IsUnique();
        book.Property(x => x.Genre).HasConversion<string>();
        //store price as exact cents
        book.Property(x => x.Price).HasConversion(value => (long)(value * 100), value => value / 100m);
    }
}
