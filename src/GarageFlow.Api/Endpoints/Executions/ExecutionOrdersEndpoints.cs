using GarageFlow.Api.DTOs.Executions;
using GarageFlow.Application.Executions;
using GarageFlow.Application.Executions.Commands;
using GarageFlow.Application.Executions.DTOs;
using GarageFlow.Application.Executions.Handlers;
using GarageFlow.Application.Executions.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Endpoints.Executions;

public static class ExecutionOrdersEndpoints
{
    public static IEndpointRouteBuilder MapExecutionOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/execution-orders").WithTags("ExecutionOrders");

        group.MapPost("/", CreateExecutionOrder)
            .WithName("CreateExecutionOrder")
            .WithSummary("Cria uma nova Ordem de Execução.")
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
            .Produces<ExecutionOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/start", StartExecutionOrder)
            .WithName("StartExecutionOrder")
            .WithSummary("Inicia a execução da Ordem de Execução.")
            .Produces<ExecutionOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/complete", CompleteExecutionOrder)
            .WithName("CompleteExecutionOrder")
            .WithSummary("Conclui a Ordem de Execução.")
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
        try
        {
            var command = new CreateExecutionOrderCommand(request.ServiceOrderId, request.ServiceId);
            var dto = await handler.HandleAsync(command, cancellationToken);
            var response = MapToResponse(dto);
            return Results.Created($"/execution-orders/{dto.Id}", response);
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> GetExecutionOrderById(
        Guid id,
        GetExecutionOrderByIdHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(new GetExecutionOrderByIdQuery(id), cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
    }

    private static async Task<IResult> ListExecutionOrders(
        ListExecutionOrdersHandler handler,
        CancellationToken cancellationToken,
        int page = ExecutionOrderPaginationDefaults.DefaultPage,
        int pageSize = ExecutionOrderPaginationDefaults.DefaultPageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > ExecutionOrderPaginationDefaults.MaxPageSize)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Parâmetros de paginação inválidos",
                Detail = DomainErrorMessages.InvalidPaginationParameters,
                Status = 400
            });
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
        try
        {
            var dto = await handler.HandleAsync(new MarkExecutionOrderReadyCommand(id), cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
    }

    private static async Task<IResult> StartExecutionOrder(
        Guid id,
        StartExecutionOrderRequest request,
        StartExecutionOrderHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new StartExecutionOrderCommand(id, request.MechanicId);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (InvalidExecutionOrderStatusTransitionException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito de estado", Detail = ex.Message, Status = 409 });
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

    private static async Task<IResult> CompleteExecutionOrder(
        Guid id,
        CompleteExecutionOrderHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(new CompleteExecutionOrderCommand(id), cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (StockQuantityConflictException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito de estoque", Detail = ex.Message, Status = 409 });
        }
        catch (SeparationOrderCustodyPreconditionException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Pré-condição de separação não atendida", Detail = ex.Message, Status = 409 });
        }
        catch (InvalidExecutionOrderStatusTransitionException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito de estado", Detail = ex.Message, Status = 409 });
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
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
