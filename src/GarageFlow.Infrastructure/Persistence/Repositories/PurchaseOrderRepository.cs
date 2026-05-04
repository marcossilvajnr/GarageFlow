using GarageFlow.Domain.Purchasing;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseOrderRepository(GarageFlowDbContext dbContext) : IPurchaseOrderRepository
{
    public async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.PurchaseOrders
            .Include(po => po.SeparationOrderRefs)
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.PurchaseOrders
            .Include(po => po.SeparationOrderRefs)
            .Include(po => po.Items)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(po => po.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
        => await dbContext.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
