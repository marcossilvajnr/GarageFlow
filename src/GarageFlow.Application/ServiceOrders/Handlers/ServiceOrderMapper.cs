using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Application.ServiceOrders.Mappers;
using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Application.ServiceOrders.Handlers;

internal static class ServiceOrderMapper
{
    internal static ServiceOrderDto ToDto(ServiceOrder serviceOrder) =>
        new(
            serviceOrder.Id,
            serviceOrder.CustomerId,
            serviceOrder.VehicleId,
            serviceOrder.FrontDeskEmployeeId,
            ServiceOrderStatusMapper.ToApplication(serviceOrder.Status),
            serviceOrder.Diagnostic is not null ? ToDiagnosticDto(serviceOrder.Diagnostic) : null,
            serviceOrder.Quote is not null ? ToQuoteDto(serviceOrder.Quote) : null,
            serviceOrder.CreatedAt,
            serviceOrder.UpdatedAt,
            serviceOrder.Services.Select(ToServiceItemDto).ToList(),
            serviceOrder.ServiceHistory.Select(ToServiceHistoryDto).ToList());

    internal static QuoteDto ToQuoteDto(Quote quote) =>
        new(
            quote.Id,
            quote.ServiceOrderId,
            quote.Items.Select(ToQuoteItemDto).ToList(),
            quote.TotalAmount,
            QuoteStatusMapper.ToApplication(quote.Status),
            quote.GeneratedAt,
            quote.AcceptedAt,
            quote.RejectedAt,
            quote.RejectionReason);

    private static QuoteItemDto ToQuoteItemDto(QuoteItem item) =>
        new(
            item.Id,
            item.ServiceId,
            item.ServiceName,
            item.LaborPrice,
            item.PartsTotal,
            item.SuppliesTotal,
            item.Subtotal);

    private static DiagnosticDto ToDiagnosticDto(Diagnostic diagnostic) =>
        new(
            diagnostic.Id,
            diagnostic.MechanicId,
            diagnostic.Description,
            diagnostic.SelectedServices,
            diagnostic.StartedAt,
            diagnostic.CompletedAt,
            DiagnosticStatusMapper.ToApplication(diagnostic.Status));

    private static ServiceOrderServiceItemDto ToServiceItemDto(ServiceOrderServiceItem item) =>
        new(
            item.Id,
            item.ServiceId,
            ServiceSourceMapper.ToApplication(item.Source),
            item.AddedByActorId,
            item.AddedAt,
            item.IsActive,
            item.RemovedAt,
            item.RemovedByActorId,
            item.RemovalReason);

    private static ServiceOrderServiceHistoryDto ToServiceHistoryDto(ServiceOrderServiceHistory history) =>
        new(
            history.Id,
            history.ServiceId,
            ServiceOrderServiceActionMapper.ToApplication(history.Action),
            ServiceSourceMapper.ToApplication(history.Source),
            history.ActorId,
            history.OccurredAt,
            history.Reason);
}
