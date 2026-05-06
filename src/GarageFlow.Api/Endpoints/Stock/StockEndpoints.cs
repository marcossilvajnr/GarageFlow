using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Application.Stock;
using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Handlers;
using GarageFlow.Application.Stock.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Endpoints.Stock;

public static class StockEndpoints
{
    public static IEndpointRouteBuilder MapStockEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/stock").WithTags("Stock");

        group.MapPost("/entries", CreateStockEntry)
            .WithName("CreateStockEntry")
            .WithSummary("Registra entrada de estoque.")
            .Produces<StockPositionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/reservations", ReserveStock)
            .WithName("ReserveStock")
            .WithSummary("Reserva quantidade no estoque.")
            .Produces<StockPositionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/releases", ReleaseStockReservation)
            .WithName("ReleaseStockReservation")
            .WithSummary("Libera reserva de estoque.")
            .RequireAuthorization("Administrative")
            .Produces<StockPositionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/consumptions", ConsumeStock)
            .WithName("ConsumeStock")
            .WithSummary("Consome quantidade reservada de estoque.")
            .Produces<StockPositionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/adjustments", AdjustStock)
            .WithName("AdjustStock")
            .WithSummary("Ajusta saldo de estoque com motivo.")
            .Produces<StockPositionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{itemType}/{itemId:guid}", GetStockPosition)
            .WithName("GetStockPosition")
            .WithSummary("Consulta posição atual de estoque por item.")
            .Produces<StockPositionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/{itemType}/{itemId:guid}/operations", ListStockOperations)
            .WithName("ListStockOperations")
            .WithSummary("Lista extrato de operações de estoque por item.")
            .Produces<PagedStockOperationsResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateStockEntry(
        CreateStockEntryRequest request,
        CreateStockEntryHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateStockEntryCommand(
                request.ItemId,
                request.ItemType,
                request.Quantity,
                request.MinimumQuantity,
                request.Reason,
                request.ReferenceId);

            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(ToPositionResponse(dto));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(ToProblem("Não encontrado", ex.Message, 404));
        }
        catch (DuplicateStockDataException ex)
        {
            return Results.Conflict(ToProblem("Conflito", ex.Message, 409));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(ToProblem("Erro de validação", ex.Message, 400));
        }
    }

    private static async Task<IResult> ReserveStock(
        ReserveStockRequest request,
        ReserveStockHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(
                new ReserveStockCommand(request.ItemId, request.ItemType, request.Quantity, request.Reason, request.ReferenceId),
                cancellationToken);

            return Results.Ok(ToPositionResponse(dto));
        }
        catch (StockQuantityConflictException ex)
        {
            return Results.Conflict(ToProblem("Conflito", ex.Message, 409));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(ToProblem("Não encontrado", ex.Message, 404));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(ToProblem("Erro de validação", ex.Message, 400));
        }
    }

    private static async Task<IResult> ReleaseStockReservation(
        ReleaseStockReservationRequest request,
        ReleaseStockReservationHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(
                new ReleaseStockReservationCommand(
                    request.ItemId,
                    request.ItemType,
                    request.Quantity,
                    request.Reason,
                    request.PerformedBy,
                    request.ReferenceId,
                    request.ReferenceType),
                cancellationToken);

            return Results.Ok(ToPositionResponse(dto));
        }
        catch (StockQuantityConflictException ex)
        {
            return Results.Conflict(ToProblem("Conflito", ex.Message, 409));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(ToProblem("Não encontrado", ex.Message, 404));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(ToProblem("Erro de validação", ex.Message, 400));
        }
    }

    private static async Task<IResult> ConsumeStock(
        ConsumeStockRequest request,
        ConsumeStockHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(
                new ConsumeStockCommand(request.ItemId, request.ItemType, request.Quantity, request.Reason, request.ReferenceId),
                cancellationToken);

            return Results.Ok(ToPositionResponse(dto));
        }
        catch (StockQuantityConflictException ex)
        {
            return Results.Conflict(ToProblem("Conflito", ex.Message, 409));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(ToProblem("Não encontrado", ex.Message, 404));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(ToProblem("Erro de validação", ex.Message, 400));
        }
    }

    private static async Task<IResult> AdjustStock(
        AdjustStockRequest request,
        AdjustStockHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(
                new AdjustStockCommand(request.ItemId, request.ItemType, request.QuantityDelta, request.Reason, request.ReferenceId),
                cancellationToken);

            return Results.Ok(ToPositionResponse(dto));
        }
        catch (StockQuantityConflictException ex)
        {
            return Results.Conflict(ToProblem("Conflito", ex.Message, 409));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(ToProblem("Não encontrado", ex.Message, 404));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(ToProblem("Erro de validação", ex.Message, 400));
        }
    }

    private static async Task<IResult> GetStockPosition(
        StockItemType itemType,
        Guid itemId,
        GetStockPositionHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await handler.HandleAsync(new GetStockPositionQuery(itemId, itemType), cancellationToken);
            return Results.Ok(ToPositionResponse(dto));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(ToProblem("Não encontrado", ex.Message, 404));
        }
    }

    private static async Task<IResult> ListStockOperations(
        StockItemType itemType,
        Guid itemId,
        ListStockOperationsHandler handler,
        CancellationToken cancellationToken,
        DateTime? from = null,
        DateTime? to = null,
        int page = StockPaginationDefaults.DefaultPage,
        int pageSize = StockPaginationDefaults.DefaultPageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > StockPaginationDefaults.MaxPageSize)
        {
            return Results.BadRequest(ToProblem("Erro de validação", DomainErrorMessages.InvalidPaginationParameters, 400));
        }

        var query = new ListStockOperationsQuery(itemId, itemType, from, to, page, pageSize);

        try
        {
            var result = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(new PagedStockOperationsResponse(
                result.Items.Select(ToOperationResponse).ToList(),
                result.Page,
                result.PageSize,
                result.TotalCount));
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(ToProblem("Não encontrado", ex.Message, 404));
        }
    }

    private static ProblemDetails ToProblem(string title, string detail, int status)
        => new() { Title = title, Detail = detail, Status = status };

    private static StockPositionResponse ToPositionResponse(StockPositionDto dto) =>
        new(
            dto.StockId,
            dto.ItemId,
            dto.ItemType,
            dto.TotalQuantity,
            dto.ReservedQuantity,
            dto.AvailableQuantity,
            dto.MinimumQuantity,
            dto.CreatedAt,
            dto.UpdatedAt);

    private static StockOperationResponse ToOperationResponse(StockOperationDto dto) =>
        new(dto.Id, dto.Type, dto.Quantity, dto.Reason, dto.ReferenceId, dto.ReferenceType, dto.PerformedBy, dto.CreatedAt);
}
