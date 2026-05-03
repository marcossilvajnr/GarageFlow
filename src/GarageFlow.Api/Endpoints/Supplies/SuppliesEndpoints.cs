using GarageFlow.Api.DTOs.Supplies;
using GarageFlow.Application.Supplies;
using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Application.Supplies.Handlers;
using GarageFlow.Application.Supplies.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Endpoints.Supplies;

public static class SuppliesEndpoints
{
    public static IEndpointRouteBuilder MapSupplyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/supplies").WithTags("Supplies");

        group.MapPost("/", CreateSupply)
            .WithName("CreateSupply")
            .WithSummary("Cria um novo insumo.")
            .Produces<SupplyResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetSupplyById)
            .WithName("GetSupplyById")
            .WithSummary("Consulta insumo por Id.")
            .Produces<SupplyResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListSupplies)
            .WithName("ListSupplies")
            .WithSummary("Lista insumos com paginação.")
            .Produces<PagedSupplyResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateSupply)
            .WithName("UpdateSupply")
            .WithSummary("Atualiza dados do insumo.")
            .Produces<SupplyResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeactivateSupply)
            .WithName("DeactivateSupply")
            .WithSummary("Desativa insumo (soft delete).")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateSupply(
        CreateSupplyRequest request,
        CreateSupplyHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateSupplyCommand(
                request.Name,
                request.Code,
                request.UnitOfMeasure,
                request.BaseCost,
                request.PreferredSupplierId);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Created($"/supplies/{dto.Id}", MapToResponse(dto));
        }
        catch (DuplicateSupplyDataException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> GetSupplyById(
        Guid id,
        GetSupplyByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetSupplyByIdQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListSupplies(
        ListSuppliesHandler handler,
        CancellationToken cancellationToken,
        int page = SuppliesPaginationDefaults.DefaultPage,
        int pageSize = SuppliesPaginationDefaults.DefaultPageSize)
    {
        if (page < 1 || pageSize < 1)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Erro de validação",
                Detail = DomainErrorMessages.InvalidPaginationParameters,
                Status = 400
            });
        }

        var result = await handler.HandleAsync(new ListSuppliesQuery(page, pageSize), cancellationToken);
        var response = new PagedSupplyResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateSupply(
        Guid id,
        UpdateSupplyRequest request,
        UpdateSupplyHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateSupplyCommand(id, request.Name, request.UnitOfMeasure, request.BaseCost, request.PreferredSupplierId);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (DuplicateSupplyDataException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> DeactivateSupply(
        Guid id,
        DeactivateSupplyHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleAsync(new DeactivateSupplyCommand(id), cancellationToken);
            return Results.NoContent();
        }
        catch (EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static SupplyResponse MapToResponse(Application.Supplies.DTOs.SupplyDto dto) => new(
        dto.Id,
        dto.Name,
        dto.Code,
        dto.UnitOfMeasure,
        dto.BaseCost,
        dto.PreferredSupplierId,
        dto.IsActive,
        dto.CreatedAt,
        dto.UpdatedAt);
}
