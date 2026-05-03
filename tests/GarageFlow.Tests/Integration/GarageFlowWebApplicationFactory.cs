using GarageFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Tests.Integration;

public sealed class GarageFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IServiceProvider _inMemoryServiceProvider = new ServiceCollection()
        .AddEntityFrameworkInMemoryDatabase()
        .BuildServiceProvider();

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
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<GarageFlowDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            var dbName = $"GarageFlowTest_{Guid.NewGuid()}";
            services.AddDbContext<GarageFlowDbContext>(options =>
                options
                    .UseInternalServiceProvider(_inMemoryServiceProvider)
                    .UseInMemoryDatabase(dbName));
        });

        builder.UseEnvironment("Development");
    }
}
