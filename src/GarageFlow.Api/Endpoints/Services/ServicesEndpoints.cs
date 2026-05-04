using GarageFlow.Api.DTOs.Services;
using GarageFlow.Application.Services;
using GarageFlow.Application.Services.Commands;
using GarageFlow.Application.Services.Handlers;
using GarageFlow.Application.Services.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Endpoints.Services;

public static class ServicesEndpoints
{
    public static IEndpointRouteBuilder MapServiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/services").WithTags("Services");

        group.MapPost("/", CreateService)
            .WithName("CreateService")
            .WithSummary("Cria um novo serviço.")
            .Produces<ServiceResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetServiceById)
            .WithName("GetServiceById")
            .WithSummary("Consulta serviço por Id.")
            .Produces<ServiceResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListServices)
            .WithName("ListServices")
            .WithSummary("Lista serviços com paginação.")
            .Produces<PagedServiceResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateService)
            .WithName("UpdateService")
            .WithSummary("Atualiza dados do serviço.")
            .Produces<ServiceResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeactivateService)
            .WithName("DeactivateService")
            .WithSummary("Desativa serviço (soft delete).")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/parts", AddServicePart)
            .WithName("AddServicePart")
            .WithSummary("Adiciona peça ao serviço.")
            .Produces<ServiceResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/parts/{partId:guid}", RemoveServicePart)
            .WithName("RemoveServicePart")
            .WithSummary("Remove peça do serviço.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateService(
        CreateServiceRequest request,
        CreateServiceHandler handler,
        CancellationToken cancellationToken)
    {
        try
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
        catch (DuplicateServiceDataException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Title = "Erro de validação", Detail = ex.Message, Status = 400 });
        }
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
                Detail = DomainErrorMessages.InvalidPaginationParameters,
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
        try
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
        catch (DuplicateServiceDataException ex)
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

    private static async Task<IResult> DeactivateService(
        Guid id,
        DeactivateServiceHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleAsync(new DeactivateServiceCommand(id), cancellationToken);
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

    private static async Task<IResult> AddServicePart(
        Guid id,
        AddServicePartRequest request,
        AddServicePartHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AddServicePartCommand(id, request.PartId, request.Quantity);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (DuplicateServicePartException ex)
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

    private static async Task<IResult> RemoveServicePart(
        Guid id,
        Guid partId,
        RemoveServicePartHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleAsync(new RemoveServicePartCommand(id, partId), cancellationToken);
            return Results.NoContent();
        }
        catch (EntityNotFoundException)
        {
            return Results.NotFound();
        }
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
        dto.Parts.Select(p => new ServicePartResponse(p.PartId, p.PartName, p.Quantity)).ToList());
}
