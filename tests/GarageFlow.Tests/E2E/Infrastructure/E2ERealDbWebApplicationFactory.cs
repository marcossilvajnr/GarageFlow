using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.E2E.Infrastructure;

public sealed class E2ERealDbWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string RealDbConnectionEnvVar = "E2E_REAL_DB_CONNECTION";
    private const string DefaultConnectionEnvVar = "ConnectionStrings__GarageFlow";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var connectionString = Environment.GetEnvironmentVariable(RealDbConnectionEnvVar)
                ?? Environment.GetEnvironmentVariable(DefaultConnectionEnvVar);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"E2E real DB requer connection string. Defina '{RealDbConnectionEnvVar}' " +
                    $"ou '{DefaultConnectionEnvVar}'.");
            }

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
            // Replace authentication with test scheme so E2E can call protected endpoints without real JWT.
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<TestAuthSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });

        builder.UseEnvironment("Development");
    }
}
