# GarageFlow — Catálogo Canônico de Agregados

## Objetivo
Este documento descreve os agregados canônicos do domínio no estado atual.
É uma visão de contrato público de negócio, não uma instrução de implementação incremental.

## Padrão Canônico de Implementação
Todos os agregados seguem:
- construtor privado
- factory estático `Create()`
- propriedades com `private set`
- validações com `DomainException` em português

## Catálogo
| Agregado (PT) | Classe (EN) | Bounded Context | Namespace |
|---|---|---|---|
| Ordem de Serviço | `ServiceOrder` | Gestão de Ordens de Serviço | `GarageFlow.Domain.ServiceOrders` |
| Ordem de Execução | `ExecutionOrder` | Produção | `GarageFlow.Domain.Executions` |
| Ordem de Separação | `SeparationOrder` | Gestão de Estoque | `GarageFlow.Domain.Stock` |
| Ordem de Compra | `PurchaseOrder` | Compras | `GarageFlow.Domain.Purchasing` |
| Estoque | `Stock` | Gestão de Estoque | `GarageFlow.Domain.Stock` |
| Cliente | `Customer` | Gestão de Clientes | `GarageFlow.Domain.Customers` |
| Veículo | `Vehicle` | Gestão de Clientes | `GarageFlow.Domain.Vehicles` |
| Serviço | `Service` | Catálogo | `GarageFlow.Domain.Services` |
| Peça | `Part` | Catálogo | `GarageFlow.Domain.Parts` |
| Insumo | `Supply` | Catálogo | `GarageFlow.Domain.Supplies` |
| Fornecedor | `Supplier` | Fornecedores | `GarageFlow.Domain.Suppliers` |
| Funcionário | `Employee` | Gestão de Pessoas | `GarageFlow.Domain.Employees` |

## Contratos de Estado
### Service
- Campos principais:
  - `Id`, `Code`, `Name`, `Description?`, `BasePrice`, `EstimatedDurationMinutes?`, `IsActive`
- Composição obrigatória de catálogo:
  - `Parts: IReadOnlyList<ServicePartItem>`
  - `Supplies: IReadOnlyList<ServiceSupplyItem>`
- `ServicePartItem`:
  - `PartId`, `PartName`, `Quantity`
- `ServiceSupplyItem`:
  - `SupplyId`, `SupplyName`, `Quantity`, `Unit`
- `Unit` canônico para composição de insumo:
  - `Liter | Milliliter | Gram | Kilogram | Unit`
- Regras comportamentais:
  - `AddPart(partId, partName, quantity)` exige `quantity > 0` e `PartId` não duplicado.
  - `RemovePart(partId)` exige item existente.
  - `AddSupply(supplyId, supplyName, quantity, unit)` exige `quantity > 0` e `SupplyId` não duplicado.
  - `RemoveSupply(supplyId)` exige item existente.

### Part
- Campos principais:
  - `Id`, `Code`, `Sku`, `Name`, `UnitOfMeasure`, `UnitPrice`, `IsActive`
- Regras comportamentais:
  - `Code` e `Sku` são identificadores de catálogo e seguem unicidade no contexto.
  - `UnitPrice` não aceita valor negativo.
  - `Deactivate()` aplica remoção lógica (`IsActive = false`).

### Supply
- Campos principais:
  - `Id`, `Code`, `Name`, `UnitOfMeasure`, `BaseCost`, `PreferredSupplierId?`, `IsActive`
- Regras comportamentais:
  - `Code` é identificador de catálogo e segue unicidade no contexto.
  - `BaseCost` não aceita valor negativo.
  - `PreferredSupplierId` é opcional para vínculo preferencial no catálogo.
  - `Deactivate()` aplica remoção lógica (`IsActive = false`).

### ServiceOrder
- Fluxo principal: `Received -> InDiagnostic -> WaitingApproval -> Approved -> InExecution -> Finished -> Delivered`
- Fluxo alternativo de decisão: `WaitingApproval -> Rejected`
- Gate de finalização: `Finish()` só com `CompletedServices == TotalServices`.
- Fonte única de evento de finalização: `ServiceOrderFinishedEvent` emitido em `Finish()`.
- Eventos do ciclo de diagnóstico e orçamento no boundary público (`DiagnosticStartedEvent`, `DiagnosticCompletedEvent`, `QuoteGeneratedEvent`, `QuoteApprovedEvent`) são publicados oficialmente por `ServiceOrder`.
- `ServiceOrderInExecutionEvent` e `VehicleDeliveredEvent` são eventos internos de domínio (não fazem parte do catálogo canônico de integração).
- Composição rastreável de serviços na OS:
  - cada serviço associado possui origem (`FrontDesk` ou `Diagnostic`)
  - cada inclusão/remoção preserva autoria e timestamp
  - remoção preserva motivo
- Após `Diagnostic.Completed`, a composição de serviços da OS é congelada até o próximo ciclo de atendimento.

### Diagnostic (entidade interna de ServiceOrder)
- Campos principais:
  - `Id`, `ServiceOrderId`, `MechanicId`, `Description`, `SelectedServices`, `StartedAt`, `CompletedAt`, `Status`
- `Status` canônico:
  - `InProgress | Completed`
- Regras comportamentais:
  - `Start(serviceOrderId, mechanicId)` inicia em `InProgress` e registra `DiagnosticStartedEvent`.
  - `AddService(serviceId)` só em `InProgress`, sem duplicidade (máximo 1 ocorrência por serviço).
  - `RemoveService(serviceId)` só em `InProgress`, item existente e mantendo ao menos 1 serviço selecionado.
  - `Complete(description)` exige `InProgress`, descrição obrigatória e ao menos 1 serviço; encerra com `DiagnosticCompletedEvent`.

### ServiceItem (value object interno de ServiceOrder)
- É snapshot estrutural do catálogo no momento de geração do orçamento.
- Campos:
  - `ServiceId`, `ServiceName`, `Parts`, `Supplies`
- `ServiceItemPart`:
  - `PartId`, `PartName`, `Quantity`
- `ServiceItemSupply`:
  - `SupplyId`, `SupplyName`, `Quantity`, `Unit`
- Regra canônica de preço:
  - `ServiceItem` não armazena preço.
  - O preço de mão de obra vem de `Service.BasePrice` no momento da geração de `Quote`.

### Quote
- `QuoteItem` canônico:
  - `ServiceId`, `ServiceName`, `LaborPrice`, `PartsTotal`, `SuppliesTotal`, `Subtotal`
- Fórmulas:
  - `LaborPrice` = `Service.BasePrice`
  - `PartsTotal` = soma de `Part.UnitPrice * Quantity`
  - `SuppliesTotal` = soma de `Supply.BaseCost * Quantity`
  - `Subtotal` = `LaborPrice + PartsTotal + SuppliesTotal`
  - `TotalAmount` (Quote) = soma dos `Subtotal` de todos os itens
- Regra de imutabilidade:
  - uma versão de orçamento não permite alteração de itens nem de valores após geração
  - transições permitidas por versão: `WaitingCustomerApproval -> CustomerApproved` ou `WaitingCustomerApproval -> CustomerRejected`
  - mudança solicitada pelo cliente gera nova versão e preserva histórico das anteriores

### ExecutionOrder
- Fluxo: `Pending -> Ready -> InExecution -> Completed`
- `StartExecution(mechanicId)` exige `Status == Ready`.
- `MarkReadyToStart()` é idempotente e representa gate de prontidão de execução.

### SeparationOrder
- Fluxo com estoque: `Pending -> WaitingPickup -> Separated -> Completed`
- Fluxo sem estoque: `Pending -> WaitingPurchase -> WaitingPickup -> Separated -> Completed`
- `Completed` exige dupla confirmação de custódia (estoquista e mecânico).
- Estrutura de itens separada por tipo:
  - `Parts: IReadOnlyList<SeparationPartItem>`
  - `Supplies: IReadOnlyList<SeparationSupplyItem>`
- `SeparationPartItem`:
  - `PartId`, `PartName`, `Quantity`, `IsReserved`
- `SeparationSupplyItem`:
  - `SupplyId`, `SupplyName`, `Quantity`, `Unit`, `IsReserved`
- Regra de cancelamento de separação:
  - Peças podem retornar ao estoque quando houver cancelamento antes da execução.
  - Insumos não retornam ao estoque após separação.
  - Tentativa de liberação (`Release`) para insumo deve ser rejeitada.

### PurchaseOrder
- Fluxo: `Created -> Started -> Completed`
- `SupplierId` obrigatório antes de `Start()`.
- Conclusão da compra dispara orquestração externa de reabastecimento e retomada da separação.

## Regras Canônicas Transversais
- RN-009: início da execução depende da separação concluída e é orquestrado pela camada de aplicação.
- RN-010: `ActualTimeMinutes` preserva precisão decimal de `TotalMinutes`.
- RN-013: separação concluída depende de dupla confirmação de custódia.
- RN-018: ordem de compra é gerada automaticamente pelo sistema.
- RN-020: caminho sem estoque é automático na camada de aplicação: `Replenish -> Reserve -> ResumeAfterPurchase`.
- RN-026: serviços do diagnóstico só podem ser alterados em `InProgress`.
- RN-027: mecânico não cadastra serviço no catálogo.
- RN-028: diagnóstico concluído não pode ser reaberto.
- RN-029: alterações de serviços da OS exigem rastreabilidade por origem/ator/tempo.
- RN-030: serviços da OS ficam congelados após conclusão do diagnóstico.
- RN-031: orçamento é imutável por versão; mudança gera nova versão.

## Eventos Canônicos do BC Estoque
No escopo do BC de Estoque, os eventos canônicos e seus publicadores oficiais são:

| Evento | Publicador Canônico | Escopo |
|---|---|---|
| `PartsReservedEvent` | `Stock` | Evento de domínio do BC Estoque |
| `InsufficientStockEvent` | `Stock` | Evento de integração Estoque -> Compras |
| `StockUpdatedEvent` | `Stock` | Evento de domínio do BC Estoque |

## Eventos de Integração Canônicos
Este catálogo é a fonte canônica única para eventos de integração entre bounded contexts.

| Evento | Publicador Canônico |
|---|---|
| `QuoteApprovedEvent` | `ServiceOrder` |
| `ExecutionOrderCreatedEvent` | `ExecutionOrder` |
| `ExecutionOrderReadyEvent` | `ExecutionOrder` |
| `ExecutionOrderCompletedEvent` | `ExecutionOrder` |
| `SeparationOrderCompletedEvent` | `SeparationOrder` |
| `InsufficientStockEvent` | `Stock` |
| `PurchaseOrderCompletedEvent` | `PurchaseOrder` |

## Regras de Unicidade
As regras de unicidade (CPF, CNPJ, placa, RENAVAM e códigos de catálogo) são garantidas por índice único no banco, com tradução da violação para `DomainException` em português.
