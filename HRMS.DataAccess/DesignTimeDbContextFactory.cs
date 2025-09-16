using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HRMS.DataAccess;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Server=(localdb)\\MSSQLLocalDB;Database=HRMSDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var basePath = Directory.GetCurrentDirectory();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        IConfigurationRoot configuration = BuildConfigurationForPath(basePath, environment);

        if (!string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
        {
            return configuration;
        }

        var apiPath = Path.Combine(basePath, "..", "HRMS.API");
        return BuildConfigurationForPath(apiPath, environment);
    }

    private static IConfigurationRoot BuildConfigurationForPath(string basePath, string? environment)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true);

        if (!string.IsNullOrWhiteSpace(environment))
        {
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
        }

        builder.AddEnvironmentVariables();

        return builder.Build();
    }
}
