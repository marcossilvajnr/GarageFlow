namespace GarageFlow.Api.DTOs.Suppliers;

public sealed record PagedSupplierResponse(
    IReadOnlyList<SupplierResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
