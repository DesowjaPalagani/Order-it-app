# Order-it-app
A minimal ASP.NET Core MVC sample for a food ordering website.

## Overview
This repository demonstrates a simple layered architecture with:

- **Models** (`Order` entity)
- **Data** (`OrderContext` using EF Core In‑Memory database)
- **Controllers** (`OrdersController` with basic CRUD)
- **Views** (Razor pages for listing/creating orders)

The project uses the older Startup pattern and is configured for .NET 8.

## Getting Started

1. Ensure **.NET 8 SDK** or newer is installed.
2. Navigate to the project root and run:
   ```bash
   dotnet restore
   dotnet run
   ```
3. Open `https://localhost:5001/` in your browser. The default route points to the orders index.

### MongoDB configuration

The app uses MongoDB to store user accounts. Before trying to register or log in, make
sure you have a Mongo instance available and edit `appsettings.json` (or supply an
environment variable) with a valid connection string and credentials. Example:

```json
"MongoDb": {
  "ConnectionString": "mongodb+srv://<user>:<password>@cluster0.example.net/?retryWrites=true&w=majority",
  "DatabaseName": "OrderItDb",
  "UsersCollection": "Users"
}
```

Failure to provide valid credentials will result in a runtime error when the account
endpoints are called.

## Architectural Notes

- Dependency injection configures `OrderContext` as a service.
- Error handling, static files, and routing middleware are set up in `Startup`.
- The `Order` model includes validation attributes; the controller uses `ModelState.IsValid`.

Feel free to extend the domain, add persistence, authentication, etc.