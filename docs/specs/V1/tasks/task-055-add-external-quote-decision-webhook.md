# Task-055 — Add External Quote Decision Webhook

## 0) Metadata
- `task_id`: `task-055`
- `slug`: `add-external-quote-decision-webhook`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-054-add-service-order-status-read-model-endpoint.md](task-054-add-service-order-status-read-model-endpoint.md)

## 1) Objetivo
Adicionar um webhook externo para receber aprovação ou recusa de orçamento do cliente, reaproveitando as regras internas atuais e registrando logs estruturados do fluxo externo.

## 2) Escopo
### In
- Criar endpoint externo para decisão de orçamento.
- Criar `HandleExternalQuoteDecisionHandler` na Application.
- Reusar `AcceptQuoteHandler` e `RejectQuoteHandler`.
- Registrar logs estruturados de recebimento e processamento da notificação.
- Retornar o estado atualizado do orçamento.

### Out
- Criar tabela para notificações externas.
- Criar migration.
- Implementar idempotência durável.
- Implementar integração real com e-mail, SMTP, SendGrid, Gmail ou similar.
- Criar outbox/inbox pattern.
- Remover os endpoints internos `quote/accept` e `quote/reject`.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md)
- [docs/specs/V1/aggregates/quote.md](../aggregates/quote.md)
- [docs/specs/V1/tasks/task-017-create-quote-from-consolidated-services.md](task-017-create-quote-from-consolidated-services.md)
- [docs/specs/V1/tasks/task-037-canonical-state-machine-conformance-gate-pre-jwt-e2e.md](task-037-canonical-state-machine-conformance-gate-pre-jwt-e2e.md)
- [docs/specs/V1/tasks/task-041-apply-end-to-end-observability-and-state-transition-logging.md](task-041-apply-end-to-end-observability-and-state-transition-logging.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)
- [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md)

## 4) Decisões Arquiteturais Já Tomadas
- O webhook externo deve ser rota dedicada, não reuso direto dos endpoints internos.
- A regra de aprovação/recusa continua nos handlers atuais.
- `HandleExternalQuoteDecisionHandler` representa semanticamente o fluxo externo.
- Observabilidade desta task será apenas por log estruturado.
- Não haverá persistência da notificação externa nesta task.
- A API externa deve converter payload externo em command de Application.

## 5) Regras de Negócio Aplicáveis
- A decisão externa só pode ser aplicada quando a OS possuir orçamento pendente de decisão.
- Aprovação move `Quote` para `CustomerApproved` e `ServiceOrder` para `Approved`.
- Recusa move `Quote` para `CustomerRejected` e `ServiceOrder` para `Rejected`.
- Recusa exige motivo.
- Decisão duplicada deve falhar.
- OS inexistente ou orçamento inexistente devem falhar com erro estável.

## 6) Contratos e Interfaces
### 6.1 API pública
- Endpoint sugerido: `POST /external/service-order-quote-notifications`
- Request:
  - `serviceOrderId: Guid`
  - `decision: Approved | Rejected`
  - `reason?: string`
  - `externalNotificationId?: string`
  - `source: string`
- Response:
  - `QuoteResponse`

### 6.2 Matriz HTTP obrigatória
- Aprovação válida -> `200`
- Recusa válida com motivo -> `200`
- Decisão inválida -> `400`
- `serviceOrderId` vazio -> `400`
- `source` vazio -> `400`
- Recusa sem motivo -> `400`
- OS inexistente -> `404`
- Orçamento inexistente -> `404`
- Orçamento já decidido -> `409`
- Falha de autorização/autenticação externa -> conforme política definida na API

### 6.3 Contratos internos
- Criar enum:
  - `ExternalQuoteDecision`
- Criar command:
  - `HandleExternalQuoteDecisionCommand`
- Criar handler:
  - `HandleExternalQuoteDecisionHandler`
- Reusar:
  - `AcceptQuoteHandler`
  - `RejectQuoteHandler`
  - `AcceptQuoteCommand`
  - `RejectQuoteCommand`
  - `QuoteDto`

### 6.4 Logs estruturados obrigatórios
- Ao receber:
  - mensagem base: `external_quote_decision_received`
  - campos: `serviceOrderId`, `decision`, `source`, `externalNotificationId`
- Ao concluir:
  - mensagem base: `external_quote_decision_processed`
  - campos: `serviceOrderId`, `decision`, `quoteId`, `quoteStatus`, `source`, `externalNotificationId`
- Em caso de erro, usar logs padrão do pipeline/exception handling; não capturar para mascarar erro.

## 7) Plano Técnico por Camada
### Domain
- Sem alteração esperada.
- Reusar invariantes de `ServiceOrder.AcceptQuote` e `ServiceOrder.RejectQuote`.

### Application
- Criar handler externo com `ILogger<HandleExternalQuoteDecisionHandler>`.
- Validar formato mínimo do command externo.
- Delegar decisão para handlers atuais.
- Registrar logs estruturados antes e depois da delegação.

### Infrastructure
- Sem migration.
- Sem nova tabela.
- Sem repositório novo.

### API
- Criar DTO de request externo.
- Criar endpoint em grupo externo, mantendo clareza de integração.
- Definir política de autenticação/autorização externa mínima sem misturar com roles internas, ou documentar justificativa se reusar auth atual.
- Reusar mapper de `QuoteDto` para `QuoteResponse`.

### Tests
- Cobrir handler externo e endpoint.
- Verificar log estruturado no teste de aplicação.

### Docs
- Documentar que a integração externa nesta task é webhook provider-neutral.
- Registrar que e-mail real fica fora de escopo.

## 8) Arquivos a Criar/Alterar
- [src/GarageFlow.Application/ServiceOrders/Enums/ExternalQuoteDecision.cs](../../../../src/GarageFlow.Application/ServiceOrders/Enums/ExternalQuoteDecision.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/HandleExternalQuoteDecisionCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/HandleExternalQuoteDecisionCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/HandleExternalQuoteDecisionHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/HandleExternalQuoteDecisionHandler.cs)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs)
- `src/GarageFlow.Api/ServiceOrders/DTOs/ExternalQuoteDecisionNotificationRequest.cs` (ou pasta externa equivalente com justificativa)
- [src/GarageFlow.Api/ServiceOrders/Endpoints/ServiceOrdersEndpoints.cs](../../../../src/GarageFlow.Api/ServiceOrders/Endpoints/ServiceOrdersEndpoints.cs) ou novo endpoint externo dedicado
- [tests/GarageFlow.Tests/Application/ServiceOrders/QuoteHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/ServiceOrders/QuoteHandlersTests.cs)
- [tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs)

Contrato de arquivos:
- Mudanças fora desta lista devem ser justificadas explicitamente na resposta final.
- Não criar migration nesta task.

## 9) Critérios de Pronto
- [ ] Webhook externo aceita aprovação.
- [ ] Webhook externo aceita recusa com motivo.
- [ ] Handler externo reusa handlers internos.
- [ ] Logs estruturados de recebimento e processamento são emitidos.
- [ ] Nenhuma tabela/migration foi criada.
- [ ] Endpoints internos de accept/reject continuam funcionando.
- [ ] Matriz HTTP obrigatória coberta.
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` verde.

## 10) Estratégia de Testes
### Domínio
- [ ] Sem novos testes obrigatórios, pois regra é reaproveitada.

### Aplicação
- [ ] aprovação externa retorna `QuoteDto` aprovado;
- [ ] recusa externa retorna `QuoteDto` recusado;
- [ ] decisão inválida falha;
- [ ] recusa sem motivo falha;
- [ ] OS inexistente falha;
- [ ] orçamento já decidido falha;
- [ ] logs `external_quote_decision_received` e `external_quote_decision_processed` são emitidos.

### Integração
- [ ] `POST /external/service-order-quote-notifications` com aprovação retorna `200`;
- [ ] request de recusa com motivo retorna `200`;
- [ ] decisão inválida retorna `400`;
- [ ] recusa sem motivo retorna `400`;
- [ ] OS inexistente retorna `404`;
- [ ] orçamento já decidido retorna `409`.

### E2E
- [ ] Não criar novo E2E obrigatório nesta task.
- [ ] Fluxos E2E existentes de aprovação/recusa devem continuar verdes.

## 11) Riscos e Mitigações
- Risco: duplicar regra de aprovação/recusa.
  - Mitigação: handler externo deve delegar para handlers atuais.
- Risco: parecer integração real de e-mail sem ser.
  - Mitigação: documentar como webhook provider-neutral.
- Risco: perder rastreabilidade por não persistir notificação.
  - Mitigação: log estruturado obrigatório com identificadores externos.
- Risco: misturar autenticação externa com RBAC interno.
  - Mitigação: decisão de política deve ser explícita no endpoint e documentada.

## 12) Checklist de Execução para IA
- [ ] Ler docs canônicos de OS, quote e observabilidade.
- [ ] Criar command/enum/handler na Application.
- [ ] Reusar `AcceptQuoteHandler` e `RejectQuoteHandler`.
- [ ] Adicionar logs estruturados.
- [ ] Não criar tabela nem migration.
- [ ] Criar endpoint externo dedicado.
- [ ] Não remover endpoints internos.
- [ ] Não capturar exceção para transformar por texto.
- [ ] Rodar `dotnet build`.
- [ ] Rodar `dotnet test`.
- [ ] Reportar evidências e logs cobertos.

## 13) Evidência Esperada de Fechamento
- Comando `dotnet build` com resultado.
- Comando `dotnet test` com contagem de testes.
- Evidência de aprovação externa.
- Evidência de recusa externa.
- Evidência de log estruturado no handler.
- Nota explícita de que persistência/idempotência durável ficou fora de escopo.
