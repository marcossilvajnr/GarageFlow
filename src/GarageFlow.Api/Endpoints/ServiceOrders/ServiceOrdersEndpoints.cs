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

        group.MapPost("/{id:guid}/services", AddServiceToServiceOrder)
            .WithName("AddServiceToServiceOrder")
            .WithSummary("Adiciona um serviço à Ordem de Serviço (FrontDesk).")
            .Produces<ServiceOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/services/{serviceId:guid}", RemoveServiceFromServiceOrder)
            .WithName("RemoveServiceFromServiceOrder")
            .WithSummary("Remove um serviço da Ordem de Serviço (FrontDesk).")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/diagnostic/start", StartDiagnostic)
            .WithName("StartDiagnostic")
            .WithSummary("Inicia o diagnóstico da Ordem de Serviço.")
            .Produces<ServiceOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/diagnostic/services", AddDiagnosticService)
            .WithName("AddDiagnosticService")
            .WithSummary("Adiciona um serviço ao diagnóstico da Ordem de Serviço.")
            .Produces<ServiceOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/diagnostic/services/{serviceId:guid}", RemoveDiagnosticService)
            .WithName("RemoveDiagnosticService")
            .WithSummary("Remove um serviço do diagnóstico da Ordem de Serviço.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/diagnostic/complete", CompleteDiagnostic)
            .WithName("CompleteDiagnostic")
            .WithSummary("Conclui o diagnóstico da Ordem de Serviço.")
            .Produces<ServiceOrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

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

    private static async Task<IResult> AddServiceToServiceOrder(
        Guid id,
        AddServiceToServiceOrderRequest request,
        AddServiceToServiceOrderHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AddServiceToServiceOrderCommand(id, request.ServiceId, request.ActorId);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (DuplicateServiceOrderServiceException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
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

    private static async Task<IResult> RemoveServiceFromServiceOrder(
        Guid id,
        Guid serviceId,
        [FromBody] RemoveServiceFromServiceOrderRequest request,
        RemoveServiceFromServiceOrderHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RemoveServiceFromServiceOrderCommand(id, serviceId, request.ActorId, request.Reason);
            await handler.HandleAsync(command, cancellationToken);
            return Results.NoContent();
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

    private static async Task<IResult> StartDiagnostic(
        Guid id,
        StartDiagnosticRequest request,
        StartDiagnosticHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new StartDiagnosticCommand(id, request.MechanicId);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (DiagnosticAlreadyStartedException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
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

    private static async Task<IResult> AddDiagnosticService(
        Guid id,
        AddDiagnosticServiceRequest request,
        AddDiagnosticServiceHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AddDiagnosticServiceCommand(id, request.ServiceId);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (DiagnosticNotInProgressException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (DuplicateDiagnosticServiceException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
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

    private static async Task<IResult> RemoveDiagnosticService(
        Guid id,
        Guid serviceId,
        RemoveDiagnosticServiceHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RemoveDiagnosticServiceCommand(id, serviceId);
            await handler.HandleAsync(command, cancellationToken);
            return Results.NoContent();
        }
        catch (DiagnosticNotInProgressException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (DiagnosticLastServiceException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new ProblemDetails { Title = "Não encontrado", Detail = ex.Message, Status = 404 });
        }
    }

    private static async Task<IResult> CompleteDiagnostic(
        Guid id,
        CompleteDiagnosticRequest request,
        CompleteDiagnosticHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CompleteDiagnosticCommand(id, request.Description);
            var dto = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(MapToResponse(dto));
        }
        catch (DiagnosticNotInProgressException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
        }
        catch (DiagnosticNoServicesException ex)
        {
            return Results.Conflict(new ProblemDetails { Title = "Conflito", Detail = ex.Message, Status = 409 });
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

    private static ServiceOrderResponse MapToResponse(ServiceOrderDto dto) =>
        new(
            dto.Id,
            dto.CustomerId,
            dto.VehicleId,
            dto.Status,
            dto.Diagnostic is not null ? MapToDiagnosticResponse(dto.Diagnostic) : null,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.Services.Select(MapToServiceResponse).ToList(),
            dto.ServiceHistory.Select(MapToServiceHistoryResponse).ToList());

    private static DiagnosticResponse MapToDiagnosticResponse(DiagnosticDto dto) =>
        new(
            dto.Id,
            dto.MechanicId,
            dto.Description,
            dto.SelectedServices,
            dto.StartedAt,
            dto.CompletedAt,
            dto.Status);

    private static ServiceOrderServiceResponse MapToServiceResponse(ServiceOrderServiceItemDto dto) =>
        new(
            dto.Id,
            dto.ServiceId,
            dto.Source,
            dto.AddedByActorId,
            dto.AddedAt,
            dto.IsActive,
            dto.RemovedAt,
            dto.RemovedByActorId,
            dto.RemovalReason);

    private static ServiceOrderServiceHistoryResponse MapToServiceHistoryResponse(ServiceOrderServiceHistoryDto dto) =>
        new(
            dto.Id,
            dto.ServiceId,
            dto.Action,
            dto.Source,
            dto.ActorId,
            dto.OccurredAt,
            dto.Reason);
}
