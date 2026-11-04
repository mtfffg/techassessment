using Enums;

namespace Models;

public class SearchBookModel
{
    public EGenre? Genre { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public EBookSort Sort { get; set; } = EBookSort.Title;
    public bool Descending { get; set; }
}
