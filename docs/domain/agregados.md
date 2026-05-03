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
| Veículo | `Vehicle` | Gestão de Clientes | `GarageFlow.Domain.Customers` |
| Serviço | `Service` | Catálogo | `GarageFlow.Domain.Catalog` |
| Peça | `Part` | Catálogo | `GarageFlow.Domain.Catalog` |
| Insumo | `Supply` | Catálogo | `GarageFlow.Domain.Catalog` |
| Fornecedor | `Supplier` | Fornecedores | `GarageFlow.Domain.Suppliers` |

## Contratos de Estado
### ServiceOrder
- Fluxo: `Received -> InDiagnostic -> WaitingApproval -> InExecution -> Finished -> Delivered`
- Gate de finalização: `Finish()` só com `CompletedServices == TotalServices`.
- Fonte única de evento de finalização: `ServiceOrderFinishedEvent` emitido em `Finish()`.
- Eventos do ciclo de diagnóstico e orçamento no boundary público (`DiagnosticStartedEvent`, `DiagnosticCompletedEvent`, `QuoteGeneratedEvent`, `QuoteApprovedEvent`) são publicados oficialmente por `ServiceOrder`.

### ExecutionOrder
- Fluxo: `Pending -> Ready -> InExecution -> Completed`
- `StartExecution(mechanicId)` exige `Status == Ready`.
- `MarkReadyToStart()` é idempotente e representa gate de prontidão de execução.

### SeparationOrder
- Fluxo com estoque: `Pending -> WaitingPickup -> Separated -> Completed`
- Fluxo sem estoque: `Pending -> WaitingPurchase -> WaitingPickup -> Separated -> Completed`
- `Completed` exige dupla confirmação de custódia (estoquista e mecânico).

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
