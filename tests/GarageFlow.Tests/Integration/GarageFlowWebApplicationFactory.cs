using GarageFlow.Api.Common.Authorization;
using GarageFlow.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GarageFlow.Tests.Integration;

public sealed class GarageFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    internal const string TestJwtIssuer = "garageflow";
    internal const string TestJwtAudience = "garageflow-api";
    internal const string TestJwtSecretKey = "test-only-secret-key-32-bytes-long!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GarageFlow"] = "Host=localhost;Database=test;Username=test;Password=test",
                ["Jwt:Issuer"] = TestJwtIssuer,
                ["Jwt:Audience"] = TestJwtAudience,
                ["Jwt:SecretKey"] = TestJwtSecretKey,
                ["Jwt:ExpirationInMinutes"] = "60",
                ["Auth:SeedUsers:0:Username"] = "admin",
                ["Auth:SeedUsers:0:Password"] = "admin123",
                ["Auth:SeedUsers:0:DisplayName"] = "Administrador Teste",
                ["Auth:SeedUsers:0:Role"] = ApiRoles.Administrative,
                ["Database:AutoMigrateOnStartup"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<GarageFlowDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(GarageFlowDbContext));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<GarageFlowDbContext>));

            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            services.AddSingleton(connection);
            services.AddDbContext<GarageFlowDbContext>(options =>
                options.UseSqlite(connection));

            // Replace authentication with the test scheme so tests don't need real JWT tokens.
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<TestAuthSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
            dbContext.Database.EnsureCreated();
        });

        builder.UseEnvironment("Development");
    }
}
