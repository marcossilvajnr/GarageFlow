namespace GarageFlow.Api.Suppliers.DTOs;

public sealed record PagedSupplierResponse(
    IReadOnlyList<SupplierResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
