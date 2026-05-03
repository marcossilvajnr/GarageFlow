using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class PartRepository(GarageFlowDbContext dbContext) : IPartRepository
{
    public async Task<Part?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Parts.FindAsync([id], cancellationToken);

    public async Task<(IReadOnlyList<Part> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Parts.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Part part, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Parts.AnyAsync(p => p.Code == part.Code, cancellationToken))
            throw new DuplicatePartDataException(DomainErrorMessages.DuplicatePartCode);

        if (await dbContext.Parts.AnyAsync(p => p.Sku == part.Sku, cancellationToken))
            throw new DuplicatePartDataException(DomainErrorMessages.DuplicatePartSku);

        await dbContext.Parts.AddAsync(part, cancellationToken);
    }

    public void Update(Part part)
        => dbContext.Parts.Update(part);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: "23505" } pgEx)
        {
            var message = pgEx.ConstraintName?.Contains("sku") == true
                ? DomainErrorMessages.DuplicatePartSku
                : DomainErrorMessages.DuplicatePartCode;

            throw new DuplicatePartDataException(message);
        }
    }
}
