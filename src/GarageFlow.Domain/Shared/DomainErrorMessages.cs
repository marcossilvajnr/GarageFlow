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

    // Quote generation (context-specific)
    public const string QuoteRequiresAtLeastOneItem = "O orçamento deve ter pelo menos um item";
    public const string QuoteNoConsolidatedServices = "A Ordem de Serviço não possui serviços consolidados ativos para gerar orçamento";
    public const string QuoteAlreadyExists = "Esta Ordem de Serviço já possui um orçamento gerado";
    public const string QuoteAlreadyDecided = "O orçamento desta Ordem de Serviço já foi decidido";
    public const string ServiceOrderNotWaitingForQuoteApproval = "A Ordem de Serviço não está aguardando aprovação do orçamento";
    public const string QuoteRejectionReasonRequired = "Motivo de rejeição do orçamento é obrigatório";
    public const string QuoteInvalidLaborPrice = "Preço de mão de obra não pode ser negativo";
    public const string QuoteInvalidPartsTotal = "Total de peças não pode ser negativo";
    public const string QuoteInvalidSuppliesTotal = "Total de insumos não pode ser negativo";
    public static string QuoteNotFound(Guid serviceOrderId) =>
        $"Orçamento da Ordem de Serviço '{serviceOrderId}' não encontrado";
    public static string ServiceNotAvailableForQuote(Guid serviceId) =>
        $"Serviço '{serviceId}' não está disponível no catálogo para geração de orçamento";

    // SeparationOrder aggregate (context-specific)
    public const string InvalidSeparationOrderExecutionOrderId = "Ordem de Execução é obrigatória";
    public const string SeparationOrderMustHaveAtLeastOneItem = "Separação deve ter pelo menos um item";
    public const string InvalidSeparationPartId = "Id da peça de separação inválido";
    public const string InvalidSeparationSupplyId = "Id do insumo de separação inválido";
    public const string InvalidSeparationItemName = "Item de separação inválido";
    public const string InvalidSeparationItemQuantity = "Item de separação inválido";
    public const string DuplicateSeparationPartItem = "Item de separação inválido";
    public const string DuplicateSeparationSupplyItem = "Item de separação inválido";
    public const string InvalidSeparationStockistId = "Estoquista é obrigatório";
    public const string SeparationOrderNotPending = "Separação não está Pendente";
    public const string SeparationOrderNotWaitingPurchase = "Separação não está Aguardando Compra";
    public const string SeparationOrderNotWaitingPickup = "Separação não está Aguardando Retirada";
    public const string SeparationOrderNotSeparated = "Separação não está Separada";
    public const string SeparationOrderItemsNotReserved = "Itens da separação ainda não foram reservados";
    public const string SeparationOrderWaitingStockistConfirmation = "Aguardando confirmação do estoquista";

    // SeparationOrder not found (handlers - context-specific)
    public static string SeparationOrderNotFound(Guid id) => $"Ordem de Separação '{id}' não encontrada";

    // ExecutionOrder aggregate (context-specific)
    public const string InvalidExecutionOrderServiceOrderId = "OS é obrigatória";
    public const string InvalidExecutionOrderServiceId = "Serviço é obrigatório";
    public const string InvalidExecutionOrderMechanicId = "Mecânico é obrigatório";
    public const string ExecutionOrderNotReady = "Ordem de Execução não está Pronta para Início";
    public const string ExecutionOrderNotInExecution = "Ordem de Execução não está Em Execução";

    // ExecutionOrder not found (handlers - context-specific)
    public static string ExecutionOrderNotFound(Guid id) => $"Ordem de Execução '{id}' não encontrada";

    // PurchaseOrder aggregate (context-specific)
    public const string PurchaseOrderMustHaveAtLeastOneSeparationOrder = "Deve haver pelo menos uma Ordem de Separação";
    public const string InvalidPurchaseOrderSeparationOrderId = "Deve haver pelo menos uma Ordem de Separação";
    public const string PurchaseOrderMustHaveAtLeastOneItem = "Ordem de Compra deve ter pelo menos um item";
    public const string InvalidPurchaseItemId = "Item da ordem de compra inválido";
    public const string InvalidPurchaseItemType = "Item da ordem de compra inválido";
    public const string InvalidPurchaseItemName = "Item da ordem de compra inválido";
    public const string InvalidPurchaseItemQuantity = "Item da ordem de compra inválido";
    public const string InvalidPurchaseItemUnitPrice = "Item da ordem de compra inválido";
    public const string PurchaseOrderSupplierRequired = "Fornecedor é obrigatório";
    public const string PurchaseOrderCannotChangeSupplierAfterStart = "Não é possível alterar fornecedor após início";
    public const string PurchaseOrderSupplierNotSet = "Fornecedor não foi selecionado";
    public const string PurchaseOrderNotCreated = "Ordem de Compra não está no status Criada";
    public const string PurchaseOrderNotStarted = "Ordem de Compra não está Iniciada";

    // PurchaseOrder not found (handlers - context-specific)
    public static string PurchaseOrderNotFound(Guid id) => $"Ordem de Compra '{id}' não encontrada";
}
