using GarageFlow.Application.Services.Commands;
using GarageFlow.Application.Services.DTOs;
using GarageFlow.Domain.Services;

namespace GarageFlow.Application.Services.Handlers;

public sealed class CreateServiceHandler(IServiceRepository repository)
{
    public async Task<ServiceDto> HandleAsync(CreateServiceCommand command, CancellationToken cancellationToken = default)
    {
        var service = Service.Create(
            command.Code,
            command.Name,
            command.Description,
            command.BasePrice,
            command.EstimatedDurationMinutes);

        await repository.AddAsync(service, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return ServiceMapper.ToDto(service);
    }
}
