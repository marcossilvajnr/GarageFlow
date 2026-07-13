# Task-044 — E2E Happy Path: Service Order with Stock Shortage and Purchase

## 0) Metadata
- `task_id`: `task-044`
- `slug`: `e2e-happy-path-service-order-with-stock-shortage-and-purchase`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: [task-043-e2e-happy-path-service-order-with-sufficient-stock.md](task-043-e2e-happy-path-service-order-with-sufficient-stock.md)

## 1) Objetivo
Implementar teste E2E do fluxo de OS com falta de estoque, compra, retomada da separação e conclusão da execução.

## 2) Escopo
### In
- Cobrir fluxo com ruptura de estoque e ativação de `PurchaseOrder`.
- Validar retomada de `SeparationOrder` após compra concluída.
- Validar conclusão de execução e fechamento de OS.

### Out
- Cancelamento do cliente.

## 3) Contexto Canônico Obrigatório
- [docs/specs/V1/tasks/task-039-document-e2e-critical-flow-coverage.md](task-039-document-e2e-critical-flow-coverage.md)
- [docs/specs/V1/aggregates/purchase-order.md](../aggregates/purchase-order.md)
- [docs/specs/V1/aggregates/separation-order.md](../aggregates/separation-order.md)
- [docs/specs/V1/aggregates/execution-order.md](../aggregates/execution-order.md)

## 4) Regras de Negócio Aplicáveis
- `RN-012`, `RN-017`, `RN-020`.

## 5) Contratos e Interfaces
- Endpoints existentes de separação, compra e execução.
- Verificar transições de estado e respostas HTTP esperadas.

## 6) Plano Técnico
### Tests
- Cenário E2E com asserts explícitos no ciclo:
  `WaitingPurchase -> PurchaseCompleted -> Resume -> Ready/Completed`.

## 7) Arquivos a Criar/Alterar
- `tests/GarageFlow.Tests/E2E/**/service-order-stock-shortage-purchase*.cs`

## 8) Critérios de Pronto
- [x] Fluxo E2E com compra verde.
- [x] Retomada automática validada.
- [x] Fechamento da OS validado.

## 9) Estratégia de Testes
- [x] Happy path completo com ruptura e compra.

## 10) Evidência de Execução
- Teste E2E implementado em:
  - [tests/GarageFlow.Tests/E2E/ServiceOrders/ServiceOrderStockShortagePurchaseE2ETests.cs](../../../../tests/GarageFlow.Tests/E2E/ServiceOrders/ServiceOrderStockShortagePurchaseE2ETests.cs)
- Cobertura validada no cenário:
  - fluxo completo `ServiceOrder -> Diagnostic -> Quote (accept) -> SeparationOrder (ruptura) -> PurchaseOrder -> retomada -> ExecutionOrder -> fechamento da OS`;
  - asserts de HTTP por etapa crítica;
  - asserts de estados canônicos finais de `ServiceOrder`, `SeparationOrder`, `PurchaseOrder` e `ExecutionOrder`;
  - consistência de transições com validações negativas de bypass.
- Execução dos comandos de validação:
  - `dotnet test --filter "FullyQualifiedName~E2E"`
  - `dotnet test`
