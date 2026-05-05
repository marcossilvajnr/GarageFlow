# Task-025 — Create Stock Base and Operations

## 0) Metadata
- `task_id`: `task-025`
- `slug`: `create-stock-base-and-operations`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-007-create-part-crud.md`, `task-008-create-supply-crud.md`, `task-021-create-purchase-order-base.md`

## 1) Objetivo
Implementar a base do agregado `Stock`, com operações de estoque canônicas e rastreabilidade mínima por operação, sem acoplar ainda com os fluxos de `SeparationOrder` e `ExecutionOrder`.

## 2) Escopo
### In
- Criar agregado `Stock` como raiz para controle de saldo por item de catálogo.
- Implementar operações de estoque:
  - `Entry` (entrada)
  - `Reserve` (reserva)
  - `Release` (liberação de reserva)
  - `Consume` (baixa/consumo)
  - `Adjust` (ajuste manual)
- Garantir invariantes de saldo:
  - não permitir saldo físico negativo;
  - não permitir saldo reservado negativo;
  - não permitir reservar acima do disponível.
- Expor API para:
  - consultar posição de estoque por item;
  - consultar extrato de operações por item e período.
- Implementar testes de domínio, aplicação e integração da base de estoque.

### Out
- Integração automática com `SeparationOrder`.
- Integração automática com `ExecutionOrder`.
- Compensações distribuídas/event bus/outbox/worker.
- Regras avançadas de inventário cíclico.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/domain/agregados.md)
- [docs/specs/V1/aggregates/stock.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/stock.md)
- [docs/architecture/application-and-integrations.md](/Users/marcos/Projects/GarageFlow/docs/architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- Estoque deve manter rastreabilidade por operação (quem/quando/por quê quando aplicável).
- Operações com quantidade inválida (<= 0) devem ser rejeitadas.
- Reserva depende de disponibilidade suficiente.
- Liberação não pode exceder o reservado.
- Consumo não pode exceder o reservado.

Regras mandatórias desta task:
- `Stock` é o nome canônico do agregado e do contexto técnico nesta implementação.
- Mensagens para usuário permanecem em português via `DomainErrorMessages`.
- Identificadores de código permanecem em inglês.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /stock/entries`
  - objetivo: registrar entrada de estoque.
- `POST /stock/reservations`
  - objetivo: registrar reserva de estoque.
- `POST /stock/releases`
  - objetivo: liberar quantidade reservada.
- `POST /stock/consumptions`
  - objetivo: registrar baixa/consumo.
- `POST /stock/adjustments`
  - objetivo: registrar ajuste manual com motivo obrigatório.
- `GET /stock/{itemType}/{itemId}`
  - objetivo: consultar posição atual (`OnHand`, `Reserved`, `Available`).
- `GET /stock/{itemType}/{itemId}/operations`
  - objetivo: consultar extrato por período/paginação.

`itemType` esperado:
- `part`
- `supply`

### 5.2 Matriz de erro obrigatória
- item inexistente (`part`/`supply`) -> `404`
- quantidade inválida -> `400`
- reserva acima do disponível -> `409`
- liberação acima do reservado -> `409`
- consumo acima do reservado -> `409`
- ajuste sem motivo -> `400`

Regras mandatórias:
- proibido parsing de `ex.Message` para definir HTTP status.
- mapear por tipo/causa da exceção.

### 5.3 Contratos internos
- Commands/Queries esperados:
  - `CreateStockEntryCommand`
  - `ReserveStockCommand`
  - `ReleaseStockReservationCommand`
  - `ConsumeStockCommand`
  - `AdjustStockCommand`
  - `GetStockPositionQuery`
  - `ListStockOperationsQuery`
- Repositórios/portas:
  - `IStockRepository`
  - `IStockOperationRepository` (se separado)

### 5.4 Erros de domínio
- Mensagens em português.
- sem strings inline em handlers/endpoints.
- catálogo central em `DomainErrorMessages`.

## 6) Plano Técnico por Camada
### Domain
- Criar agregado `Stock`.
- Criar entidade/VO para operação de estoque com tipo, quantidade e metadados mínimos.
- Implementar invariantes de saldo e métodos de operação.

### Application
- Implementar commands/queries e handlers de operações.
- Implementar validações de borda e orquestração de consulta.

### Infrastructure
- Mapear `Stock` e operações no EF Core.
- Índices por `itemType + itemId`.
- Persistir operações em ordem temporal.

### API
- Endpoints REST para operações e consulta.
- DTOs separados por arquivo.
- Mapping de erro consistente com tipos de exceção.

### Tests
- Domínio: invariantes e transições de saldo.
- Aplicação: handlers de operação e erros de negócio.
- Integração: endpoints principais e códigos HTTP.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Domain/Stock/Stock.cs`
- `src/GarageFlow.Domain/Stock/StockOperation.cs`
- `src/GarageFlow.Domain/Stock/StockOperationType.cs`
- `src/GarageFlow.Domain/Stock/IStockRepository.cs`
- `src/GarageFlow.Application/Stock/Commands/CreateStockEntryCommand.cs`
- `src/GarageFlow.Application/Stock/Commands/ReserveStockCommand.cs`
- `src/GarageFlow.Application/Stock/Commands/ReleaseStockReservationCommand.cs`
- `src/GarageFlow.Application/Stock/Commands/ConsumeStockCommand.cs`
- `src/GarageFlow.Application/Stock/Commands/AdjustStockCommand.cs`
- `src/GarageFlow.Application/Stock/Queries/GetStockPositionQuery.cs`
- `src/GarageFlow.Application/Stock/Queries/ListStockOperationsQuery.cs`
- `src/GarageFlow.Application/Stock/DTOs/StockPositionDto.cs`
- `src/GarageFlow.Application/Stock/DTOs/StockOperationDto.cs`
- `src/GarageFlow.Application/Stock/Handlers/CreateStockEntryHandler.cs`
- `src/GarageFlow.Application/Stock/Handlers/ReserveStockHandler.cs`
- `src/GarageFlow.Application/Stock/Handlers/ReleaseStockReservationHandler.cs`
- `src/GarageFlow.Application/Stock/Handlers/ConsumeStockHandler.cs`
- `src/GarageFlow.Application/Stock/Handlers/AdjustStockHandler.cs`
- `src/GarageFlow.Application/Stock/Handlers/GetStockPositionHandler.cs`
- `src/GarageFlow.Application/Stock/Handlers/ListStockOperationsHandler.cs`
- `src/GarageFlow.Api/DTOs/Stock/CreateStockEntryRequest.cs`
- `src/GarageFlow.Api/DTOs/Stock/ReserveStockRequest.cs`
- `src/GarageFlow.Api/DTOs/Stock/ReleaseStockReservationRequest.cs`
- `src/GarageFlow.Api/DTOs/Stock/ConsumeStockRequest.cs`
- `src/GarageFlow.Api/DTOs/Stock/AdjustStockRequest.cs`
- `src/GarageFlow.Api/DTOs/Stock/StockPositionResponse.cs`
- `src/GarageFlow.Api/DTOs/Stock/PagedStockOperationsResponse.cs`
- `src/GarageFlow.Api/Endpoints/Stock/StockEndpoints.cs`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/Stock/StockConfiguration.cs`
- `src/GarageFlow.Infrastructure/Persistence/Repositories/StockRepository.cs`
- `tests/GarageFlow.Tests/Domain/Stock/StockTests.cs`
- `tests/GarageFlow.Tests/Application/Stock/StockHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/Stock/StockEndpointsTests.cs`

### Alterar (esperado)
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs`
- `src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs`
- `src/GarageFlow.Infrastructure/DependencyInjection.cs`
- `src/GarageFlow.Application/DependencyInjection.cs`
- `src/GarageFlow.Api/Program.cs`

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] operações de estoque implementadas com invariantes válidas.
- [ ] consultas de posição e extrato funcionando.
- [ ] matriz de erro aderente e sem parsing textual.

## 9) Estratégia de Testes
### Domínio
- [ ] entrada aumenta saldo físico.
- [ ] reserva reduz disponível e aumenta reservado.
- [ ] liberação reduz reservado.
- [ ] consumo reduz reservado e físico.
- [ ] operações inválidas lançam exceções corretas.

### Aplicação
- [ ] handlers aplicam regras de validação e persistência.
- [ ] item inexistente retorna erro de entidade não encontrada.

### Integração
- [ ] endpoints de operação retornam códigos esperados.
- [ ] `GET` de posição retorna `OnHand`, `Reserved`, `Available`.
- [ ] `GET` de extrato retorna operações paginadas.

## 10) Riscos e Mitigações
- Risco: inconsistência de saldo por concorrência.
  - Mitigação: concorrência otimista e validação transacional.
- Risco: crescimento de extrato degradar consulta.
  - Mitigação: paginação obrigatória e índices por item/período.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Manter `Stock` como nome do agregado/contexto.
- [ ] Não usar parsing de mensagem para mapear HTTP.
- [ ] Garantir mensagens em português no catálogo central.
- [ ] Respeitar caminhos de arquivo definidos na task.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, sem conflito com canônico.

## Guardrails Não-Negociáveis
- Proibido acoplar esta task diretamente com `SeparationOrder` ou `ExecutionOrder`.
- Proibido worker/outbox/event bus.
- Proibido string inline de erro.
- Proibido criar estrutura de pastas alternativa.

## Assumptions
- O canônico de domínio prevalece em caso de divergência com implementação prévia.
- Integrações automáticas com separação/execução serão tratadas em tasks subsequentes.
