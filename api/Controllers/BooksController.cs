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

    [HttpPost]
    public async Task<IActionResult> AddBook([FromBody] AddBookModel request)
    {
        //save selected book
        var book = await requests.AddBook(request);
        if (book == null)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "This book is already in your store." });
        }

        return Created($"/api/books/{book.Id}", book);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        //delete selected book
        var deleted = await requests.DeleteBook(id);
        if (!deleted)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Book not found." });
        }

        return NoContent();
    }
}
