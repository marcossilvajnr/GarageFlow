using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class ConfirmSeparationStockistWithdrawalHandler(
    ISeparationOrderRepository separationOrderRepository,
    IStockRepository stockRepository,
    IEmployeeRepository employeeRepository)
{
    public async Task<SeparationOrderDto> HandleAsync(ConfirmSeparationStockistWithdrawalCommand command, CancellationToken cancellationToken = default)
    {
        var separationOrder = await separationOrderRepository.GetByIdAsync(command.SeparationOrderId, cancellationToken);
        if (separationOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFound(command.SeparationOrderId));

        if (separationOrder.Status != SeparationOrderStatus.WaitingPickup)
            throw new InvalidSeparationOrderStatusTransitionException(DomainErrorMessages.SeparationOrderNotWaitingPickup);

        await Employees.EmployeeActorValidator.ValidateAsync(
            employeeRepository,
            command.StockistId,
            DomainErrorMessages.InvalidSeparationStockistId,
            [EmployeeRole.Stockist, EmployeeRole.Administrative],
            cancellationToken);

        foreach (var part in separationOrder.Parts)
        {
            var stock = await stockRepository.GetByItemAsync(part.PartId, StockItemType.Part, cancellationToken);
            if (stock is null)
                throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(StockItemType.Part, part.PartId));

            stock.Consume(part.Quantity, referenceId: separationOrder.Id);
        }

        foreach (var supply in separationOrder.Supplies)
        {
            var stock = await stockRepository.GetByItemAsync(supply.SupplyId, StockItemType.Supply, cancellationToken);
            if (stock is null)
                throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(StockItemType.Supply, supply.SupplyId));

            stock.Consume(supply.Quantity, referenceId: separationOrder.Id);
        }

        separationOrder.ConfirmStockistWithdrawal(command.StockistId);

        await separationOrderRepository.SaveChangesAsync(cancellationToken);

        return SeparationOrderMapper.ToDto(separationOrder);
    }
}
