using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Mappers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class ReleaseStockReservationHandler(
    IStockRepository stockRepository,
    IPartRepository partRepository,
    ISupplyRepository supplyRepository,
    ISeparationOrderRepository separationOrderRepository)
{
    public async Task<StockPositionDto> HandleAsync(ReleaseStockReservationCommand command, CancellationToken cancellationToken = default)
    {
        var itemType = StockItemTypeMapper.ToDomain(command.ItemType);

        await StockItemExistenceValidator.EnsureExistsAsync(
            command.ItemId,
            itemType,
            partRepository,
            supplyRepository,
            cancellationToken);

        var stock = await stockRepository.GetByItemAsync(command.ItemId, itemType, cancellationToken);
        if (stock is null)
            throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(itemType, command.ItemId));

        // RN-033 / task-033: detect post-custody state and enforce mandatory reference fields.
        // Post-custody = at least one completed SeparationOrder exists for this stock item.
        var isPostCustody = await separationOrderRepository
            .HasCompletedOrderForItemAsync(command.ItemId, itemType, cancellationToken);

        if (isPostCustody)
        {
            if (command.ReferenceId is null)
                throw new DomainException(DomainErrorMessages.StockExceptionalReleaseReferenceIdRequired);

            if (string.IsNullOrWhiteSpace(command.ReferenceType))
                throw new DomainException(DomainErrorMessages.StockExceptionalReleaseReferenceTypeRequired);

            if (!string.Equals(command.ReferenceType, "SeparationOrder", StringComparison.OrdinalIgnoreCase))
                throw new DomainException(DomainErrorMessages.InvalidStockReferenceType);

            var referencedOrder = await separationOrderRepository.GetByIdAsync(command.ReferenceId.Value, cancellationToken);
            if (referencedOrder is null)
                throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFound(command.ReferenceId.Value));
        }

        stock.Release(
            command.Quantity,
            command.Reason ?? string.Empty,
            command.PerformedBy ?? string.Empty,
            command.ReferenceId,
            command.ReferenceType);

        await stockRepository.SaveChangesAsync(cancellationToken);
        return StockMapper.ToPositionDto(stock);
    }
}
