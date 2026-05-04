using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class CreateSeparationOrderHandler(ISeparationOrderRepository separationOrderRepository)
{
    public async Task<SeparationOrderDto> HandleAsync(CreateSeparationOrderCommand command, CancellationToken cancellationToken = default)
    {
        var parts = command.Parts.Select(p => SeparationPartItem.Create(p.PartId, p.PartName, p.Quantity)).ToList();
        var supplies = command.Supplies.Select(s => SeparationSupplyItem.Create(s.SupplyId, s.SupplyName, s.Quantity, s.Unit)).ToList();

        var separationOrder = SeparationOrder.Create(command.ExecutionOrderId, parts, supplies);

        await separationOrderRepository.AddAsync(separationOrder, cancellationToken);
        await separationOrderRepository.SaveChangesAsync(cancellationToken);

        return SeparationOrderMapper.ToDto(separationOrder);
    }
}
