namespace Application.Models;

public class CatalogBookVM
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int? PublishedYear { get; set; }
    public string? CoverUrl { get; set; }
    public string OpenLibraryKey { get; set; } = string.Empty;
}
