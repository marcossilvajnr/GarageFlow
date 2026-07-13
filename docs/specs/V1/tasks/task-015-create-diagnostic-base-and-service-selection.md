# Task-015 — Create Diagnostic Base and Service Selection

## 0) Metadata
- `task_id`: `task-015`
- `slug`: `create-diagnostic-base-and-service-selection`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-000-template.md](task-000-template.md), [task-011-create-service-order-base.md](task-011-create-service-order-base.md), [task-012-create-service-order-service-integration-frontdesk.md](task-012-create-service-order-service-integration-frontdesk.md)

## 1) Objetivo
Implementar o núcleo de diagnóstico na `ServiceOrder`, permitindo ao mecânico iniciar diagnóstico, selecionar/remover serviços do catálogo e concluir diagnóstico com rastreabilidade e invariantes de domínio.

## 2) Escopo
### In
- Criar `Diagnostic` como entidade interna da `ServiceOrder`.
- Implementar operações de domínio:
  - `Start(serviceOrderId, mechanicId)`
  - `AddService(serviceId)`
  - `RemoveService(serviceId)`
  - `Complete(description)`
- Persistir estado de diagnóstico e serviços selecionados.
- Expor endpoints dedicados:
  - `POST /service-orders/{id}/diagnostic/start`
  - `POST /service-orders/{id}/diagnostic/services`
  - `DELETE /service-orders/{id}/diagnostic/services/{serviceId}`
  - `POST /service-orders/{id}/diagnostic/complete`
- Incluir dados de diagnóstico em `GET /service-orders/{id}`.
- Garantir rastreabilidade mínima (`mechanicId`, `startedAt`, `completedAt`, `status`).

### Out
- Geração de orçamento (`Quote`) e cálculo financeiro.
- Integração com separação, execução, estoque e compras.
- Reabertura de diagnóstico após conclusão.
- Aprovação/rejeição de orçamento.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/specs/V1/aggregates/diagnostic.md](../aggregates/diagnostic.md)
- [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- RN-026: serviços só podem ser adicionados/removidos enquanto `Diagnostic.Status == InProgress`.
- RN-027: mecânico não cadastra serviço no catálogo (apenas seleciona serviço existente).
- RN-028: diagnóstico não pode ser reaberto após `Completed`.

Regras mandatórias desta task:
- `Complete(description)` exige descrição não vazia.
- diagnóstico deve ter pelo menos 1 serviço para concluir.
- `RemoveService` não pode remover o único serviço selecionado.
- `serviceId` não pode duplicar em `SelectedServices`.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /service-orders/{id}/diagnostic/start`
  - Request: `mechanicId: Guid`
  - Response: `200` com `ServiceOrderResponse` atualizado.

- `POST /service-orders/{id}/diagnostic/services`
  - Request: `serviceId: Guid`
  - Response: `200` com `ServiceOrderResponse` atualizado.

- `DELETE /service-orders/{id}/diagnostic/services/{serviceId}`
  - Request: sem body
  - Response: `204`.

- `POST /service-orders/{id}/diagnostic/complete`
  - Request: `description: string`
  - Response: `200` com `ServiceOrderResponse` atualizado.

- `GET /service-orders/{id}`
  - Response inclui `diagnostic` completo (status, serviço(s), timestamps, descrição quando existir).

### 5.2 Matriz de erro obrigatória
- `POST /diagnostic/start`
  - OS inexistente -> `404`
  - `mechanicId` inválido -> `400`
  - diagnóstico já iniciado/concluído -> `409`

- `POST /diagnostic/services`
  - OS inexistente -> `404`
  - serviço inexistente/inativo -> `404`/`400`
  - diagnóstico não iniciado -> `409`
  - diagnóstico concluído -> `409`
  - serviço duplicado -> `409`

- `DELETE /diagnostic/services/{serviceId}`
  - OS inexistente -> `404`
  - diagnóstico não iniciado/concluído -> `409`
  - serviço inexistente no diagnóstico -> `404`
  - tentativa de remover único serviço -> `409`

- `POST /diagnostic/complete`
  - OS inexistente -> `404`
  - diagnóstico não iniciado -> `409`
  - descrição vazia -> `400`
  - sem serviços -> `409`
  - diagnóstico já concluído -> `409`

Regras mandatórias:
- proibido parsing de `ex.Message` para mapear status HTTP.
- mapear status por tipo/causa da exceção.

### 5.3 Contratos internos
- Commands:
  - `StartDiagnosticCommand`
  - `AddDiagnosticServiceCommand`
  - `RemoveDiagnosticServiceCommand`
  - `CompleteDiagnosticCommand`
- Handlers correspondentes em `Application/ServiceOrders/Handlers`.
- Repositórios:
  - `IServiceOrderRepository`
  - `IServiceRepository`

## 6) Plano Técnico por Camada
### Domain
- Criar tipos de diagnóstico em `ServiceOrders`:
  - `Diagnostic`
  - `DiagnosticStatus` (`InProgress`, `Completed`)
- Atualizar `ServiceOrder` para conter diagnóstico e métodos de orquestração.
- Garantir invariantes com `DomainException` e mensagens via `DomainErrorMessages`.

### Application
- Criar commands/handlers para start/add/remove/complete.
- Validar existência/atividade de serviço ao adicionar.
- Atualizar DTO/mappers da OS para incluir `diagnostic`.

### Infrastructure
- Mapear `Diagnostic` e coleção de `SelectedServices` com EF Core (`OwnsOne`/`OwnsMany`).
- Atualizar repositório para carregar `Diagnostic` nas leituras.
- Criar migration.

### API
- Criar DTOs de request/response de diagnóstico.
- Evoluir `ServiceOrdersEndpoints` com endpoints de diagnóstico.
- Garantir códigos HTTP da matriz.

### Tests
- Domínio: invariantes de ciclo de vida do diagnóstico.
- Aplicação: handlers com sucesso e falhas de regra.
- Integração: contratos HTTP dos 4 endpoints + leitura de OS com diagnóstico.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- [src/GarageFlow.Domain/ServiceOrders/Diagnostic.cs](../../../../src/GarageFlow.Domain/ServiceOrders/Diagnostic.cs)
- [src/GarageFlow.Domain/ServiceOrders/DiagnosticStatus.cs](../../../../src/GarageFlow.Domain/ServiceOrders/DiagnosticStatus.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/StartDiagnosticCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/StartDiagnosticCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/AddDiagnosticServiceCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/AddDiagnosticServiceCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/RemoveDiagnosticServiceCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/RemoveDiagnosticServiceCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/CompleteDiagnosticCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/CompleteDiagnosticCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/StartDiagnosticHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/StartDiagnosticHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/AddDiagnosticServiceHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/AddDiagnosticServiceHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/RemoveDiagnosticServiceHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/RemoveDiagnosticServiceHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/CompleteDiagnosticHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/CompleteDiagnosticHandler.cs)
- `src/GarageFlow.Api/DTOs/ServiceOrders/StartDiagnosticRequest.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/AddDiagnosticServiceRequest.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/CompleteDiagnosticRequest.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/DiagnosticResponse.cs`
- [tests/GarageFlow.Tests/Domain/ServiceOrders/DiagnosticTests.cs](../../../../tests/GarageFlow.Tests/Domain/ServiceOrders/DiagnosticTests.cs)
- [tests/GarageFlow.Tests/Application/ServiceOrders/DiagnosticHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/ServiceOrders/DiagnosticHandlersTests.cs)

### Alterar (esperado)
- [src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs](../../../../src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs)
- [src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs](../../../../src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs)
- [src/GarageFlow.Infrastructure/Persistence/Configurations/ServiceOrders/ServiceOrderConfiguration.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Configurations/ServiceOrders/ServiceOrderConfiguration.cs)
- [src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs)
- `src/GarageFlow.Infrastructure/Persistence/Migrations/*`
- `src/GarageFlow.Api/Endpoints/ServiceOrders/ServiceOrdersEndpoints.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/ServiceOrderResponse.cs`
- [tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs)

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] Testes da task verdes (domínio, aplicação e integração).
- [ ] Endpoints de diagnóstico disponíveis e funcionais no Swagger.
- [ ] `GET /service-orders/{id}` retorna estado de diagnóstico.
- [ ] Regras RN-026/RN-028 respeitadas.
- [ ] Sem parsing de mensagem para status HTTP.

## 9) Estratégia de Testes
### Domínio
- [ ] Iniciar diagnóstico com dados válidos.
- [ ] Impedir reabertura após concluído.
- [ ] Adicionar serviço duplicado (erro).
- [ ] Remover único serviço (erro).
- [ ] Concluir sem descrição (erro).
- [ ] Concluir sem serviços (erro).

### Aplicação
- [ ] Cobrir handlers de start/add/remove/complete com cenários de sucesso e erro.

### Integração
- [ ] Validar contratos HTTP e matriz de erro de todos os endpoints de diagnóstico.

## 10) Riscos e Mitigações
- Risco: conflito entre serviços da recepção e do diagnóstico.
  - Mitigação: nesta task diagnóstico mantém sua própria seleção, sem consolidar com orçamento.
- Risco: ambiguidade de transição de status.
  - Mitigação: centralizar transições no domínio com métodos explícitos.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos docs canônicos.
- [ ] Implementar somente escopo de diagnóstico base.
- [ ] Não antecipar orçamento/execução/estoque.
- [ ] Garantir matriz de erro sem parsing de mensagem.
- [ ] Executar build e testes.

## 12) Guardrails Não-Negociáveis
- Proibido implementar `Quote` nesta task.
- Proibido reabrir diagnóstico concluído.
- Proibido cadastro de serviço via fluxo do mecânico.
- Proibido strings inline de erro.

## 13) Assumptions
- Consolidação entre serviços de atendimento e diagnóstico será tratada em task seguinte.
- Eventos de integração do diagnóstico serão definidos quando o fluxo assíncrono for introduzido.
