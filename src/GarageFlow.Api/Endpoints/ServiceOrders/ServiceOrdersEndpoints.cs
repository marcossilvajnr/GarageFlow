using GarageFlow.Api.DTOs.ServiceOrders;
using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Application.ServiceOrders.Handlers;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Endpoints.ServiceOrders;

public static class ServiceOrdersEndpoints
{
    public static IEndpointRouteBuilder MapServiceOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/service-orders").WithTags("ServiceOrders");

        group.MapPost("/", CreateServiceOrder)
            .WithName("CreateServiceOrder")
            .WithSummary("Cria uma nova Ordem de Serviço.")
            .Produces<ServiceOrderResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetServiceOrderById)
            .WithName("GetServiceOrderById")
            .WithSummary("Consulta Ordem de Serviço por Id.")
            .Produces<ServiceOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", ListServiceOrders)
            .WithName("ListServiceOrders")
            .WithSummary("Lista Ordens de Serviço com paginação.")
            .Produces<PagedServiceOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateServiceOrder(
        CreateServiceOrderRequest request,
        CreateServiceOrderHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateServiceOrderCommand(request.CustomerId, request.VehicleId);
            var dto = await handler.HandleAsync(command, cancellationToken);
            var response = MapToResponse(dto);
            return Results.Created($"/service-orders/{dto.Id}", response);
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> GetServiceOrderById(
        Guid id,
        GetServiceOrderByIdHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(new GetServiceOrderByIdQuery(id), cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
    }

    private static async Task<IResult> ListServiceOrders(
        ListServiceOrdersHandler handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Parâmetros de paginação inválidos",
                Detail = DomainErrorMessages.InvalidPaginationParameters,
                Status = 400
            });
        }

        var result = await handler.HandleAsync(new ListServiceOrdersQuery(page, pageSize), cancellationToken);
        var response = new PagedServiceOrderResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static ServiceOrderResponse MapToResponse(ServiceOrderDto dto) =>
        new(dto.Id, dto.CustomerId, dto.VehicleId, dto.Status, dto.CreatedAt, dto.UpdatedAt);
}
