using GarageFlow.Api.DTOs.Parts;
using GarageFlow.Application.Parts;
using GarageFlow.Application.Parts.Commands;
using GarageFlow.Application.Parts.Handlers;
using GarageFlow.Application.Parts.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Endpoints.Parts;

public static class PartsEndpoints
{
    public static IEndpointRouteBuilder MapPartEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/parts")
            .WithTags("Parts")
            .RequireAuthorization("Administrative");

        group.MapPost("/", CreatePart)
            .WithName("CreatePart")
            .WithSummary("Cria uma nova peça.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PartResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetPartById)
            .WithName("GetPartById")
            .WithSummary("Consulta peça por Id.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PartResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListParts)
            .WithName("ListParts")
            .WithSummary("Lista peças com paginação.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PagedPartResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdatePart)
            .WithName("UpdatePart")
            .WithSummary("Atualiza dados da peça.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PartResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeactivatePart)
            .WithName("DeactivatePart")
            .WithSummary("Desativa peça (soft delete).")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreatePart(
        CreatePartRequest request,
        CreatePartHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreatePartCommand(
                request.Name,
                request.Code,
                request.Sku,
                request.UnitOfMeasure,
                request.UnitPrice);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Created($"/parts/{dto.Id}", MapToResponse(dto));
        }
        catch (DuplicatePartDataException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
    }

    private static async Task<IResult> GetPartById(
        Guid id,
        GetPartByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetPartByIdQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListParts(
        ListPartsHandler handler,
        CancellationToken cancellationToken,
        int page = PartsPaginationDefaults.DefaultPage,
        int pageSize = PartsPaginationDefaults.DefaultPageSize)
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

        var result = await handler.HandleAsync(new ListPartsQuery(page, pageSize), cancellationToken);
        var response = new PagedPartResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdatePart(
        Guid id,
        UpdatePartRequest request,
        UpdatePartHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdatePartCommand(id, request.Name, request.UnitOfMeasure, request.UnitPrice);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (DuplicatePartDataException ex)
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

    private static async Task<IResult> DeactivatePart(
        Guid id,
        DeactivatePartHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleAsync(new DeactivatePartCommand(id), cancellationToken);
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

    private static PartResponse MapToResponse(Application.Parts.DTOs.PartDto dto) => new(
        dto.Id,
        dto.Name,
        dto.Code,
        dto.Sku,
        dto.UnitOfMeasure,
        dto.UnitPrice,
        dto.IsActive,
        dto.CreatedAt,
        dto.UpdatedAt);
}
