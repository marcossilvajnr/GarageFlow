using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarageFlow.Infrastructure.Persistence.Repositories;

internal sealed class VehicleRepository(GarageFlowDbContext dbContext) : IVehicleRepository
{
    public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Vehicles.FindAsync([id], cancellationToken);

    public async Task<(IReadOnlyList<Vehicle> Items, int TotalCount)> ListByCustomerIdAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Vehicles.AsNoTracking();

        if (customerId != Guid.Empty)
            query = query.Where(v => v.CustomerId == customerId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
        => await dbContext.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.LicensePlate.Value == licensePlate, cancellationToken);

    public async Task<Vehicle?> GetByRenavamAsync(string renavam, CancellationToken cancellationToken = default)
        => await dbContext.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Renavam.Value == renavam, cancellationToken);

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        await dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
    }

    public void Update(Vehicle vehicle)
        => dbContext.Vehicles.Update(vehicle);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: "23505" } pgEx)
        {
            var message = pgEx.ConstraintName?.Contains("license_plate") == true
                ? DomainErrorMessages.DuplicateLicensePlate
                : DomainErrorMessages.DuplicateRenavam;

            throw new DuplicateVehicleDataException(message);
        }
    }
}
