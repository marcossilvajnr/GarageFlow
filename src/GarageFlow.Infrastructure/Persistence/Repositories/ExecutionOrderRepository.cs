using GarageFlow.Domain.Executions;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class ExecutionOrderRepository(GarageFlowDbContext dbContext) : IExecutionOrderRepository
{
    public async Task<ExecutionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.ExecutionOrders
            .FirstOrDefaultAsync(eo => eo.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<ExecutionOrder> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ExecutionOrders.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(eo => eo.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ExecutionOrder executionOrder, CancellationToken cancellationToken = default)
        => await dbContext.ExecutionOrders.AddAsync(executionOrder, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
