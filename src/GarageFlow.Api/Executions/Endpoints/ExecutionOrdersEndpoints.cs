using GarageFlow.Api.Common.Pagination;
using GarageFlow.Api.Executions.DTOs;
using GarageFlow.Application.Executions.Commands;
using GarageFlow.Application.Executions.DTOs;
using GarageFlow.Application.Executions.Handlers;
using GarageFlow.Application.Executions.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Executions.Endpoints;

public static class ExecutionOrdersEndpoints
{
    public static IEndpointRouteBuilder MapExecutionOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/execution-orders").WithTags("ExecutionOrders");

        group.MapPost("/", CreateExecutionOrder)
            .WithName("CreateExecutionOrder")
            .WithSummary("Cria uma nova Ordem de Execução.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<ExecutionOrderResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetExecutionOrderById)
            .WithName("GetExecutionOrderById")
            .WithSummary("Consulta Ordem de Execução por Id.")
            .Produces<ExecutionOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", ListExecutionOrders)
            .WithName("ListExecutionOrders")
            .WithSummary("Lista Ordens de Execução com paginação.")
            .Produces<PagedExecutionOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/mark-ready", MarkExecutionOrderReady)
            .WithName("MarkExecutionOrderReady")
            .WithSummary("Marca a Ordem de Execução como pronta para início.")
            .RequireAuthorization("StockistOrAdministrative")
            .Produces<ExecutionOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/start", StartExecutionOrder)
            .WithName("StartExecutionOrder")
            .WithSummary("Inicia a execução da Ordem de Execução.")
            .RequireAuthorization("MechanicOrAdministrative")
            .Produces<ExecutionOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/complete", CompleteExecutionOrder)
            .WithName("CompleteExecutionOrder")
            .WithSummary("Conclui a Ordem de Execução.")
            .RequireAuthorization("MechanicOrAdministrative")
            .Produces<ExecutionOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> CreateExecutionOrder(
        CreateExecutionOrderRequest request,
        CreateExecutionOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateExecutionOrderCommand(request.ServiceOrderId, request.ServiceId, request.MechanicId);
        var dto = await handler.HandleAsync(command, cancellationToken);
        var response = MapToResponse(dto);
        return Results.Created($"/execution-orders/{dto.Id}", response);
    }

    private static async Task<IResult> GetExecutionOrderById(
        Guid id,
        GetExecutionOrderByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetExecutionOrderByIdQuery(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListExecutionOrders(
        ListExecutionOrdersHandler handler,
        CancellationToken cancellationToken,
        int page = ApiPagination.DefaultPage,
        int pageSize = ApiPagination.DefaultPageSize)
    {
        if (!ApiPagination.IsValid(page, pageSize))
        {
            return Results.BadRequest(ApiPagination.CreateInvalidPaginationProblemDetails());
        }

        var result = await handler.HandleAsync(new ListExecutionOrdersQuery(page, pageSize), cancellationToken);
        var response = new PagedExecutionOrderResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize);

        return Results.Ok(response);
    }

    private static async Task<IResult> MarkExecutionOrderReady(
        Guid id,
        MarkExecutionOrderReadyHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new MarkExecutionOrderReadyCommand(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> StartExecutionOrder(
        Guid id,
        StartExecutionOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new StartExecutionOrderCommand(id);
        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> CompleteExecutionOrder(
        Guid id,
        CompleteExecutionOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new CompleteExecutionOrderCommand(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static ExecutionOrderResponse MapToResponse(ExecutionOrderDto dto) =>
        new(
            dto.Id,
            dto.ServiceOrderId,
            dto.ServiceId,
            dto.MechanicId,
            dto.Status,
            dto.StartedAt,
            dto.CompletedAt,
            dto.ActualTimeMinutes,
            dto.CreatedAt);
}
