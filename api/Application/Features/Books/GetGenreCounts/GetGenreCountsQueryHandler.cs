using Application.Interfaces;
using Application.Models;

namespace Application.Features.Books.GetGenreCounts;

public class GetGenreCountsQueryHandler(IBookRepository books)
{
    public Task<IReadOnlyList<GenreCountVM>> Handle()
    {
        //get genre counts
        return books.CountByGenre();
    }
}
