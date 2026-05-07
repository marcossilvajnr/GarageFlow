namespace GarageFlow.Api.DTOs.Purchasing;

public sealed record AssignPurchaseOrderSupplierRequest(Guid SupplierId, Guid EmployeeId);
