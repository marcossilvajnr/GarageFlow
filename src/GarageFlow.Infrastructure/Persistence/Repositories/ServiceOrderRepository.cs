using GarageFlow.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class ServiceOrderRepository(GarageFlowDbContext dbContext) : IServiceOrderRepository
{
    public async Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.ServiceOrders
            .Include(so => so.Services)
            .Include(so => so.ServiceHistory)
            .FirstOrDefaultAsync(so => so.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<ServiceOrder> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ServiceOrders
            .Include(so => so.Services)
            .Include(so => so.ServiceHistory)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(so => so.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default)
        => await dbContext.ServiceOrders.AddAsync(serviceOrder, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
