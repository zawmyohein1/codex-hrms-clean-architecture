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

## PR #5 – API
- Added `DepartmentsController`, `EmployeesController`, and `LeaveBalancesController` (all under `api/{controller}`) that expose paged list, lookup, create, update, and delete endpoints backed by the service layer.
- Introduced a shared controller base to translate known service exceptions into RFC 7807 problem details and to normalize paging parameters.
- Enabled Swagger (development-only) so the new endpoints can be discovered quickly during manual testing, and added controller smoke tests that exercise the CRUD endpoints with stubbed services.

### Sample requests
```http
GET /api/employees?page=1&size=10
```

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "items": [
    {
      "id": 1,
      "empNo": "EMP001",
      "fullName": "Ada Lovelace",
      "email": "ada@example.com",
      "departmentName": "Engineering",
      "hireDate": "2020-01-01T00:00:00"
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 10
}
```

```http
POST /api/departments
Content-Type: application/json

{
  "name": "People Operations"
}
```

```http
HTTP/1.1 201 Created
Location: /api/departments/5
Content-Type: application/json

{
  "id": 5,
  "name": "People Operations"
}
```

> ℹ️ Validation follows the service layer guardrails: input strings are trimmed and inspected with string-based `StringComparison` overloads (for example `email.Contains("@", StringComparison.Ordinal)`), identifier arguments must be positive integers, and invalid arguments return HTTP 400 with RFC 7807 payloads while missing entities return HTTP 404.

> ⚠️ **Developer note:** After pulling this PR, run `dotnet restore HRMS.sln` locally before the first build to regenerate `project.assets.json`.

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


### PR #6 – MVC UI

- Added standard MVC folder structure: `Controllers`, `Views`, `Models`.
- Implemented `HomeController` with `Index()` action.
- Created `Views/Home/Index.cshtml` with placeholder: **"HRMS UI Home Page"**.
- Added shared layout `Views/Shared/_Layout.cshtml` (Bootstrap, navbar, footer).
- Configured `_ViewStart.cshtml` to use the layout.
- Added `_ViewImports.cshtml` with TagHelpers and `HRMS.Models` namespace.
- Confirmed UI project references `HRMS.Models`.
- Prepared base structure for future CRUD pages.

### PR #7 – MVC CRUD Pages

- Implemented full CRUD pages in **HRMS.UI**:
  - **Employees**
    - `EmployeesController` with Index, Details, Create, Edit, Delete.
    - Views under `Views/Employees` (Bootstrap forms and tables).
  - **Departments**
    - `DepartmentsController` with Index, Details, Create, Edit, Delete.
    - Views under `Views/Departments`.
- All CRUD actions communicate with **HRMS.API** via `HttpClient`.
- Added DTO usage from `HRMS.Models`.
- Configured base API URL in `appsettings.json`.
- Updated shared layout (`_Layout.cshtml`) with navbar links to Employees and Departments.
- Verified minimal but functional UI with Bootstrap styling.


