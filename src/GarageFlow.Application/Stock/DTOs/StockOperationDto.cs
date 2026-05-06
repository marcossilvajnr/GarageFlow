using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.DTOs;

public sealed record StockOperationDto(
    Guid Id,
    StockOperationType Type,
    decimal Quantity,
    string? Reason,
    Guid? ReferenceId,
    string? ReferenceType,
    string? PerformedBy,
    DateTime CreatedAt);
