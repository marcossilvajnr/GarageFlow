using GarageFlow.Api.Services.DTOs;
using GarageFlow.Application.Services;
using GarageFlow.Application.Services.Commands;
using GarageFlow.Application.Services.Handlers;
using GarageFlow.Application.Services.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Services.Endpoints;

public static class ServicesEndpoints
{
    private const string InvalidPaginationParameters = "Parâmetros de paginação inválidos";

    public static IEndpointRouteBuilder MapServiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/services")
            .WithTags("Services")
            .RequireAuthorization("Administrative");

        group.MapPost("/", CreateService)
            .WithName("CreateService")
            .WithSummary("Cria um novo serviço.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetServiceById)
            .WithName("GetServiceById")
            .WithSummary("Consulta serviço por Id.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListServices)
            .WithName("ListServices")
            .WithSummary("Lista serviços com paginação.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PagedServiceResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateService)
            .WithName("UpdateService")
            .WithSummary("Atualiza dados do serviço.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeactivateService)
            .WithName("DeactivateService")
            .WithSummary("Desativa serviço (soft delete).")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/parts", AddServicePart)
            .WithName("AddServicePart")
            .WithSummary("Adiciona peça ao serviço.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/parts/{partId:guid}", RemoveServicePart)
            .WithName("RemoveServicePart")
            .WithSummary("Remove peça do serviço.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/supplies", AddServiceSupply)
            .WithName("AddServiceSupply")
            .WithSummary("Adiciona insumo ao serviço.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/supplies/{supplyId:guid}", RemoveServiceSupply)
            .WithName("RemoveServiceSupply")
            .WithSummary("Remove insumo do serviço.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateService(
        CreateServiceRequest request,
        CreateServiceHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCommand(
            request.Code,
            request.Name,
            request.Description,
            request.BasePrice,
            request.EstimatedDurationMinutes);

        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Created($"/services/{dto.Id}", MapToResponse(dto));
    }

    private static async Task<IResult> GetServiceById(
        Guid id,
        GetServiceByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetServiceByIdQuery(id), cancellationToken);
        return dto is null ? Results.NotFound() : Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListServices(
        ListServicesHandler handler,
        CancellationToken cancellationToken,
        int page = ServicesPaginationDefaults.DefaultPage,
        int pageSize = ServicesPaginationDefaults.DefaultPageSize)
    {
        if (page < 1 || pageSize < 1)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Erro de validação",
                Detail = InvalidPaginationParameters,
                Status = 400
            });
        }

        var result = await handler.HandleAsync(new ListServicesQuery(page, pageSize), cancellationToken);
        var response = new PagedServiceResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateService(
        Guid id,
        UpdateServiceRequest request,
        UpdateServiceHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCommand(
            id,
            request.Name,
            request.Description,
            request.BasePrice,
            request.EstimatedDurationMinutes);

        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> DeactivateService(
        Guid id,
        DeactivateServiceHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateServiceCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AddServicePart(
        Guid id,
        AddServicePartRequest request,
        AddServicePartHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new AddServicePartCommand(id, request.PartId, request.Quantity);
        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> RemoveServicePart(
        Guid id,
        Guid partId,
        RemoveServicePartHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new RemoveServicePartCommand(id, partId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AddServiceSupply(
        Guid id,
        AddServiceSupplyRequest request,
        AddServiceSupplyHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new AddServiceSupplyCommand(id, request.SupplyId, request.Quantity);
        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> RemoveServiceSupply(
        Guid id,
        Guid supplyId,
        RemoveServiceSupplyHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new RemoveServiceSupplyCommand(id, supplyId), cancellationToken);
        return Results.NoContent();
    }

    private static ServiceResponse MapToResponse(Application.Services.DTOs.ServiceDto dto) => new(
        dto.Id,
        dto.Code,
        dto.Name,
        dto.Description,
        dto.BasePrice,
        dto.EstimatedDurationMinutes,
        dto.IsActive,
        dto.CreatedAt,
        dto.UpdatedAt,
        dto.Parts.Select(p => new ServicePartResponse(p.PartId, p.PartName, p.Quantity)).ToList(),
        dto.Supplies.Select(s => new ServiceSupplyResponse(s.SupplyId, s.SupplyName, s.Quantity, s.Unit)).ToList());
}
