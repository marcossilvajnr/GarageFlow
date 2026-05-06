using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using GarageFlow.Infrastructure.Persistence;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.E2E.Infrastructure;

public sealed class E2ERealDbWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string RealDbConnectionEnvVar = "E2E_REAL_DB_CONNECTION";
    private const string DefaultConnectionEnvVar = "ConnectionStrings__GarageFlow";
    private const string DockerComposeDefaultConnection =
        "Host=localhost;Port=5432;Database=garageflow;Username=garageflow;Password=garageflow";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = Environment.GetEnvironmentVariable(RealDbConnectionEnvVar);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable(DefaultConnectionEnvVar);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DockerComposeDefaultConnection;
        }

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GarageFlow"] = connectionString,
                ["Jwt:Issuer"] = "garageflow",
                ["Jwt:Audience"] = "garageflow-api",
                ["Jwt:SecretKey"] = "test-only-secret-key-32-bytes-long!",
                ["Database:AutoMigrateOnStartup"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<GarageFlowDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(GarageFlowDbContext));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<GarageFlowDbContext>));

            services.AddDbContext<GarageFlowDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Replace authentication with test scheme so E2E can call protected endpoints without real JWT.
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<TestAuthSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });

        builder.UseEnvironment("Development");
    }
}
