using GarageFlow.Application.Supplies.DTOs;
using GarageFlow.Application.Supplies.Queries;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Supplies.Handlers;

public sealed record ListSuppliesResult(
    IReadOnlyList<SupplyDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed class ListSuppliesHandler(ISupplyRepository repository)
{
    public async Task<ListSuppliesResult> HandleAsync(ListSuppliesQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await repository.ListAsync(query.Page, query.PageSize, cancellationToken);
        var dtos = items.Select(SupplyMapper.ToDto).ToList();

        return new ListSuppliesResult(dtos, query.Page, query.PageSize, totalCount);
    }
}
