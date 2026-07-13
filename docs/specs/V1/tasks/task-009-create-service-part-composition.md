# Task-009 — Create Service-Part Composition

## 0) Metadata
- `task_id`: `task-009`
- `slug`: `create-service-part-composition`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-000-template.md](task-000-template.md), [task-006-create-service-crud.md](task-006-create-service-crud.md), [task-007-create-part-crud.md](task-007-create-part-crud.md)

## 1) Objetivo
Implementar o vínculo de composição entre `Service` e `Part`, permitindo adicionar/remover peças em um serviço e expor essa composição nos contratos de leitura do catálogo.

## 2) Escopo
### In
- Evoluir o agregado `Service` para suportar `Parts` como composição interna canônica nesta task.
- Implementar operações de composição de peça: `AddPart` e `RemovePart`.
- Expor endpoints dedicados de composição de peça:
  - `POST /services/{id}/parts`
  - `DELETE /services/{id}/parts/{partId}`
- Incluir composição de peças (`parts`) nas respostas de `GET /services/{id}` e `GET /services`.
- Persistir coleção de peças com unicidade por `(service_id, part_id)`.
- Criar testes de domínio, aplicação e integração para composição de peça.

### Out
- Operações de composição de insumo (ficam para `task-010`).
- Atualização de quantidade por endpoint dedicado (nesta etapa é apenas add/remove).
- Movimentação de estoque ou orquestração de execução/OS.
- Mensageria/eventos externos.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN)
- [RN-024]: serviço mantém tempo estimado definido manualmente.
- [RN-025]: distinção entre peça e insumo deve ser preservada na modelagem.
- [RN-027]: mecânico não cadastra serviço no catálogo.
- Regra canônica de composição: `ServicePartItem` sem duplicidade por `PartId` e `Quantity > 0`.
- Política de snapshot: `PartName` é persistido no vínculo no momento da associação.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /services/{id}/parts`
  - Request:
    - `partId: Guid` (obrigatório)
    - `quantity: int` (obrigatório, `> 0`)
  - Response: `200` com `ServiceResponse` atualizado.

- `DELETE /services/{id}/parts/{partId}`
  - Response: `204` sem corpo.

- `GET /services/{id}`
  - Response inclui:
    - `parts: ServicePartResponse[]`

- `GET /services`
  - Response paginada inclui composição de peças (`parts`) por item.

### 5.2 Matriz de erro obrigatória
- `POST /services/{id}/parts`
  - `serviceId` inexistente -> `404`
  - `partId` inexistente -> `404`
  - `quantity <= 0` -> `400`
  - vínculo duplicado `(service_id, part_id)` -> `409`

- `DELETE /services/{id}/parts/{partId}`
  - `serviceId` inexistente -> `404`
  - `partId` não vinculado ao serviço -> `404`

- `GET /services/{id}`
  - recurso inexistente -> `404`

- `GET /services`
  - paginação inválida -> `400`

Regras mandatórias:
- Proibido parsing de mensagem (`ex.Message`) para definir HTTP status.
- Mapeamento de erro deve ser por tipo/causa de domínio.

### 5.3 Contratos internos
- Commands:
  - `AddServicePartCommand`
  - `RemoveServicePartCommand`
- Handlers:
  - `AddServicePartHandler`
  - `RemoveServicePartHandler`
- Repositórios:
  - `IServiceRepository`
  - `IPartRepository` (consulta de existência e snapshot de nome)

## 6) Plano Técnico por Camada
### Domain
- Evoluir `Service` com:
  - `Parts: IReadOnlyList<ServicePartItem>`
- Implementar:
  - `AddPart(partId, partName, quantity)`
  - `RemovePart(partId)`
- Garantir invariantes:
  - `partId` válido
  - `quantity > 0`
  - sem duplicidade por `PartId`
- Mensagens via `DomainErrorMessages` (sem strings inline).

### Application
- Criar commands/handlers para add/remove de peça em serviço.
- Carregar `Part` por `partId` para snapshot de `PartName` no vínculo.
- Atualizar DTOs/mappers de serviço para expor composição de peças (`parts`).

### Infrastructure
- Atualizar `ServiceConfiguration` para mapear coleção de composição de peças (`Parts`).
- Criar estrutura persistente da composição de peças com índice único composto `(service_id, part_id)`.
- Garantir carregamento consistente da composição em `GetById` e `List`.
- Traduzir violação de unicidade para `DomainException`.

### API
- Criar DTO de request para `POST /services/{id}/parts`.
- Adicionar endpoints dedicados de composição no grupo `/services`.
- Atualizar DTOs de response de serviço para incluir lista de composição de peças.

### Tests
- Domínio:
  - adicionar peça válida
  - adicionar peça duplicada (erro)
  - adicionar peça com quantidade inválida (erro)
  - remover peça vinculada
  - remover peça inexistente (erro)
- Aplicação:
  - handlers com cenários de sucesso e erro (404/400/409)
- Integração:
  - endpoints de composição e retorno da composição nos GETs

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- [src/GarageFlow.Application/Services/Commands/AddServicePartCommand.cs](../../../../src/GarageFlow.Application/Services/Commands/AddServicePartCommand.cs)
- [src/GarageFlow.Application/Services/Commands/RemoveServicePartCommand.cs](../../../../src/GarageFlow.Application/Services/Commands/RemoveServicePartCommand.cs)
- [src/GarageFlow.Application/Services/Handlers/AddServicePartHandler.cs](../../../../src/GarageFlow.Application/Services/Handlers/AddServicePartHandler.cs)
- [src/GarageFlow.Application/Services/Handlers/RemoveServicePartHandler.cs](../../../../src/GarageFlow.Application/Services/Handlers/RemoveServicePartHandler.cs)
- `src/GarageFlow.Api/DTOs/Services/AddServicePartRequest.cs`
- [tests/GarageFlow.Tests/Domain/Services/ServiceCompositionPartTests.cs](../../../../tests/GarageFlow.Tests/Domain/Services/ServiceCompositionPartTests.cs)
- [tests/GarageFlow.Tests/Application/Services/ServiceCompositionPartHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/Services/ServiceCompositionPartHandlersTests.cs)
- [tests/GarageFlow.Tests/Integration/Services/ServicePartCompositionEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Services/ServicePartCompositionEndpointsTests.cs)

### Alterar (esperado)
- [src/GarageFlow.Domain/Services/Service.cs](../../../../src/GarageFlow.Domain/Services/Service.cs)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs)
- [src/GarageFlow.Application/Services/DTOs/ServiceDto.cs](../../../../src/GarageFlow.Application/Services/DTOs/ServiceDto.cs)
- [src/GarageFlow.Application/Services/Handlers/ServiceMapper.cs](../../../../src/GarageFlow.Application/Services/Handlers/ServiceMapper.cs)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs)
- [src/GarageFlow.Infrastructure/Persistence/Configurations/Services/ServiceConfiguration.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Configurations/Services/ServiceConfiguration.cs)
- [src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceRepository.cs)
- [src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs](../../../../src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs)
- `src/GarageFlow.Api/DTOs/Services/ServiceResponse.cs`
- `src/GarageFlow.Api/DTOs/Services/PagedServiceResponse.cs`
- `src/GarageFlow.Api/Endpoints/Services/ServicesEndpoints.cs`

Contrato de estrutura:
- Composição de serviço deve permanecer dentro do contexto `Services`.
- Não criar estrutura paralela para composição em outro bounded context.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] Testes de domínio/aplicação/integração da task verdes.
- [ ] Endpoints de composição de peça funcionando no Swagger.
- [ ] `GET /services/{id}` e `GET /services` retornando `parts`.
- [ ] Unicidade por `(service_id, part_id)` aplicada e traduzida para `DomainException`.
- [ ] Sem parsing de mensagem para transporte HTTP.

## 9) Estratégia de Testes
### Domínio
- [ ] Cobrir invariantes de `AddPart`/`RemovePart`.

### Aplicação
- [ ] Cobrir handlers de composição de peça com serviços/repositórios fake.

### Integração
- [ ] Cobrir endpoints de add/remove e leitura de composição.

## 10) Riscos e Mitigações
- Risco: divergência entre nome da peça do catálogo e snapshot no vínculo.
  - Mitigação: sempre carregar `PartName` do catálogo no handler de add.
- Risco: duplicidade de vínculo em concorrência.
  - Mitigação: índice único composto + tradução para `DomainException`.

## 11) Checklist de Execução para IA
- [ ] Ler documentos canônicos e standards.
- [ ] Implementar composição no domínio sem quebrar CRUD existente de serviço.
- [ ] Implementar endpoints dedicados com matriz de erro explícita.
- [ ] Atualizar contratos de leitura para retornar composição de peças (`parts`).
- [ ] Executar build/testes da suíte relevante.

## 12) Guardrails Não-Negociáveis
- Proibido parsing de `ex.Message` para decisão de HTTP status.
- Proibido usar strings inline de erro.
- Proibido expor endpoint de update de quantidade nesta task.
- Proibido introduzir movimento de estoque ou lógica de OS.

## 13) Assumptions
- A task de OS foi adiada e não faz parte desta entrega.
- Composição de insumo será entregue na `task-010`.
- `Supplies` não é escopo funcional desta task e não deve receber novas regras/contratos aqui.
