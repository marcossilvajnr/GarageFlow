# Task-012 — Integrate ServiceOrder with Services (FrontDesk)

## 0) Metadata
- `task_id`: `task-012`
- `slug`: `create-service-order-service-integration-frontdesk`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-000-template.md](task-000-template.md), [task-006-create-service-crud.md](task-006-create-service-crud.md), [task-011-create-service-order-base.md](task-011-create-service-order-base.md)

## 1) Objetivo
Integrar a Ordem de Serviço com o catálogo de serviços no fluxo de atendimento, permitindo adicionar e remover serviços com rastreabilidade completa de origem, ator e tempo.

## 2) Escopo
### In
- Implementar composição de serviços dentro de `ServiceOrder`.
- Implementar operações de atendimento:
  - `AddService` (origem `FrontDesk`)
  - `RemoveService` (origem `FrontDesk`, com motivo obrigatório)
- Registrar trilha auditável de alterações de serviço na OS.
- Expor endpoints dedicados:
  - `POST /service-orders/{id}/services`
  - `DELETE /service-orders/{id}/services/{serviceId}`
- Atualizar contratos de leitura da OS para expor serviços e histórico de alterações.
- Validar existência e status ativo de `Service` ao adicionar.
- Criar testes de domínio, aplicação e integração do fluxo.

### Out
- Diagnóstico (`Diagnostic`) e operações de mecânico.
- Geração/aprovação/rejeição/versionamento de orçamento (`Quote`).
- Integração com execução, separação, compra e estoque.
- Alterações por origem `Diagnostic` (ficam para task posterior).

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN)
- [RN-001]: `CustomerId` é imutável após criação da OS.
- [RN-002]: `VehicleId` é imutável após criação da OS.
- [RN-003]: status da OS segue fluxo canônico.
- [RN-029]: alteração de serviços da OS exige rastreabilidade (origem, ator, tempo).
- [RN-030]: congelamento após diagnóstico existe no domínio, mas **não é acionado nesta task**.

Regras mandatórias desta etapa:
- serviço só pode existir uma vez como ativo na OS.
- remoção não apaga histórico.
- remoção exige motivo.
- origem da alteração nesta task é exclusivamente `FrontDesk`.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /service-orders/{id}/services`
  - Request:
    - `serviceId: Guid` (obrigatório)
    - `actorId: Guid` (obrigatório)
  - Response: `200` com `ServiceOrderResponse` atualizado.

- `DELETE /service-orders/{id}/services/{serviceId}`
  - Request:
    - `actorId: Guid` (obrigatório)
    - `reason: string` (obrigatório)
  - Response: `204` sem corpo.

- `GET /service-orders/{id}`
  - Response inclui:
    - `services` (estado atual dos serviços vinculados à OS)
    - `serviceHistory` (histórico de add/remove)

- `GET /service-orders`
  - Response paginado mantém contrato da task-011 e inclui, por item, os serviços ativos.

### 5.2 Matriz de erro obrigatória
- `POST /service-orders/{id}/services`
  - `serviceOrderId` inexistente -> `404`
  - `serviceId` inexistente -> `404`
  - `service` inativo -> `400`
  - `actorId` inválido -> `400`
  - serviço já ativo na OS -> `409`

- `DELETE /service-orders/{id}/services/{serviceId}`
  - `serviceOrderId` inexistente -> `404`
  - serviço não vinculado/ativo na OS -> `404`
  - `actorId` inválido -> `400`
  - `reason` vazia -> `400`

Regras mandatórias:
- proibido parsing de `ex.Message` para decidir status HTTP.
- mapeamento de erro por tipo/causa.

### 5.3 Contratos internos
- Commands:
  - `AddServiceToServiceOrderCommand`
  - `RemoveServiceFromServiceOrderCommand`
- Handlers:
  - `AddServiceToServiceOrderHandler`
  - `RemoveServiceFromServiceOrderHandler`
- Repositórios:
  - `IServiceOrderRepository`
  - `IServiceRepository`

## 6) Plano Técnico por Camada
### Domain
- Evoluir `ServiceOrder` com:
  - `Services: IReadOnlyList<ServiceOrderServiceItem>`
  - `ServiceHistory: IReadOnlyList<ServiceOrderServiceHistory>`
- Implementar:
  - `AddService(serviceId, actorId, source)`
  - `RemoveService(serviceId, actorId, source, reason)`
- Tipos internos:
  - `ServiceOrderServiceItem`
  - `ServiceOrderServiceHistory`
  - `ServiceOrderServiceAction` (`Added`, `Removed`)
  - `ServiceSource` (`FrontDesk`, `Diagnostic`)
- Nesta task, aceitar somente `ServiceSource.FrontDesk`.
- Mensagens via `DomainErrorMessages` (sem strings inline).

### Application
- Criar commands/handlers de add/remove.
- Validar existência do `Service` e `IsActive`.
- Persistir alterações da OS com rastreabilidade.
- Atualizar DTO/mappers da OS para `services` e `serviceHistory`.

### Infrastructure
- Mapear coleções internas da OS com `OwnsMany`:
  - tabela `service_order_services`
  - tabela `service_order_service_history`
- Garantir unicidade de serviço ativo por OS.
- Atualizar repository de OS para carregar coleções nas leituras.
- Criar migration correspondente.

### API
- Criar DTOs de request para add/remove de serviço.
- Atualizar `ServiceOrdersEndpoints` com os endpoints de composição.
- Atualizar DTOs de response da OS.

### Tests
- Domínio:
  - adicionar serviço válido
  - adicionar serviço duplicado (erro)
  - remover serviço existente com motivo
  - remover serviço não vinculado (erro)
  - remover sem motivo (erro)
  - registrar histórico em add/remove
- Aplicação:
  - add/remove com sucesso
  - `serviceOrder` não encontrada
  - `service` inexistente/inativo
  - validação de `actorId` e `reason`
- Integração:
  - `POST /service-orders/{id}/services` (200/404/409/400)
  - `DELETE /service-orders/{id}/services/{serviceId}` (204/404/400)
  - `GET /service-orders/{id}` retornando `services` e `serviceHistory`

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- [src/GarageFlow.Domain/ServiceOrders/ServiceOrderServiceItem.cs](../../../../src/GarageFlow.Domain/ServiceOrders/ServiceOrderServiceItem.cs)
- [src/GarageFlow.Domain/ServiceOrders/ServiceOrderServiceHistory.cs](../../../../src/GarageFlow.Domain/ServiceOrders/ServiceOrderServiceHistory.cs)
- [src/GarageFlow.Domain/ServiceOrders/ServiceOrderServiceAction.cs](../../../../src/GarageFlow.Domain/ServiceOrders/ServiceOrderServiceAction.cs)
- [src/GarageFlow.Domain/ServiceOrders/ServiceSource.cs](../../../../src/GarageFlow.Domain/ServiceOrders/ServiceSource.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/AddServiceToServiceOrderCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/AddServiceToServiceOrderCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Commands/RemoveServiceFromServiceOrderCommand.cs](../../../../src/GarageFlow.Application/ServiceOrders/Commands/RemoveServiceFromServiceOrderCommand.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/AddServiceToServiceOrderHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/AddServiceToServiceOrderHandler.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/RemoveServiceFromServiceOrderHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/RemoveServiceFromServiceOrderHandler.cs)
- `src/GarageFlow.Api/DTOs/ServiceOrders/AddServiceToServiceOrderRequest.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/RemoveServiceFromServiceOrderRequest.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/ServiceOrderServiceResponse.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/ServiceOrderServiceHistoryResponse.cs`
- [tests/GarageFlow.Tests/Domain/ServiceOrders/ServiceOrderServicesTests.cs](../../../../tests/GarageFlow.Tests/Domain/ServiceOrders/ServiceOrderServicesTests.cs)
- [tests/GarageFlow.Tests/Application/ServiceOrders/ServiceOrderServiceHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/ServiceOrders/ServiceOrderServiceHandlersTests.cs)

### Alterar (esperado)
- [src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs](../../../../src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs)
- [src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs](../../../../src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs)
- [src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs)
- [src/GarageFlow.Infrastructure/Persistence/Configurations/ServiceOrders/ServiceOrderConfiguration.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Configurations/ServiceOrders/ServiceOrderConfiguration.cs)
- [src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs)
- [src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs](../../../../src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs)
- `src/GarageFlow.Infrastructure/Persistence/Migrations/*` (nova migration + snapshot)
- `src/GarageFlow.Api/DTOs/ServiceOrders/ServiceOrderResponse.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/PagedServiceOrderResponse.cs`
- `src/GarageFlow.Api/Endpoints/ServiceOrders/ServiceOrdersEndpoints.cs`
- [tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs)

Contrato de estrutura:
- manter `ServiceOrders` como contexto único em todas as camadas.
- não criar estrutura paralela para composição de serviços da OS.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] Testes da task verdes (domínio, aplicação e integração).
- [ ] Endpoints de add/remove funcionando no Swagger.
- [ ] `GET /service-orders/{id}` retornando `services` e `serviceHistory`.
- [ ] Rastreabilidade obrigatória aplicada (origem, ator, tempo, motivo de remoção).
- [ ] Sem parsing de mensagem para mapeamento HTTP.

## 9) Estratégia de Testes
### Domínio
- [ ] Cobrir invariantes de add/remove e histórico.

### Aplicação
- [ ] Cobrir handlers com sucesso e erros de regra/cadastro.

### Integração
- [ ] Cobrir contratos HTTP de composição e leitura da OS.

## 10) Riscos e Mitigações
- Risco: perda de rastreabilidade por remoção física.
  - Mitigação: manter histórico append-only obrigatório.
- Risco: ambiguidade entre fluxo de atendimento e diagnóstico.
  - Mitigação: restringir origem a `FrontDesk` nesta task.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Implementar somente integração OS+serviços no atendimento.
- [ ] Não antecipar diagnóstico/quote.
- [ ] Garantir origem `FrontDesk` nas operações da task.
- [ ] Executar `dotnet build` e `dotnet test`.

## 12) Guardrails Não-Negociáveis
- Proibido implementar lógica de diagnóstico nesta task.
- Proibido implementar orçamento nesta task.
- Proibido apagar histórico de alterações de serviço da OS.
- Proibido strings inline de erro.
- Proibido parsing de `ex.Message` para decisão de status HTTP.

## 13) Assumptions
- Congelamento de serviços após conclusão de diagnóstico será aplicado na task de diagnóstico.
- Integração de origem `Diagnostic` será tratada em task posterior.
- Versionamento/imutabilidade de `Quote` será tratado em task específica.
