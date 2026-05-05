# GarageFlow — Mapeamento Linguagem Ubíqua -> Código

## Objetivo
Este documento mapeia os termos do domínio em português para os nomes em inglês usados no código C#.
Todo o código deve seguir este mapeamento para garantir consistência entre documentação e implementação.

---

## Contratos Canônicos de Implementação
- **Unicidade:** regras de unicidade (CPF/CNPJ/placa/RENAVAM/códigos) são garantidas por índice único no banco.
- **Normalização textual:** aplicar `trim` nas bordas em entradas textuais de agregados.
- **Itens internos:** validação obrigatória item a item em `ServiceItem`, `QuoteItem`, `SeparationPartItem`, `SeparationSupplyItem` e itens de composição de serviço.

---

## Agregados e Entidades

| Domínio (PT) | Código (EN) | Namespace |
|--------------|-------------|-----------|
| Ordem de Serviço | ServiceOrder | GarageFlow.Domain.ServiceOrders |
| Diagnóstico | Diagnostic | GarageFlow.Domain.ServiceOrders |
| Orçamento | Quote | GarageFlow.Domain.ServiceOrders |
| Item de Serviço | ServiceItem | GarageFlow.Domain.ServiceOrders |
| Histórico de Serviços da OS | ServiceOrderServiceHistory | GarageFlow.Domain.ServiceOrders |
| Ordem de Execução | ExecutionOrder | GarageFlow.Domain.Executions |
| Ordem de Separação | SeparationOrder | GarageFlow.Domain.Stock |
| Ordem de Compra | PurchaseOrder | GarageFlow.Domain.Purchasing |
| Cliente | Customer | GarageFlow.Domain.Customers |
| Veículo | Vehicle | GarageFlow.Domain.Vehicles |
| Serviço | Service | GarageFlow.Domain.Services |
| Peça | Part | GarageFlow.Domain.Parts |
| Insumo | Supply | GarageFlow.Domain.Supplies |
| Fornecedor | Supplier | GarageFlow.Domain.Suppliers |
| Funcionário | Employee | GarageFlow.Domain.Employees |
| Estoque | Stock | GarageFlow.Domain.Stock |

---

## Tipos Internos de Composição

| Domínio (PT) | Código (EN) | Uso |
|--------------|-------------|-----|
| Item de Peça do Serviço | ServicePartItem | Composição de `Service` |
| Item de Insumo do Serviço | ServiceSupplyItem | Composição de `Service` |
| Origem do Serviço na OS | ServiceSource | `FrontDesk` \| `Diagnostic` |
| Item de Peça no Snapshot da OS | ServiceItemPart | Snapshot em `ServiceItem` |
| Item de Insumo no Snapshot da OS | ServiceItemSupply | Snapshot em `ServiceItem` |
| Item de Peça da Separação | SeparationPartItem | `SeparationOrder` |
| Item de Insumo da Separação | SeparationSupplyItem | `SeparationOrder` |
| Item do Orçamento | QuoteItem | `Quote` |

---

## Value Objects
Namespace único: `GarageFlow.Domain.ValueObjects`
Caminho físico: `GarageFlow.Domain/ValueObjects/[Nome].cs`

| Domínio (PT) | Código (EN) | Usado em |
|--------------|-------------|----------|
| CPF | Cpf | Customer |
| CNPJ | Cnpj | Customer, Supplier, Employee |
| Placa | LicensePlate | Vehicle |
| RENAVAM | Renavam | Vehicle |
| E-mail | Email | Customer, Supplier, Employee |
| Telefone | PhoneNumber | Customer, Supplier, Employee |
| Endereço | Address | Customer, Supplier, Employee |

Decisões vigentes:
- `Money` não é Value Object; valores monetários são `decimal`.
- `Name` não é Value Object; nomes são `string` validadas no agregado.

---

## Enums de Status

### ServiceOrderStatus
| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Recebida | Received |
| Em Diagnóstico | InDiagnostic |
| Aguardando Aprovação | WaitingApproval |
| Aprovada | Approved |
| Rejeitada | Rejected |
| Em Execução | InExecution |
| Finalizada | Finished |
| Entregue | Delivered |

### DiagnosticStatus
| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Em andamento | InProgress |
| Concluído | Completed |

### ExecutionOrderStatus
| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Pendente | Pending |
| Pronta para Início | Ready |
| Em Execução | InExecution |
| Concluída | Completed |

### SeparationOrderStatus
| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Pendente | Pending |
| Aguardando Compra | WaitingPurchase |
| Aguardando Retirada | WaitingPickup |
| Separada | Separated |
| Concluída | Completed |

### PurchaseOrderStatus
| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Criada | Created |
| Iniciada | Started |
| Concluída | Completed |

### QuoteStatus
| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Aguardando Aprovação do Cliente | WaitingCustomerApproval |
| Aprovado pelo Cliente | CustomerApproved |
| Rejeitado pelo Cliente | CustomerRejected |

### Unidade de medida canônica para insumos (`SupplyUnit`)
| Domínio (PT) | Código (EN) |
|--------------|-------------|
| Litro | Liter |
| Mililitro | Milliliter |
| Grama | Gram |
| Quilograma | Kilogram |
| Unidade | Unit |

---

## Métodos de Domínio

| Domínio (PT) | Código (EN) | Agregado/Entidade |
|--------------|-------------|-------------------|
| Criar OS | Create() | ServiceOrder |
| Iniciar Diagnóstico | StartDiagnostic() | ServiceOrder |
| Concluir Diagnóstico | CompleteDiagnostic() | ServiceOrder |
| Aprovar Orçamento | ApproveQuote() | ServiceOrder |
| Rejeitar Orçamento | RejectQuote() | ServiceOrder |
| Finalizar OS | Finish() | ServiceOrder |
| Registrar Entrega | RegisterDelivery() | ServiceOrder |
| Adicionar Serviço ao Diagnóstico | AddService() | Diagnostic |
| Remover Serviço do Diagnóstico | RemoveService() | Diagnostic |
| Concluir Diagnóstico Interno | Complete() | Diagnostic |
| Adicionar Peça ao Serviço | AddPart() | Service |
| Remover Peça do Serviço | RemovePart() | Service |
| Adicionar Insumo ao Serviço | AddSupply() | Service |
| Remover Insumo do Serviço | RemoveSupply() | Service |
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
| Liberar Reserva (somente peça) | Release() | Stock |
| Repor Estoque | Replenish() | Stock |
| Atribuir Fornecedor | AssignSupplier() | PurchaseOrder |
| Iniciar Compra | Start() | PurchaseOrder |
| Concluir Compra | Complete() | PurchaseOrder |
| Desativar | Deactivate() | Customer, Vehicle, Service, Part, Supply, Supplier, Employee |

---

## Regra Canônica de Preço no Orçamento
- `LaborPrice` de `QuoteItem` vem de `Service.BasePrice` no momento da geração do orçamento.
- `ServiceItem` não armazena preço; mantém somente snapshot estrutural.

## Regra Canônica de Governança do Orçamento
- `Quote` é imutável por versão após geração.
- Cada versão só pode transicionar de `WaitingCustomerApproval` para `CustomerApproved` ou `CustomerRejected`.
- Mudança de escopo solicitada pelo cliente gera nova versão de orçamento no atendimento.

---

## Repositórios

| Domínio (PT) | Código (EN) | Namespace |
|--------------|-------------|-----------|
| Repositório de OS | IServiceOrderRepository | GarageFlow.Domain.ServiceOrders |
| Repositório de OE | IExecutionOrderRepository | GarageFlow.Domain.Executions |
| Repositório de Ordem de Separação | ISeparationOrderRepository | GarageFlow.Domain.Stock |
| Repositório de OdC | IPurchaseOrderRepository | GarageFlow.Domain.Purchasing |
| Repositório de Cliente | ICustomerRepository | GarageFlow.Domain.Customers |
| Repositório de Veículo | IVehicleRepository | GarageFlow.Domain.Vehicles |
| Repositório de Serviço | IServiceRepository | GarageFlow.Domain.Services |
| Repositório de Peça | IPartRepository | GarageFlow.Domain.Parts |
| Repositório de Insumo | ISupplyRepository | GarageFlow.Domain.Supplies |
| Repositório de Fornecedor | ISupplierRepository | GarageFlow.Domain.Suppliers |
| Repositório de Funcionário | IEmployeeRepository | GarageFlow.Domain.Employees |
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

## Eventos de Integração
O catálogo canônico de eventos de integração está centralizado em
`docs/domain/agregados.md`, na seção **Eventos de Integração Canônicos**.
Este documento mantém apenas o mapeamento de linguagem ubíqua.
