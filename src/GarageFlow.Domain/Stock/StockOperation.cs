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
    public DateTime CreatedAt { get; private set; }

    private StockOperation() { }

    internal static StockOperation Create(
        StockOperationType type,
        decimal quantity,
        string? reason,
        Guid? referenceId)
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

        return new StockOperation
        {
            Type = type,
            Quantity = quantity,
            Reason = normalizedReason,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
