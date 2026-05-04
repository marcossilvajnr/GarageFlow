using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Application.ServiceOrders;

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
        var isDuplicate = customer.DocumentType == CustomerDocumentType.Cpf
            ? _customers.Any(c => c.Cpf?.Value == customer.Cpf!.Value)
            : _customers.Any(c => c.Cnpj?.Value == customer.Cnpj!.Value);

        if (isDuplicate)
        {
            var message = customer.DocumentType == CustomerDocumentType.Cpf
                ? DomainErrorMessages.DuplicateCpf
                : DomainErrorMessages.DuplicateCnpj;
            throw new DuplicateDocumentException(message);
        }

        _customers.Add(customer);
        return Task.CompletedTask;
    }

    public void Update(Customer customer) { }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
