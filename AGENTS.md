# AGENTS.md — sg-backend

Single .NET 10 ASP.NET Core Web API (`net10.0`, `BackEnd.csproj`). PostgreSQL via EF Core/Npgsql hosted on Supabase. No CI, no tests, no lint/format config.

> **This file is gitignored** — it lives locally only. Treat it as ground truth while present.

## Quick start

```bash
dotnet restore
dotnet run                    # http://localhost:5125 (launchSettings)
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update   # apply migrations
```

## Architecture

- **Controllers** per domain under `Controllers/` → `Services/` → `AppDbContext` (EF Core)
- **Auth**: JWT read from cookie `current_user`. `[HasPermission("permission_name")]` for fine-grained access. `[Authorize(Roles = "ADMIN")]` for admin-only. Custom `PermissionPolicyProvider` + `PermissionHandler`.
- **Result pattern** (`BackEnd.Utils.Result` / `Result<T>`) with `ErrorType` enum: `None, Failure, Validation, NotFound, Conflict, Unauthorized, Forbidden, Unexpected`. Controllers use `ControllerExtensions` to map results to `ProblemDetails`.
- **Error messages** in Spanish in `Constants/Errors/`.
- **AutoMapper** profiles in `DTOs/Mappings/`.
- **Swagger** at root `/`.

## DB quirks

- `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` required in `Program.cs`
- PostgreSQL enums mapped in `Program.cs` via `npgsqlOptions.MapEnum<T>()`; also declared in `AppDbContext.OnModelCreating` via `modelBuilder.HasPostgresEnum<T>()` — **both are required**
- Many C# enums use `HasConversion<int>()` (stored as int)
- Connection strings + JWT key live in `appsettings.json` (prod) and `appsettings.Development.json` (dev) — both gitignored, both real Supabase instances

## CORS

Allows `localhost`, `*.mbeju.xyz`, `*.netlify.app` with credentials. Configurable via `AllowedOrigins` in config (semicolon-separated).

## Docker

Port `5125`, multi-stage build targeting `mcr.microsoft.com/dotnet/aspnet:10.0` / `sdk:10.0`. `compose.debug.yaml` adds volume mount for remote debugger.

## Conventions

- Primary constructor DI in controllers and services
- `[Route("api/...")]` with `[ApiController]`
- Services return `Result<T>`; controllers switch on `result.ErrorType` and call `this.Handle*Problem(result)`
- DTOs split into `Requests/` and `Responses/` per domain
- No async naming suffix convention observed

## Useful context

- `docs/PAYROLL_FRONTEND_CONTRACT.md` documents payroll API state, gaps, and frontend contract
- `docs/` also contains accounting module requirements and DB seeding scripts
- 32 EF Core migrations exist in `Migrations/` (May–June 2026)
