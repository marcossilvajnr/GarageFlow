# Task-023 — Integrate SeparationOrder With ExecutionOrder Readiness

## 0) Metadata
- `task_id`: `task-023`
- `slug`: `integrate-separation-order-with-execution-order-readiness`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-019-create-separation-order-base.md`, `task-020-create-execution-order-base.md`, `task-022-integrate-purchase-order-with-separation-order-resume.md`

## 1) Objetivo
Implementar a integração de aplicação entre `SeparationOrder` e `ExecutionOrder` para suportar o gate de prontidão: ao concluir separação com dupla confirmação, a execução vinculada deve ser marcada como `Ready`.

## 2) Escopo
### In
- Implementar fluxo de integração na camada de aplicação para marcar `ExecutionOrder` como pronta quando `SeparationOrder` for concluída.
- Definir contrato explícito entre contextos por handler/orquestrador de aplicação (sem acoplamento direto entre agregados).
- Garantir transição de execução:
  - `ExecutionOrder: Pending -> Ready` ao término da separação.
- Cobrir o fluxo com testes de aplicação e integração.

### Out
- Início automático da execução (`Ready -> InExecution`) sem ação do mecânico.
- Worker/outbox/event bus externo.
- Fechamento automático da `ServiceOrder` ao concluir execução.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/domain/agregados.md)
- [docs/specs/V1/aggregates/separation-order.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/separation-order.md)
- [docs/specs/V1/aggregates/execution-order.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/execution-order.md)
- [docs/architecture/application-and-integrations.md](/Users/marcos/Projects/GarageFlow/docs/architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-009` — execução só inicia após dupla confirmação de custódia da separação.
- `RN-011` / `RN-013` — separação concluída representa transferência confirmada para execução.

Regras mandatórias desta task:
- Prontidão da execução só pode ocorrer quando `SeparationOrder.Status == Completed`.
- A integração deve afetar apenas a execução vinculada à separação (`ExecutionOrderId`).
- Se execução não existir, retornar erro controlado (`404`).
- Se execução estiver em estado incompatível para `mark-ready`, retornar conflito (`409`) conforme regra vigente.

## 5) Contratos e Interfaces
### 5.1 API pública
- manter endpoint existente de separação:
  - `POST /separation-orders/{id}/confirm-mechanic-receipt`
- comportamento estendido:
  - após confirmação do mecânico e separação `Completed`, execução vinculada deve ser marcada como `Ready`.

### 5.2 Matriz de erro obrigatória
- confirmação de recebimento:
  - separação inexistente -> `404`
  - transição inválida da separação -> `409`
  - execução vinculada inexistente -> `404`
  - execução em estado inválido para prontidão -> `409`

Regras mandatórias:
- proibido parsing de `ex.Message` para definir HTTP status.
- mapear por tipo/causa da exceção.

### 5.3 Contratos internos
- Orquestrador/handler de aplicação:
  - confirmar recebimento da separação;
  - carregar execução vinculada;
  - aplicar `MarkReadyToStart()` na execução;
  - persistir operação consistente.

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- manter agregados desacoplados (`SeparationOrder` não chama `ExecutionOrder` diretamente).
- usar regras existentes de transição da execução (`MarkReadyToStart` idempotente).

### Application
- evoluir handler de confirmação de recebimento da separação para orquestrar prontidão da execução.
- manter tratamento de falhas consistente.

### Infrastructure
- garantir repositórios necessários no fluxo integrado.
- persistência consistente na mesma unidade lógica.

### API
- manter contrato REST estável.
- refletir corretamente erros de integração de estado.

### Tests
- Aplicação: sucesso/falha da prontidão da execução após conclusão da separação.
- Integração: endpoint de confirmação refletindo mudança de estado da execução.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `tests/GarageFlow.Tests/Application/Stock/SeparationExecutionIntegrationTests.cs`
- `tests/GarageFlow.Tests/Integration/Stock/SeparationExecutionIntegrationEndpointsTests.cs`

### Alterar (esperado)
- `src/GarageFlow.Application/Stock/Handlers/ConfirmSeparationMechanicReceiptHandler.cs`
- `src/GarageFlow.Application/DependencyInjection.cs`
- `src/GarageFlow.Api/Endpoints/Stock/SeparationOrdersEndpoints.cs` (apenas mapping de erro/comportamento)
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs` (se necessário)
- `tests/GarageFlow.Tests/Integration/Stock/SeparationOrdersEndpointsTests.cs`

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] confirmação do mecânico conclui separação e marca execução vinculada como `Ready`.
- [ ] erros aderentes para execução inexistente/estado inválido.
- [ ] sem acoplamento direto entre agregados.

## 9) Estratégia de Testes
### Aplicação
- [ ] fluxo feliz: separação `Completed` -> execução `Ready`.
- [ ] execução vinculada inexistente -> erro mapeado.
- [ ] separação em estado inválido -> erro mapeado.

### Integração
- [ ] `POST /separation-orders/{id}/confirm-mechanic-receipt` retorna `200` e execução fica `Ready`.
- [ ] retorna `404` para execução inexistente.
- [ ] retorna `409` para conflito de estado.

## 10) Riscos e Mitigações
- Risco: concluir separação sem refletir prontidão na execução.
  - Mitigação: orquestração explícita no handler de confirmação final.
- Risco: inconsistência entre estados em falha parcial.
  - Mitigação: persistência na mesma unidade lógica da operação.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Implementar integração na camada de aplicação, sem acoplamento de domínio.
- [ ] Garantir mensagens de erro em português via catálogo central.
- [ ] Não fazer parsing de mensagem para status HTTP.
- [ ] Respeitar caminhos de arquivo da task.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, sem conflito com canônico.

## Guardrails Não-Negociáveis
- Proibido iniciar execução automaticamente nesta task.
- Proibido implementar worker/outbox/event bus.
- Proibido strings inline de erro.
- Proibido mapping HTTP por parsing de texto.

## Assumptions
- Fechamento de `ServiceOrder` por conclusão de execução ficará para task posterior.
