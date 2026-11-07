using System.Net;
using Newtonsoft.Json;
using Infrastructure.Catalog;
using Application.Features.Catalog.SearchCatalog;
using Xunit;

namespace Tests;

public class CatalogTests
{
    [Fact]
    public async Task SearchBooks()
    {
        //use a fake response instead of calling open library
        using var handler = new CatalogResponseHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://openlibrary.org/") };
        var service = new SearchCatalogQueryHandler(new CatalogService(http));
        handler.Json = """
            {"docs":[{"key":"/works/OL27448W","title":"The Hobbit",
            "author_name":["J. R. R. Tolkien"],"isbn":["9780547928227"],
            "first_publish_year":1937,"cover_i":123}]}
            """;

        var book = Assert.Single(await service.Handle(new SearchCatalogModel { Search = "hobbit" }));
        Assert.Equal("The Hobbit", book.Title);
        Assert.Equal("J. R. R. Tolkien", book.Author);
        Assert.Equal("9780547928227", book.Isbn);
        Assert.Equal(1937, book.PublishedYear);
        Assert.Equal("/works/OL27448W", book.OpenLibraryKey);
        Assert.Equal("https://covers.openlibrary.org/b/id/123-M.jpg", book.CoverUrl);

        //check no results and empty search
        handler.Json = """{"docs":[]}""";
        Assert.Empty(await service.Handle(new SearchCatalogModel { Search = "unknown book" }));
        await Assert.ThrowsAsync<ArgumentException>(() => service.Handle(new SearchCatalogModel { Search = " " }));

        //check invalid response and unavailable service
        handler.Json = "{}";
        await Assert.ThrowsAsync<JsonException>(() => service.Handle(new SearchCatalogModel { Search = "hobbit" }));
        handler.StatusCode = HttpStatusCode.ServiceUnavailable;
        await Assert.ThrowsAsync<HttpRequestException>(() => service.Handle(new SearchCatalogModel { Search = "hobbit" }));
    }

    private class CatalogResponseHandler : HttpMessageHandler
    {
        public string Json { get; set; } = "{}";
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //return the response set by the test
            return Task.FromResult(new HttpResponseMessage(StatusCode) { Content = new StringContent(Json) });
        }
    }
}
