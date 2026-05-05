using GarageFlow.Domain.Executions;

namespace GarageFlow.Tests.Application.Executions;

internal sealed class FakeExecutionOrderRepository : IExecutionOrderRepository
{
    private readonly List<ExecutionOrder> _store = [];

    public Task<ExecutionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.FirstOrDefault(eo => eo.Id == id));

    public Task<IReadOnlyList<ExecutionOrder>> GetByServiceOrderIdAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ExecutionOrder> result = _store.Where(eo => eo.ServiceOrderId == serviceOrderId).ToList();
        return Task.FromResult(result);
    }

    public Task<(IReadOnlyList<ExecutionOrder> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = _store.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<ExecutionOrder>, int)>((items, _store.Count));
    }

    public Task AddAsync(ExecutionOrder executionOrder, CancellationToken cancellationToken = default)
    {
        _store.Add(executionOrder);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
