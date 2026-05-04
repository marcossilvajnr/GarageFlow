using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Tests.Application.ServiceOrders;

internal sealed class FakeServiceOrderRepository : IServiceOrderRepository
{
    private readonly List<ServiceOrder> _orders = [];

    public Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_orders.FirstOrDefault(so => so.Id == id));

    public Task<(IReadOnlyList<ServiceOrder> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = _orders.Count;
        var items = (IReadOnlyList<ServiceOrder>)_orders
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((items, total));
    }

    public Task AddAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default)
    {
        _orders.Add(serviceOrder);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
