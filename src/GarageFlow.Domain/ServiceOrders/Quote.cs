using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.ServiceOrders;

public sealed class Quote
{
    private List<QuoteItem> _items = [];

    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public IReadOnlyList<QuoteItem> Items => _items.AsReadOnly();
    public decimal TotalAmount { get; private set; }
    public QuoteStatus Status { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private Quote() { }

    public static Quote Generate(Guid serviceOrderId, IEnumerable<QuoteItem> items)
    {
        if (serviceOrderId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderId);

        var itemList = items?.ToList() ?? [];

        if (itemList.Count == 0)
            throw new DomainException(DomainErrorMessages.QuoteRequiresAtLeastOneItem);

        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = serviceOrderId,
            TotalAmount = itemList.Sum(i => i.Subtotal),
            Status = QuoteStatus.WaitingCustomerApproval,
            GeneratedAt = DateTime.UtcNow
        };

        quote._items.AddRange(itemList);

        return quote;
    }

    public void Accept()
    {
        if (Status != QuoteStatus.WaitingCustomerApproval)
            throw new QuoteAlreadyDecidedException(DomainErrorMessages.QuoteAlreadyDecided);

        Status = QuoteStatus.CustomerApproved;
        AcceptedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(DomainErrorMessages.QuoteRejectionReasonRequired);

        if (Status != QuoteStatus.WaitingCustomerApproval)
            throw new QuoteAlreadyDecidedException(DomainErrorMessages.QuoteAlreadyDecided);

        Status = QuoteStatus.CustomerRejected;
        RejectedAt = DateTime.UtcNow;
        RejectionReason = reason;
    }
}
