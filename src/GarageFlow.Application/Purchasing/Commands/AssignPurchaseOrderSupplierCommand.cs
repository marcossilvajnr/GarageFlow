namespace GarageFlow.Application.Purchasing.Commands;

public sealed record AssignPurchaseOrderSupplierCommand(Guid Id, Guid SupplierId, Guid EmployeeId);
