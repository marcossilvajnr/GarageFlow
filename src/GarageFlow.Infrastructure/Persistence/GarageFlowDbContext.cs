using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Suppliers;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Infrastructure.Persistence.Configurations.Customers;
using GarageFlow.Infrastructure.Persistence.Configurations.Suppliers;
using GarageFlow.Infrastructure.Persistence.Configurations.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Infrastructure.Persistence;

public sealed class GarageFlowDbContext(DbContextOptions<GarageFlowDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
    }
}
