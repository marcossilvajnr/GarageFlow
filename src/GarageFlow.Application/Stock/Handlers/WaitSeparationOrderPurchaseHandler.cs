using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class WaitSeparationOrderPurchaseHandler(ISeparationOrderRepository separationOrderRepository)
{
    public async Task<SeparationOrderDto> HandleAsync(WaitSeparationOrderPurchaseCommand command, CancellationToken cancellationToken = default)
    {
        var separationOrder = await separationOrderRepository.GetByIdAsync(command.SeparationOrderId, cancellationToken);
        if (separationOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFound(command.SeparationOrderId));

        separationOrder.WaitForPurchase();

        await separationOrderRepository.SaveChangesAsync(cancellationToken);

        return SeparationOrderMapper.ToDto(separationOrder);
    }
}
