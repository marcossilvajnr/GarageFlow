# Task-008 — Create Supply CRUD

## 0) Metadata
- `task_id`: `task-008`
- `slug`: `create-supply-crud`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-000-template.md](task-000-template.md), [task-004-create-supplier-crud.md](task-004-create-supplier-crud.md)

## 1) Objetivo
Implementar CRUD ponta a ponta de `Supply`, estabelecendo o catálogo de insumos para controle de estoque e consumo operacional.

## 2) Escopo
### In
- Implementar agregado `Supply` com `Create`, `Update` e `Deactivate`.
- Expor casos de uso `Create`, `GetById`, `List`, `Update`, `Deactivate`.
- Persistir `Supply` com EF Core e regras de unicidade de código.
- Expor endpoints REST de insumos.
- Criar testes de domínio, aplicação e integração.

### Out
- Movimentação de estoque.
- Reserva/consumo em execução de OS.
- Estratégias avançadas de reposição.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN)
- Insumo deve possuir código único.
- Unidade de medida deve ser válida.
- Custo base deve respeitar limites do domínio.
- Pode estar vinculado a fornecedor preferencial (quando aplicável).
- Desativação lógica deve preservar histórico.

## 5) Contratos e Interfaces
### Endpoints
- `POST /supplies`
- `GET /supplies/{id}`
- `GET /supplies`
- `PUT /supplies/{id}`
- `DELETE /supplies/{id}`

### Erros
- `400`: validação
- `404`: não encontrado
- `409`: duplicidade

Matriz de erro mandatória:
- `POST /supplies`
  - validação de domínio -> `400`
  - duplicidade -> `409`
- `GET /supplies/{id}`
  - recurso inexistente -> `404`
- `GET /supplies`
  - paginação inválida -> `400`
- `PUT /supplies/{id}`
  - validação de domínio -> `400`
  - recurso inexistente -> `404`
  - duplicidade -> `409`
- `DELETE /supplies/{id}`
  - recurso inexistente -> `404`
  - já inativo -> `400`

## 6) Plano Técnico por Camada
### Domain
- Criar `Supply` e invariantes.
- Criar `ISupplyRepository`.

### Application
- Commands/queries/handlers para CRUD.
- DTOs e mappers de `Supply`.

### Infrastructure
- Mapping EF Core + índice único.
- `SupplyRepository` com tradução de unicidade para `DomainException`.
- Migration.

### API
- DTOs de request/response.
- Endpoints + mapeamento de erros.
- Validar entrada de paginação na borda: `page > 0` e `pageSize > 0`.

### Tests
- Domínio, aplicação e integração cobrindo fluxo completo.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Domain/Supplies/*`
- `src/GarageFlow.Application/Supplies/*`
- [src/GarageFlow.Infrastructure/Persistence/Configurations/SupplyConfiguration.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Configurations/SupplyConfiguration.cs)
- [src/GarageFlow.Infrastructure/Persistence/Repositories/SupplyRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/SupplyRepository.cs)
- `src/GarageFlow.Api/DTOs/Supplies/*`
- `src/GarageFlow.Api/Endpoints/Supplies/*`
- `tests/GarageFlow.Tests/Domain/Supplies/*`
- `tests/GarageFlow.Tests/Application/Supplies/*`
- `tests/GarageFlow.Tests/Integration/Supplies/*`

### Alterar (esperado)
- [src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs](../../../../src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs)
- `src/GarageFlow.Api/Program.cs`
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs) (novas chaves)

Regra de estrutura mandatória:
- O agregado e contrato de repositório de insumo devem permanecer em `src/GarageFlow.Domain/Supplies/*`.
- Não é permitido criar `Supply` sob outro contexto de domínio.

## 8) Critérios de Pronto
- `dotnet build` sem erros.
- `dotnet test` passando.
- Endpoints funcionais no Swagger.
- Unicidade e soft delete validados.
- Sem strings inline de erro.

## 9) Estratégia de Testes
- Sucesso em `Create/GetById/List/Update/Deactivate`.
- Falhas de validação de código, unidade e custo.
- Falha por duplicidade de código.
- Verificação de soft delete.

## 10) Riscos e Mitigações
- Risco: divergência entre regras de peças e insumos.
  - Mitigação: manter invariantes explícitas por agregado.
- Risco: colisão de códigos de insumo.
  - Mitigação: índice único + tradução de exceção.

## 11) Checklist de Execução para IA
- [ ] Ler documentos canônicos.
- [ ] Implementar `Supply` no domínio.
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
