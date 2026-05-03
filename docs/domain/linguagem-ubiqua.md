# GarageFlow — Mapeamento Linguagem Ubíqua → Código

## Objetivo

Este documento mapeia os termos do domínio em português
para os nomes em inglês usados no código C#.
Todo o código deve seguir este mapeamento para garantir
consistência entre a documentação e a implementação.

---

## Contratos Canônicos de Implementação

- **Unicidade:** regras de unicidade (CPF/CNPJ/placa/RENAVAM/códigos) são garantidas por índice único no banco. Violações devem ser traduzidas para `DomainException` em português.
- **Normalização textual:** para entradas de texto de agregados, aplicar `trim` nas bordas antes de validar e armazenar. Espaços internos devem ser preservados.
- **Itens internos:** validação é obrigatória item a item nos métodos de criação/geração dos agregados (`ServiceItem`, `SeparationItem`, `PurchaseItem`, `QuoteItem`).

---

## Agregados e Entidades

| Domínio (PT) | Código (EN) | Namespace |
|--------------|-------------|-----------|
| Ordem de Serviço | ServiceOrder | GarageFlow.Domain.ServiceOrders |
| Diagnóstico | Diagnostic | GarageFlow.Domain.ServiceOrders |
| Orçamento | Quote | GarageFlow.Domain.ServiceOrders |
| Item de Serviço | ServiceItem | GarageFlow.Domain.ServiceOrders |
| Ordem de Execução | ExecutionOrder | GarageFlow.Domain.Executions |
| Ordem de Separação | SeparationOrder | GarageFlow.Domain.Stock |
| Ordem de Compra | PurchaseOrder | GarageFlow.Domain.Purchasing |
| Cliente | Customer | GarageFlow.Domain.Customers |
| Veículo | Vehicle | GarageFlow.Domain.Customers |
| Serviço | Service | GarageFlow.Domain.Catalog |
| Peça | Part | GarageFlow.Domain.Catalog |
| Insumo | Supply | GarageFlow.Domain.Catalog |
| Fornecedor | Supplier | GarageFlow.Domain.Suppliers |
| Estoque | Stock | GarageFlow.Domain.Stock |

---

## Value Objects

Namespace único: `GarageFlow.Domain.ValueObjects`
Caminho físico: `GarageFlow.Domain/ValueObjects/[Nome].cs`

| Domínio (PT) | Código (EN) | Usado em |
|--------------|-------------|----------|
| CPF | Cpf | Customer |
| CNPJ | Cnpj | Customer, Supplier |
| Placa | LicensePlate | Vehicle |
| RENAVAM | Renavam | Vehicle |
| E-mail | Email | Customer, Supplier |
| Telefone | PhoneNumber | Customer, Supplier |
| Endereço | Address | Customer, Supplier |

> **Decisões vigentes:**
> - `Money` (Dinheiro) não existe como Value Object. Valores monetários são `decimal` nos agregados.
> - `Name` (Nome) não existe como Value Object. Nome é `string` validada no agregado.

---

## Status — Ordem de Serviço

| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Recebida | Received |
| Em Diagnóstico | InDiagnostic |
| Aguardando Aprovação | WaitingApproval |
| Em Execução | InExecution |
| Finalizada | Finished |
| Entregue | Delivered |

---

## Status — Ordem de Execução

| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Pendente | Pending |
| Pronta para Início | Ready |
| Em Execução | InExecution |
| Concluída | Completed |

---

## Status — Ordem de Separação

| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Pendente | Pending |
| Aguardando Compra | WaitingPurchase |
| Aguardando Retirada | WaitingPickup |
| Separada | Separated |
| Concluída | Completed |

---

## Status — Ordem de Compra

| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Criada | Created |
| Iniciada | Started |
| Concluída | Completed |

---

## Métodos de Domínio

| Domínio (PT) | Código (EN) | Agregado |
|--------------|-------------|----------|
| Criar OS | Create() | ServiceOrder |
| Iniciar Diagnóstico | StartDiagnostic() | ServiceOrder |
| Concluir Diagnóstico | CompleteDiagnostic() | ServiceOrder |
| Aprovar Orçamento | ApproveQuote() | ServiceOrder |
| Finalizar OS | Finish() | ServiceOrder |
| Registrar Entrega | RegisterDelivery() | ServiceOrder |
| Marcar Execução Pronta | MarkReadyToStart() | ExecutionOrder |
| Iniciar Execução | StartExecution() | ExecutionOrder |
| Concluir Execução | CompleteExecution() | ExecutionOrder |
| Reservar Itens da Separação | Reserve() | SeparationOrder |
| Aguardar Compra | WaitForPurchase() | SeparationOrder |
| Retomar após Compra | ResumeAfterPurchase() | SeparationOrder |
| Confirmar Retirada (Estoquista) | ConfirmStockistWithdrawal() | SeparationOrder |
| Confirmar Recebimento (Mecânico) | ConfirmMechanicReceipt() | SeparationOrder |
| Reservar Estoque | Reserve() | Stock |
| Baixar Estoque | Decrease() | Stock |
| Liberar Reserva | Release() | Stock |
| Repor Estoque | Replenish() | Stock |
| Atribuir Fornecedor | AssignSupplier() | PurchaseOrder |
| Iniciar Compra | Start() | PurchaseOrder |
| Concluir Compra | Complete() | PurchaseOrder |
| Desativar | Deactivate() | Customer, Vehicle, Service, Part, Supply |

---

## Eventos de Integração

O catálogo canônico de eventos de integração está centralizado em
`docs/Domain/agregados.md`, na seção **Eventos de Integração Canônicos**.
Este documento mantém somente o mapeamento de linguagem ubíqua e não replica
o catálogo formal para evitar drift documental.

---

## Repositórios

| Domínio (PT) | Código (EN) | Namespace |
|--------------|-------------|-----------|
| Repositório de OS | IServiceOrderRepository | GarageFlow.Domain.ServiceOrders |
| Repositório de OE | IExecutionOrderRepository | GarageFlow.Domain.Executions |
| Repositório de Ordem de Separação | ISeparationOrderRepository | GarageFlow.Domain.Stock |
| Repositório de OdC | IPurchaseOrderRepository | GarageFlow.Domain.Purchasing |
| Repositório de Cliente | ICustomerRepository | GarageFlow.Domain.Customers |
| Repositório de Veículo | IVehicleRepository | GarageFlow.Domain.Customers |
| Repositório de Serviço | IServiceRepository | GarageFlow.Domain.Catalog |
| Repositório de Peça | IPartRepository | GarageFlow.Domain.Catalog |
| Repositório de Insumo | ISupplyRepository | GarageFlow.Domain.Catalog |
| Repositório de Fornecedor | ISupplierRepository | GarageFlow.Domain.Suppliers |
| Repositório de Estoque | IStockRepository | GarageFlow.Domain.Stock |

---

## Atores

| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Cliente | Customer |
| Atendente | Attendant |
| Mecânico | Mechanic |
| Estoquista | Stockist |
| Administrativo | Administrative |

---

## Exceções de Domínio

| Categoria | Classe (EN) | Uso Canônico |
|-----------|-------------|--------------|
| Base | DomainException | Violação genérica de regra de domínio |
| Base | NotFoundException | Entidade/agregado não encontrado em operação de domínio |
| Fluxo/Status | InvalidStatusException | Transição inválida de status em fluxo de agregado |
| Estoque | InsufficientStockException | Tentativa de reserva/execução com saldo insuficiente |
| Validação de Documento | InvalidCpfException | CPF inválido em criação/atualização de entidade |
| Validação de Documento | InvalidCnpjException | CNPJ inválido em criação/atualização de entidade |
| Validação Veicular | InvalidLicensePlateException | Placa inválida em cadastro de veículo |
| Validação Veicular | InvalidRenavamException | RENAVAM inválido em cadastro de veículo |
| Validação de Contato | InvalidEmailException | E-mail inválido em cadastro de cliente/fornecedor |
| Validação de Contato | InvalidPhoneNumberException | Telefone inválido em cadastro de cliente/fornecedor |
| Validação de Endereço | InvalidAddressException | Endereço inválido/incompleto em cadastro |

> Regra vigente: mensagens de erro devem permanecer em português e a aplicação pode traduzir falhas técnicas de persistência (ex.: unicidade) para `DomainException`.
