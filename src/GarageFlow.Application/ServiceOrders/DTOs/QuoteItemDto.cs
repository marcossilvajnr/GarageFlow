namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record QuoteItemDto(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    decimal LaborPrice,
    decimal PartsTotal,
    decimal SuppliesTotal,
    decimal Subtotal);
