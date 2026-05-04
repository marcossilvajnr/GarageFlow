# Task-005 — Create Employee CRUD

## 0) Metadata
- `task_id`: `task-005`
- `slug`: `create-employee-crud`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-000-template.md`, `task-001-create-customer-crud.md`

## 1) Objetivo
Implementar CRUD ponta a ponta de `Employee`, preparando a base de colaboradores para execução de serviços e operação interna.

## 2) Escopo
### In
- Implementar agregado `Employee` com `Create`, `Update` e `Deactivate`.
- Expor casos de uso `Create`, `GetById`, `List`, `Update`, `Deactivate`.
- Persistir `Employee` com EF Core e regras de unicidade canônicas.
- Expor endpoints REST de funcionários.
- Criar testes de domínio, aplicação e integração.

### Out
- Alocação em ordens de serviço.
- Escalas de trabalho.
- Regras de folha ou RH.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/domain/agregados.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN)
- Funcionário deve ter dados obrigatórios válidos (nome, documento, contato).
- Documento principal deve ser único.
- Perfil/cargo deve respeitar enumeração válida do domínio.
- Desativação lógica deve preservar histórico.

## 5) Contratos e Interfaces
### Endpoints
- `POST /employees`
- `GET /employees/{id}`
- `GET /employees`
- `PUT /employees/{id}`
- `DELETE /employees/{id}`

### Erros
- `400`: validação
- `404`: não encontrado
- `409`: duplicidade

Matriz de erro mandatória:
- `POST /employees`
  - validação de domínio -> `400`
  - duplicidade -> `409`
- `GET /employees/{id}`
  - recurso inexistente -> `404`
- `GET /employees`
  - paginação inválida -> `400`
- `PUT /employees/{id}`
  - validação de domínio -> `400`
  - recurso inexistente -> `404`
  - duplicidade -> `409`
- `DELETE /employees/{id}`
  - recurso inexistente -> `404`
  - já inativo -> `400`

## 6) Plano Técnico por Camada
### Domain
- Criar `Employee` e invariantes.
- Reusar VOs canônicos quando aplicável.
- Criar `IEmployeeRepository`.

### Application
- Commands/queries/handlers para CRUD.
- DTOs e mappers de `Employee`.

### Infrastructure
- Mapping EF Core + índice único para identificador principal.
- `EmployeeRepository` com tradução de unicidade para `DomainException`.
- Migration.

### API
- DTOs de request/response.
- Endpoints + mapeamento de erros.
- Validar entrada de paginação na borda: `page > 0` e `pageSize > 0`.

### Tests
- Domínio, aplicação e integração cobrindo fluxo completo.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Domain/Employees/*`
- `src/GarageFlow.Application/Employees/*`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs`
- `src/GarageFlow.Infrastructure/Persistence/Repositories/EmployeeRepository.cs`
- `src/GarageFlow.Api/DTOs/Employees/*`
- `src/GarageFlow.Api/Endpoints/Employees/*`
- `tests/GarageFlow.Tests/Domain/Employees/*`
- `tests/GarageFlow.Tests/Application/Employees/*`
- `tests/GarageFlow.Tests/Integration/Employees/*`

### Alterar (esperado)
- `src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs`
- `src/GarageFlow.Api/Program.cs`
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs` (novas chaves)

Regra de estrutura mandatória:
- O agregado e contrato de repositório de funcionário devem permanecer em `src/GarageFlow.Domain/Employees/*`.
- Não é permitido criar `Employee` sob outro contexto de domínio.

## 8) Critérios de Pronto
- `dotnet build` sem erros.
- `dotnet test` passando.
- Endpoints funcionais no Swagger.
- Unicidade e soft delete validados.
- Sem strings inline de erro.

## 9) Estratégia de Testes
- Sucesso em `Create/GetById/List/Update/Deactivate`.
- Falhas de validação e enumeração inválida.
- Falha por duplicidade.
- Verificação de soft delete.

## 10) Riscos e Mitigações
- Risco: inconsistência em tipos de cargo/perfil.
  - Mitigação: enumeração de domínio com validação explícita.
- Risco: duplicidade de documento.
  - Mitigação: índice único + tradução de exceção.

## 11) Checklist de Execução para IA
- [ ] Ler documentos canônicos.
- [ ] Implementar `Employee` no domínio.
- [ ] Implementar CRUD em aplicação, infraestrutura e API.
- [ ] Implementar testes por camada.
- [ ] Executar build e testes.
- [ ] Validar aderência a `engineering-standards.md`.

## 12) Guardrails Não-Negociáveis
- Proibido parsing de `ex.Message` para decidir status HTTP.
- Proibido mapear toda `DomainException` para um único status HTTP sem distinguir causa.
- Proibido criar caminhos de arquivo fora da seção de arquivos sem justificativa explícita na resposta final.
- Proibido alterar itens de `Out` sem registrar impacto e aprovação.

## 13) Contrato de Arquivos e Estrutura
- Os caminhos definidos na seção de arquivos desta task são mandatórios.
- Qualquer desvio de estrutura deve ser registrado na resposta final com justificativa técnica.
- Não criar estrutura paralela de pastas para o mesmo contexto funcional.
