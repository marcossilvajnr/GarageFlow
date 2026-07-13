# Task-006 — Create Service CRUD

## 0) Metadata
- `task_id`: `task-006`
- `slug`: `create-service-crud`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-000-template.md](task-000-template.md), [task-001-create-customer-crud.md](task-001-create-customer-crud.md)

## 1) Objetivo
Implementar CRUD ponta a ponta de `Service`, estabelecendo o catálogo operacional de serviços executados pela oficina.

## 2) Escopo
### In
- Implementar agregado `Service` com `Create`, `Update` e `Deactivate`.
- Expor casos de uso `Create`, `GetById`, `List`, `Update`, `Deactivate`.
- Persistir `Service` com EF Core e regras de unicidade de código/nome técnico.
- Expor endpoints REST de serviços.
- Criar testes de domínio, aplicação e integração.

### Out
- Precificação dinâmica por cliente.
- Regras de comissão.
- Planejamento de execução.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN)
- Serviço deve possuir identificação única (código interno ou equivalente).
- Nome e descrição devem respeitar limites definidos no domínio.
- Preço base deve ser maior que zero.
- Desativação lógica deve preservar histórico.

## 5) Contratos e Interfaces
### Endpoints
- `POST /services`
- `GET /services/{id}`
- `GET /services`
- `PUT /services/{id}`
- `DELETE /services/{id}`

### Erros
- `400`: validação
- `404`: não encontrado
- `409`: duplicidade

Matriz de erro mandatória:
- `POST /services`
  - validação de domínio -> `400`
  - duplicidade -> `409`
- `GET /services/{id}`
  - recurso inexistente -> `404`
- `GET /services`
  - paginação inválida -> `400`
- `PUT /services/{id}`
  - validação de domínio -> `400`
  - recurso inexistente -> `404`
  - duplicidade -> `409`
- `DELETE /services/{id}`
  - recurso inexistente -> `404`
  - já inativo -> `400`

## 6) Plano Técnico por Camada
### Domain
- Criar `Service` e invariantes de cadastro/preço.
- Criar `IServiceRepository`.

### Application
- Commands/queries/handlers para CRUD.
- DTOs e mappers de `Service`.

### Infrastructure
- Mapping EF Core + índices únicos.
- `ServiceRepository` com tradução de unicidade para `DomainException`.
- Migration.

### API
- DTOs de request/response.
- Endpoints + mapeamento de erros.
- Validar entrada de paginação na borda: `page > 0` e `pageSize > 0`.

### Tests
- Domínio, aplicação e integração cobrindo fluxo completo.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Domain/Services/*`
- `src/GarageFlow.Application/Services/*`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/ServiceConfiguration.cs`
- [src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceRepository.cs)
- `src/GarageFlow.Api/DTOs/Services/*`
- `src/GarageFlow.Api/Endpoints/Services/*`
- `tests/GarageFlow.Tests/Domain/Services/*`
- `tests/GarageFlow.Tests/Application/Services/*`
- `tests/GarageFlow.Tests/Integration/Services/*`

### Alterar (esperado)
- [src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs](../../../../src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs)
- `src/GarageFlow.Api/Program.cs`
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs) (novas chaves)

Regra de estrutura mandatória:
- O agregado e contrato de repositório de serviço devem permanecer em `src/GarageFlow.Domain/Services/*`.
- Não é permitido criar `Service` sob outro contexto de domínio.

## 8) Critérios de Pronto
- `dotnet build` sem erros.
- `dotnet test` passando.
- Endpoints funcionais no Swagger.
- Unicidade e soft delete validados.
- Sem strings inline de erro.

## 9) Estratégia de Testes
- Sucesso em `Create/GetById/List/Update/Deactivate`.
- Falhas de validação de preço e campos obrigatórios.
- Falha por duplicidade de código.
- Verificação de soft delete.

## 10) Riscos e Mitigações
- Risco: divergência de regra de preço base.
  - Mitigação: invariantes explícitas no agregado.
- Risco: colisão de códigos de serviço.
  - Mitigação: índice único + tradução de exceção.

## 11) Checklist de Execução para IA
- [ ] Ler documentos canônicos.
- [ ] Implementar `Service` no domínio.
- [ ] Implementar CRUD em aplicação, infraestrutura e API.
- [ ] Implementar testes por camada.
- [ ] Executar build e testes.
- [ ] Validar aderência a [engineering-standards.md](../../../architecture/engineering-standards.md).

## 12) Guardrails Não-Negociáveis
- Proibido parsing de `ex.Message` para decidir status HTTP.
- Proibido mapear toda `DomainException` para um único status HTTP sem distinguir causa.
- Proibido criar caminhos de arquivo fora da seção de arquivos sem justificativa explícita na resposta final.
- Proibido alterar itens de `Out` sem registrar impacto e aprovação.

## 13) Contrato de Arquivos e Estrutura
- Os caminhos definidos na seção de arquivos desta task são mandatórios.
- Qualquer desvio de estrutura deve ser registrado na resposta final com justificativa técnica.
- Não criar estrutura paralela de pastas para o mesmo contexto funcional.
