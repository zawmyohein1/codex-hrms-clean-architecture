using HRMS.DataAccess;
using HRMS.DataAccess.Repositories;
using HRMS.Services;
using HRMS.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Server=.; Database=HRMS-DEMO; User Id=xxx;  Password=XXXX; Trusted_Connection=false; MultipleActiveResultSets=true;TrustServerCertificate=true;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/", () => "HRMS API");

app.MapGet("/health/db", async (AppDbContext context) =>
{
    var canConnect = await context.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new { status = "Healthy" })
        : Results.Problem("Database connection failed.", statusCode: StatusCodes.Status503ServiceUnavailable);
});

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.Migrate();
}

app.Run();
