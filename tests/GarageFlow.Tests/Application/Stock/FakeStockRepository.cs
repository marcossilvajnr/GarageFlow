using GarageFlow.Domain.Stock;
using DomainStock = GarageFlow.Domain.Stock.Stock;

namespace GarageFlow.Tests.Application.Stock;

internal sealed class FakeStockRepository : IStockRepository
{
    private readonly List<DomainStock> _stocks = [];

    public Task<DomainStock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_stocks.FirstOrDefault(s => s.Id == id));

    public Task<DomainStock?> GetByItemAsync(Guid itemId, StockItemType itemType, CancellationToken cancellationToken = default)
        => Task.FromResult(_stocks.FirstOrDefault(s => s.ItemId == itemId && s.ItemType == itemType));

    public Task<(IReadOnlyList<StockOperation> Items, int TotalCount)> ListOperationsAsync(
        Guid itemId,
        StockItemType itemType,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var stock = _stocks.FirstOrDefault(s => s.ItemId == itemId && s.ItemType == itemType);
        IEnumerable<StockOperation> operations = stock?.Operations ?? [];

        if (from.HasValue)
            operations = operations.Where(op => op.CreatedAt >= from.Value);

        if (to.HasValue)
            operations = operations.Where(op => op.CreatedAt <= to.Value);

        var totalCount = operations.Count();
        var items = operations
            .OrderByDescending(op => op.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(((IReadOnlyList<StockOperation>)items, totalCount));
    }

    public Task AddAsync(DomainStock stock, CancellationToken cancellationToken = default)
    {
        _stocks.Add(stock);
        return Task.CompletedTask;
    }

    public void Update(DomainStock stock)
    {
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
