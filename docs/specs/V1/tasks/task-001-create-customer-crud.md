# Task 001 — Create Customer CRUD

## Metadados
- Task ID: `task-001`
- Título: `create-customer-crud`
- Status: `Ready`
- Dono: `Time de Plataforma`
- Data de criação: `2026-05-03`
- Última atualização: `2026-05-03`

## 1) Objetivo
Implementar CRUD ponta a ponta de `Customer` para validar a arquitetura de referência do GarageFlow em todas as camadas.

## 2) Escopo
### In
- Criar cliente PF/PJ.
- Consultar cliente por `Id`.
- Listar clientes com paginação simples.
- Atualizar dados permitidos do cliente.
- Desativar cliente (soft delete).

### Out
- Gestão de `Vehicle`.
- Integrações assíncronas/eventos entre bounded contexts.
- Autenticação/autorização detalhada por papel.
- Filtros avançados de busca.

## 3) Contexto Canônico Obrigatório
Leitura obrigatória antes da implementação:
- `docs/Domain/regras-de-negocio.md` (RN-021, RN-023)
- `docs/Domain/linguagem-ubiqua.md`
- `docs/Domain/agregados.md` (fonte canônica de eventos de integração)
- `docs/Domain/value-objects.md`
- `docs/architecture/architecture-overview.md`
- `docs/architecture/application-and-integrations.md`
- `docs/architecture/engineering-standards.md`

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-021` — CPF/CNPJ únicos por cliente.
- `RN-023` — Cliente não é deletado fisicamente; apenas desativado.
- Normalização textual canônica: `trim` nas bordas para texto em agregados.
- Mensagens de erro de domínio em português.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /customers`
  - Request: `name`, `documentType` (`Cpf|Cnpj`), `document`, `email`, `phoneNumber`, `address`.
  - Response `201`: `id`, dados do cliente, `isActive`, `createdAt`.
- `GET /customers/{id}`
  - Response `200`: dados do cliente.
  - Response `404`: cliente não encontrado.
- `GET /customers`
  - Query: `page`, `pageSize` (opcional).
  - Response `200`: lista + metadados de paginação.
- `PUT /customers/{id}`
  - Request: campos permitidos (não inclui documento).
  - Response `200`: cliente atualizado.
- `DELETE /customers/{id}`
  - Efeito: `Deactivate()`.
  - Response `204`.

Matriz de erro mandatória:
- `POST /customers`
  - validação de domínio -> `400`
  - duplicidade de CPF/CNPJ -> `409`
- `GET /customers/{id}`
  - cliente inexistente -> `404`
- `GET /customers`
  - paginação inválida -> `400`
- `PUT /customers/{id}`
  - validação de domínio -> `400`
  - cliente inexistente -> `404`
  - duplicidade de CPF/CNPJ (quando aplicável) -> `409`
- `DELETE /customers/{id}`
  - cliente inexistente -> `404`
  - cliente já inativo -> `400`

### 5.2 Contratos internos
- Commands:
  - `CreateCustomerCommand`
  - `UpdateCustomerCommand`
  - `DeactivateCustomerCommand`
- Queries:
  - `GetCustomerByIdQuery`
  - `ListCustomersQuery`
- Repositório:
  - `ICustomerRepository` (por agregado raiz)
- Unidade transacional:
  - Commit por caso de uso na camada de aplicação.

### 5.3 Erros de domínio e persistência
- `400` para validações de domínio (`DomainException`).
- `404` para não encontrado.
- `409` para conflito de unicidade CPF/CNPJ.
- Tradução obrigatória de violação de índice único para `DomainException` em português.

## 6) Plano Técnico por Camada
### Domain
- Implementar `Customer` como agregado raiz com:
  - construtor privado
  - factory `Create()`
  - propriedades `private set`
  - método `Deactivate()`
- Usar VOs canônicos (`Cpf`, `Cnpj`, `Email`, `PhoneNumber`, `Address`).
- Garantir que documento não seja alterado após criação.

### Application
- Implementar commands/queries e handlers.
- Aplicar validação de entrada de caso de uso.
- Respeitar invariantes no agregado.

### Infrastructure
- Mapear `Customer` no EF Core.
- Mapear VOs conforme padrão atual de persistência.
- Criar índice único para documento (CPF/CNPJ).
- Implementar repositório `Customer`.
- Implementar tradução de exceção de unicidade para `DomainException`.
- Criar migration correspondente.

### API
- Criar endpoints REST de `Customer`.
- Criar DTOs de request/response.
- Mapear exceções para status HTTP padronizados.
- Validar entrada de paginação na borda: `page > 0` e `pageSize > 0`.

### Tests
- Domínio:
  - criação PF válida
  - criação PJ válida
  - nome inválido
  - documento inválido
  - desativação idempotente/erro quando aplicável
- Aplicação:
  - handlers com sucesso e falhas de domínio
  - conflito de unicidade
- Integração:
  - fluxo HTTP completo de CRUD
  - persistência de soft delete
  - `409` em documento duplicado

## 7) Arquivos a Criar/Alterar
### Domain (`src/GarageFlow.Domain`)
- `Customers/Customer.cs`
- `Customers/ICustomerRepository.cs`
- `Customers/CustomerDocumentType.cs` (se necessário para modelagem)
- `Exceptions/DomainException.cs` (se ainda inexistente)

### Application (`src/GarageFlow.Application`)
- `Customers/Commands/CreateCustomerCommand.cs`
- `Customers/Commands/UpdateCustomerCommand.cs`
- `Customers/Commands/DeactivateCustomerCommand.cs`
- `Customers/Queries/GetCustomerByIdQuery.cs`
- `Customers/Queries/ListCustomersQuery.cs`
- `Customers/Handlers/*`
- `Customers/DTOs/*`
- `DependencyInjection.cs` (registro dos handlers/serviços)

### Infrastructure (`src/GarageFlow.Infrastructure`)
- `Persistence/GarageFlowDbContext.cs` (DbSet e configurações)
- `Persistence/Configurations/Customers/*`
- `Persistence/Repositories/CustomerRepository.cs`
- `Persistence/Migrations/*` (migration de customer)
- `DependencyInjection.cs` (registro de repositório)

### API (`src/GarageFlow.Api`)
- `Endpoints/CustomersEndpoints.cs`
- `Program.cs` (mapear endpoints)

Regra de estrutura mandatória:
- O agregado e contrato de repositório de cliente devem permanecer em `src/GarageFlow.Domain/Customers/*`.
- Não é permitido criar `Customer` sob outro contexto de domínio.

### Tests (`tests/GarageFlow.Tests` ou projetos separados)
- `Domain/Customers/CustomerTests.cs`
- `Application/Customers/*Tests.cs`
- `Integration/CustomersEndpointsTests.cs`

## 8) Critérios de Pronto
- [ ] Endpoints `POST/GET by id/GET list/PUT/DELETE` de `Customer` funcionando.
- [ ] Soft delete implementado via `Deactivate()` sem remoção física.
- [ ] CPF/CNPJ único com retorno de conflito (`409`) e mensagem de negócio em português.
- [ ] Build da solução sem erros.
- [ ] Testes de domínio, aplicação e integração implementados e verdes.
- [ ] Dependências entre camadas respeitadas.

## 9) Estratégia de Testes
### Domínio
- [ ] `Create()` com CPF válido cria cliente ativo.
- [ ] `Create()` com CNPJ válido cria cliente ativo.
- [ ] Documento inválido lança `DomainException`.
- [ ] `Deactivate()` desativa cliente.

### Aplicação
- [ ] Handler de criação persiste cliente válido.
- [ ] Handler de atualização altera apenas campos permitidos.
- [ ] Handler de desativação aplica soft delete.
- [ ] Unicidade duplicada retorna falha de domínio traduzida.

### Integração
- [ ] `POST /customers` retorna `201`.
- [ ] `GET /customers/{id}` retorna `200` para existente.
- [ ] `PUT /customers/{id}` retorna `200`.
- [ ] `DELETE /customers/{id}` retorna `204` e mantém registro inativo.
- [ ] Repetir documento em novo `POST` retorna `409`.

## 10) Riscos e Mitigações
- Risco: modelagem ambígua de documento (CPF/CNPJ) no agregado.
  - Mitigação: adotar um contrato explícito de entrada (`documentType` + `document`) e normalizar no domínio.
- Risco: mapeamento EF Core de VOs gerar acoplamento indevido.
  - Mitigação: configuração explícita por VO e testes de integração de persistência.
- Risco: divergência entre erro técnico de banco e erro de domínio.
  - Mitigação: camada de tradução centralizada para violação de unicidade.

## 11) Checklist de Execução para IA
- [ ] Ler todos os documentos canônicos listados nesta task.
- [ ] Não alterar regra de negócio sem atualização canônica em `docs/Domain`.
- [ ] Implementar verticalmente: Domain -> Application -> Infrastructure -> API -> Tests.
- [ ] Garantir mensagens de erro em português.
- [ ] Garantir `409` para conflito de unicidade.
- [ ] Garantir que `DELETE` é soft delete.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, desde que não conflite com o canônico.

## 12) Guardrails Não-Negociáveis
- Proibido parsing de `ex.Message` para decidir status HTTP.
- Proibido mapear toda `DomainException` para um único status HTTP sem distinguir causa.
- Proibido criar caminhos de arquivo fora da seção de arquivos sem justificativa explícita na resposta final.
- Proibido alterar itens de `Out` sem registrar impacto e aprovação.

## 13) Contrato de Arquivos e Estrutura
- Os caminhos definidos na seção de arquivos desta task são mandatórios.
- Qualquer desvio de estrutura deve ser registrado na resposta final com justificativa técnica.
- Não criar estrutura paralela de pastas para o mesmo contexto funcional.
