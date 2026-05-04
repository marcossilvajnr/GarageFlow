namespace GarageFlow.Application.Purchasing.Commands;

public sealed record CreatePurchaseOrderCommand(
    IReadOnlyList<Guid> SeparationOrderIds,
    IReadOnlyList<CreatePurchaseItemCommand> Items);
