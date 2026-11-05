using System.Text.Json.Serialization;
using Configuration;
using Data;
using Models;
using Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("Configuration/appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"Configuration/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
if (args.Length > 0) builder.Configuration.AddCommandLine(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddProblemDetails();
builder.Services.AddScoped<ApiExceptionHandler>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddHttpClient<CatalogService>(http =>
{
    http.BaseAddress = new Uri("https://openlibrary.org/");
    http.Timeout = TimeSpan.FromSeconds(10);
    //identify our app to open library
    var userAgent = "BookstoreTechnicalAssessment/1.0 (+https://github.com/mtfffg/techassessment)";
    http.DefaultRequestHeaders.Add("User-Agent", userAgent);
});
builder.Services.AddScoped<IBookRepository, BookRepository>();

var dataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataPath);
builder.Services.AddDbContext<BookStoreDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("BookStore")
        ?? $"Data Source={Path.Combine(dataPath, "bookstore.db")}");
});

var app = builder.Build();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(context =>
    {
        return context.RequestServices.GetRequiredService<ApiExceptionHandler>().HandleAsync(context);
    });
});
app.UseStatusCodePages();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Request-Id"] = context.TraceIdentifier;
    using var scope = app.Logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = context.TraceIdentifier });
    await next(context);
});

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "ok" });
});
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    await DatabaseInitializer.Initialize(scope.ServiceProvider.GetRequiredService<BookStoreDbContext>());
}

app.Run();

public partial class Program;
