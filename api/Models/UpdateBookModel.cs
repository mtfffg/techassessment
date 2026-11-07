using Enums;

namespace Models;

public class UpdateBookModel
{
    public EGenre? Genre { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
}
