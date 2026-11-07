using Domain.Enums;

namespace Application.Features.Books.UpdateBook;

public class UpdateBookModel
{
    public Guid Id { get; set; }
    public EGenre? Genre { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
}
