using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GarageFlow.Infrastructure.Persistence;

public sealed class GarageFlowDbContextFactory : IDesignTimeDbContextFactory<GarageFlowDbContext>
{
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__GarageFlow";

    public GarageFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Environment variable '{ConnectionStringEnvironmentVariable}' is required for design-time operations. " +
                $"Export it in your shell before running EF migrations.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<GarageFlowDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new GarageFlowDbContext(optionsBuilder.Options);
    }
}
