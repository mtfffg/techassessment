using Enums;

namespace Models;

public class BookVM
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int? PublishedYear { get; set; }
    public string? CoverUrl { get; set; }
    public string OpenLibraryKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EGenre Genre { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
