# codex-hrms-clean-architecture

Clean Architecture HRMS demo with .NET 8

## Solution Layout
- **HRMS.Models** (Class Library)
- **HRMS.DataAccess** (Class Library)
- **HRMS.Services** (Class Library)
- **HRMS.API** (ASP.NET Core Web API)
- **HRMS.UI** (ASP.NET Core MVC)
- **HRMS.Tests** (xUnit)

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
