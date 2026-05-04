namespace GarageFlow.Domain.ServiceOrders;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ServiceOrder> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
