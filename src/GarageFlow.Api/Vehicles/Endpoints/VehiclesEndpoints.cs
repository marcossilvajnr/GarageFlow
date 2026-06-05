using GarageFlow.Api.Vehicles.DTOs;
using GarageFlow.Application.Vehicles.Commands;
using GarageFlow.Application.Vehicles.Handlers;
using GarageFlow.Application.Vehicles.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Vehicles.Endpoints;

public static class VehiclesEndpoints
{
    private const int MinPage = 1;
    private const int MinPageSize = 1;
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;

    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/vehicles")
            .WithTags("Vehicles")
            .RequireAuthorization("Administrative");

        group.MapPost("/", CreateVehicle)
            .WithName("CreateVehicle")
            .WithSummary("Cria um novo veículo.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<VehicleResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetVehicleById)
            .WithName("GetVehicleById")
            .WithSummary("Consulta veículo por Id.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<VehicleResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListVehicles)
            .WithName("ListVehicles")
            .WithSummary("Lista veículos com paginação.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<PagedVehicleResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateVehicle)
            .WithName("UpdateVehicle")
            .WithSummary("Atualiza dados do veículo.")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<VehicleResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeactivateVehicle)
            .WithName("DeactivateVehicle")
            .WithSummary("Desativa veículo (soft delete).")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateVehicle(
        CreateVehicleRequest request,
        CreateVehicleHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateVehicleCommand(
            request.CustomerId,
            request.LicensePlate,
            request.Renavam,
            request.Make,
            request.Model,
            request.Year,
            request.Color);

        var dto = await handler.HandleAsync(command, cancellationToken);
        var response = MapToResponse(dto);
        return Results.Created($"/vehicles/{dto.Id}", response);
    }

    private static async Task<IResult> GetVehicleById(
        Guid id,
        GetVehicleByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = await handler.HandleAsync(new GetVehicleByIdQuery(id), cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> ListVehicles(
        ListVehiclesHandler handler,
        CancellationToken cancellationToken,
        Guid? customerId = null,
        int page = DefaultPage,
        int pageSize = DefaultPageSize)
    {
        if (page < MinPage || pageSize < MinPageSize)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Parâmetros de paginação inválidos",
                Detail = $"'page' deve ser >= {MinPage} e 'pageSize' deve ser >= {MinPageSize}",
                Status = 400
            });
        }

        var result = await handler.HandleAsync(new ListVehiclesQuery(customerId, page, pageSize), cancellationToken);
        var response = new PagedVehicleResponse(
            result.Items.Select(MapToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateVehicle(
        Guid id,
        UpdateVehicleRequest request,
        UpdateVehicleHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateVehicleCommand(id, request.Make, request.Model, request.Year, request.Color);
        var dto = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(MapToResponse(dto));
    }

    private static async Task<IResult> DeactivateVehicle(
        Guid id,
        DeactivateVehicleHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateVehicleCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static VehicleResponse MapToResponse(Application.Vehicles.DTOs.VehicleDto dto) => new(
        dto.Id,
        dto.CustomerId,
        dto.LicensePlate,
        dto.Renavam,
        dto.Make,
        dto.Model,
        dto.Year,
        dto.Color,
        dto.IsActive,
        dto.CreatedAt,
        dto.UpdatedAt);
}
