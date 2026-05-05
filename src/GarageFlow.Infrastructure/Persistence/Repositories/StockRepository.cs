using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class StockRepository(GarageFlowDbContext dbContext) : IStockRepository
{
    public async Task<Stock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Stocks
            .Include(s => s.Operations)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Stock?> GetByItemAsync(Guid itemId, StockItemType itemType, CancellationToken cancellationToken = default)
        => await dbContext.Stocks
            .Include(s => s.Operations)
            .FirstOrDefaultAsync(s => s.ItemId == itemId && s.ItemType == itemType, cancellationToken);

    public async Task<(IReadOnlyList<StockOperation> Items, int TotalCount)> ListOperationsAsync(
        Guid itemId,
        StockItemType itemType,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var operationsQuery = dbContext.Stocks
            .AsNoTracking()
            .Where(s => s.ItemId == itemId && s.ItemType == itemType)
            .SelectMany(s => s.Operations)
            .AsQueryable();

        if (from.HasValue)
            operationsQuery = operationsQuery.Where(op => op.CreatedAt >= from.Value);

        if (to.HasValue)
            operationsQuery = operationsQuery.Where(op => op.CreatedAt <= to.Value);

        var totalCount = await operationsQuery.CountAsync(cancellationToken);

        var items = await operationsQuery
            .OrderByDescending(op => op.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Stock stock, CancellationToken cancellationToken = default)
        => await dbContext.Stocks.AddAsync(stock, cancellationToken);

    public void Update(Stock stock)
        => dbContext.Stocks.Update(stock);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateStockConstraint(ex))
        {
            throw new DuplicateStockDataException(DomainErrorMessages.DuplicateStockItem);
        }
    }

    private static bool IsDuplicateStockConstraint(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException { SqlState: "23505" } pgEx &&
            pgEx.ConstraintName == "ux_stocks_item_type_item_id")
        {
            return true;
        }

        var message = ex.InnerException?.Message ?? ex.Message;

        return message.Contains("ux_stocks_item_type_item_id", StringComparison.OrdinalIgnoreCase)
            || message.Contains("UNIQUE constraint failed: stocks.item_type, stocks.item_id", StringComparison.OrdinalIgnoreCase);
    }
}
