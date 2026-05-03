using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class SupplierRepository(GarageFlowDbContext dbContext) : ISupplierRepository
{
    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Suppliers.FindAsync([id], cancellationToken);

    public async Task<(IReadOnlyList<Supplier> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Suppliers.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        var isDuplicate = await dbContext.Suppliers
            .AnyAsync(s => s.Cnpj.Value == supplier.Cnpj.Value, cancellationToken);

        if (isDuplicate)
            throw new DuplicateSupplierDataException(DomainErrorMessages.DuplicateCnpjSupplier);

        await dbContext.Suppliers.AddAsync(supplier, cancellationToken);
    }

    public void Update(Supplier supplier)
        => dbContext.Suppliers.Update(supplier);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: "23505" } pgEx)
        {
            throw new DuplicateSupplierDataException(DomainErrorMessages.DuplicateCnpjSupplier);
        }
    }
}
