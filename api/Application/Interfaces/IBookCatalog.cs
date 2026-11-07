using Application.Models;

namespace Application.Interfaces;

public interface IBookCatalog
{
    Task<List<CatalogBookVM>> SearchBooks(string search);
}
