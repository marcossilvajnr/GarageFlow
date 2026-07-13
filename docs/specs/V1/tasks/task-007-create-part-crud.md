# Task-007 — Create Part CRUD

## 0) Metadata
- `task_id`: `task-007`
- `slug`: `create-part-crud`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-000-template.md](task-000-template.md), [task-004-create-supplier-crud.md](task-004-create-supplier-crud.md)

## 1) Objetivo
Implementar CRUD ponta a ponta de `Part`, estabelecendo o catálogo de peças para estoque, compra e uso em ordens de serviço.

## 2) Escopo
### In
- Implementar agregado `Part` com `Create`, `Update` e `Deactivate`.
- Expor casos de uso `Create`, `GetById`, `List`, `Update`, `Deactivate`.
- Persistir `Part` com EF Core e regras de unicidade de código/SKU.
- Expor endpoints REST de peças.
- Criar testes de domínio, aplicação e integração.

### Out
- Movimentação de estoque.
- Reserva de peças por OS.
- Estratégias avançadas de custo.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN)
- Peça deve possuir código/SKU único.
- Unidade de medida deve ser válida.
- Custo e preço devem respeitar limites de domínio.
- Pode estar vinculada a fornecedor preferencial (quando aplicável).
- Desativação lógica deve preservar histórico.

## 5) Contratos e Interfaces
### Endpoints
- `POST /parts`
- `GET /parts/{id}`
- `GET /parts`
- `PUT /parts/{id}`
- `DELETE /parts/{id}`

### Erros
- `400`: validação
- `404`: não encontrado
- `409`: duplicidade

Matriz de erro mandatória:
- `POST /parts`
  - validação de domínio -> `400`
  - duplicidade -> `409`
- `GET /parts/{id}`
  - recurso inexistente -> `404`
- `GET /parts`
  - paginação inválida -> `400`
- `PUT /parts/{id}`
  - validação de domínio -> `400`
  - recurso inexistente -> `404`
  - duplicidade -> `409`
- `DELETE /parts/{id}`
  - recurso inexistente -> `404`
  - já inativo -> `400`

## 6) Plano Técnico por Camada
### Domain
- Criar `Part` e invariantes.
- Criar `IPartRepository`.

### Application
- Commands/queries/handlers para CRUD.
- DTOs e mappers de `Part`.

### Infrastructure
- Mapping EF Core + índices únicos.
- `PartRepository` com tradução de unicidade para `DomainException`.
- Migration.

### API
- DTOs de request/response.
- Endpoints + mapeamento de erros.
- Validar entrada de paginação na borda: `page > 0` e `pageSize > 0`.

### Tests
- Domínio, aplicação e integração cobrindo fluxo completo.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Domain/Parts/*`
- `src/GarageFlow.Application/Parts/*`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/PartConfiguration.cs`
- [src/GarageFlow.Infrastructure/Persistence/Repositories/PartRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/PartRepository.cs)
- `src/GarageFlow.Api/DTOs/Parts/*`
- `src/GarageFlow.Api/Endpoints/Parts/*`
- `tests/GarageFlow.Tests/Domain/Parts/*`
- `tests/GarageFlow.Tests/Application/Parts/*`
- `tests/GarageFlow.Tests/Integration/Parts/*`

### Alterar (esperado)
- [src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs](../../../../src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs)
- `src/GarageFlow.Api/Program.cs`
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs) (novas chaves)

Regra de estrutura mandatória:
- O agregado e contrato de repositório de peça devem permanecer em `src/GarageFlow.Domain/Parts/*`.
- Não é permitido criar `Part` sob outro contexto de domínio.

## 8) Critérios de Pronto
- `dotnet build` sem erros.
- `dotnet test` passando.
- Endpoints funcionais no Swagger.
- Unicidade e soft delete validados.
- Sem strings inline de erro.

## 9) Estratégia de Testes
- Sucesso em `Create/GetById/List/Update/Deactivate`.
- Falhas de validação de código, unidade e preço.
- Falha por duplicidade de código/SKU.
- Verificação de soft delete.

## 10) Riscos e Mitigações
- Risco: inconsistência de unidade de medida.
  - Mitigação: validação explícita no domínio.
- Risco: colisão de código/SKU.
  - Mitigação: índice único + tradução de exceção.

## 11) Checklist de Execução para IA
- [ ] Ler documentos canônicos.
- [ ] Implementar `Part` no domínio.
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
