# Task-004 — Create Supplier CRUD

## 0) Metadata
- `task_id`: `task-004`
- `slug`: `create-supplier-crud`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-000-template.md](task-000-template.md), [task-001-create-customer-crud.md](task-001-create-customer-crud.md)

## 1) Objetivo
Implementar CRUD ponta a ponta de `Supplier`, estabelecendo a base de fornecedores para compras e abastecimento.

## 2) Escopo
### In
- Implementar agregado `Supplier` com `Create`, `Update` e `Deactivate`.
- Expor casos de uso `Create`, `GetById`, `List`, `Update`, `Deactivate`.
- Persistir `Supplier` com EF Core e regras de unicidade canônicas.
- Expor endpoints REST de fornecedores.
- Criar testes de domínio, aplicação e integração.

### Out
- Fluxos de compra, cotação e pedido.
- Integrações assíncronas.
- Alterações funcionais em outros contextos.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN)
- Fornecedor deve possuir identificação e contato válidos.
- Documento fiscal (CPF/CNPJ quando aplicável) deve ser válido.
- Unicidade de identificador principal do fornecedor deve ser garantida.
- Desativação lógica deve preservar histórico.

## 5) Contratos e Interfaces
### Endpoints
- `POST /suppliers`
- `GET /suppliers/{id}`
- `GET /suppliers`
- `PUT /suppliers/{id}`
- `DELETE /suppliers/{id}`

### Erros
- `400`: validação
- `404`: não encontrado
- `409`: duplicidade

Matriz de erro mandatória:
- `POST /suppliers`
  - validação de domínio -> `400`
  - duplicidade -> `409`
- `GET /suppliers/{id}`
  - recurso inexistente -> `404`
- `GET /suppliers`
  - paginação inválida -> `400`
- `PUT /suppliers/{id}`
  - validação de domínio -> `400`
  - recurso inexistente -> `404`
  - duplicidade -> `409`
- `DELETE /suppliers/{id}`
  - recurso inexistente -> `404`
  - já inativo -> `400`

## 6) Plano Técnico por Camada
### Domain
- Criar `Supplier` e invariantes.
- Reusar VOs canônicos quando aplicável (`Email`, `PhoneNumber`, `Address`, `Cpf`/`Cnpj`).
- Criar `ISupplierRepository`.

### Application
- Commands/queries/handlers para CRUD.
- DTOs e mappers de `Supplier`.

### Infrastructure
- Mapping EF Core + índice único.
- `SupplierRepository` com tradução de unicidade para `DomainException`.
- Migration.

### API
- DTOs de request/response.
- Endpoints + mapeamento de erros.
- Validar entrada de paginação na borda: `page > 0` e `pageSize > 0`.

### Tests
- Domínio, aplicação e integração cobrindo fluxo completo.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Domain/Suppliers/*`
- `src/GarageFlow.Application/Suppliers/*`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/SupplierConfiguration.cs`
- [src/GarageFlow.Infrastructure/Persistence/Repositories/SupplierRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/SupplierRepository.cs)
- `src/GarageFlow.Api/DTOs/Suppliers/*`
- `src/GarageFlow.Api/Endpoints/Suppliers/*`
- `tests/GarageFlow.Tests/Domain/Suppliers/*`
- `tests/GarageFlow.Tests/Application/Suppliers/*`
- `tests/GarageFlow.Tests/Integration/Suppliers/*`

### Alterar (esperado)
- [src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs](../../../../src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs)
- `src/GarageFlow.Api/Program.cs`
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs) (novas chaves)

Regra de estrutura mandatória:
- O agregado e contrato de repositório de fornecedor devem permanecer em `src/GarageFlow.Domain/Suppliers/*`.
- Não é permitido criar `Supplier` sob outro contexto de domínio.

## 8) Critérios de Pronto
- `dotnet build` sem erros.
- `dotnet test` passando.
- Endpoints funcionais no Swagger.
- Regras de unicidade e soft delete validadas.
- Sem strings inline de erro.

## 9) Estratégia de Testes
- Sucesso em `Create/GetById/List/Update/Deactivate`.
- Falhas de validação de campos obrigatórios.
- Falha por duplicidade.
- Verificação de soft delete.

## 10) Riscos e Mitigações
- Risco: duplicidade de documento/identificador.
  - Mitigação: índice único + tratamento de exceção.
- Risco: inconsistência de validação entre camadas.
  - Mitigação: invariantes no domínio + testes de integração.

## 11) Checklist de Execução para IA
- [ ] Ler documentos canônicos.
- [ ] Implementar `Supplier` no domínio.
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
