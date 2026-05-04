using GarageFlow.Domain.Purchasing;

namespace GarageFlow.Tests.Application.Purchasing;

internal sealed class FakePurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly List<PurchaseOrder> _store = [];

    public Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.FirstOrDefault(po => po.Id == id));

    public Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = _store.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<PurchaseOrder>, int)>((items, _store.Count));
    }

    public Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        _store.Add(purchaseOrder);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
