# Task-027 — Consume Stock on Execution Completion

## 0) Metadata
- `task_id`: `task-027`
- `slug`: `consume-stock-on-execution-completion`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-020-create-execution-order-base.md](task-020-create-execution-order-base.md), [task-023-integrate-separation-order-with-execution-order-readiness.md](task-023-integrate-separation-order-with-execution-order-readiness.md), [task-025-create-stock-base-and-operations.md](task-025-create-stock-base-and-operations.md), [task-026-integrate-separation-order-with-stock-reservation-and-release.md](task-026-integrate-separation-order-with-stock-reservation-and-release.md)

## 1) Objetivo
Consumir definitivamente o estoque reservado (`Stock.Consume`) quando a `ExecutionOrder` for concluída com sucesso, mantendo consistência transacional entre execução e estoque.

## 2) Escopo
### In
- Orquestrar consumo de estoque na conclusão da execução:
  - `POST /execution-orders/{id}/complete`
- Consumir itens de `Part` e `Supply` associados à `SeparationOrder` da execução.
- Garantir que o consumo utilize a quantidade previamente reservada para cada item.
- Garantir consistência transacional entre mudança de status da `ExecutionOrder` e operações de `Stock`.
- Cobrir com testes de aplicação e integração.

### Out
- Reversão de consumo após execução concluída.
- Ajuste retroativo de estoque para cancelamento pós-conclusão.
- Worker/outbox/event bus.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/Domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/Domain/agregados.md](../../../domain/agregados.md)
- [docs/Domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/specs/V1/aggregates/execution-order.md](../aggregates/execution-order.md)
- [docs/specs/V1/aggregates/separation-order.md](../aggregates/separation-order.md)
- [docs/specs/V1/aggregates/stock.md](../aggregates/stock.md)
- [docs/specs/V1/tasks/task-020-create-execution-order-base.md](task-020-create-execution-order-base.md)
- [docs/specs/V1/tasks/task-023-integrate-separation-order-with-execution-order-readiness.md](task-023-integrate-separation-order-with-execution-order-readiness.md)
- [docs/specs/V1/tasks/task-025-create-stock-base-and-operations.md](task-025-create-stock-base-and-operations.md)
- [docs/specs/V1/tasks/task-026-integrate-separation-order-with-stock-reservation-and-release.md](task-026-integrate-separation-order-with-stock-reservation-and-release.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-014` — disponibilidade de estoque nunca pode ficar negativa.
- `RN-016` — operações de estoque devem preservar invariantes de saldo/reserva.
- `RN-018` — conclusão da execução deve refletir processamento efetivo do serviço.
- `RN-020` — fluxo de separação/retomada deve permanecer consistente com execução.

Regras mandatórias desta task:
- consumo só pode ocorrer na conclusão válida da `ExecutionOrder`.
- consumo deve utilizar as quantidades reservadas na `SeparationOrder` vinculada.
- se houver falha no consumo de qualquer item, a execução não pode ser concluída.
- sem parsing de mensagem para mapear HTTP.

## 5) Contratos e Interfaces
### 5.1 API pública
- Manter contrato existente:
  - `POST /execution-orders/{id}/complete`
- Sem criação de endpoint novo nesta task.

### 5.2 Matriz de erro obrigatória
- execução inexistente -> `404`
- separação vinculada inexistente -> `404`
- estoque do item inexistente -> `404`
- transição inválida de execução -> `409`
- quantidade reservada insuficiente para consumo -> `409`

### 5.3 Contratos internos
- `CompleteExecutionOrderHandler` deve orquestrar:
  1) carregar `ExecutionOrder`
  2) carregar `SeparationOrder` vinculada (status esperado: `Completed`)
  3) consumir estoque de `Parts` e `Supplies`
  4) concluir execução
- Repositórios/portas esperadas:
  - `IExecutionOrderRepository`
  - `ISeparationOrderRepository`
  - `IStockRepository`

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- Sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- Reutilizar invariantes existentes de `Stock` para consumo (`Consume`).
- Não acoplar diretamente agregados no domínio.

### Application
- Evoluir `CompleteExecutionOrderHandler` para orquestrar consumo de estoque por item.
- Garantir atomicidade lógica: ou consome tudo e conclui, ou não conclui.

### Infrastructure
- Reutilizar repositórios atuais no mesmo scope transacional.
- Sem nova tabela nesta task.

### API
- Manter endpoint de conclusão da execução.
- Ajustar mapping de exceções para `404/409` conforme origem.

### Tests
- Aplicação: fluxo feliz, estoque insuficiente, execução em status inválido, separação ausente.
- Integração: endpoint de conclusão refletindo consumo efetivo no estoque.

## 7) Arquivos a Criar/Alterar
### Alterar (esperado)
- [src/GarageFlow.Application/Executions/Handlers/CompleteExecutionOrderHandler.cs](../../../../src/GarageFlow.Application/Executions/Handlers/CompleteExecutionOrderHandler.cs)
- `src/GarageFlow.Api/Endpoints/Executions/ExecutionOrdersEndpoints.cs` (se necessário para mapping de exceções)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs) (se necessário)
- [tests/GarageFlow.Tests/Application/Executions/ExecutionOrderHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/Executions/ExecutionOrderHandlersTests.cs)
- [tests/GarageFlow.Tests/Application/Stock/SeparationExecutionIntegrationTests.cs](../../../../tests/GarageFlow.Tests/Application/Stock/SeparationExecutionIntegrationTests.cs) (se necessário)
- [tests/GarageFlow.Tests/Integration/Executions/ExecutionOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Executions/ExecutionOrdersEndpointsTests.cs)

### Criar (opcional, se necessário)
- [tests/GarageFlow.Tests/Integration/Executions/ExecutionStockConsumptionEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Executions/ExecutionStockConsumptionEndpointsTests.cs)

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] conclusão da execução consome estoque reservado de todos os itens da separação.
- [ ] estoque insuficiente impede conclusão da execução com `409`.
- [ ] sem alteração parcial: falha em um item não conclui execução.
- [ ] contratos HTTP aderentes à matriz de erro desta task.

## 9) Estratégia de Testes
### Aplicação
- [ ] concluir execução com separação completa consome estoque e muda status para concluída.
- [ ] concluir execução com estoque reservado insuficiente retorna conflito e mantém execução não concluída.
- [ ] concluir execução em status inválido retorna conflito.
- [ ] concluir execução sem separação vinculada retorna não encontrado.

### Integração
- [ ] `POST /execution-orders/{id}/complete` retorna `200` no fluxo feliz e reflete novo saldo no `GET /stock/{itemType}/{itemId}`.
- [ ] `POST /execution-orders/{id}/complete` retorna `409` quando consumo não pode ser aplicado.
- [ ] `POST /execution-orders/{id}/complete` retorna `404` quando dependência obrigatória não existe.

## 10) Riscos e Mitigações
- Risco: concluir execução sem refletir consumo no estoque.
  - Mitigação: orquestração única no handler com persistência no mesmo `DbContext`.
- Risco: consumo parcial em caso de falha no meio do processo.
  - Mitigação: só confirmar conclusão após processar todos os itens e persistir com sucesso.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Não criar endpoint novo sem necessidade.
- [ ] Garantir mapping de erro por tipo de exceção.
- [ ] Manter identificadores em inglês e mensagens em português.
- [ ] Respeitar paths mandatórios da task.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, sem conflito com canônico.

## Guardrails Não-Negociáveis
- Proibido implementar reversão de consumo nesta task.
- Proibido adicionar worker/outbox/event bus.
- Proibido parsing de `ex.Message` para semântica HTTP.

## Assumptions
- `SeparationOrder` da execução já terá passado pelo fluxo de custódia (`Completed`) antes da conclusão da execução.
- A task usa apenas infraestrutura existente de repositórios e mapeamentos de `Stock`.
