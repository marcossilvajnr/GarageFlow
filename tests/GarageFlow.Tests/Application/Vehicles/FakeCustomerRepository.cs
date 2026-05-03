using GarageFlow.Domain.Customers;

namespace GarageFlow.Tests.Application.Vehicles;

internal sealed class FakeCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = [];

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));

    public Task<(IReadOnlyList<Customer> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = _customers.Count;
        var items = (IReadOnlyList<Customer>)_customers
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((items, total));
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _customers.Add(customer);
        return Task.CompletedTask;
    }

    public void Update(Customer customer) { }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public IReadOnlyList<Customer> All => _customers;
}
