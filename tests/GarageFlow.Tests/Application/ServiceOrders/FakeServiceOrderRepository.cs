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

    public Task<(IReadOnlyList<ServiceOrder> Items, int TotalCount)> ListOperationalAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        static int Priority(ServiceOrderStatus status) => status switch
        {
            ServiceOrderStatus.InExecution => 1,
            ServiceOrderStatus.Approved => 2,
            ServiceOrderStatus.WaitingApproval => 3,
            ServiceOrderStatus.InDiagnostic => 4,
            ServiceOrderStatus.Received => 5,
            _ => int.MaxValue
        };

        var filtered = _orders
            .Where(so => so.Status is ServiceOrderStatus.InExecution
                or ServiceOrderStatus.Approved
                or ServiceOrderStatus.WaitingApproval
                or ServiceOrderStatus.InDiagnostic
                or ServiceOrderStatus.Received)
            .OrderBy(so => Priority(so.Status))
            .ThenBy(so => so.CreatedAt)
            .ToList();

        var total = filtered.Count;
        var items = (IReadOnlyList<ServiceOrder>)filtered
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
