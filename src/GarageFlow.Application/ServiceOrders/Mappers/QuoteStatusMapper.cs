using AppQuoteStatus = GarageFlow.Application.ServiceOrders.Enums.QuoteStatus;
using DomainQuoteStatus = GarageFlow.Domain.ServiceOrders.QuoteStatus;

namespace GarageFlow.Application.ServiceOrders.Mappers;

internal static class QuoteStatusMapper
{
    internal static DomainQuoteStatus ToDomain(AppQuoteStatus status) =>
        status switch
        {
            AppQuoteStatus.WaitingCustomerApproval => DomainQuoteStatus.WaitingCustomerApproval,
            AppQuoteStatus.CustomerApproved => DomainQuoteStatus.CustomerApproved,
            AppQuoteStatus.CustomerRejected => DomainQuoteStatus.CustomerRejected,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    internal static AppQuoteStatus ToApplication(DomainQuoteStatus status) =>
        status switch
        {
            DomainQuoteStatus.WaitingCustomerApproval => AppQuoteStatus.WaitingCustomerApproval,
            DomainQuoteStatus.CustomerApproved => AppQuoteStatus.CustomerApproved,
            DomainQuoteStatus.CustomerRejected => AppQuoteStatus.CustomerRejected,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
