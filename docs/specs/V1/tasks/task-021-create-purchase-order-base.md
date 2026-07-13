# Task-021 — Create PurchaseOrder Base

## 0) Metadata
- `task_id`: `task-021`
- `slug`: `create-purchase-order-base`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-019-create-separation-order-base.md](task-019-create-separation-order-base.md), [task-004-create-supplier-crud.md](task-004-create-supplier-crud.md), [task-007-create-part-crud.md](task-007-create-part-crud.md), [task-008-create-supply-crud.md](task-008-create-supply-crud.md)

## 1) Objetivo
Implementar a base funcional da `PurchaseOrder` com máquina de estados própria, seleção de fornecedor e conclusão de compra, estabelecendo o contexto de compras que afeta a separação.

## 2) Escopo
### In
- Implementar agregado `PurchaseOrder` ponta a ponta (Domain/Application/Infrastructure/API/Tests).
- Implementar entidade interna `PurchaseItem`.
- Implementar máquina de estados:
  - `Created -> Started -> Completed`
- Implementar comandos de ciclo de vida:
  - criar ordem de compra;
  - atribuir fornecedor;
  - iniciar compra;
  - concluir compra.
- Expor endpoints de criação, consulta e transições da compra.

### Out
- Retomada automática de `SeparationOrder` após compra concluída.
- Reposição automática em `Stock` após compra concluída.
- Orquestração de reserva de itens para separação após compra.
- Integração por eventos entre agregados (fica para task posterior).

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/specs/V1/aggregates/purchase-order.md](../aggregates/purchase-order.md)
- [docs/specs/V1/aggregates/separation-order.md](../aggregates/separation-order.md)
- [docs/specs/V1/aggregates/stock.md](../aggregates/stock.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-017` — falta de estoque gera ordem de compra.
- `RN-018` — ordem de compra é gerada automaticamente pelo sistema.
- `RN-019` — fornecedor deve ser selecionado antes de iniciar compra.
- `RN-020` — conclusão de compra permite retomada da separação (integração fora desta task).

Regras mandatórias desta task:
- `SeparationOrderIds` deve conter pelo menos um id válido.
- `Items` deve conter pelo menos um item válido.
- `AssignSupplier()` só pode ocorrer com status `Created`.
- `Start()` só pode ocorrer com status `Created` e `SupplierId` definido.
- `Complete()` só pode ocorrer com status `Started`.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /purchase-orders`
- `GET /purchase-orders/{id}`
- `GET /purchase-orders`
- `POST /purchase-orders/{id}/assign-supplier`
- `POST /purchase-orders/{id}/start`
- `POST /purchase-orders/{id}/complete`

### 5.2 Matriz de erro obrigatória
- criação:
  - payload inválido -> `400`
  - `separationOrderIds` vazio/inválido -> `400`
  - `items` vazio/inválido -> `400`
- comandos de transição:
  - ordem inexistente -> `404`
  - transição inválida de estado -> `409`
  - fornecedor ausente/inválido -> `400`

Regras mandatórias:
- proibido parsing de `ex.Message` para definir HTTP status.
- mapear por tipo/causa da exceção.

### 5.3 Contratos internos
- Commands:
  - `CreatePurchaseOrderCommand`
  - `AssignPurchaseOrderSupplierCommand`
  - `StartPurchaseOrderCommand`
  - `CompletePurchaseOrderCommand`
- Queries:
  - `GetPurchaseOrderByIdQuery`
  - `ListPurchaseOrdersQuery`
- Repositório:
  - `IPurchaseOrderRepository`

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- Implementar `PurchaseOrder`.
- Implementar `PurchaseOrderStatus`.
- Implementar `PurchaseItem` e `PurchaseItemType`.
- Implementar transições e validações de pré-condição.

### Application
- Implementar commands/queries/handlers do fluxo de compras.
- Implementar mapeamento para DTOs.

### Infrastructure
- EF Core mappings para agregado e itens.
- repositório EF da compra.
- migration para tabelas de compra.

### API
- Endpoints REST da compra.
- DTOs de request/response por operação.

### Tests
- Domínio: criação, validações e transições de status.
- Aplicação: handlers de sucesso/falha.
- Integração: contratos HTTP e códigos de erro.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- [src/GarageFlow.Domain/Purchasing/PurchaseOrder.cs](../../../../src/GarageFlow.Domain/Purchasing/PurchaseOrder.cs)
- [src/GarageFlow.Domain/Purchasing/PurchaseOrderStatus.cs](../../../../src/GarageFlow.Domain/Purchasing/PurchaseOrderStatus.cs)
- [src/GarageFlow.Domain/Purchasing/PurchaseItem.cs](../../../../src/GarageFlow.Domain/Purchasing/PurchaseItem.cs)
- [src/GarageFlow.Domain/Purchasing/PurchaseItemType.cs](../../../../src/GarageFlow.Domain/Purchasing/PurchaseItemType.cs)
- [src/GarageFlow.Domain/Purchasing/IPurchaseOrderRepository.cs](../../../../src/GarageFlow.Domain/Purchasing/IPurchaseOrderRepository.cs)
- [src/GarageFlow.Application/Purchasing/PurchaseOrderPaginationDefaults.cs](../../../../src/GarageFlow.Application/Purchasing/PurchaseOrderPaginationDefaults.cs)
- [src/GarageFlow.Application/Purchasing/Commands/CreatePurchaseOrderCommand.cs](../../../../src/GarageFlow.Application/Purchasing/Commands/CreatePurchaseOrderCommand.cs)
- [src/GarageFlow.Application/Purchasing/Commands/AssignPurchaseOrderSupplierCommand.cs](../../../../src/GarageFlow.Application/Purchasing/Commands/AssignPurchaseOrderSupplierCommand.cs)
- [src/GarageFlow.Application/Purchasing/Commands/StartPurchaseOrderCommand.cs](../../../../src/GarageFlow.Application/Purchasing/Commands/StartPurchaseOrderCommand.cs)
- [src/GarageFlow.Application/Purchasing/Commands/CompletePurchaseOrderCommand.cs](../../../../src/GarageFlow.Application/Purchasing/Commands/CompletePurchaseOrderCommand.cs)
- [src/GarageFlow.Application/Purchasing/Queries/GetPurchaseOrderByIdQuery.cs](../../../../src/GarageFlow.Application/Purchasing/Queries/GetPurchaseOrderByIdQuery.cs)
- [src/GarageFlow.Application/Purchasing/Queries/ListPurchaseOrdersQuery.cs](../../../../src/GarageFlow.Application/Purchasing/Queries/ListPurchaseOrdersQuery.cs)
- `src/GarageFlow.Application/Purchasing/Handlers/*`
- `src/GarageFlow.Application/Purchasing/DTOs/*`
- [src/GarageFlow.Infrastructure/Persistence/Configurations/Purchasing/PurchaseOrderConfiguration.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Configurations/Purchasing/PurchaseOrderConfiguration.cs)
- [src/GarageFlow.Infrastructure/Persistence/Repositories/PurchaseOrderRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/PurchaseOrderRepository.cs)
- `src/GarageFlow.Api/DTOs/Purchasing/*`
- `src/GarageFlow.Api/Endpoints/Purchasing/PurchaseOrdersEndpoints.cs`
- [tests/GarageFlow.Tests/Domain/Purchasing/PurchaseOrderTests.cs](../../../../tests/GarageFlow.Tests/Domain/Purchasing/PurchaseOrderTests.cs)
- [tests/GarageFlow.Tests/Application/Purchasing/PurchaseOrderHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/Purchasing/PurchaseOrderHandlersTests.cs)
- [tests/GarageFlow.Tests/Integration/Purchasing/PurchaseOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Purchasing/PurchaseOrdersEndpointsTests.cs)

### Alterar (esperado)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs)
- [src/GarageFlow.Infrastructure/DependencyInjection.cs](../../../../src/GarageFlow.Infrastructure/DependencyInjection.cs)
- [src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs](../../../../src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs)
- `src/GarageFlow.Infrastructure/Persistence/Migrations/*`
- `src/GarageFlow.Api/Program.cs`

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] Fluxo `Created -> Started -> Completed` validado.
- [ ] Atribuição de fornecedor com pré-condições validada.
- [ ] Endpoints funcionando com matriz de erro aderente.
- [ ] Migration criada para o contexto de compras.
- [ ] Schema validado pelos testes de integração de compras.

## 9) Estratégia de Testes
### Domínio
- [ ] criar compra válida.
- [ ] bloquear criação sem `separationOrderIds`.
- [ ] bloquear criação sem `items`.
- [ ] bloquear criação com item inválido.
- [ ] atribuir fornecedor válido em `Created`.
- [ ] bloquear atribuição de fornecedor inválido.
- [ ] bloquear atribuição após início.
- [ ] iniciar com fornecedor definido.
- [ ] bloquear início sem fornecedor.
- [ ] bloquear início em status inválido.
- [ ] concluir em `Started`.
- [ ] bloquear conclusão em status inválido.

### Aplicação
- [ ] handlers com sucesso e conflitos de estado.

### Integração
- [ ] contratos dos endpoints (`200/201/400/404/409`).

## 10) Riscos e Mitigações
- Risco: confundir responsabilidade de compra com orquestração de estoque/separação.
  - Mitigação: manter agregado `PurchaseOrder` isolado nesta task; integração vem depois.
- Risco: ambiguidade em itens mistos (`Part`/`Supply`).
  - Mitigação: modelar `PurchaseItemType` explícito e validar invariantes no domínio.

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
- Proibido implementar retomada automática de `SeparationOrder` nesta task.
- Proibido implementar atualização automática de `Stock` nesta task.
- Proibido strings inline de erro.
- Proibido mapping HTTP por parsing de texto.

## Assumptions
- A integração `PurchaseOrderCompleted -> SeparationOrder.ResumeAfterPurchase` será tratada na `task-022`.
