using GarageFlow.Api.Auth.Endpoints;
using GarageFlow.Api.Customers.Endpoints;
using GarageFlow.Api.Development.Endpoints;
using GarageFlow.Api.Employees.Endpoints;
using GarageFlow.Api.Executions.Endpoints;
using GarageFlow.Api.Health.Endpoints;
using GarageFlow.Api.Parts.Endpoints;
using GarageFlow.Api.Purchasing.Endpoints;
using GarageFlow.Api.ServiceOrders.Endpoints;
using GarageFlow.Api.ServiceOrders.External;
using GarageFlow.Api.Services.Endpoints;
using GarageFlow.Api.Stock.Endpoints;
using GarageFlow.Api.Suppliers.Endpoints;
using GarageFlow.Api.Supplies.Endpoints;
using GarageFlow.Api.Vehicles.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace GarageFlow.Api;

public static class GarageFlowApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapGarageFlowApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthEndpoints();
        endpoints.MapAuthEndpoints();
        endpoints.MapCustomerEndpoints();
        endpoints.MapVehicleEndpoints();
        endpoints.MapSupplierEndpoints();
        endpoints.MapEmployeeEndpoints();
        endpoints.MapServiceEndpoints();
        endpoints.MapPartEndpoints();
        endpoints.MapSupplyEndpoints();
        endpoints.MapServiceOrderEndpoints();
        endpoints.MapExternalServiceOrderQuoteEndpoints();
        endpoints.MapStockEndpoints();
        endpoints.MapSeparationOrderEndpoints();
        endpoints.MapExecutionOrderEndpoints();
        endpoints.MapPurchaseOrderEndpoints();

        return endpoints;
    }

    public static IEndpointRouteBuilder MapGarageFlowDevelopmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDevelopmentDatabaseEndpoints();

        return endpoints;
    }
}
