using GarageFlow.Application.Services.Commands;
using GarageFlow.Application.Services.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Services.Handlers;

public sealed class AddServicePartHandler(
    IServiceRepository serviceRepository,
    IPartRepository partRepository)
{
    public async Task<ServiceDto> HandleAsync(AddServicePartCommand command, CancellationToken cancellationToken = default)
    {
        var service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceNotFound(command.ServiceId));

        var part = await partRepository.GetByIdAsync(command.PartId, cancellationToken);
        if (part is null)
            throw new EntityNotFoundException(DomainErrorMessages.PartNotFound(command.PartId));

        service.AddPart(part.Id, part.Name, command.Quantity);

        await serviceRepository.SaveChangesAsync(cancellationToken);

        return ServiceMapper.ToDto(service);
    }
}
