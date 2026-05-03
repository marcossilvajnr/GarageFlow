using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Tests.Application.Supplies;

internal sealed class FakeSupplyRepository : ISupplyRepository
{
    private readonly List<Supply> _supplies = [];

    public Task<Supply?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_supplies.FirstOrDefault(s => s.Id == id));

    public Task<(IReadOnlyList<Supply> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = _supplies.Count;
        var items = (IReadOnlyList<Supply>)_supplies
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((items, total));
    }

    public Task AddAsync(Supply supply, CancellationToken cancellationToken = default)
    {
        if (_supplies.Any(s => s.Code == supply.Code))
            throw new DuplicateSupplyDataException(DomainErrorMessages.DuplicateSupplyCode);

        _supplies.Add(supply);
        return Task.CompletedTask;
    }

    public void Update(Supply supply) { }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public IReadOnlyList<Supply> All => _supplies;
}
