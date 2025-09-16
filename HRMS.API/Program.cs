using HRMS.DataAccess;
using HRMS.DataAccess.Repositories;
using HRMS.Services;
using HRMS.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Server=(localdb)\\MSSQLLocalDB;Database=HRMSDb;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();

var app = builder.Build();

app.MapGet("/", () => "HRMS API");

app.MapGet("/health/db", async (AppDbContext context) =>
{
    var canConnect = await context.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new { status = "Healthy" })
        : Results.Problem("Database connection failed.", statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.Run();
