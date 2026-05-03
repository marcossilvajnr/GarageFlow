using GarageFlow.Application.Services.DTOs;
using GarageFlow.Application.Services.Queries;
using GarageFlow.Domain.Services;

namespace GarageFlow.Application.Services.Handlers;

public sealed class GetServiceByIdHandler(IServiceRepository repository)
{
    public async Task<ServiceDto?> HandleAsync(GetServiceByIdQuery query, CancellationToken cancellationToken = default)
    {
        var service = await repository.GetByIdAsync(query.Id, cancellationToken);
        return service is null ? null : ServiceMapper.ToDto(service);
    }
}
