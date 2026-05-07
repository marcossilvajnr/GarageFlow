using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Employees;

internal static class EmployeeActorValidator
{
    internal static async Task ValidateAsync(
        IEmployeeRepository employeeRepository,
        Guid employeeId,
        string invalidEmployeeIdMessage,
        EmployeeRole[] allowedRoles,
        CancellationToken cancellationToken)
    {
        if (employeeId == Guid.Empty)
            throw new DomainException(invalidEmployeeIdMessage);

        var employee = await employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee is null)
            throw new EntityNotFoundException(DomainErrorMessages.EmployeeNotFound(employeeId));

        if (!employee.IsActive)
            throw new DomainException(DomainErrorMessages.EmployeeInactive);

        if (!allowedRoles.Contains(employee.Role))
        {
            var allowedRolesDisplay = string.Join(", ", allowedRoles.Select(role => role.ToString()));
            throw new DomainException(DomainErrorMessages.EmployeeRoleIncompatible(allowedRolesDisplay));
        }
    }
}
