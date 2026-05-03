using GarageFlow.Application.Parts.DTOs;
using GarageFlow.Application.Parts.Queries;
using GarageFlow.Domain.Parts;

namespace GarageFlow.Application.Parts.Handlers;

public sealed record ListPartsResult(
    IReadOnlyList<PartDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed class ListPartsHandler(IPartRepository repository)
{
    public async Task<ListPartsResult> HandleAsync(ListPartsQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await repository.ListAsync(query.Page, query.PageSize, cancellationToken);
        var dtos = items.Select(PartMapper.ToDto).ToList();

        return new ListPartsResult(dtos, query.Page, query.PageSize, totalCount);
    }
}
