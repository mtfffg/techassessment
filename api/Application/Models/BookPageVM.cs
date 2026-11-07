namespace Application.Models;

public class BookPageVM
{
    public IReadOnlyList<BookVM> Items { get; set; } = new List<BookVM>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
