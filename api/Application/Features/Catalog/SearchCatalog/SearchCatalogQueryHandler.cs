using Application.Interfaces;
using Application.Models;

namespace Application.Features.Catalog.SearchCatalog;

public class SearchCatalogQueryHandler(IBookCatalog catalog)
{
    public Task<List<CatalogBookVM>> Handle(SearchCatalogModel request)
    {
        //check search term
        if (string.IsNullOrWhiteSpace(request.Search) || request.Search.Trim().Length > 200)
        {
            throw new ArgumentException("Enter a search term between 1 and 200 characters.");
        }

        //search the catalog
        return catalog.SearchBooks(request.Search.Trim());
    }
}
