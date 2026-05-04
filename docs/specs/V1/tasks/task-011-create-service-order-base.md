# Task-011 — Create ServiceOrder Base

## 0) Metadata
- `task_id`: `task-011`
- `slug`: `create-service-order-base`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-000-template.md`, `task-001-create-customer-crud.md`, `task-003-create-vehicle-crud.md`

## 1) Objetivo
Implementar a base da Ordem de Serviço (`ServiceOrder`) com criação e leitura, estabelecendo o agregado raiz e o contrato inicial da API sem iniciar diagnóstico, orçamento ou execução.

## 2) Escopo
### In
- Criar agregado `ServiceOrder` com estado inicial `Received`.
- Implementar operações base:
  - `Create`
  - `GetById`
  - `List` paginado
- Persistir `ServiceOrder` em tabela própria com mapping EF Core.
- Expor endpoints REST base:
  - `POST /service-orders`
  - `GET /service-orders/{id}`
  - `GET /service-orders`
- Validar existência de `Customer` e `Vehicle` no `Create`.
- Garantir `CustomerId` e `VehicleId` imutáveis após criação.
- Criar testes de domínio, aplicação e integração do slice base.

### Out
- Integração de OS com serviços (atendimento).
- Diagnóstico.
- Orçamento (`Quote`), aprovação/rejeição e versionamento.
- Execução, separação, compra e estoque.
- Eventos de integração da OS neste momento.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/domain/agregados.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN)
- [RN-001]: `CustomerId` é imutável após criação da OS.
- [RN-002]: `VehicleId` é imutável após criação da OS.
- [RN-003]: status da OS segue fluxo canônico; nesta task nasce em `Received`.
- [RN-023]: sem hard delete para cadastros base consumidos pela OS.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /service-orders`
  - Request:
    - `customerId: Guid` (obrigatório)
    - `vehicleId: Guid` (obrigatório)
  - Response: `201` com `ServiceOrderResponse`.

- `GET /service-orders/{id}`
  - Response: `200` com `ServiceOrderResponse`.

- `GET /service-orders`
  - Query: `page`, `pageSize`
  - Response: `200` com `PagedServiceOrderResponse`.

### 5.2 Matriz de erro obrigatória
- `POST /service-orders`
  - `customerId` inválido -> `400`
  - `vehicleId` inválido -> `400`
  - cliente inexistente -> `404`
  - veículo inexistente -> `404`

- `GET /service-orders/{id}`
  - OS inexistente -> `404`

- `GET /service-orders`
  - `page <= 0` ou `pageSize <= 0` -> `400`

Regras mandatórias:
- Proibido parsing de mensagem (`ex.Message`) para decisão de status HTTP.
- Mapeamento de erro por tipo/causa.

### 5.3 Contratos internos
- Commands:
  - `CreateServiceOrderCommand`
- Queries:
  - `GetServiceOrderByIdQuery`
  - `ListServiceOrdersQuery`
- Handlers:
  - `CreateServiceOrderHandler`
  - `GetServiceOrderByIdHandler`
  - `ListServiceOrdersHandler`
- Repositórios:
  - `IServiceOrderRepository`
  - `ICustomerRepository` (validação de existência)
  - `IVehicleRepository` (validação de existência)

## 6) Plano Técnico por Camada
### Domain
- Criar `ServiceOrder` com:
  - `Id`, `CustomerId`, `VehicleId`, `Status`, `CreatedAt`, `UpdatedAt`
- Criar `ServiceOrderStatus` com valor inicial obrigatório `Received`.
- Factory canônica: `Create(customerId, vehicleId)`.
- Não implementar métodos de diagnóstico/quote nesta task.
- Mensagens via `DomainErrorMessages` (sem strings inline).

### Application
- Implementar command/query/handlers da OS base.
- `CreateServiceOrderHandler` valida existência de cliente e veículo antes de criar.
- Implementar mapper para `ServiceOrderDto`.
- Implementar paginação de listagem com defaults nomeados.

### Infrastructure
- Mapping EF Core para `service_orders`.
- Índices recomendados:
  - `customer_id`
  - `vehicle_id`
  - `status`
- Repositório `ServiceOrderRepository` com `Add`, `GetById`, `List`, `SaveChanges`.
- Criar migration da tabela da OS.

### API
- Criar DTOs de request/response de `ServiceOrder`.
- Criar endpoints em `ServiceOrdersEndpoints`.
- Registrar endpoints no `Program`.

### Tests
- Domínio:
  - criar OS válida
  - rejeitar criação com IDs inválidos
  - status inicial `Received`
- Aplicação:
  - create sucesso
  - create com cliente inexistente (404)
  - create com veículo inexistente (404)
  - get by id existente/inexistente
  - list com paginação válida
- Integração:
  - `POST /service-orders` retorna 201
  - `GET /service-orders/{id}` retorna 200/404
  - `GET /service-orders` retorna 200 e valida paginação 400

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs`
- `src/GarageFlow.Domain/ServiceOrders/ServiceOrderStatus.cs`
- `src/GarageFlow.Domain/ServiceOrders/IServiceOrderRepository.cs`
- `src/GarageFlow.Application/ServiceOrders/Commands/CreateServiceOrderCommand.cs`
- `src/GarageFlow.Application/ServiceOrders/Queries/GetServiceOrderByIdQuery.cs`
- `src/GarageFlow.Application/ServiceOrders/Queries/ListServiceOrdersQuery.cs`
- `src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs`
- `src/GarageFlow.Application/ServiceOrders/DTOs/PagedServiceOrderResult.cs`
- `src/GarageFlow.Application/ServiceOrders/Handlers/CreateServiceOrderHandler.cs`
- `src/GarageFlow.Application/ServiceOrders/Handlers/GetServiceOrderByIdHandler.cs`
- `src/GarageFlow.Application/ServiceOrders/Handlers/ListServiceOrdersHandler.cs`
- `src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs`
- `src/GarageFlow.Application/ServiceOrders/ServiceOrdersPaginationDefaults.cs`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/ServiceOrders/ServiceOrderConfiguration.cs`
- `src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/CreateServiceOrderRequest.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/ServiceOrderResponse.cs`
- `src/GarageFlow.Api/DTOs/ServiceOrders/PagedServiceOrderResponse.cs`
- `src/GarageFlow.Api/Endpoints/ServiceOrders/ServiceOrdersEndpoints.cs`
- `tests/GarageFlow.Tests/Domain/ServiceOrders/ServiceOrderTests.cs`
- `tests/GarageFlow.Tests/Application/ServiceOrders/ServiceOrderHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs`

### Alterar (esperado)
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs`
- `src/GarageFlow.Application/DependencyInjection.cs`
- `src/GarageFlow.Infrastructure/DependencyInjection.cs`
- `src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs`
- `src/GarageFlow.Infrastructure/Persistence/Migrations/*` (nova migration + snapshot)
- `src/GarageFlow.Api/Program.cs`

Contrato de estrutura:
- Contexto de OS deve usar pasta própria `ServiceOrders` em todas as camadas.
- Não criar pastas alternativas para o mesmo contexto.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] Testes de domínio/aplicação/integração da task verdes.
- [ ] Endpoints base de OS funcionando no Swagger.
- [ ] OS criada sempre com status inicial `Received`.
- [ ] `CustomerId` e `VehicleId` não têm mutação exposta na task.
- [ ] Sem parsing de mensagem para definição de status HTTP.

## 9) Estratégia de Testes
### Domínio
- [ ] Cobrir criação e invariantes de base.

### Aplicação
- [ ] Cobrir handlers de create/get/list com cenários de sucesso e erro.

### Integração
- [ ] Cobrir contratos HTTP base de OS.

## 10) Riscos e Mitigações
- Risco: ambiguidade sobre operações de serviços/diagnóstico na OS base.
  - Mitigação: manter explicitamente fora de escopo nesta task.
- Risco: regressão em convenções de paginação.
  - Mitigação: validar `page` e `pageSize` na borda da API.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos canônicos obrigatórios.
- [ ] Implementar apenas OS base (create/get/list).
- [ ] Não antecipar diagnóstico/quote/serviços.
- [ ] Seguir padrão de 1 tipo público por arquivo.
- [ ] Centralizar mensagens em `DomainErrorMessages`.
- [ ] Executar `dotnet build` e `dotnet test`.

## 12) Guardrails Não-Negociáveis
- Proibido implementar integração OS -> serviços nesta task.
- Proibido implementar diagnóstico ou orçamento nesta task.
- Proibido strings inline de erro.
- Proibido parsing de `ex.Message` para decidir status HTTP.

## 13) Assumptions
- Integração da OS com serviços do atendimento será tratada na `task-012`.
- Diagnóstico e integração com serviços serão tratados em tasks posteriores.
- Orçamento imutável/versionado será tratado em task específica após integração de diagnóstico.
