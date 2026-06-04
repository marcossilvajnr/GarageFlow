namespace GarageFlow.Api.Purchasing.DTOs;

public sealed record AssignPurchaseOrderSupplierRequest(Guid SupplierId, Guid EmployeeId);
