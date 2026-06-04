namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record QuoteItemResponse(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    decimal LaborPrice,
    decimal PartsTotal,
    decimal SuppliesTotal,
    decimal Subtotal);
