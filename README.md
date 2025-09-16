# codex-hrms-clean-architecture

Clean Architecture HRMS demo with .NET 8.

## Solution Layout
- **HRMS.Models** (Class Library)
- **HRMS.DataAccess** (Class Library)
- **HRMS.Services** (Class Library)
- **HRMS.API** (ASP.NET Core Web API)
- **HRMS.UI** (ASP.NET Core MVC)
- **HRMS.Tests** (xUnit)

## Data Access & EF Core
- `HRMS.DataAccess` exposes `AppDbContext`, fluent entity configurations, and a reusable `IGenericRepository<T>`/`GenericRepository<T>` pair.
- Entity Framework Core is configured for SQL Server and wired into `HRMS.API` via dependency injection.
- A database health probe is available at `GET /health/db` (returns HTTP 200 when the database connection succeeds).

### Packages
The data access project references the following EF Core packages:
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`

### Migrations
Create or update migrations from the solution root (files only, no database update required):

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> -p HRMS.DataAccess -s HRMS.API

# Apply migrations to the configured database
dotnet ef database update -p HRMS.DataAccess -s HRMS.API
```

## Running

Restore dependencies and run the projects:

```bash
# API
cd HRMS.API
# dotnet run

# UI
cd ../HRMS.UI
# dotnet run
```

Run tests:

```bash
cd ../HRMS.Tests
# dotnet test
```
