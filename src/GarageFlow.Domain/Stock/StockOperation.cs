using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Stock;

public sealed class StockOperation
{
    public Guid Id { get; private set; }
    public StockOperationType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public string? PerformedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StockOperation() { }

    internal static StockOperation Create(
        StockOperationType type,
        decimal quantity,
        string? reason,
        Guid? referenceId,
        string? performedBy = null,
        string? referenceType = null)
    {
        if (type == StockOperationType.Adjust)
        {
            if (quantity == 0)
                throw new DomainException(DomainErrorMessages.InvalidStockAdjustmentQuantity);
        }
        else
        {
            if (quantity <= 0)
                throw new DomainException(DomainErrorMessages.InvalidStockOperationQuantity);
        }

        var normalizedReason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();

        if (normalizedReason is { Length: > StockConstants.MaxReasonLength })
            throw new DomainException(DomainErrorMessages.InvalidStockOperationReason);

        var normalizedPerformedBy = string.IsNullOrWhiteSpace(performedBy)
            ? null
            : performedBy.Trim();

        if (normalizedPerformedBy is { Length: > StockConstants.MaxPerformedByLength })
            throw new DomainException(DomainErrorMessages.InvalidStockPerformedBy);

        var normalizedReferenceType = string.IsNullOrWhiteSpace(referenceType)
            ? null
            : referenceType.Trim();

        if (normalizedReferenceType is { Length: > StockConstants.MaxReferenceTypeLength })
            throw new DomainException(DomainErrorMessages.InvalidStockReferenceType);

        return new StockOperation
        {
            Id = Guid.NewGuid(),
            Type = type,
            Quantity = quantity,
            Reason = normalizedReason,
            ReferenceId = referenceId,
            ReferenceType = normalizedReferenceType,
            PerformedBy = normalizedPerformedBy,
            CreatedAt = DateTime.UtcNow
        };
    }
}
