# Task-017 — Create Quote from Consolidated ServiceOrder Services

## 0) Metadata
- `task_id`: `task-017`
- `slug`: `create-quote-from-consolidated-services`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-000-template.md](task-000-template.md), [task-006-create-service-crud.md](task-006-create-service-crud.md), [task-007-create-part-crud.md](task-007-create-part-crud.md), [task-008-create-supply-crud.md](task-008-create-supply-crud.md), [task-016-consolidate-service-order-services-from-diagnostic.md](task-016-consolidate-service-order-services-from-diagnostic.md)

## 1) Objetivo
Implementar geração de orçamento (`Quote`) a partir dos serviços consolidados na `ServiceOrder`, com snapshot financeiro auditável e fluxo de decisão do cliente (`accept` / `reject`) sem edição manual dos itens de orçamento.

## 2) Escopo
### In
- Criar modelo de `Quote` associado à `ServiceOrder`.
- Gerar `Quote` a partir dos serviços ativos da OS.
- Calcular por item:
  - `LaborPrice` (BasePrice do serviço)
  - `PartsTotal`
  - `SuppliesTotal`
  - `Subtotal`
- Calcular `TotalAmount` como soma dos subtotais.
- Persistir snapshot dos valores no momento da geração.
- Expor endpoints:
  - `POST /service-orders/{id}/quote/generate`
  - `GET /service-orders/{id}/quote`
  - `POST /service-orders/{id}/quote/accept`
  - `POST /service-orders/{id}/quote/reject`
- Registrar status da decisão do orçamento (`WaitingCustomerApproval`, `CustomerApproved`, `CustomerRejected`) e timestamp de decisão.

### Out
- Edição manual de itens de orçamento.
- Versionamento múltiplo de orçamento (v2, v3, ...).
- Reabertura de orçamento aceito/rejeitado.
- Execução, separação e movimentação de estoque.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/specs/V1/aggregates/quote.md](../aggregates/quote.md)
- [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md)
- [docs/specs/V1/aggregates/service.md](../aggregates/service.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- Preço de mão de obra vem do `Service.BasePrice`.
- Valores de peças/insumos são obtidos no momento da geração do orçamento.
- Orçamento não é editado diretamente; cliente decide aceitar/rejeitar.
- Se cliente quiser ajuste, fluxo retorna ao atendimento (fora desta task).

Regras mandatórias desta task:
- geração exige ao menos 1 serviço ativo consolidado na OS.
- `accept`/`reject` só com orçamento `WaitingCustomerApproval`.
- orçamento já decidido não muda decisão nesta task.
- valores persistidos em snapshot no `Quote`.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /service-orders/{id}/quote/generate`
  - Request: sem body
  - Response: `200` com `QuoteResponse`.

- `GET /service-orders/{id}/quote`
  - Response: `200` com `QuoteResponse`.

- `POST /service-orders/{id}/quote/accept`
  - Request: sem body
  - Response: `200` com `QuoteResponse` atualizado.

- `POST /service-orders/{id}/quote/reject`
  - Request: `reason: string` (obrigatório)
  - Response: `200` com `QuoteResponse` atualizado.

### 5.2 Matriz de erro obrigatória
- `POST /quote/generate`
  - OS inexistente -> `404`
  - sem serviços consolidados -> `409`
  - serviço referenciado não encontrado/inativo no catálogo -> `409`

- `GET /quote`
  - OS inexistente -> `404`
  - quote inexistente -> `404`

- `POST /quote/accept`
  - OS inexistente -> `404`
  - quote inexistente -> `404`
  - quote não `WaitingCustomerApproval` -> `409`

- `POST /quote/reject`
  - OS inexistente -> `404`
  - quote inexistente -> `404`
  - motivo vazio -> `400`
  - quote não `WaitingCustomerApproval` -> `409`

Regras mandatórias:
- proibido parsing de `ex.Message` para mapear status HTTP.
- mapeamento por tipo/causa da exceção.

### 5.3 Contratos internos
- Commands:
  - `GenerateQuoteCommand`
  - `AcceptQuoteCommand`
  - `RejectQuoteCommand`
- Query:
  - `GetServiceOrderQuoteQuery`
- Handlers correspondentes em `Application/ServiceOrders/Handlers`.
- Repositórios:
  - `IServiceOrderRepository`
  - `IServiceRepository`
  - `IPartRepository`
  - `ISupplyRepository`

## 6) Plano Técnico por Camada
### Domain
- Evoluir `ServiceOrder` com `Quote` interno (0..1).
- Criar tipos:
  - `Quote`
  - `QuoteItem`
  - `QuoteStatus` (`WaitingCustomerApproval`, `CustomerApproved`, `CustomerRejected`)
- Regras de domínio:
  - `GenerateQuote(...)`
  - `AcceptQuote()`
  - `RejectQuote(reason)`

### Application
- Implementar handlers de geração, consulta e decisão.
- Montar cálculo financeiro com dados de catálogo e composição de serviços.
- Persistir snapshot no agregado da OS.

### Infrastructure
- Mapear `Quote` e `QuoteItems` com EF Core (`OwnsOne`/`OwnsMany`).
- Criar migration para novas estruturas de orçamento.

### API
- Adicionar endpoints de quote em `ServiceOrdersEndpoints`.
- Criar DTOs de request/response de orçamento.

### Tests
- Domínio:
  - gerar quote com totais corretos;
  - aceitar/rejeitar com transições válidas;
  - bloquear transições inválidas.
- Aplicação:
  - handlers com sucesso e falhas.
- Integração:
  - contratos HTTP dos 4 endpoints.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- [src/GarageFlow.Domain/ServiceOrders/Quote.cs](../../../../src/GarageFlow.Domain/ServiceOrders/Quote.cs)
- [src/GarageFlow.Domain/ServiceOrders/QuoteItem.cs](../../../../src/GarageFlow.Domain/ServiceOrders/QuoteItem.cs)
- [src/GarageFlow.Domain/ServiceOrders/QuoteStatus.cs](../../../../src/GarageFlow.Domain/ServiceOrders/QuoteStatus.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/GenerateQuoteCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/GenerateQuoteCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/AcceptQuoteCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/AcceptQuoteCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/RejectQuoteCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/RejectQuoteCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Queries/GetServiceOrderQuoteQuery.cs](../../../../src/GarageFlow.Application/ServiceOrders/Queries/GetServiceOrderQuoteQuery.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/GenerateQuoteHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/GenerateQuoteHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/AcceptQuoteHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/AcceptQuoteHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/RejectQuoteHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/RejectQuoteHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/GetServiceOrderQuoteHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/GetServiceOrderQuoteHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/DTOs/QuoteDto.cs](../../../../src/GarageFlow.Application/ServiceOrders/DTOs/QuoteDto.cs)
- [src/GarageFlow.Application/ServiceOrders/DTOs/QuoteItemDto.cs](../../../../src/GarageFlow.Application/ServiceOrders/DTOs/QuoteItemDto.cs)
- `src/GarageFlow.Api/DTOs/ServiceOrders/QuoteResponse.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/QuoteItemResponse.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/RejectQuoteRequest.cs`
- [tests/GarageFlow.Tests/Domain/ServiceOrders/QuoteTests.cs](../../../../tests/GarageFlow.Tests/Domain/ServiceOrders/QuoteTests.cs)
- [tests/GarageFlow.Tests/Application/ServiceOrders/QuoteHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/ServiceOrders/QuoteHandlersTests.cs)

### Alterar (esperado)
- [src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs](../../../../src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs)
- [src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs](../../../../src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs)
- [src/GarageFlow.Infrastructure/Persistence/Configurations/ServiceOrders/ServiceOrderConfiguration.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Configurations/ServiceOrders/ServiceOrderConfiguration.cs)
- `src/GarageFlow.Infrastructure/Persistence/Migrations/*`
- `src/GarageFlow.Api/Endpoints/ServiceOrders/ServiceOrdersEndpoints.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/ServiceOrderResponse.cs` (incluir quote quando existir)
- [tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs)

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] Endpoints de quote funcionais no Swagger.
- [ ] Totais de orçamento corretos e persistidos em snapshot.
- [ ] Fluxo `WaitingCustomerApproval -> CustomerApproved/CustomerRejected` respeitado.
- [ ] Sem parsing de mensagem para HTTP status.
- [ ] Migration aplicada ao novo schema de quote.

## 9) Estratégia de Testes
### Domínio
- [ ] Geração com 1+ serviços consolidados.
- [ ] Cálculo de subtotal e total.
- [ ] Bloqueio de geração sem serviços.
- [ ] Aceite válido.
- [ ] Rejeição válida com motivo.
- [ ] Bloqueio de decisão dupla.

### Aplicação
- [ ] Handlers de gerar/consultar/aceitar/rejeitar com sucesso e erro.

### Integração
- [ ] `POST /quote/generate` (`200/404/409`).
- [ ] `GET /quote` (`200/404`).
- [ ] `POST /quote/accept` (`200/404/409`).
- [ ] `POST /quote/reject` (`200/400/404/409`).

## 10) Riscos e Mitigações
- Risco: cálculo incorreto por dependência de catálogo incompleto.
  - Mitigação: validar presença de todos os serviços/itens antes de concluir geração.
- Risco: divergência de valores após mudança de catálogo.
  - Mitigação: persistir snapshot no `Quote`.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Implementar apenas quote (sem estoque/execução).
- [ ] Garantir snapshot financeiro no momento da geração.
- [ ] Garantir fluxo de decisão sem edição de item.
- [ ] Executar build e testes.

## 12) Guardrails Não-Negociáveis
- Proibido editar manualmente itens do quote nesta task.
- Proibido recalcular quote automaticamente após aceitar/rejeitar.
- Proibido implementar fluxo de execução/estoque.
- Proibido strings inline de erro.

## 13) Assumptions
- Ajustes solicitados pelo cliente retornarão ao atendimento e serão tratados por novo ciclo de consolidação/geração em task futura.
- Versionamento avançado de orçamento (múltiplas versões) será tratado em etapa posterior.
