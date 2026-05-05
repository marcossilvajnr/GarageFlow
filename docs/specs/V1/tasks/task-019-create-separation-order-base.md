# Task-019 — Create SeparationOrder Base

## 0) Metadata
- `task_id`: `task-019`
- `slug`: `create-separation-order-base`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-018-create-service-order-quote-decision-status-gate.md`

## 1) Objetivo
Implementar a base funcional da `SeparationOrder` com fluxo de estados próprio, confirmação de custódia e contratos HTTP, preparando o terreno para integração com `ExecutionOrder` em task posterior.

## 2) Escopo
### In
- Implementar agregado `SeparationOrder` ponta a ponta (Domain/Application/Infrastructure/API/Tests).
- Implementar fluxo de status da separação:
  - `Pending -> WaitingPurchase -> WaitingPickup -> Separated -> Completed`
  - `Pending -> WaitingPickup -> Separated -> Completed`
- Implementar dupla confirmação de custódia:
  - confirmação de retirada pelo estoquista;
  - confirmação de recebimento pelo mecânico.
- Implementar criação da separação com listas de `Parts` e `Supplies`.
- Expor endpoints de ciclo de vida da separação.

### Out
- Integração automática com `ExecutionOrder`.
- Gatilho por evento para tornar execução `Ready`.
- Orquestração de compra completa (apenas status da separação nesta task).
- Movimentação física de estoque no agregado `Stock`.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/domain/agregados.md)
- [docs/specs/V1/aggregates/separation-order.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/separation-order.md)
- [docs/specs/V1/aggregates/service-order.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/service-order.md)
- [docs/architecture/application-and-integrations.md](/Users/marcos/Projects/GarageFlow/docs/architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-011` — separação é criada para suportar a execução.
- `RN-012` — separação diferencia caminho com e sem disponibilidade.
- `RN-013` — conclusão depende de dupla confirmação de custódia.
- `RN-020` — retomada após compra concluída é suportada no fluxo de separação.

Regras mandatórias desta task:
- `Completed` só ocorre após confirmação do mecânico sobre uma separação já marcada como `Separated`.
- confirmação do mecânico sem confirmação prévia do estoquista deve falhar.
- itens devem manter distinção entre `Parts` e `Supplies`.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /separation-orders`
- `GET /separation-orders/{id}`
- `GET /separation-orders`
- `POST /separation-orders/{id}/reserve`
- `POST /separation-orders/{id}/wait-purchase`
- `POST /separation-orders/{id}/resume-after-purchase`
- `POST /separation-orders/{id}/confirm-stockist-withdrawal`
- `POST /separation-orders/{id}/confirm-mechanic-receipt`

### 5.2 Matriz de erro obrigatória
- criação:
  - payload inválido -> `400`
  - itens ausentes/inválidos -> `400`
- comandos de transição:
  - separação inexistente -> `404`
  - transição inválida de estado -> `409`
  - confirmação sem pré-condição de custódia -> `409`

Regras mandatórias:
- proibido parsing de `ex.Message` para definir HTTP status.
- mapear por tipo/causa da exceção.

### 5.3 Contratos internos
- Commands:
  - `CreateSeparationOrderCommand`
  - `ReserveSeparationOrderCommand`
  - `WaitSeparationOrderPurchaseCommand`
  - `ResumeSeparationOrderAfterPurchaseCommand`
  - `ConfirmSeparationStockistWithdrawalCommand`
  - `ConfirmSeparationMechanicReceiptCommand`
- Queries:
  - `GetSeparationOrderByIdQuery`
  - `ListSeparationOrdersQuery`
- Repositório:
  - `ISeparationOrderRepository`

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- Implementar `SeparationOrder` e seus itens (`SeparationPartItem`, `SeparationSupplyItem`).
- Implementar `SeparationOrderStatus` com máquina de estados canônica.
- Implementar métodos de transição e validação de pré-condições.

### Application
- Implementar commands/queries/handlers da separação.
- Implementar mapeamento para DTOs.

### Infrastructure
- EF Core mappings para agregado e coleções de itens.
- Repositório EF da separação.
- migration para tabelas da separação.

### API
- Endpoints REST da separação.
- DTOs de request/response por operação.

### Tests
- Domínio: transições válidas/inválidas e dupla confirmação.
- Aplicação: handlers de sucesso/falha.
- Integração: contratos HTTP e códigos de erro.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Domain/Stock/SeparationOrder.cs`
- `src/GarageFlow.Domain/Stock/SeparationOrderStatus.cs`
- `src/GarageFlow.Domain/Stock/SeparationPartItem.cs`
- `src/GarageFlow.Domain/Stock/SeparationSupplyItem.cs`
- `src/GarageFlow.Domain/Stock/ISeparationOrderRepository.cs`
- `src/GarageFlow.Application/Stock/Commands/CreateSeparationOrderCommand.cs`
- `src/GarageFlow.Application/Stock/Commands/ReserveSeparationOrderCommand.cs`
- `src/GarageFlow.Application/Stock/Commands/WaitSeparationOrderPurchaseCommand.cs`
- `src/GarageFlow.Application/Stock/Commands/ResumeSeparationOrderAfterPurchaseCommand.cs`
- `src/GarageFlow.Application/Stock/Commands/ConfirmSeparationStockistWithdrawalCommand.cs`
- `src/GarageFlow.Application/Stock/Commands/ConfirmSeparationMechanicReceiptCommand.cs`
- `src/GarageFlow.Application/Stock/Queries/GetSeparationOrderByIdQuery.cs`
- `src/GarageFlow.Application/Stock/Queries/ListSeparationOrdersQuery.cs`
- `src/GarageFlow.Application/Stock/Handlers/*`
- `src/GarageFlow.Application/Stock/DTOs/*`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/Stock/SeparationOrderConfiguration.cs`
- `src/GarageFlow.Infrastructure/Persistence/Repositories/SeparationOrderRepository.cs`
- `src/GarageFlow.Api/DTOs/Stock/*`
- `src/GarageFlow.Api/Endpoints/Stock/SeparationOrdersEndpoints.cs`
- `tests/GarageFlow.Tests/Domain/Stock/SeparationOrderTests.cs`
- `tests/GarageFlow.Tests/Application/Stock/SeparationOrderHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/Stock/SeparationOrdersEndpointsTests.cs`

### Alterar (esperado)
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs`
- `src/GarageFlow.Application/DependencyInjection.cs`
- `src/GarageFlow.Infrastructure/DependencyInjection.cs`
- `src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs`
- `src/GarageFlow.Infrastructure/Persistence/Migrations/*`
- `src/GarageFlow.Api/Program.cs`

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] Fluxos de status da separação cobrindo caminho com e sem compra.
- [ ] Dupla confirmação de custódia validada.
- [ ] Endpoints funcionando com matriz de erro aderente.
- [ ] Migration criada para o contexto de separação.
- [ ] Schema validado pelos testes de integração da separação.

## 9) Estratégia de Testes
### Domínio
- [ ] criar separação válida com peças e/ou insumos.
- [ ] bloquear criação sem itens.
- [ ] reservar com `Pending`.
- [ ] transição para `WaitingPurchase` com `Pending`.
- [ ] retomada de compra apenas em `WaitingPurchase`.
- [ ] confirmação estoquista apenas em `WaitingPickup`.
- [ ] confirmação mecânico apenas em `Separated` e com custódia prévia.

### Aplicação
- [ ] handlers com sucesso e conflitos de estado.

### Integração
- [ ] contratos dos endpoints (`200/201/400/404/409`).

## 10) Riscos e Mitigações
- Risco: ambiguidade entre responsabilidade de separação e estoque.
  - Mitigação: manter foco da task na máquina de estados da separação.
- Risco: antecipar integração com execução.
  - Mitigação: manter explicitamente fora de escopo nesta task.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Implementar vertical slice completo.
- [ ] Garantir mensagens de erro em português via catálogo central.
- [ ] Não fazer parsing de mensagem para status HTTP.
- [ ] Respeitar caminhos de arquivo da task.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, sem conflito com canônico.

## Guardrails Não-Negociáveis
- Proibido implementar integração com `ExecutionOrder` nesta task.
- Proibido strings inline de erro.
- Proibido mapping HTTP por parsing de texto.

## Assumptions
- A integração `SeparationOrder -> ExecutionOrder` será tratada na `task-021`.
