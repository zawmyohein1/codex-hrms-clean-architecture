var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "HRMS API");

app.Run();
