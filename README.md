# TinyUrl API

TinyUrl is a minimal URL shortening Web API built on .NET 8 using ASP.NET Core minimal APIs and SQLite (EF Core). It provides endpoints to shorten URLs, redirect short codes to original URLs, list and manage saved URLs.

## Table of contents
- Features
- Requirements
- Quick start (local)
- Database & migrations
- Running in Visual Studio 2022
- Endpoints (examples)
- Deployment notes (Azure App Service)
- Troubleshooting
- Implementation notes

## Features
- Shorten a long URL to a short code
- Redirect short code to original URL and count clicks
- List, get details and delete short URLs
- CORS config for local dev and a production static site
- Swagger UI in Development

## Requirements
- .NET 8 SDK
- Visual Studio 2022 (updated) or `dotnet` CLI
- EF Core Tools (for migrations): `dotnet tool install --global dotnet-ef` (if not already installed)

## Quick start (local)
1. Clone repository.
2. Open solution in Visual Studio 2022 or use terminal.
3. Restore packages:
   - Visual Studio: right-click solution -> __Restore NuGet Packages__
   - CLI: `dotnet restore`
4. In Debug configuration the app stores the SQLite DB in the project folder as `tinyurl.db`. In Release the app uses the App Service __HOME__ folder (see Database & migrations).
5. Run:
   - Visual Studio: press F5 or use __Debug > Start Debugging__.
   - CLI: `dotnet run --project TinyUrl.Api`

Swagger UI (in Development): `http://localhost:{port}/swagger`

## Database & migrations
- The project uses EF Core with SQLite. Connection is configured in `Program.cs`.
- Debug: DB path = `Directory.GetCurrentDirectory()/tinyurl.db`
- Release (intended for Azure App Service): DB path = `Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "D:\\home", "data", "tinyurl.db")`

Create and apply migrations:
- Using CLI:
  - `dotnet ef migrations add InitialCreate --project TinyUrl.Api`
  - `dotnet ef database update --project TinyUrl.Api`
- Using Visual Studio: open __Package Manager Console__ and run the same commands (make sure Default project is TinyUrl.Api).

Note: The project file currently includes `tinyurl.db` as an embedded resource/copy-to-output; keep that in mind if you want a clean blank DB when first run.

## Running in Visual Studio 2022
- Open the solution.
- Ensure the build configuration is correct (Debug/Release) via __Build > Configuration Manager__.
- To enable detailed error pages locally or on Azure for debugging, set __ASPNETCORE_ENVIRONMENT__ to `Development`.
- Press F5 or use __Debug > Start Debugging__.

## Endpoints

1. Create short URL
   - POST /api/urls
   - Body (JSON): `{ "originalUrl": "https://example.com/path", "isPrivate": false }`
   - Response: `{ "id": 1, "shortCode": "Ab12Cd" }`

   Example curl: