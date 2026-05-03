namespace GarageFlow.Domain.Shared;

/// <summary>
/// Centralized catalog of domain error messages shared across contexts.
/// Use this for transversal Value Objects and common domain validations.
/// </summary>
public static class DomainErrorMessages
{
    // Document VOs (transversal)
    public const string InvalidCpf = "CPF inválido";
    public const string InvalidCnpj = "CNPJ inválido";

    // Contact VOs (transversal)
    public const string InvalidEmail = "E-mail inválido";
    public const string InvalidPhoneNumber = "Telefone inválido";

    // Address VO (transversal)
    public const string InvalidStreet = "Logradouro inválido";
    public const string InvalidNumber = "Número inválido";
    public const string InvalidComplement = "Complemento inválido";
    public const string InvalidNeighborhood = "Bairro inválido";
    public const string InvalidCity = "Cidade inválida";
    public const string InvalidState = "UF inválida";
    public const string InvalidZipCode = "CEP inválido";

    // Vehicle VOs (transversal)
    public const string InvalidLicensePlate = "Placa inválida";
    public const string InvalidRenavam = "RENAVAM inválido";

    // Customer aggregate (context-specific)
    public const string InvalidName = "Nome do cliente inválido";
    public const string InvalidDocumentType = "Tipo de documento do cliente inválido";
    public const string CustomerAlreadyInactive = "Cliente já está inativo";

    // Customer uniqueness (repository - context-specific)
    public const string DuplicateCpf = "CPF já cadastrado";
    public const string DuplicateCnpj = "CNPJ já cadastrado";

    // Customer not found (handlers - context-specific)
    public static string CustomerNotFound(Guid id) => $"Cliente '{id}' não encontrado";
}
