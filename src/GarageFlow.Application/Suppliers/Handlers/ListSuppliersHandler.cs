using GarageFlow.Application.Suppliers.DTOs;
using GarageFlow.Application.Suppliers.Queries;
using GarageFlow.Domain.Suppliers;

namespace GarageFlow.Application.Suppliers.Handlers;

public sealed record ListSuppliersResult(
    IReadOnlyList<SupplierDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed class ListSuppliersHandler(ISupplierRepository repository)
{
    public async Task<ListSuppliersResult> HandleAsync(ListSuppliersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await repository.ListAsync(query.Page, query.PageSize, cancellationToken);
        var dtos = items.Select(SupplierMapper.ToDto).ToList();

        return new ListSuppliersResult(dtos, query.Page, query.PageSize, totalCount);
    }
}
