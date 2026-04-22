using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GarageFlow.Infrastructure.Persistence;

public sealed class GarageFlowDbContextFactory : IDesignTimeDbContextFactory<GarageFlowDbContext>
{
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__GarageFlow";

    public GarageFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GarageFlowDbContext>();
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
            ?? throw new InvalidOperationException(
                $"Environment variable '{ConnectionStringEnvironmentVariable}' was not found.");

        optionsBuilder.UseNpgsql(connectionString);

        return new GarageFlowDbContext(optionsBuilder.Options);
    }
}
