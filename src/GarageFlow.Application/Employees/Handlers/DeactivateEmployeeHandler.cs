using GarageFlow.Application.Employees.Commands;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Employees.Handlers;

public sealed class DeactivateEmployeeHandler(IEmployeeRepository repository)
{
    public async Task HandleAsync(DeactivateEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        var employee = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (employee is null)
            throw new EntityNotFoundException(DomainErrorMessages.EmployeeNotFound(command.Id));

        employee.Deactivate();

        repository.Update(employee);
        await repository.SaveChangesAsync(cancellationToken);
    }
}