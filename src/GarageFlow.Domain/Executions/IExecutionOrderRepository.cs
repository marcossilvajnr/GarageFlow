namespace GarageFlow.Domain.Executions;

public interface IExecutionOrderRepository
{
    Task<ExecutionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExecutionOrder>> GetByServiceOrderIdAsync(Guid serviceOrderId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForServiceOrderServiceAsync(Guid serviceOrderId, Guid serviceId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ExecutionOrder> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(ExecutionOrder executionOrder, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
