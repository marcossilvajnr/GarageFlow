using GarageFlow.Application.Customers.Handlers;
using GarageFlow.Application.Employees.Handlers;
using GarageFlow.Application.Suppliers.Handlers;
using GarageFlow.Application.Vehicles.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateCustomerHandler>();
        services.AddScoped<UpdateCustomerHandler>();
        services.AddScoped<DeactivateCustomerHandler>();
        services.AddScoped<GetCustomerByIdHandler>();
        services.AddScoped<ListCustomersHandler>();

        services.AddScoped<CreateVehicleHandler>();
        services.AddScoped<UpdateVehicleHandler>();
        services.AddScoped<DeactivateVehicleHandler>();
        services.AddScoped<GetVehicleByIdHandler>();
        services.AddScoped<ListVehiclesHandler>();

        services.AddScoped<CreateSupplierHandler>();
        services.AddScoped<UpdateSupplierHandler>();
        services.AddScoped<DeactivateSupplierHandler>();
        services.AddScoped<GetSupplierByIdHandler>();
        services.AddScoped<ListSuppliersHandler>();

        services.AddScoped<CreateEmployeeHandler>();
        services.AddScoped<UpdateEmployeeHandler>();
        services.AddScoped<DeactivateEmployeeHandler>();
        services.AddScoped<GetEmployeeByIdHandler>();
        services.AddScoped<ListEmployeesHandler>();

        return services;
    }
}
