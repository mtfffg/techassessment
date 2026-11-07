# Bookstore Technical Assessment

.NET 10 API, Angular 21, SQLite, PrimeNG and Tailwind CSS.
Requires the .NET 10 SDK and Node.js 24.

Use Add book to search Open Library, select a result, then choose a genre and price. Saving adds the book to SQLite and refreshes the table. The list supports genre filtering, sorting and pagination. Use Edit to change genre, price and stock, or Delete to remove a book.

Run the API from the root: `dotnet run --project api`.
Run the website from `web`: `npm ci`, then `npm start`.
Open http://localhost:4200. The API runs on port 5080. SQLite is created automatically.

Tests: `dotnet test BookStore.slnx` and, from `web`, `npm test -- --watch=false`.

The API keeps Domain, Application and Infrastructure in one project. Controllers send requests through `RequestService` to separate command and query handlers in `Application/Features`. Repository and catalog interfaces keep EF Core and Open Library outside the handlers. The grouped genre query is in `api/Infrastructure/Data/BookRepository.cs`.

Angular uses Core for models and repository interfaces, Application for services and shared state, Infrastructure for HTTP calls, and Presentation for components. Header controls the genre filter and add-book dialog through the shared service. Serilog writes JSON logs with a request ID.
