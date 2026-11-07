# Bookstore Technical Assessment

.NET 10 API, Angular 21, SQLite, PrimeNG and Tailwind CSS.
Requires the .NET 10 SDK and Node.js 24.

Use Add book to search Open Library, select a result, then choose a genre and price. Saving adds the book to SQLite and refreshes the table. The list supports genre filtering, sorting and pagination. Use Edit to change genre, price and stock, or Delete to remove a book.

Run the API from the root: `dotnet run --project api`.
Run the website from `web`: `npm ci`, then `npm start`.
Open http://localhost:4200. The API runs on port 5080. SQLite is created automatically.

Tests: `dotnet test BookStore.slnx` and, from `web`, `npm test -- --watch=false`.

I used a controller/service/repository structure for the API and component, model and service folders for Angular because this is the structure I work with at my current job. I started with that approach because it is familiar and keeps the request flow easy for me to follow and maintain. I understand this differs from the Clean Architecture, Vertical Slice and CQRS structure requested in the assessment. A separate [codex/clean-architecture branch](https://github.com/mtfffg/techassessment/tree/codex/clean-architecture) contains that refactor for comparison; it has not been merged into main.
