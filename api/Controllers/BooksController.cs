using Microsoft.AspNetCore.Mvc;
using Models;
using Enums;
using Services;

namespace Controllers;

[ApiController]
[Route("api/books")]
public class BooksController(RequestService requests) : ControllerBase
{
    [HttpGet]
    public Task<BookPageVM> GetBooks(
        [FromQuery] EGenre? genre = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] EBookSort sort = EBookSort.Title, [FromQuery] bool descending = false)
    {
        //build the search model
        var search = new SearchBookModel
        {
            Genre = genre,
            Page = page,
            PageSize = pageSize,
            Sort = sort,
            Descending = descending
        };

        //call service
        return requests.Send(search);
    }

    [HttpGet("genre-counts")]
    public Task<IReadOnlyList<GenreCountVM>> GetGenreCounts()
    {
        //get genre counts
        return requests.GetGenreCounts();
    }
}
