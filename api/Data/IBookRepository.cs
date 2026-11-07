using Models;

namespace Data;

public interface IBookRepository
{
    Task<BookPageVM> List(SearchBookModel search);

    Task<IReadOnlyList<GenreCountVM>> CountByGenre();

    Task<BookVM?> Add(AddBookModel book);

    Task<BookVM?> Get(Guid id);

    Task<BookVM?> Update(Guid id, UpdateBookModel book);

    Task<bool> Delete(Guid id);
}
