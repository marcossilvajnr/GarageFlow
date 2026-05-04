using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Suppliers;
using GarageFlow.Domain.Supplies;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Infrastructure.Persistence;
using GarageFlow.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "GarageFlow";
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__GarageFlow";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' was not found. " +
                $"Set the '{ConnectionStringEnvironmentVariable}' environment variable.");

        services.AddDbContext<GarageFlowDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IPartRepository, PartRepository>();
        services.AddScoped<ISupplyRepository, SupplyRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();

        return services;
    }
}
