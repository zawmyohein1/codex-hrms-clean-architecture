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
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.Configuration.FileExtensions`
- `Microsoft.Extensions.Configuration.Json`
- `Microsoft.Extensions.Configuration.Binder`
- `Microsoft.Extensions.Configuration.EnvironmentVariables`

These dependencies keep the design-time context factory compiling without needing a local `appsettings.json`.

### Migrations
Create or update migrations from the solution root (files only, no database update required):

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> -p HRMS.DataAccess -s HRMS.API

# Apply migrations to the configured database
dotnet ef database update -p HRMS.DataAccess -s HRMS.API
```

## Services Layer (PR #4)
- Added strongly-typed CRUD services for `Department`, `Employee`, and `LeaveBalance` that sit on top of the shared `IGenericRepository<T>` abstraction.
- Services operate exclusively with DTOs (`Create*`, `Update*`, `PagedRequest`, `PagedResult<T>`) and include basic validation for identifiers, strings, email format, and non-negative leave balances.
- `HRMS.API` now registers `IDepartmentService`, `IEmployeeService`, and `ILeaveBalanceService` alongside the existing DbContext/repository wiring.
- `HRMS.Tests` includes an in-memory smoke test ensuring the service layer can create and fetch core entities.

> ⚠️ **Developer note:** After pulling this PR, run `dotnet restore HRMS.sln` locally before the first build to regenerate `project.assets.json`.

### Dependency Injection quick reference
`Program.cs` wires up the services as scoped dependencies:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
```

### Example minimal API endpoint
You can call the services from minimal API endpoints or controllers. For example, to fetch an employee by id:

```csharp
app.MapGet("/api/employees/{id:int}", async (int id, IEmployeeService employees, CancellationToken token) =>
{
    var employee = await employees.GetByIdAsync(id, token);
    return employee is not null
        ? Results.Ok(employee)
        : Results.NotFound();
});
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
