using Models;

namespace Data;

public interface IBookRepository
{
    Task<BookPageVM> List(SearchBookModel search);

    Task<IReadOnlyList<GenreCountVM>> CountByGenre();
}
