using GarageFlow.Application.Services.DTOs;
using GarageFlow.Application.Services.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Services.Handlers;

public sealed class GetServiceByIdHandler(IServiceRepository repository)
{
    public async Task<ServiceDto> HandleAsync(GetServiceByIdQuery query, CancellationToken cancellationToken = default)
    {
        var service = await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrorMessages.ServiceNotFound(query.Id));

        return ServiceMapper.ToDto(service);
    }
}
