using Application.Features.Books.AddBook;
using Application.Features.Books.DeleteBook;
using Application.Features.Books.GetBook;
using Application.Features.Books.GetBooks;
using Application.Features.Books.GetGenreCounts;
using Application.Features.Books.UpdateBook;
using Application.Features.Catalog.SearchCatalog;
using Application.Interfaces;
using Application.Messaging;

namespace Configuration;

public static class ServiceConfiguration
{
    public static void AddBookHandlers(this IServiceCollection services)
    {
        //register mediator and handlers
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<AddBookCommandHandler>();
        services.AddScoped<GetBooksQueryHandler>();
        services.AddScoped<GetBookQueryHandler>();
        services.AddScoped<UpdateBookCommandHandler>();
        services.AddScoped<DeleteBookCommandHandler>();
        services.AddScoped<GetGenreCountsQueryHandler>();
        services.AddScoped<SearchCatalogQueryHandler>();
    }
}
