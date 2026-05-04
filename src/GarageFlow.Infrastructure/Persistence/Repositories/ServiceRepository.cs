using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class ServiceRepository(GarageFlowDbContext dbContext) : IServiceRepository
{
    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Services
            .Include(s => s.Parts)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Service> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Services.AsNoTracking().Include(s => s.Parts);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Services.AnyAsync(s => s.Code == service.Code, cancellationToken))
            throw new DuplicateServiceDataException(DomainErrorMessages.DuplicateServiceCode);

        if (await dbContext.Services.AnyAsync(s => s.Name == service.Name, cancellationToken))
            throw new DuplicateServiceDataException(DomainErrorMessages.DuplicateServiceName);

        await dbContext.Services.AddAsync(service, cancellationToken);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
        => await dbContext.Services.AnyAsync(s => s.Name == name && s.Id != excludeId, cancellationToken);

    public void Update(Service service)
        => dbContext.Entry(service).State = EntityState.Modified;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: "23505" } pgEx)
        {
            if (pgEx.ConstraintName?.Contains("service_parts") == true)
                throw new DuplicateServicePartException(DomainErrorMessages.DuplicateServicePart);

            var message = pgEx.ConstraintName?.Contains("code") == true
                ? DomainErrorMessages.DuplicateServiceCode
                : DomainErrorMessages.DuplicateServiceName;

            throw new DuplicateServiceDataException(message);
        }
    }
}
