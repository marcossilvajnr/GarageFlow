# Task-024 — Close ServiceOrder After All Executions Completed

## 0) Metadata
- `task_id`: `task-024`
- `slug`: `close-service-order-after-all-executions-completed`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-020-create-execution-order-base.md`, `task-023-integrate-separation-order-with-execution-order-readiness.md`

## 1) Objetivo
Implementar o fechamento da `ServiceOrder` quando todas as `ExecutionOrders` vinculadas forem concluídas, garantindo consistência do ciclo operacional da OS.

## 2) Escopo
### In
- Implementar integração de aplicação para avaliar fechamento da OS após conclusão de execução.
- Definir regra de fechamento:
  - somente fechar quando todas as execuções da OS estiverem `Completed`.
- Atualizar status da `ServiceOrder` para estado final canônico.
- Cobrir com testes de aplicação e integração.

### Out
- Reabertura de OS finalizada.
- Faturamento/pagamento pós-serviço.
- Notificações externas.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/domain/agregados.md)
- [docs/specs/V1/aggregates/service-order.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/service-order.md)
- [docs/specs/V1/aggregates/execution-order.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/execution-order.md)
- [docs/architecture/application-and-integrations.md](/Users/marcos/Projects/GarageFlow/docs/architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-009` — execução segue gate de prontidão.
- `RN-010` — execução concluída registra tempo real.

Regras mandatórias desta task:
- fechar OS apenas quando todas execuções vinculadas estiverem concluídas.
- manter OS aberta caso exista ao menos uma execução não concluída.
- sem acoplamento direto de agregados (orquestração na aplicação).

## 5) Contratos e Interfaces
### 5.1 API pública
- manter endpoint existente de execução:
  - `POST /execution-orders/{id}/complete`
- comportamento estendido:
  - após concluir execução, aplicação avalia e fecha OS se elegível.

### 5.2 Matriz de erro obrigatória
- execução inexistente -> `404`
- transição inválida da execução -> `409`
- OS vinculada inexistente -> `404`

Regras mandatórias:
- proibido parsing de `ex.Message` para definir HTTP status.
- mapear por tipo/causa da exceção.

### 5.3 Contratos internos
- Orquestrador/handler de aplicação:
  - concluir execução;
  - listar execuções da OS;
  - se todas `Completed`, atualizar status da OS para final.

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- manter agregados desacoplados.
- preservar invariantes existentes de `ExecutionOrder` e `ServiceOrder`.

### Application
- evoluir fluxo de conclusão da execução para avaliar fechamento da OS.
- implementar regra de “all completed”.

### Infrastructure
- garantir consulta de execuções por `ServiceOrderId`.
- persistência consistente da atualização de status da OS.

### API
- manter contrato REST estável.
- refletir erros de integração adequadamente.

### Tests
- Aplicação: fechar OS quando última execução concluir.
- Integração: endpoint de conclusão refletindo status final da OS.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `tests/GarageFlow.Tests/Application/Executions/ExecutionServiceOrderCompletionIntegrationTests.cs`
- `tests/GarageFlow.Tests/Integration/Executions/ExecutionServiceOrderCompletionEndpointsTests.cs`

### Alterar (esperado)
- `src/GarageFlow.Application/Executions/Handlers/CompleteExecutionOrderHandler.cs`
- `src/GarageFlow.Application/DependencyInjection.cs`
- `src/GarageFlow.Api/Endpoints/Executions/ExecutionOrdersEndpoints.cs` (mapping de erro/comportamento)
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs` (se necessário)
- `tests/GarageFlow.Tests/Application/Executions/ExecutionOrderHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/Executions/ExecutionOrdersEndpointsTests.cs`

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] conclusão da última execução fecha a OS.
- [ ] conclusão parcial não fecha a OS.
- [ ] erros aderentes para execução/OS inexistentes.

## 9) Estratégia de Testes
### Aplicação
- [ ] concluir execução intermediária mantém OS aberta.
- [ ] concluir última execução fecha OS.
- [ ] execução vinculada a OS inexistente retorna erro mapeado.

### Integração
- [ ] `POST /execution-orders/{id}/complete` com última execução leva OS ao status final.
- [ ] `POST /execution-orders/{id}/complete` com execuções pendentes mantém OS em andamento.

## 10) Riscos e Mitigações
- Risco: fechar OS antes da hora.
  - Mitigação: regra explícita de “all completed”.
- Risco: inconsistência entre status de execução e OS.
  - Mitigação: orquestração única na conclusão de execução.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Implementar integração na camada de aplicação.
- [ ] Garantir mensagens de erro em português via catálogo central.
- [ ] Não fazer parsing de mensagem para status HTTP.
- [ ] Respeitar caminhos de arquivo da task.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, sem conflito com canônico.

## Guardrails Não-Negociáveis
- Proibido implementar faturamento/pagamento nesta task.
- Proibido worker/outbox/event bus.
- Proibido strings inline de erro.
- Proibido mapping HTTP por parsing de texto.

## Assumptions
- O status final exato da `ServiceOrder` deve seguir o canônico vigente no domínio.
