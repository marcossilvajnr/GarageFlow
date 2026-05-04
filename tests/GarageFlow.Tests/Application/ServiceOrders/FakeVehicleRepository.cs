using GarageFlow.Domain.Vehicles;

namespace GarageFlow.Tests.Application.ServiceOrders;

internal sealed class FakeVehicleRepository : IVehicleRepository
{
    private readonly List<Vehicle> _vehicles = [];

    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_vehicles.FirstOrDefault(v => v.Id == id));

    public Task<(IReadOnlyList<Vehicle> Items, int TotalCount)> ListByCustomerIdAsync(
        Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _vehicles.AsEnumerable();
        if (customerId != Guid.Empty)
            query = query.Where(v => v.CustomerId == customerId);

        var total = query.Count();
        var items = (IReadOnlyList<Vehicle>)query
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((items, total));
    }

    public Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
        => Task.FromResult(_vehicles.FirstOrDefault(v => v.LicensePlate.Value == licensePlate));

    public Task<Vehicle?> GetByRenavamAsync(string renavam, CancellationToken cancellationToken = default)
        => Task.FromResult(_vehicles.FirstOrDefault(v => v.Renavam.Value == renavam));

    public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        _vehicles.Add(vehicle);
        return Task.CompletedTask;
    }

    public void Update(Vehicle vehicle) { }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
