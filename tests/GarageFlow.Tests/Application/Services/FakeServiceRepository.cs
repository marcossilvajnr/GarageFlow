using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Application.Services;

internal sealed class FakeServiceRepository : IServiceRepository
{
    private readonly List<Service> _services = [];

    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_services.FirstOrDefault(s => s.Id == id));

    public Task<(IReadOnlyList<Service> Items, int TotalCount)> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = _services.Count;
        var items = (IReadOnlyList<Service>)_services
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((items, total));
    }

    public Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        if (_services.Any(s => s.Code == service.Code))
            throw new DuplicateServiceDataException(DomainErrorMessages.DuplicateServiceCode);

        if (_services.Any(s => s.Name == service.Name))
            throw new DuplicateServiceDataException(DomainErrorMessages.DuplicateServiceName);

        _services.Add(service);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
        => Task.FromResult(_services.Any(s => s.Name == name && s.Id != excludeId));

    public void Update(Service service) { }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public IReadOnlyList<Service> All => _services;
}
