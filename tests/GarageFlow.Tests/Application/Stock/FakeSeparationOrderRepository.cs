using GarageFlow.Domain.Stock;

namespace GarageFlow.Tests.Application.Stock;

internal sealed class FakeSeparationOrderRepository : ISeparationOrderRepository
{
    private readonly List<SeparationOrder> _store = [];

    public Task<SeparationOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.FirstOrDefault(so => so.Id == id));

    public Task<(IReadOnlyList<SeparationOrder> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = _store.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<SeparationOrder>, int)>((items, _store.Count));
    }

    public Task AddAsync(SeparationOrder separationOrder, CancellationToken cancellationToken = default)
    {
        _store.Add(separationOrder);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
