using Models;

namespace Services;

// Routes API requests to the book service (the mediator pattern required by the assessment).
public class RequestService(BookService books, CatalogService catalog)
{
    public Task<BookPageVM> Send(SearchBookModel request)
    {
        //get books list
        return books.GetBooks(request);
    }

    public Task<IReadOnlyList<GenreCountVM>> GetGenreCounts()
    {
        //get genre counts
        return books.GetGenreCounts();
    }

    public Task<List<CatalogBookVM>> SearchCatalog(string search)
    {
        //search open library
        return catalog.SearchBooks(search);
    }
}
