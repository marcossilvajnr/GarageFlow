using GarageFlow.Application.Customers.Handlers;
using GarageFlow.Application.Employees.Handlers;
using GarageFlow.Application.Parts.Handlers;
using GarageFlow.Application.Services.Handlers;
using GarageFlow.Application.Suppliers.Handlers;
using GarageFlow.Application.Supplies.Handlers;
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

        services.AddScoped<CreateServiceHandler>();
        services.AddScoped<UpdateServiceHandler>();
        services.AddScoped<DeactivateServiceHandler>();
        services.AddScoped<GetServiceByIdHandler>();
        services.AddScoped<ListServicesHandler>();
        services.AddScoped<AddServicePartHandler>();
        services.AddScoped<RemoveServicePartHandler>();

        services.AddScoped<CreatePartHandler>();
        services.AddScoped<UpdatePartHandler>();
        services.AddScoped<DeactivatePartHandler>();
        services.AddScoped<GetPartByIdHandler>();
        services.AddScoped<ListPartsHandler>();

        services.AddScoped<CreateSupplyHandler>();
        services.AddScoped<UpdateSupplyHandler>();
        services.AddScoped<DeactivateSupplyHandler>();
        services.AddScoped<GetSupplyByIdHandler>();
        services.AddScoped<ListSuppliesHandler>();

        return services;
    }
}
