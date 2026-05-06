# Task-040 — Fix Execution-ServiceOrder State Sync Gap

## 0) Metadata
- `task_id`: `task-040`
- `slug`: `fix-execution-serviceorder-state-sync-gap`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: `task-039-document-e2e-critical-flow-coverage.md`

## 1) Objetivo
Eliminar a brecha de consistência entre `ExecutionOrder` e `ServiceOrder` no início da execução, garantindo falha explícita quando a OS vinculada não existir.

## 2) Escopo
### In
- Ajustar `StartExecutionOrderHandler` para não ignorar ausência de `ServiceOrder`.
- Garantir exceção canônica de `not found` quando vínculo estiver inválido.
- Ajustar testes de aplicação/integração para refletir o contrato endurecido.

### Out
- Mudanças de regra de negócio de fluxo.
- JWT/Auth.
- Novos endpoints.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- `docs/domain/agregados.md`
- `docs/domain/regras-de-negocio.md`
- `docs/specs/V1/aggregates/service-order.md`
- `docs/specs/V1/aggregates/execution-order.md`
- `docs/specs/V1/tasks/task-037-canonical-state-machine-conformance-gate-pre-jwt-e2e.md`

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-003` — progressão de status da OS.
- `RN-009` — execução só inicia após pré-condições de separação e estado.

## 5) Contratos e Interfaces
### 5.1 API pública
- Sem novos endpoints.
- `POST /execution-orders/{id}/start` deve retornar `404` quando `ServiceOrder` vinculada não existir.

### 5.2 Contratos internos
- `StartExecutionOrderHandler`:
  - buscar `ServiceOrder` por `executionOrder.ServiceOrderId`;
  - lançar `EntityNotFoundException` se não encontrada;
  - só então chamar `StartExecutionFlow()`.

### 5.3 Erros de domínio
- Reutilizar `DomainErrorMessages.ServiceOrderNotFound(...)`.

## 6) Plano Técnico por Camada
### Domain
- Sem mudança de regra adicional além do sincronismo já previsto.

### Application
- Remover null-safe no start de execução (`serviceOrder?.StartExecutionFlow()`).
- Aplicar guard obrigatório de existência da OS.

### Infrastructure
- Sem mudanças estruturais.

### API
- Sem mudança de contrato além da consequência do guard (`404` quando aplicável).

### Tests
- Ajustar seeds de testes para criar `ServiceOrder` válida onde necessário.
- Manter teste específico de `ServiceOrder` inexistente cobrindo erro.

## 7) Arquivos a Criar/Alterar
- `src/GarageFlow.Application/Executions/Handlers/StartExecutionOrderHandler.cs`
- `tests/GarageFlow.Tests/Application/Executions/ExecutionOrderHandlersTests.cs`
- `tests/GarageFlow.Tests/Application/Executions/ExecutionServiceOrderCompletionIntegrationTests.cs`
- `tests/GarageFlow.Tests/Integration/Executions/ExecutionOrdersEndpointsTests.cs`
- `tests/GarageFlow.Tests/Integration/Purchasing/PurchaseOrderSeparationIntegrationEndpointsTests.cs`

## 8) Critérios de Pronto
- [x] Brecha removida (sem null-safe silencioso).
- [x] `StartExecution` falha com `EntityNotFoundException` quando OS não existe.
- [x] Testes ajustados para contrato endurecido.
- [x] `dotnet test` verde.

## 9) Estratégia de Testes
### Aplicação
- [x] Start de execução com OS existente segue fluxo normal.
- [x] Start de execução com OS inexistente falha com `EntityNotFoundException`.

### Integração
- [x] Endpoint de start retorna `404` quando vínculo inválido.
- [x] Fluxos válidos de execução/compra/separação seguem verdes.

## 10) Riscos e Mitigações
- Risco: quebra de testes que criavam `ExecutionOrder` órfã.
  - Mitigação: ajustar seeds para sempre criar `ServiceOrder` válida exceto no cenário de erro explícito.

## 11) Checklist de Execução para IA
- [x] Não alterar regra canônica de estado.
- [x] Endurecer vínculo `ExecutionOrder -> ServiceOrder`.
- [x] Garantir regressão zero via suíte completa.

## 12) Evidência de Execução
- Ajuste aplicado no handler de start da execução.
- Suíte focada (`Execution*` e `PurchaseOrderSeparation*`) verde.
- `dotnet test` completo verde.
