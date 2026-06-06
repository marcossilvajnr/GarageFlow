namespace GarageFlow.Application.Common.Errors;

public enum ApplicationErrorKind
{
    Validation = 0,
    NotFound = 1,
    Conflict = 2,
    StateConflict = 3,
    Unauthorized = 4,
    Unexpected = 5
}
