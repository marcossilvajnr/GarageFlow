namespace GarageFlow.Domain.Stock;

public interface ISeparationOrderRepository
{
    Task<SeparationOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<SeparationOrder> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(SeparationOrder separationOrder, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
