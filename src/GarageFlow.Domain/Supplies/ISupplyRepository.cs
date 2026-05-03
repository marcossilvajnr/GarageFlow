namespace GarageFlow.Domain.Supplies;

public interface ISupplyRepository
{
    Task<Supply?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Supply> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Supply supply, CancellationToken cancellationToken = default);
    void Update(Supply supply);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
