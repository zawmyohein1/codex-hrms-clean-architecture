# codex-hrms-clean-architecture

Clean Architecture HRMS demo built with **.NET 8**.

---

## Solution Layout
- **HRMS.Models** — Class Library (entities, DTOs)  
- **HRMS.DataAccess** — Class Library (EF Core DbContext, repositories, migrations)  
- **HRMS.Services** — Class Library (business logic, DTO-based services)  
- **HRMS.API** — ASP.NET Core Web API (service endpoints)  
- **HRMS.UI** — ASP.NET Core MVC (presentation layer)  
- **HRMS.Tests** — xUnit tests (unit & integration tests)  

---

## Data Access & EF Core

- `HRMS.DataAccess` defines `AppDbContext`, fluent entity configurations, and a reusable `IGenericRepository<T>`/`GenericRepository<T>` pair.  
- EF Core is configured for **SQL Server**, injected into `HRMS.API` via DI.  
- Database health probe available at `GET /health/db` (HTTP 200 = healthy).  

### Required Packages
Ensure the following packages are installed (same version everywhere, e.g. **8.0.6**):

**HRMS.API**
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`

**HRMS.DataAccess**
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.Extensions.Configuration.*` (for design-time factory)

### EF Core Migrations
Generate and apply migrations:

```powershell
# Add a migration
Add-Migration <MigrationName> -StartupProject HRMS.API -Project HRMS.DataAccess -Context AppDbContext

# Apply migrations
Update-Database -StartupProject HRMS.API -Project HRMS.DataAccess -Context AppDbContext

A valid migration includes both:
YYYYMMDDHHMMSS_<Name>.cs
YYYYMMDDHHMMSS_<Name>.Designer.cs
If the .Designer.cs file is missing, EF cannot detect the migration.
Auto-apply Migrations at Runtime
For development convenience, migrations are applied automatically at API startup

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.Migrate();
}

PR History
PR #1 – Solution & Projects Scaffold

Created initial solution structure (HRMS.sln) with 6 projects.

Added project references to enforce clean architecture dependencies.

PR #2 – Entities & DTOs

Introduced core entities (Department, Employee, LeaveBalance) in HRMS.Models.

Added DTOs for create/update requests and paged queries.

PR #3 – DataAccess & EF Core

Implemented AppDbContext with DbSets for entities.

Added IGenericRepository<T> and GenericRepository<T>.

Added EF Core configuration classes for entities.

Configured design-time DbContext factory.

Initial migration created (InitialCreate).

PR #4 – Services Layer

Added strongly-typed services (DepartmentService, EmployeeService, LeaveBalanceService).

Services operate on DTOs with validation (IDs, strings, email format, non-negative balances).

Registered services + repository in DI container.

Added smoke tests using EF Core in-memory provider.

PR #5 – API Controllers

Added DepartmentsController, EmployeesController, LeaveBalancesController.

CRUD endpoints with paging, lookup, create, update, delete.

Swagger enabled for development.

Controllers translate service exceptions to RFC 7807 problem details.

Added API smoke tests with stubbed services.

PR #6 – MVC UI Scaffold

Added MVC folders: Controllers, Views, Models.

Implemented HomeController and Views/Home/Index.cshtml.

Added shared layout _Layout.cshtml (Bootstrap navbar + footer).

Configured _ViewStart.cshtml and _ViewImports.cshtml.

Confirmed reference to HRMS.Models.

PR #7 – MVC CRUD Pages

Added Employees CRUD (EmployeesController, Views/Employees/*).

Added Departments CRUD (DepartmentsController, Views/Departments/*).

Views use Bootstrap forms and tables.

UI communicates with HRMS.API via HttpClient.

Configured API base URL in appsettings.json.

Updated _Layout.cshtml with navbar links.

Verified functional UI with Bootstrap styling.

Developer Notes

Always keep EF Core package versions consistent (8.0.6).

Ensure migrations include .Designer.cs or EF won’t detect them.

DefaultConnection must match in both Program.cs and DesignTimeDbContextFactory.

Use dotnet restore HRMS.sln after pulling new PRs to regenerate assets.




