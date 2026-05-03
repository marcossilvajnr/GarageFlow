using GarageFlow.Application.Services.DTOs;
using GarageFlow.Application.Services.Queries;
using GarageFlow.Domain.Services;

namespace GarageFlow.Application.Services.Handlers;

public sealed class ListServicesHandler(IServiceRepository repository)
{
    public async Task<PagedResult<ServiceDto>> HandleAsync(ListServicesQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await repository.ListAsync(query.Page, query.PageSize, cancellationToken);
        var dtos = items.Select(ServiceMapper.ToDto).ToList();

        return new PagedResult<ServiceDto>(dtos, query.Page, query.PageSize, totalCount);
    }
}
