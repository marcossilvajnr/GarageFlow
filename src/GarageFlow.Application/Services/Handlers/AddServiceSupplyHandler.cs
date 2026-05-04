using GarageFlow.Application.Services.Commands;
using GarageFlow.Application.Services.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Services.Handlers;

public sealed class AddServiceSupplyHandler(
    IServiceRepository serviceRepository,
    ISupplyRepository supplyRepository)
{
    public async Task<ServiceDto> HandleAsync(AddServiceSupplyCommand command, CancellationToken cancellationToken = default)
    {
        var service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceNotFound(command.ServiceId));

        var supply = await supplyRepository.GetByIdAsync(command.SupplyId, cancellationToken);
        if (supply is null)
            throw new EntityNotFoundException(DomainErrorMessages.SupplyNotFound(command.SupplyId));

        var unit = MapSupplyUnit(supply.UnitOfMeasure, command.SupplyId);

        service.AddSupply(supply.Id, supply.Name, command.Quantity, unit);

        await serviceRepository.SaveChangesAsync(cancellationToken);

        return ServiceMapper.ToDto(service);
    }

    private static SupplyUnit MapSupplyUnit(string unitOfMeasure, Guid supplyId)
        => unitOfMeasure.ToUpperInvariant() switch
        {
            "L" => SupplyUnit.Liter,
            "ML" => SupplyUnit.Milliliter,
            "G" => SupplyUnit.Gram,
            "KG" => SupplyUnit.Kilogram,
            "UN" => SupplyUnit.Unit,
            _ => throw new DomainException(DomainErrorMessages.InvalidServiceSupplyUnit)
        };
}
