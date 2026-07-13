# Task-003 — Create Vehicle CRUD

## 0) Metadata
- `task_id`: `task-003`
- `slug`: `create-vehicle-crud`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-001-create-customer-crud.md](task-001-create-customer-crud.md), [task-002-create-remaining-value-objects.md](task-002-create-remaining-value-objects.md)

## 1) Objetivo
Implementar CRUD ponta a ponta do agregado `Vehicle`, consolidando o uso de `LicensePlate` e `Renavam` no fluxo completo de domínio, aplicação, persistência e API.

## 2) Escopo
### In
- Implementar agregado `Vehicle` com operações `Create`, `Update` e `Deactivate`.
- Expor casos de uso `Create`, `GetById`, `List`, `Update` e `Deactivate`.
- Persistir `Vehicle` com EF Core, incluindo índices únicos para `LicensePlate` e `Renavam`.
- Expor endpoints REST de `Vehicle` na API.
- Criar testes de domínio, aplicação e integração para o fluxo de `Vehicle`.

### Out
- Alterações em fluxos de `ServiceOrders`, `Executions`, `Stock` e `Purchasing`.
- Eventos de integração novos.
- Refactors funcionais em `Customer` além do necessário para vínculo de `Vehicle`.

## 3) Contexto Canônico Obrigatório
Leitura obrigatória antes de implementar:
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/domain/value-objects.md](../../../domain/value-objects.md)
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/architecture/architecture-overview.md](../../../architecture/architecture-overview.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

Referências de governança:
- Catálogo de eventos canônico: [docs/domain/agregados.md](../../../domain/agregados.md)
- Catálogo central de mensagens: `DomainErrorMessages` (`GarageFlow.Domain.Shared`).

## 4) Regras de Negócio Aplicáveis (RN)
- `Vehicle` deve estar vinculado a um `Customer` existente.
- `LicensePlate` e `Renavam` devem ser válidos via VOs canônicos.
- `LicensePlate` e `Renavam` devem ser únicos no contexto de veículos.
- Desativação lógica (`soft delete`) deve preservar histórico.
- Falhas de negócio devem resultar em `DomainException` com mensagem em português via `DomainErrorMessages`.

## 5) Contratos e Interfaces
### Contratos REST
- `POST /vehicles`
- `GET /vehicles/{id}`
- `GET /vehicles`
- `PUT /vehicles/{id}`
- `DELETE /vehicles/{id}` (desativação)

### DTOs esperados
- `CreateVehicleRequest`
- `UpdateVehicleRequest`
- `VehicleResponse`
- `ListVehiclesResponse`

### Erros
- `400`: payload inválido / validação
- `404`: recurso não encontrado
- `409`: violação de unicidade (`LicensePlate`/`Renavam`)

Matriz de erro mandatória:
- `POST /vehicles`
  - validação de domínio -> `400`
  - duplicidade de `LicensePlate`/`Renavam` -> `409`
- `GET /vehicles/{id}`
  - veículo inexistente -> `404`
- `PUT /vehicles/{id}`
  - validação de domínio -> `400`
  - veículo inexistente -> `404`
  - duplicidade de `LicensePlate`/`Renavam` (quando aplicável) -> `409`
- `DELETE /vehicles/{id}`
  - veículo inexistente -> `404`
  - veículo já inativo (regra de domínio) -> `400`

## 6) Plano Técnico por Camada
### Domain
- Criar entidade/agregado `Vehicle` com invariantes e transições de estado.
- Reusar VOs `LicensePlate` e `Renavam`.
- Definir contrato de repositório `IVehicleRepository`.

### Application
- Criar comandos/queries e handlers para `Create`, `GetById`, `List`, `Update`, `Deactivate`.
- Criar DTOs e mapper da camada de aplicação.
- Validar existência de `Customer` no fluxo de criação/atualização.

### Infrastructure
- Criar mapeamento EF Core para `Vehicle`.
- Criar repositório `VehicleRepository`.
- Aplicar índices únicos para `LicensePlate` e `Renavam`.
- Traduzir violação de unicidade para exceção de domínio.
- Gerar migration correspondente.

### API
- Criar DTOs de request/response de `Vehicle` (1 tipo público por arquivo).
- Criar endpoints com códigos HTTP corretos.
- Mapear exceções para respostas padronizadas.
- Validar entrada de paginação na borda: `page > 0` e `pageSize > 0`.

### Tests
- Domínio: invariantes e transições de estado de `Vehicle`.
- Aplicação: handlers de comandos/queries com cenários de sucesso e erro.
- Integração: endpoints + persistência (incluindo conflito de unicidade).

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- [src/GarageFlow.Domain/Vehicles/Vehicle.cs](../../../../src/GarageFlow.Domain/Vehicles/Vehicle.cs)
- [src/GarageFlow.Domain/Vehicles/IVehicleRepository.cs](../../../../src/GarageFlow.Domain/Vehicles/IVehicleRepository.cs)
- `src/GarageFlow.Application/Vehicles/Commands/*`
- `src/GarageFlow.Application/Vehicles/Queries/*`
- `src/GarageFlow.Application/Vehicles/Handlers/*`
- `src/GarageFlow.Application/Vehicles/DTOs/*`
- `src/GarageFlow.Application/Vehicles/Mappers/*`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/VehicleConfiguration.cs`
- [src/GarageFlow.Infrastructure/Persistence/Repositories/VehicleRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/VehicleRepository.cs)
- `src/GarageFlow.Api/DTOs/Vehicles/*`
- `src/GarageFlow.Api/Endpoints/Vehicles/*`
- `tests/GarageFlow.Tests/Domain/Vehicles/*`
- `tests/GarageFlow.Tests/Application/Vehicles/*`
- `tests/GarageFlow.Tests/Integration/Vehicles/*`

### Alterar (esperado)
- [src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs](../../../../src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs)
- `src/GarageFlow.Infrastructure/DependencyInjection/*`
- `src/GarageFlow.Api/Program.cs`
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs) (novas chaves se necessário)

Regra de estrutura mandatória:
- O agregado e contrato de repositório de veículo devem permanecer em `src/GarageFlow.Domain/Vehicles/*`.
- Não é permitido criar `Vehicle` sob `src/GarageFlow.Domain/Customers/*`.

## 8) Critérios de Pronto
- `dotnet build` sem erros.
- `dotnet test` com suíte passando.
- Endpoints de `Vehicle` operacionais no Swagger.
- Unicidade de `LicensePlate` e `Renavam` garantida no banco + tradução para erro de domínio.
- Sem strings inline de erro.
- Identificadores em inglês e mensagens para usuário em português.
- Aderência às regras de organização de tipos (1 tipo público por arquivo).

## 9) Estratégia de Testes
### Domain
- Criar veículo com dados válidos.
- Falhar com placa/RENAVAM inválidos.
- Falhar em transições inválidas de estado.
- Desativação lógica deve marcar inativo e preservar dados.

### Application
- `CreateVehicleHandler`: sucesso, customer inexistente, duplicidade.
- `UpdateVehicleHandler`: sucesso, veículo inexistente, dados inválidos.
- `DeactivateVehicleHandler`: sucesso e já inativo.
- Queries `GetById` e `List`: retorno e paginação.

### Integration
- `POST /vehicles`: `201`, `400`, `409`.
- `GET /vehicles/{id}`: `200`, `404`.
- `PUT /vehicles/{id}`: `200`, `400`, `404`, `409`.
- `DELETE /vehicles/{id}`: `204`, `404`.

## 10) Riscos e Mitigações
- Risco: divergência entre mapeamento EF e invariantes do domínio.
  - Mitigação: testes de integração cobrindo gravação/leitura completa.
- Risco: tratamento inconsistente de violação de índice único.
  - Mitigação: padronizar tradução de `DbUpdateException` para exceção de domínio.
- Risco: acoplamento indevido com `Customer`.
  - Mitigação: validar existência por contrato de aplicação, sem carregar regras de customer no agregado `Vehicle`.

## 11) Checklist de Execução para IA
- [ ] Ler documentação canônica obrigatória.
- [ ] Implementar agregado `Vehicle` e `IVehicleRepository`.
- [ ] Implementar comandos/queries/handlers/DTOs/mappers de `Vehicle`.
- [ ] Implementar mapeamento EF, repositório e migration de `Vehicle`.
- [ ] Implementar endpoints REST e contratos de API.
- [ ] Garantir catálogo de mensagens central (`DomainErrorMessages`) sem strings inline.
- [ ] Criar testes de domínio, aplicação e integração.
- [ ] Executar `dotnet build` e `dotnet test`.
- [ ] Confirmar aderência a [engineering-standards.md](../../../architecture/engineering-standards.md).

## 12) Guardrails Não-Negociáveis
- Proibido parsing de `ex.Message` para decidir status HTTP.
- Proibido mapear toda `DomainException` para um único status HTTP sem distinguir causa.
- Proibido criar caminhos de arquivo fora da seção de arquivos sem justificativa explícita na resposta final.
- Proibido alterar itens de `Out` sem registrar impacto e aprovação.

## 13) Contrato de Arquivos e Estrutura
- Os caminhos definidos na seção de arquivos desta task são mandatórios.
- Qualquer desvio de estrutura deve ser registrado na resposta final com justificativa técnica.
- Não criar estrutura paralela de pastas para o mesmo contexto funcional.
