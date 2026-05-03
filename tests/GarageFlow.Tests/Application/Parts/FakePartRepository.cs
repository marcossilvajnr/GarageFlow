using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Application.Parts;

internal sealed class FakePartRepository : IPartRepository
{
    private readonly List<Part> _parts = [];

    public Task<Part?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_parts.FirstOrDefault(p => p.Id == id));

    public Task<(IReadOnlyList<Part> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = _parts.Count;
        var items = (IReadOnlyList<Part>)_parts
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((items, total));
    }

    public Task AddAsync(Part part, CancellationToken cancellationToken = default)
    {
        if (_parts.Any(p => p.Code == part.Code))
            throw new DuplicatePartDataException(DomainErrorMessages.DuplicatePartCode);

        if (_parts.Any(p => p.Sku == part.Sku))
            throw new DuplicatePartDataException(DomainErrorMessages.DuplicatePartSku);

        _parts.Add(part);
        return Task.CompletedTask;
    }

    public void Update(Part part) { }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public IReadOnlyList<Part> All => _parts;
}
