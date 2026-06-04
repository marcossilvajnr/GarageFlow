namespace GarageFlow.Application.Stock.Commands;

public sealed record CreateSeparationOrderCommand(
    Guid ExecutionOrderId,
    IReadOnlyList<CreateSeparationPartItemCommand> Parts,
    IReadOnlyList<CreateSeparationSupplyItemCommand> Supplies);
