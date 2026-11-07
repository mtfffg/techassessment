using Microsoft.AspNetCore.Mvc;
using Application.Models;
using Application.Features.Books.AddBook;
using Application.Features.Books.GetBooks;
using Application.Features.Books.UpdateBook;
using Domain.Enums;
using Application.Interfaces;
using Application.Features.Books.GetBook;
using Application.Features.Books.DeleteBook;

namespace Controllers;

[ApiController]
[Route("api/books")]
public class BooksController(IRequestService requests) : ControllerBase
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
        var book = await requests.Send(request);
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
        var deleted = await requests.Send(new DeleteBookModel { Id = id });
        if (!deleted)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Book not found." });
        }

        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBook(Guid id)
    {
        //get saved book
        var book = await requests.Send(new GetBookModel { Id = id });
        if (book == null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Book not found." });
        }

        return Ok(book);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] UpdateBookModel request)
    {
        //update store details
        request.Id = id;
        var book = await requests.Send(request);
        if (book == null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Book not found." });
        }

        return Ok(book);
    }
}
