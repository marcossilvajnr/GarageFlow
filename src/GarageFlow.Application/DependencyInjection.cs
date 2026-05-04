using GarageFlow.Application.Customers.Handlers;
using GarageFlow.Application.Employees.Handlers;
using GarageFlow.Application.Executions.Handlers;
using GarageFlow.Application.Parts.Handlers;
using GarageFlow.Application.ServiceOrders.Handlers;
using GarageFlow.Application.Services.Handlers;
using GarageFlow.Application.Stock.Handlers;
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
        services.AddScoped<AddServiceSupplyHandler>();
        services.AddScoped<RemoveServiceSupplyHandler>();

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

        services.AddScoped<CreateServiceOrderHandler>();
        services.AddScoped<GetServiceOrderByIdHandler>();
        services.AddScoped<ListServiceOrdersHandler>();
        services.AddScoped<AddServiceToServiceOrderHandler>();
        services.AddScoped<RemoveServiceFromServiceOrderHandler>();
        services.AddScoped<StartDiagnosticHandler>();
        services.AddScoped<AddDiagnosticServiceHandler>();
        services.AddScoped<RemoveDiagnosticServiceHandler>();
        services.AddScoped<CompleteDiagnosticHandler>();
        services.AddScoped<ConsolidateDiagnosticServicesHandler>();
        services.AddScoped<GenerateQuoteHandler>();
        services.AddScoped<AcceptQuoteHandler>();
        services.AddScoped<RejectQuoteHandler>();
        services.AddScoped<GetServiceOrderQuoteHandler>();

        services.AddScoped<CreateSeparationOrderHandler>();
        services.AddScoped<GetSeparationOrderByIdHandler>();
        services.AddScoped<ListSeparationOrdersHandler>();
        services.AddScoped<ReserveSeparationOrderHandler>();
        services.AddScoped<WaitSeparationOrderPurchaseHandler>();
        services.AddScoped<ResumeSeparationOrderAfterPurchaseHandler>();
        services.AddScoped<ConfirmSeparationStockistWithdrawalHandler>();
        services.AddScoped<ConfirmSeparationMechanicReceiptHandler>();

        services.AddScoped<CreateExecutionOrderHandler>();
        services.AddScoped<GetExecutionOrderByIdHandler>();
        services.AddScoped<ListExecutionOrdersHandler>();
        services.AddScoped<MarkExecutionOrderReadyHandler>();
        services.AddScoped<StartExecutionOrderHandler>();
        services.AddScoped<CompleteExecutionOrderHandler>();

        return services;
    }
}
