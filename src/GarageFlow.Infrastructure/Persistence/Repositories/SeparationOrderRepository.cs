using GarageFlow.Domain.Stock;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class SeparationOrderRepository(GarageFlowDbContext dbContext) : ISeparationOrderRepository
{
    public async Task<SeparationOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.SeparationOrders
            .Include(so => so.Parts)
            .Include(so => so.Supplies)
            .FirstOrDefaultAsync(so => so.Id == id, cancellationToken);

    public async Task<SeparationOrder?> GetByExecutionOrderIdAsync(Guid executionOrderId, CancellationToken cancellationToken = default)
        => await dbContext.SeparationOrders
            .Include(so => so.Parts)
            .Include(so => so.Supplies)
            .FirstOrDefaultAsync(so => so.ExecutionOrderId == executionOrderId, cancellationToken);

    public async Task<(IReadOnlyList<SeparationOrder> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.SeparationOrders
            .Include(so => so.Parts)
            .Include(so => so.Supplies)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(so => so.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(SeparationOrder separationOrder, CancellationToken cancellationToken = default)
        => await dbContext.SeparationOrders.AddAsync(separationOrder, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
