# Task-045 — E2E Customer Cancellation at Latest Allowed Stage

## 0) Metadata
- `task_id`: `task-045`
- `slug`: `e2e-customer-cancellation-at-latest-allowed-stage`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: [task-044-e2e-happy-path-service-order-with-stock-shortage-and-purchase.md](task-044-e2e-happy-path-service-order-with-stock-shortage-and-purchase.md)

## 1) Objetivo
Implementar teste E2E de cancelamento da OS pelo cliente no último ponto permitido pelas regras canônicas.

## 2) Escopo
### In
- Identificar e executar cancelamento no limite operacional permitido.
- Validar bloqueio de etapas posteriores.
- Validar consistência de estado de OS e entidades correlatas.

### Out
- Regras administrativas excepcionais fora do fluxo padrão.

## 3) Contexto Canônico Obrigatório
- [docs/specs/V1/tasks/task-039-document-e2e-critical-flow-coverage.md](task-039-document-e2e-critical-flow-coverage.md)
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md)

## 4) Regras de Negócio Aplicáveis
- Regras de cancelamento e janelas de transição da OS no canônico vigente.

## 5) Contratos e Interfaces
- Endpoints existentes de OS e etapas correlatas.
- HTTP e estado final conforme regra canônica.

## 6) Plano Técnico
### Tests
- Cenário E2E focado em “último momento permitido”.
- Asserts de proibição de avanço após cancelamento.

## 7) Arquivos a Criar/Alterar
- `tests/GarageFlow.Tests/E2E/**/service-order-cancellation-latest-stage*.cs`

## 8) Critérios de Pronto
- [x] Cancelamento no limite permitido validado.
- [x] Estado final consistente e sem bypass.
- [x] Cenário reexecutável em pipeline.

## 9) Estratégia de Testes
- [x] Fluxo de cancelamento no último estágio permitido.

## 10) Evidência de Execução
- Teste E2E implementado em:
  - [tests/GarageFlow.Tests/E2E/ServiceOrders/ServiceOrderCancellationLatestStageE2ETests.cs](../../../../tests/GarageFlow.Tests/E2E/ServiceOrders/ServiceOrderCancellationLatestStageE2ETests.cs)
- Cobertura validada no cenário:
  - cancelamento do cliente no último estágio permitido do fluxo atual (`WaitingApproval`) via `quote/reject`;
  - asserts de HTTP por etapa crítica;
  - estado final da `ServiceOrder` em `Rejected`;
  - bloqueio de avanço pós-cancelamento (`quote/accept`, reinício de diagnóstico, regeneração de orçamento);
  - ausência de criação de agregados correlatos (`ExecutionOrder`, `SeparationOrder`, `PurchaseOrder`).
- Execução dos comandos de validação:
  - `dotnet test --filter "FullyQualifiedName~E2E"`
  - `dotnet test`
