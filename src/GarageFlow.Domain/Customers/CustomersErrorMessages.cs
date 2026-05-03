namespace GarageFlow.Domain.Customers;

public static class CustomersErrorMessages
{
    // Customer aggregate
    public const string InvalidName = "Nome do cliente inválido";
    public const string InvalidDocumentType = "Tipo de documento do cliente inválido";
    public const string CustomerAlreadyInactive = "Cliente já está inativo";

    // Document VOs
    public const string InvalidCpf = "CPF inválido";
    public const string InvalidCnpj = "CNPJ inválido";

    // Contact VOs
    public const string InvalidEmail = "E-mail inválido";
    public const string InvalidPhoneNumber = "Telefone inválido";

    // Address VO
    public const string InvalidStreet = "Logradouro inválido";
    public const string InvalidNumber = "Número inválido";
    public const string InvalidComplement = "Complemento inválido";
    public const string InvalidNeighborhood = "Bairro inválido";
    public const string InvalidCity = "Cidade inválida";
    public const string InvalidState = "UF inválida";
    public const string InvalidZipCode = "CEP inválido";

    // Uniqueness (repository)
    public const string DuplicateCpf = "CPF já cadastrado";
    public const string DuplicateCnpj = "CNPJ já cadastrado";

    // Not found (handlers)
    public static string CustomerNotFound(Guid id) => $"Cliente '{id}' não encontrado";
}
