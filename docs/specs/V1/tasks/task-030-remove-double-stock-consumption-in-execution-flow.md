# Task-030 — Remove Double Stock Consumption in Execution Flow

## 0) Metadata
- `task_id`: `task-030`
- `slug`: `remove-double-stock-consumption-in-execution-flow`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-029-consume-stock-on-stockist-withdrawal.md](task-029-consume-stock-on-stockist-withdrawal.md)

## 1) Objetivo
Remover qualquer baixa duplicada de estoque no fluxo de execução, garantindo que o consumo definitivo ocorra exclusivamente em `ConfirmStockistWithdrawal`.

## 2) Escopo
### In
- Revisar fluxo de `ExecutionOrder` para impedir novo `Stock.Consume` no ciclo de execução.
- Manter `ExecutionOrder` responsável apenas por estado operacional e confirmações do processo de execução.
- Ajustar handlers e testes para refletir o novo ponto único de baixa.

### Out
- Alterar políticas de autorização.
- Alterar contratos públicos de endpoints já existentes.
- Introduzir worker/outbox/event bus.

## 3) Contexto Canônico Obrigatório
- [docs/Domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/Domain/agregados.md](../../../domain/agregados.md)
- [docs/specs/V1/aggregates/stock.md](../aggregates/stock.md)
- [docs/specs/V1/aggregates/execution-order.md](../aggregates/execution-order.md)
- [docs/specs/V1/tasks/task-029-consume-stock-on-stockist-withdrawal.md](task-029-consume-stock-on-stockist-withdrawal.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-013` — dupla confirmação de custódia na separação.
- `RN-014` — `AvailableQuantity` nunca negativa.
- `RN-016` — operações de estoque preservam invariantes.

Regra mandatória da task:
- baixa de estoque deve ser realizada uma única vez, na retirada física confirmada do estoquista.

## 5) Contratos e Interfaces
### 5.1 API pública
- Sem criação de novos endpoints.
- Sem alteração de payloads públicos nesta task.

### 5.2 Matriz de erro obrigatória
- Fluxos de execução devem manter semântica de erro atual (`400/404/409`) sem parsing de mensagem.

### 5.3 Contratos internos
- `ExecutionOrder` e handlers correlatos não podem acionar `Stock.Consume`.
- Fonte única de consumo permanece no handler de `ConfirmStockistWithdrawal`.

## 6) Plano Técnico por Camada
### Domain
- Garantir que o agregado de execução não tenha responsabilidade de consumo de estoque.

### Application
- Remover chamadas de consumo em handlers de execução, quando houver.
- Manter transições de estado de execução consistentes.

### Infrastructure
- Sem novas tabelas.
- Ajustes apenas de wiring/repositórios se necessário.

### API
- Sem novas rotas.

### Tests
- Cobrir que execução não causa nova baixa.
- Cobrir que baixa já ocorreu no fluxo de separação.

## 7) Arquivos a Criar/Alterar
### Alterar (esperado)
- `src/GarageFlow.Application/Executions/Handlers/*`
- `src/GarageFlow.Domain/Executions/*`
- `tests/GarageFlow.Tests/Application/Executions/*`
- [tests/GarageFlow.Tests/Application/Stock/SeparationExecutionIntegrationTests.cs](../../../../tests/GarageFlow.Tests/Application/Stock/SeparationExecutionIntegrationTests.cs)

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] Nenhum fluxo de execução realiza `Stock.Consume`.
- [ ] Testes comprovam ausência de dupla baixa.

## 9) Estratégia de Testes
- [ ] teste de execução completa sem mudança adicional de saldo em estoque.
- [ ] teste de não-regressão no fluxo separação -> retirada -> execução.

## 10) Riscos e Mitigações
- Risco: manter consumo residual em código legado de execução.
  - Mitigação: busca textual por `Consume(` em handlers/agregados de execução + testes.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos docs canônicos.
- [ ] Não alterar contratos públicos sem necessidade.
- [ ] Não usar parsing de mensagem para HTTP.
- [ ] Validar teste de não dupla baixa.
