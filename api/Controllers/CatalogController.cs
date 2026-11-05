using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController(RequestService requests, ILogger<CatalogController> logger) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "")
    {
        try
        {
            //search open library
            var books = await requests.SearchCatalog(q);
            return Ok(books);
        }
        catch (HttpRequestException exception)
        {
            //if open library is unavailable
            logger.LogWarning(exception, "Open Library search failed");
            return Problem(statusCode: 503, title: "Open Library is unavailable. Please try again.");
        }
        catch (TaskCanceledException exception)
        {
            //if open library takes too long
            logger.LogWarning(exception, "Open Library search timed out");
            return Problem(statusCode: 504, title: "Open Library took too long to respond. Please try again.");
        }
        catch (JsonException exception)
        {
            //if open library returns invalid data
            logger.LogWarning(exception, "Open Library returned invalid search data");
            return Problem(statusCode: 502, title: "Open Library returned an invalid response. Please try again.");
        }
    }
}
