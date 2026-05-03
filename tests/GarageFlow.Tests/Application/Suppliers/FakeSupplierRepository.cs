using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Suppliers;

namespace GarageFlow.Tests.Application.Suppliers;

internal sealed class FakeSupplierRepository : ISupplierRepository
{
    private readonly List<Supplier> _suppliers = [];

    public Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_suppliers.FirstOrDefault(s => s.Id == id));

    public Task<(IReadOnlyList<Supplier> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = _suppliers.Count;
        var items = (IReadOnlyList<Supplier>)_suppliers
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((items, total));
    }

    public Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        var isDuplicate = _suppliers.Any(s => s.Cnpj.Value == supplier.Cnpj.Value);

        if (isDuplicate)
            throw new DuplicateSupplierDataException(DomainErrorMessages.DuplicateCnpjSupplier);

        _suppliers.Add(supplier);
        return Task.CompletedTask;
    }

    public void Update(Supplier supplier) { }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public IReadOnlyList<Supplier> All => _suppliers;
}
