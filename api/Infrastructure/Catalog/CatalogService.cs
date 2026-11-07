using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Application.Models;
using Application.Interfaces;

namespace Infrastructure.Catalog;

public class CatalogService(HttpClient http) : IBookCatalog
{
    public async Task<List<CatalogBookVM>> SearchBooks(string search)
    {
        //call open library
        var term = Uri.EscapeDataString(search.Trim());
        var url = $"search.json?q={term}&fields=key,title,author_name,first_publish_year,isbn,cover_i&limit=10";
        var json = await http.GetStringAsync(url);
        var response = JObject.Parse(json);
        var results = response["docs"] as JArray;

        if (results == null)
        {
            throw new JsonException("Open Library returned an invalid search response.");
        }

        //build book results
        var books = new List<CatalogBookVM>();
        foreach (var result in results.Take(10))
        {
            if (result is not JObject bookData)
            {
                continue;
            }

            var authors = (bookData["author_name"] as JArray)?.ToObject<List<string>>() ?? new List<string>();
            var isbns = (bookData["isbn"] as JArray)?.ToObject<List<string>>() ?? new List<string>();
            var book = new CatalogBookVM
            {
                Title = bookData["title"]?.ToObject<string>() ?? string.Empty,
                OpenLibraryKey = bookData["key"]?.ToObject<string>() ?? string.Empty,
                Author = string.Join(", ", authors),
                Isbn = isbns.FirstOrDefault() ?? string.Empty
            };

            //skip results without a title or key
            if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.OpenLibraryKey))
            {
                continue;
            }

            //set year if available
            if (int.TryParse(bookData["first_publish_year"]?.ToString(), out var publishedYear))
            {
                book.PublishedYear = publishedYear;
            }

            //use cover id or isbn for cover url
            if (int.TryParse(bookData["cover_i"]?.ToString(), out var coverId) && coverId > 0)
            {
                book.CoverUrl = $"https://covers.openlibrary.org/b/id/{coverId}-M.jpg";
            }
            else if (!string.IsNullOrWhiteSpace(book.Isbn))
            {
                book.CoverUrl = $"https://covers.openlibrary.org/b/isbn/{Uri.EscapeDataString(book.Isbn)}-M.jpg";
            }

            books.Add(book);
        }

        return books;
    }

}
