# Bookstore Technical Assessment

.NET 10 API, Angular 21, SQLite, PrimeNG and Tailwind CSS.
Requires the .NET 10 SDK and Node.js 24.

Use Add book to search Open Library, select a result, then choose a genre and price. Saving adds the book to SQLite and refreshes the table. The list supports genre filtering, sorting and pagination, and books can be deleted from the table.

Run the API from the root: `dotnet run --project api`.
Run the website from `web`: `npm ci`, then `npm start`.
Open http://localhost:4200. The API runs on port 5080. SQLite is created automatically.

Tests: `dotnet test BookStore.slnx` and, from `web`, `npm test -- --watch=false`.
