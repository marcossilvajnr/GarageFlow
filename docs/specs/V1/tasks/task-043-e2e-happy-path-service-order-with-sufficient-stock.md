# Task-043 — E2E Happy Path: Service Order with Sufficient Stock

## 0) Metadata
- `task_id`: `task-043`
- `slug`: `e2e-happy-path-service-order-with-sufficient-stock`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: `task-042-setup-e2e-real-db-infrastructure.md`

## 1) Objetivo
Implementar teste E2E do fluxo completo de OS com estoque suficiente, do atendimento ao fechamento.

## 2) Escopo
### In
- Criar cenário E2E cobrindo:
  - criação OS,
  - diagnóstico,
  - orçamento,
  - separação,
  - execução,
  - fechamento da OS.
- Validar estados finais esperados de `ServiceOrder`, `SeparationOrder` e `ExecutionOrder`.

### Out
- Cenário com compra.
- Cenário de cancelamento.

## 3) Contexto Canônico Obrigatório
- `docs/specs/V1/tasks/task-039-document-e2e-critical-flow-coverage.md`
- `docs/domain/agregados.md`
- `docs/specs/V1/aggregates/service-order.md`
- `docs/specs/V1/aggregates/separation-order.md`
- `docs/specs/V1/aggregates/execution-order.md`

## 4) Regras de Negócio Aplicáveis
- `RN-003`, `RN-007`, `RN-009`, `RN-011`, `RN-012`, `RN-013`.

## 5) Contratos e Interfaces
- Usar apenas endpoints públicos existentes.
- Validar `200/201` nos passos felizes e consistência de dados retornados.

## 6) Plano Técnico
### Tests
- Implementar cenário E2E único e legível com asserts de estado por etapa.

## 7) Arquivos a Criar/Alterar
- `tests/GarageFlow.Tests/E2E/**/service-order-sufficient-stock*.cs`

## 8) Critérios de Pronto
- [x] Fluxo E2E verde de ponta a ponta.
- [x] Assert de estado final da OS concluída.
- [x] Evidência reexecutável em pipeline.

## 9) Estratégia de Testes
- [x] Happy path completo com estoque suficiente.

## 10) Evidência de Execução
- Teste E2E implementado em:
  - `tests/GarageFlow.Tests/E2E/ServiceOrders/ServiceOrderSufficientStockE2ETests.cs`
- Cobertura validada no cenário:
  - fluxo completo `ServiceOrder -> Diagnostic -> Quote (accept) -> SeparationOrder -> ExecutionOrder -> fechamento da OS`;
  - asserts de HTTP por etapa crítica;
  - asserts de estados canônicos finais de `ServiceOrder`, `SeparationOrder` e `ExecutionOrder`;
  - consistência de transições com validações negativas de bypass.
- Execução dos comandos de validação:
  - `dotnet test --filter "FullyQualifiedName~E2E"`
  - `dotnet test`
