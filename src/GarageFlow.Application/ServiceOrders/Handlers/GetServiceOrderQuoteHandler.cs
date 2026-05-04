using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class GetServiceOrderQuoteHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task<QuoteDto> HandleAsync(
        GetServiceOrderQuoteQuery query,
        CancellationToken cancellationToken = default)
    {
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(query.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(query.ServiceOrderId));

        if (serviceOrder.Quote is null)
            throw new QuoteNotFoundException(DomainErrorMessages.QuoteNotFound(query.ServiceOrderId));

        return ServiceOrderMapper.ToQuoteDto(serviceOrder.Quote);
    }
}
