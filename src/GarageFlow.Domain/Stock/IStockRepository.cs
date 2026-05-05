namespace GarageFlow.Domain.Stock;

public interface IStockRepository
{
    Task<Stock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Stock?> GetByItemAsync(Guid itemId, StockItemType itemType, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<StockOperation> Items, int TotalCount)> ListOperationsAsync(
        Guid itemId,
        StockItemType itemType,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Stock stock, CancellationToken cancellationToken = default);
    void Update(Stock stock);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
