namespace GarageFlow.Domain.Suppliers;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Supplier> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
    void Update(Supplier supplier);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
