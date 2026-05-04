namespace GarageFlow.Application.Stock.Commands;

public sealed record ConfirmSeparationStockistWithdrawalCommand(Guid SeparationOrderId, Guid StockistId);
