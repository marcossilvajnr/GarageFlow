using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Suppliers;
using GarageFlow.Domain.Supplies;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Infrastructure.Persistence.Configurations;
using GarageFlow.Infrastructure.Persistence.Configurations.Customers;
using GarageFlow.Infrastructure.Persistence.Configurations.Employees;
using GarageFlow.Infrastructure.Persistence.Configurations.Parts;
using GarageFlow.Infrastructure.Persistence.Configurations.Services;
using GarageFlow.Infrastructure.Persistence.Configurations.ServiceOrders;
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
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Supply> Supplies => Set<Supply>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceConfiguration());
        modelBuilder.ApplyConfiguration(new PartConfiguration());
        modelBuilder.ApplyConfiguration(new SupplyConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceOrderConfiguration());
    }
}
