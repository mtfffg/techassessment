# Bookstore Technical Assessment

.NET 10 API with EF Core/SQLite and an Angular 21 client using PrimeNG 21 and Tailwind CSS 4.

## Run

Prerequisites: .NET 10 SDK, Node.js 24.x and npm. VS Code is sufficient; Visual Studio is not required.

From the repository root:

```powershell
dotnet restore BookStore.slnx
dotnet run --project api
```

In a second terminal:

```powershell
cd web
npm ci
npm start
```

Open http://localhost:4200. The API runs on http://localhost:5080. Angular proxies `/api` requests to the API through `web/proxy.conf.json`.

In VS Code, F5 with **Debug Book Store API** starts the backend. Start Angular separately with `npm start` in `web`, or use the **Web: run** task.

No API keys or database installation are needed. The application creates `api/App_Data/bookstore.db` and seeds four sample books if the database is empty. That folder is ignored by Git.

## Folder structure

```text
api/
  Controllers/      HTTP endpoints
  Models/           Internal *Model.cs classes and *VM.cs view models
  Enums/            EGenre and EBookSort
  Configuration/    Settings and HTTP error handling
  Services/         BookService and RequestService
  Data/             EF Core context, repository, its interface and seeding
    Models/         Database entities named *Model.cs
  Properties/       .NET launch profile used by VS Code and dotnet run
  Program.cs        Startup and dependency registration
  Api.csproj        One application project

tests/              Backend tests and Tests.csproj

web/src/app/
  home/             Home page component and HTML
  header/           Shared header component and HTML
  models/           IBook, IBookPage and ISearchBook (one interface per file)
  enums/            EGenre and EBookSort
  services/         BooksService and its tests
  app.routes.ts     Page routes
  app.config.ts     Angular providers and PrimeNG theme
```

The backend has one application project and one separate test project. `BooksController` passes `SearchBookModel` to `RequestService`, which routes it to `BookService`. This is the mediator pattern required by the assessment. `BookService` validates input and calls the repository in `Data`. `IBookRepository` sits alongside `BookRepository`. The EF entity is `Data/Models/BookModel.cs`, and API responses use plain view-model classes with `get; set;` properties. Genre counts use a parameterless method rather than an empty request model.

The Angular app uses a conventional component/models/services layout. `BooksService.getBooks` only makes the API call. Home handles loading, results, errors, the genre filter and pagination. Signals let Angular update the screen when a response arrives. Starting another request cancels the previous one, and leaving Home cancels any pending request. Each page has its own routed component. Model interfaces have ordinary mutable properties, with one interface per file.

These are the requested structural choices. They depart from the assessment's explicit frontend layers, repository abstraction, Header-owned genre filter and three-sibling Header/Body/Footer shell. The backend retains request-based mediator dispatch through `RequestService`, with book operations in `BookService`, but does not use separate feature slices or enforce Clean Architecture through separate projects. Review those differences before submission.

## Current functionality

- Dark theme, Home page and shared header; no site footer or count badge.
- SQLite book list with server-side filtering, pagination and sorting.
- Pagination sits outside the table; dropdown overlays attach to the document body to avoid clipping.
- Loading, empty and error states, with retry.
- Structured JSON logs and request IDs in log scopes and the `X-Request-Id` header.
- Backend query/service tests and small Angular service/component unit tests.

Still to implement: Open Library search, selecting and adding a result, single-book retrieval, editing, deletion, write validation and duplicate-conflict handling. This is not yet a complete assessment submission. Search/add and edit can be separate dialog components on Home; additional pages can be added when needed.

## API and data

- `GET /health`
- `GET /api/books?genre=Fantasy&page=1&pageSize=10&sort=Price&descending=false`
- `GET /api/books/genre-counts`

Sort fields: `Title`, `Author`, `Price`, `PublishedYear`. Page size must be between 1 and 100. Sorting includes an ID tie-breaker for consistent paging.

The explicit grouped query required by the assessment is `BookRepository.CountByGenre` in `api/Data/BookRepository.cs`. EF translates it to a grouped SQLite query.

SQLite prices are stored as integer cents through an EF converter and returned as decimal values. The UI currently displays USD. Writes will validate at most two decimal places. ISBN and Open Library key have unique database indexes; handling missing ISBNs and returning duplicate errors are part of the add-book work.

The development initializer uses `EnsureCreated`. For schema changes during development, remove the development database and restart. Use migrations before retaining data across schema changes. Seed books are local fixtures and are never refreshed from Open Library.

Settings live in `api/Configuration/appsettings.json`; an optional `appsettings.Development.json` in that folder is also loaded. Environment variables and command-line arguments override file settings. Set `ConnectionStrings__BookStore` to override the database connection string. If the API port changes, update the Angular proxy too. Do not commit secrets.

## Checks

From the repository root:

```powershell
dotnet build BookStore.slnx
dotnet test BookStore.slnx
```

From `web`:

```powershell
npm test -- --watch=false
npm run build
```

Backend tests use isolated in-memory SQLite databases, not the development database.

`*.spec.ts` files are frontend unit tests. They run with `npm test` and are not part of the running application. The four tests check:

- `BooksService` sends the selected filter, page and sorting options to the API.
- Changing genre resets Home to page one.
- Sorting keeps the selected page size.
- An API failure shows an error, and retry clears it.

The component tests use a fake service; the service test uses Angular's HTTP test controller. Neither calls a real API. `tsconfig.spec.json` explicitly sets `rootDir` to `src` so the test output layout is unambiguous.

The production build passes the existing budgets. Home loads as a separate route bundle.

At setup, `npm audit` reported 16 moderate findings in Angular 21 CLI development dependencies. `npm audit --omit=dev` reported zero production findings. The suggested automatic fix upgrades Angular CLI to 22; it was not applied because this assessment specifies Angular 21.

## Review

Run both projects, select Fantasy and check that only The Hobbit appears. Sort by price and change the page size. Stop the API, change genre, then restart the API and retry. Trace the request through Home, BooksService, BooksController, RequestService, BookService and BookRepository.

Review and commit each working increment so the history reflects the development process.
