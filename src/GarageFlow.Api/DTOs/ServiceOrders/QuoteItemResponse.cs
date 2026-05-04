using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Api.DTOs.ServiceOrders;

public sealed record QuoteItemResponse(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    decimal LaborPrice,
    decimal PartsTotal,
    decimal SuppliesTotal,
    decimal Subtotal);
