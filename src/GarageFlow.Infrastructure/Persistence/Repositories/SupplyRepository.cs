using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class SupplyRepository(GarageFlowDbContext dbContext) : ISupplyRepository
{
    public async Task<Supply?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Supplies.FindAsync([id], cancellationToken);

    public async Task<(IReadOnlyList<Supply> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Supplies.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Supply supply, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Supplies.AnyAsync(s => s.Code == supply.Code, cancellationToken))
            throw new DuplicateSupplyDataException(DomainErrorMessages.DuplicateSupplyCode);

        await dbContext.Supplies.AddAsync(supply, cancellationToken);
    }

    public void Update(Supply supply)
        => dbContext.Supplies.Update(supply);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            throw new DuplicateSupplyDataException(DomainErrorMessages.DuplicateSupplyCode);
        }
    }
}
