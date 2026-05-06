using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GarageFlow.Infrastructure.Persistence;

public sealed class GarageFlowDbContextFactory : IDesignTimeDbContextFactory<GarageFlowDbContext>
{
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__GarageFlow";
    private const string DockerComposeDefaultConnection =
        "Host=localhost;Port=5432;Database=garageflow;Username=garageflow;Password=garageflow";

    public GarageFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GarageFlowDbContext>();
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DockerComposeDefaultConnection;
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new GarageFlowDbContext(optionsBuilder.Options);
    }
}
