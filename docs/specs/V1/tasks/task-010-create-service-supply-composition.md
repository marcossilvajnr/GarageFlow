# Task-010 — Create Service-Supply Composition

## 0) Metadata
- `task_id`: `task-010`
- `slug`: `create-service-supply-composition`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-000-template.md`, `task-008-create-supply-crud.md`, `task-009-create-service-part-composition.md`

## 1) Objetivo
Implementar o vínculo de composição entre `Service` e `Supply`, permitindo adicionar/remover insumos em um serviço com unidade canônica `SupplyUnit` e snapshot de nome no vínculo.

## 2) Escopo
### In
- Implementar operações de composição de insumo: `AddSupply` e `RemoveSupply` no agregado `Service`.
- Expor endpoints dedicados de composição de insumo:
  - `POST /services/{id}/supplies`
  - `DELETE /services/{id}/supplies/{supplyId}`
- Reutilizar o contrato de leitura de serviço da task-009 e evoluí-lo para retornar `parts` e `supplies` por padrão nos GETs.
- Persistir coleção de insumos com unicidade por `(service_id, supply_id)`.
- Criar testes de domínio, aplicação e integração para composição de insumo.

### Out
- Endpoint para atualização de quantidade de insumo (continua add/remove only).
- Movimentação de estoque, reserva/liberação ou baixa de insumos.
- Fluxos de execução/diagnóstico/OS.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/domain/agregados.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN)
- [RN-024]: serviço mantém tempo estimado definido manualmente.
- [RN-025]: insumo é consumível; modelagem de composição deve preservar distinção entre peça e insumo.
- [RN-027]: mecânico não cadastra serviço no catálogo.
- Regra canônica de composição: `ServiceSupplyItem` sem duplicidade por `SupplyId` e `Quantity > 0`.
- Unidade canônica de insumo na composição: `SupplyUnit`.
- Política de snapshot: `SupplyName` é persistido no vínculo no momento da associação.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /services/{id}/supplies`
  - Request:
    - `supplyId: Guid` (obrigatório)
    - `quantity: decimal` (obrigatório, `> 0`)
  - Response: `200` com `ServiceResponse` atualizado.

- `DELETE /services/{id}/supplies/{supplyId}`
  - Response: `204` sem corpo.

- `GET /services/{id}` e `GET /services`
  - Devem retornar composição completa (`parts` e `supplies`) ao final desta task.

Observação de contrato:
- `SupplyUnit` e `SupplyName` devem ser obtidos do catálogo (`Supply`) no momento de `AddSupply`.

### 5.2 Matriz de erro obrigatória
- `POST /services/{id}/supplies`
  - `serviceId` inexistente -> `404`
  - `supplyId` inexistente -> `404`
  - `quantity <= 0` -> `400`
  - vínculo duplicado `(service_id, supply_id)` -> `409`

- `DELETE /services/{id}/supplies/{supplyId}`
  - `serviceId` inexistente -> `404`
  - `supplyId` não vinculado ao serviço -> `404`

Regras mandatórias:
- Proibido parsing de mensagem (`ex.Message`) para definir HTTP status.
- Mapeamento de erro por tipo/causa.

### 5.3 Contratos internos
- Commands:
  - `AddServiceSupplyCommand`
  - `RemoveServiceSupplyCommand`
- Handlers:
  - `AddServiceSupplyHandler`
  - `RemoveServiceSupplyHandler`
- Repositórios:
  - `IServiceRepository`
  - `ISupplyRepository` (consulta de existência, nome e unidade)

## 6) Plano Técnico por Camada
### Domain
- Implementar no agregado `Service`:
  - `Supplies: IReadOnlyList<ServiceSupplyItem>`
  - `AddSupply(supplyId, supplyName, quantity, unit)`
  - `RemoveSupply(supplyId)`
- Garantir invariantes:
  - `supplyId` válido
  - `quantity > 0`
  - sem duplicidade por `SupplyId`
  - unidade válida (`SupplyUnit`)
- Mensagens via `DomainErrorMessages` (sem strings inline).

### Application
- Criar commands/handlers de add/remove de insumo.
- Carregar `Supply` para obter `SupplyName` e `SupplyUnit` no snapshot da composição.
- Reutilizar DTOs/mappers de serviço da task-009 e garantir preenchimento de `supplies`.

### Infrastructure
- Evoluir mapping de composição de serviço para suportar persistência de insumos com índice único `(service_id, supply_id)`.
- Garantir carregamento da coleção de insumos nos métodos de leitura do serviço.
- Traduzir violação de unicidade para `DomainException`.

### API
- Criar DTO de request para `POST /services/{id}/supplies`.
- Adicionar endpoints dedicados de composição de insumo no grupo `/services`.
- Manter shape de resposta de serviço com composição completa.

### Tests
- Domínio:
  - adicionar insumo válido
  - adicionar insumo duplicado (erro)
  - adicionar insumo com quantidade inválida (erro)
  - remover insumo vinculado
  - remover insumo inexistente (erro)
- Aplicação:
  - handlers com cenários de sucesso e erro (404/400/409)
- Integração:
  - endpoints de add/remove e leitura da composição de insumos nos GETs de serviço

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Application/Services/Commands/AddServiceSupplyCommand.cs`
- `src/GarageFlow.Application/Services/Commands/RemoveServiceSupplyCommand.cs`
- `src/GarageFlow.Application/Services/Handlers/AddServiceSupplyHandler.cs`
- `src/GarageFlow.Application/Services/Handlers/RemoveServiceSupplyHandler.cs`
- `src/GarageFlow.Api/DTOs/Services/AddServiceSupplyRequest.cs`
- `tests/GarageFlow.Tests/Domain/Services/ServiceCompositionSupplyTests.cs`
- `tests/GarageFlow.Tests/Application/Services/ServiceCompositionSupplyHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/Services/ServiceSupplyCompositionEndpointsTests.cs`

### Alterar (esperado)
- `src/GarageFlow.Domain/Services/Service.cs`
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs`
- `src/GarageFlow.Application/Services/DTOs/ServiceDto.cs`
- `src/GarageFlow.Application/Services/Handlers/ServiceMapper.cs`
- `src/GarageFlow.Application/DependencyInjection.cs`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/Services/ServiceConfiguration.cs`
- `src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceRepository.cs`
- `src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs`
- `src/GarageFlow.Api/DTOs/Services/ServiceResponse.cs`
- `src/GarageFlow.Api/DTOs/Services/PagedServiceResponse.cs`
- `src/GarageFlow.Api/Endpoints/Services/ServicesEndpoints.cs`

Contrato de estrutura:
- Composição de serviço deve permanecer em `Services`.
- Não criar abstrações paralelas fora da trilha definida na task-009.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] Testes de domínio/aplicação/integração da task verdes.
- [ ] Endpoints de composição de insumo funcionando no Swagger.
- [ ] `GET /services/{id}` e `GET /services` retornando `parts` e `supplies` completos.
- [ ] Unicidade por `(service_id, supply_id)` aplicada e traduzida para `DomainException`.
- [ ] `SupplyUnit` persistido no vínculo de composição.

## 9) Estratégia de Testes
### Domínio
- [ ] Cobrir invariantes de `AddSupply`/`RemoveSupply`.

### Aplicação
- [ ] Cobrir handlers de composição de insumo com repositórios fake.

### Integração
- [ ] Cobrir endpoints de add/remove e leitura de composição.

## 10) Riscos e Mitigações
- Risco: inconsistência de unidade/nome entre catálogo e composição.
  - Mitigação: resolver `SupplyName` e `SupplyUnit` exclusivamente do catálogo no handler.
- Risco: duplicidade de vínculo em concorrência.
  - Mitigação: índice único composto + tradução para `DomainException`.

## 11) Checklist de Execução para IA
- [ ] Confirmar execução prévia da `task-009`.
- [ ] Implementar operações de composição de insumo mantendo contrato de resposta da 009.
- [ ] Garantir `SupplyUnit` canônico no domínio e na persistência.
- [ ] Executar build/testes da suíte relevante.

## 12) Guardrails Não-Negociáveis
- Proibido parsing de `ex.Message` para decisão de HTTP status.
- Proibido strings inline de erro.
- Proibido endpoint de update de quantidade nesta task.
- Proibido introduzir movimento de estoque ou regra de OS.

## 13) Assumptions
- Task de OS será recriada depois, após composição consolidada.
- `task-010` é incremental sobre `task-009`.
