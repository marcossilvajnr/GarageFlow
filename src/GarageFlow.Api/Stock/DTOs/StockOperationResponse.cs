using GarageFlow.Domain.Stock;

namespace GarageFlow.Api.Stock.DTOs;

public sealed record StockOperationResponse(
    Guid Id,
    StockOperationType Type,
    decimal Quantity,
    string? Reason,
    Guid? ReferenceId,
    string? ReferenceType,
    string? PerformedBy,
    DateTime CreatedAt);
