using GarageFlow.Domain.Customers;
using GarageFlow.Infrastructure.Persistence.Configurations.Customers;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Infrastructure.Persistence;

public sealed class GarageFlowDbContext(DbContextOptions<GarageFlowDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
