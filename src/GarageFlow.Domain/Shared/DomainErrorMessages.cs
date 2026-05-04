namespace GarageFlow.Domain.Shared;

/// <summary>
/// Centralized catalog of domain error messages shared across contexts.
/// Use this for transversal Value Objects and common domain validations.
/// </summary>
public static class DomainErrorMessages
{
    // Pagination (transversal)
    public const string InvalidPaginationParameters = "Página e tamanho da página devem ser maiores que zero.";

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

    // Vehicle aggregate (context-specific)
    public const string InvalidCustomerId = "Id do cliente inválido";
    public const string InvalidVehicleMake = "Marca do veículo inválida";
    public const string InvalidVehicleModel = "Modelo do veículo inválido";
    public const string InvalidVehicleYear = "Ano do veículo inválido";
    public const string InvalidVehicleColor = "Cor do veículo inválida";
    public const string VehicleAlreadyInactive = "Veículo já está inativo";

    // Vehicle uniqueness (repository - context-specific)
    public const string DuplicateLicensePlate = "Placa já cadastrada";
    public const string DuplicateRenavam = "RENAVAM já cadastrado";

    // Vehicle not found (handlers - context-specific)
    public static string VehicleNotFound(Guid id) => $"Veículo '{id}' não encontrado";

    // Supplier aggregate (context-specific)
    public const string InvalidSupplierName = "Nome do fornecedor inválido";
    public const string SupplierAlreadyInactive = "Fornecedor já está inativo";

    // Supplier uniqueness (repository - context-specific)
    public const string DuplicateCnpjSupplier = "CNPJ já cadastrado para fornecedor";

    // Supplier not found (handlers - context-specific)
    public static string SupplierNotFound(Guid id) => $"Fornecedor '{id}' não encontrado";

    // Employee aggregate (context-specific)
    public const string InvalidEmployeeName = "Nome do funcionário inválido";
    public const string InvalidEmployeeDocumentType = "Tipo de documento do funcionário inválido";
    public const string InvalidEmployeeRole = "Cargo do funcionário inválido";
    public const string EmployeeAlreadyInactive = "Funcionário já está inativo";

    // Employee uniqueness (repository - context-specific)
    public const string DuplicateEmployeeCpf = "CPF já cadastrado para funcionário";
    public const string DuplicateEmployeeCnpj = "CNPJ já cadastrado para funcionário";

    // Employee not found (handlers - context-specific)
    public static string EmployeeNotFound(Guid id) => $"Funcionário '{id}' não encontrado";

    // Service aggregate (context-specific)
    public const string InvalidServiceCode = "Código do serviço inválido";
    public const string InvalidServiceName = "Nome do serviço inválido";
    public const string InvalidServiceDescription = "Descrição do serviço inválida";
    public const string InvalidServiceBasePrice = "Preço base do serviço deve ser maior que zero";
    public const string InvalidServiceEstimatedDuration = "Duração estimada do serviço deve ser maior que zero";
    public const string ServiceAlreadyInactive = "Serviço já está inativo";

    // Service uniqueness (repository - context-specific)
    public const string DuplicateServiceCode = "Código do serviço já cadastrado";
    public const string DuplicateServiceName = "Nome do serviço já cadastrado";

    // Service not found (handlers - context-specific)
    public static string ServiceNotFound(Guid id) => $"Serviço '{id}' não encontrado";

    // Service composition - parts (context-specific)
    public const string InvalidServicePartQuantity = "Quantidade da peça deve ser maior que zero";
    public const string DuplicateServicePart = "Peça já vinculada ao serviço";
    public static string ServicePartNotFound(Guid serviceId, Guid partId) =>
        $"Peça '{partId}' não vinculada ao serviço '{serviceId}'";

    // Part aggregate (context-specific)
    public const string InvalidPartName = "Nome da peça inválido";
    public const string InvalidPartCode = "Código da peça inválido";
    public const string InvalidPartSku = "SKU da peça inválido";
    public const string InvalidPartUnitOfMeasure = "Unidade de medida da peça inválida";
    public const string InvalidPartUnitPrice = "Preço não pode ser negativo";
    public const string PartAlreadyInactive = "Peça já está inativa";

    // Part uniqueness (repository - context-specific)
    public const string DuplicatePartCode = "Código da peça já cadastrado";
    public const string DuplicatePartSku = "SKU da peça já cadastrado";

    // Part not found (handlers - context-specific)
    public static string PartNotFound(Guid id) => $"Peça '{id}' não encontrada";

    // Supply aggregate (context-specific)
    public const string InvalidSupplyName = "Nome do insumo inválido";
    public const string InvalidSupplyCode = "Código do insumo inválido";
    public const string InvalidSupplyUnitOfMeasure = "Unidade de medida do insumo inválida";
    public const string InvalidSupplyBaseCost = "Custo base do insumo não pode ser negativo";
    public const string SupplyAlreadyInactive = "Insumo já está inativo";

    // Supply uniqueness (repository - context-specific)
    public const string DuplicateSupplyCode = "Código do insumo já cadastrado";

    // Supply not found (handlers - context-specific)
    public static string SupplyNotFound(Guid id) => $"Insumo '{id}' não encontrado";

    // Service composition - supplies (context-specific)
    public const string InvalidServiceSupplyQuantity = "Quantidade do insumo deve ser maior que zero";
    public const string DuplicateServiceSupply = "Insumo já vinculado ao serviço";
    public const string InvalidServiceSupplyUnit = "Unidade do insumo não é compatível com composição de serviço";
    public static string ServiceSupplyNotFound(Guid serviceId, Guid supplyId) =>
        $"Insumo '{supplyId}' não vinculado ao serviço '{serviceId}'";

    // ServiceOrder aggregate (context-specific)
    public const string InvalidServiceOrderCustomerId = "Id do cliente da OS inválido";
    public const string InvalidServiceOrderVehicleId = "Id do veículo da OS inválido";
    public const string ServiceOrderVehicleCustomerMismatch = "Veículo não pertence ao cliente informado para a OS";

    // ServiceOrder not found (handlers - context-specific)
    public static string ServiceOrderNotFound(Guid id) => $"Ordem de Serviço '{id}' não encontrada";

    // ServiceOrder service operations (context-specific)
    public const string InvalidServiceOrderServiceId = "Id do serviço da OS inválido";
    public const string InvalidServiceOrderActorId = "Id do ator da operação é inválido";
    public const string ServiceOrderServiceAlreadyActive = "Serviço já está ativo nesta Ordem de Serviço";
    public const string ServiceOrderServiceRemovalReasonRequired = "Motivo de remoção do serviço é obrigatório";
    public const string ServiceOrderServiceInactive = "O serviço está inativo e não pode ser adicionado à Ordem de Serviço";
    public static string ServiceOrderServiceNotFound(Guid serviceId) =>
        $"Serviço '{serviceId}' não está vinculado ou ativo nesta Ordem de Serviço";

    // ServiceOrder identity (shared by diagnostic)
    public const string InvalidServiceOrderId = "Id da Ordem de Serviço inválido";

    // Diagnostic operations (context-specific)
    public const string InvalidDiagnosticMechanicId = "Id do mecânico do diagnóstico é inválido";
    public const string DiagnosticAlreadyStarted = "O diagnóstico desta Ordem de Serviço já foi iniciado";
    public const string DiagnosticNotStarted = "O diagnóstico desta Ordem de Serviço não foi iniciado";
    public const string DiagnosticAlreadyCompleted = "O diagnóstico já foi concluído";
    public const string DiagnosticDescriptionRequired = "Descrição do diagnóstico é obrigatória";
    public const string DiagnosticMustHaveAtLeastOneService = "O diagnóstico deve ter pelo menos um serviço";
    public const string DiagnosticServiceAlreadyAdded = "Serviço já adicionado ao diagnóstico";
    public static string DiagnosticServiceNotFound(Guid serviceId) =>
        $"Serviço '{serviceId}' não encontrado no diagnóstico";

    // Diagnostic consolidation (context-specific)
    public const string DiagnosticNotCompleted = "O diagnóstico desta Ordem de Serviço não foi concluído";
    public const string DiagnosticConsolidationNoServices = "O diagnóstico não possui serviços selecionados para consolidar";
}
