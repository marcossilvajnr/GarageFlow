# Task-018 — Create ServiceOrder Quote Decision Status Gate

## 0) Metadata
- `task_id`: `task-018`
- `slug`: `create-service-order-quote-decision-status-gate`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-017-create-quote-from-consolidated-services.md](task-017-create-quote-from-consolidated-services.md)

## 1) Objetivo
Endurecer o gate de status da `ServiceOrder` a partir da decisão do orçamento, garantindo transições explícitas, contratos HTTP consistentes e cobertura de regressão para preparar `SeparationOrder` e `ExecutionOrder` nas próximas tasks.

## 2) Escopo
### In
- Formalizar transições de status da OS após decisão do orçamento:
  - `WaitingApproval -> Approved`
  - `WaitingApproval -> Rejected`
- Bloquear avanço para fluxos operacionais quando orçamento estiver rejeitado.
- Garantir que aceite/rejeição do orçamento produzem estado coerente da OS.
- Expor estado final da OS nos endpoints já existentes (`GET /service-orders/{id}` e `GET /service-orders`).
- Cobrir regras de decisão única (sem reabertura automática nesta task).

### Out
- Criação de `SeparationOrder`.
- Criação de `ExecutionOrder`.
- Reabertura/revisão de orçamento rejeitado.
- Movimentação/reserva de estoque.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md)
- [docs/specs/V1/aggregates/quote.md](../aggregates/quote.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-011` — orçamento é etapa mandatória antes do avanço operacional da OS.
- `RN-012` — cliente aprova/rejeita orçamento e decisão governa o próximo passo.
- `RN-018` — transições de status devem respeitar máquina de estados da OS.

Regras mandatórias desta task:
- orçamento `CustomerApproved` move OS para status apto às próximas etapas operacionais.
- orçamento `CustomerRejected` mantém OS bloqueada para separação/execução.
- aceite/rejeição só pode ocorrer quando orçamento estiver `WaitingCustomerApproval`.
- nomenclatura de status deve seguir o enum vigente em `ServiceOrderStatus`.

## 5) Contratos e Interfaces
### 5.1 API pública (se aplicável)
Nenhum endpoint novo obrigatório. A task evolui comportamento dos endpoints existentes:
- `POST /service-orders/{id}/quote/accept`
- `POST /service-orders/{id}/quote/reject`
- `GET /service-orders/{id}`
- `GET /service-orders`

### 5.2 Matriz de erro obrigatória
- `POST /service-orders/{id}/quote/accept`
  - OS inexistente -> `404`
  - orçamento inexistente -> `404`
  - orçamento não `WaitingCustomerApproval` -> `409`

- `POST /service-orders/{id}/quote/reject`
  - OS inexistente -> `404`
  - orçamento inexistente -> `404`
  - motivo vazio -> `400`
  - orçamento não `WaitingCustomerApproval` -> `409`

Regras mandatórias:
- proibido parsing de `ex.Message` para decidir status HTTP.
- mapeamento por tipo/causa da exceção.

### 5.3 Contratos internos
- Commands/handlers de quote (`AcceptQuote`, `RejectQuote`) devem refletir status da OS.
- `ServiceOrder` é autoridade de transição de estado.
- Eventos seguem catálogo canônico em [docs/domain/agregados.md](../../../domain/agregados.md).

### 5.4 Erros de domínio
- Mensagens em português via catálogo central (`DomainErrorMessages`).
- Sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- Ajustar regras de transição no agregado `ServiceOrder` para refletir decisão do orçamento no status global da OS.
- Garantir invariantes para impedir avanço quando orçamento rejeitado.

### Application
- Ajustar handlers de aceite/rejeição para operar exclusivamente via métodos do agregado.
- Garantir retorno consistente de DTO/status após decisão.

### Infrastructure
- Sem nova entidade obrigatória.
- Atualizar mapeamentos apenas se houver necessidade de persistir campos adicionais de status/timestamp já existentes no domínio.

### API
- Garantir que responses de OS reflitam status pós-decisão do orçamento.
- Manter matriz de erro consistente com a task.

### Tests
- Domínio: transição válida/inválida de status após `accept`/`reject`.
- Aplicação: handlers com cenários de sucesso e conflito.
- Integração: contratos HTTP dos endpoints de decisão + leitura da OS.
- Regressão opcional: se endpoint de separação existir no código atual, validar bloqueio com OS `Rejected` (`409`).

## 7) Arquivos a Criar/Alterar
### Alterar (esperado)
- [src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs](../../../../src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs)
- [src/GarageFlow.Domain/ServiceOrders/ServiceOrderStatus.cs](../../../../src/GarageFlow.Domain/ServiceOrders/ServiceOrderStatus.cs) (se necessário)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/AcceptQuoteHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/AcceptQuoteHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/RejectQuoteHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/RejectQuoteHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs)
- [src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs](../../../../src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs)
- `src/GarageFlow.Api/Endpoints/ServiceOrders/ServiceOrdersEndpoints.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/ServiceOrderResponse.cs`
- [tests/GarageFlow.Tests/Domain/ServiceOrders/QuoteTests.cs](../../../../tests/GarageFlow.Tests/Domain/ServiceOrders/QuoteTests.cs)
- [tests/GarageFlow.Tests/Application/ServiceOrders/QuoteHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/ServiceOrders/QuoteHandlersTests.cs)
- [tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs)

### Criar (opcional, somente se necessário)
- arquivos auxiliares de testes para cenários de status gate.

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.
- Não é permitido criar estrutura alternativa de pastas sem atualização prévia da task.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] Status da OS coerente após `accept`/`reject`.
- [ ] OS com orçamento rejeitado bloqueia avanço operacional (onde aplicável no código atual).
- [ ] Endpoints de leitura retornam estado atualizado da OS.
- [ ] Sem parsing de mensagem para status HTTP.
- [ ] Cobertura de cenários de conflito de decisão (`quote inexistente`, `quote não WaitingCustomerApproval`, `motivo vazio`) nos testes de integração.

## 9) Estratégia de Testes
### Domínio
- [ ] `AcceptQuote` altera status da OS para estado aprovado.
- [ ] `RejectQuote` altera status da OS para estado rejeitado.
- [ ] Bloqueio de decisão duplicada.

### Aplicação
- [ ] Handler de aceite com sucesso e conflitos.
- [ ] Handler de rejeição com sucesso e conflitos.

### Integração
- [ ] `POST /quote/accept` (`200/404/409`).
- [ ] `POST /quote/reject` (`200/400/404/409`).
- [ ] `GET /service-orders/{id}` reflete status pós-decisão.

## 10) Riscos e Mitigações
- Risco: divergência entre `QuoteStatus` e `ServiceOrderStatus`.
  - Mitigação: centralizar transição no agregado `ServiceOrder`.
- Risco: regressão em endpoints legados que dependem de status anterior.
  - Mitigação: cobrir regressão com testes de integração.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Implementar por vertical slice (Domain -> Application -> Infrastructure -> API -> Tests).
- [ ] Garantir mensagens de erro em português via catálogo central.
- [ ] Não fazer parsing de texto de mensagem para decidir status HTTP.
- [ ] Respeitar estritamente os caminhos de arquivo definidos na task.
- [ ] Reportar qualquer endpoint legado impactado pela nova regra de gate.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, desde que não conflite com o canônico.

## Guardrails Não-Negociáveis
- Proibido parsing de `ex.Message` para decidir semântica de transporte.
- Proibido mapear toda `DomainException` para um único status HTTP sem distinção da causa.
- Proibido liberar avanço operacional quando orçamento rejeitado.
- Proibido criar endpoints novos de separação/execução nesta task.

## Assumptions
- Task-019 implementará a base de `SeparationOrder`.
- Task-020 implementará a base de `ExecutionOrder`.
- Task-021 fará a integração formal `Separation -> Execution`.
