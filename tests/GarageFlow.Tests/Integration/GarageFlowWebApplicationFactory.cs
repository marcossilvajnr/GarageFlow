using GarageFlow.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
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
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GarageFlow"] = "Host=localhost;Database=test;Username=test;Password=test"
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

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
            dbContext.Database.EnsureCreated();
        });

        builder.UseEnvironment("Development");
    }
}
