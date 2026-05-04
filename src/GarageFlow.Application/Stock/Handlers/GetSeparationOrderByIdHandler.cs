using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class GetSeparationOrderByIdHandler(ISeparationOrderRepository separationOrderRepository)
{
    public async Task<SeparationOrderDto> HandleAsync(GetSeparationOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var separationOrder = await separationOrderRepository.GetByIdAsync(query.Id, cancellationToken);
        if (separationOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFound(query.Id));

        return SeparationOrderMapper.ToDto(separationOrder);
    }
}
