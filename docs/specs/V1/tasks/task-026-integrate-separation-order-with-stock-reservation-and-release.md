# Task-026 — Integrate SeparationOrder with Stock Reservation and Release

## 0) Metadata
- `task_id`: `task-026`
- `slug`: `integrate-separation-order-with-stock-reservation-and-release`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-019-create-separation-order-base.md`, `task-022-integrate-purchase-order-with-separation-order-resume.md`, `task-025-create-stock-base-and-operations.md`

## 1) Objetivo
Integrar `SeparationOrder` com o agregado `Stock`, aplicando reserva automática de itens no fluxo de separação e liberação controlada quando houver retorno elegível.

## 2) Escopo
### In
- Orquestrar reserva de estoque ao executar:
  - `POST /separation-orders/{id}/reserve`
  - `POST /separation-orders/{id}/resume-after-purchase`
- Aplicar operação `Stock.Reserve(...)` para itens de `Part` e `Supply`.
- Garantir idempotência de reserva por status da `SeparationOrder`.
- Implementar liberação de reserva para peças quando houver retorno elegível no fluxo da separação (sem liberar insumo).
- Garantir consistência transacional entre mudança de status da separação e movimentação no `Stock`.
- Cobrir com testes de aplicação e integração.

### Out
- Consumo definitivo de estoque na conclusão da execução (será task posterior).
- Reversão de consumo após execução.
- Worker/outbox/event bus.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/Domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/Domain/regras-de-negocio.md)
- [docs/Domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/Domain/agregados.md)
- [docs/Domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/Domain/linguagem-ubiqua.md)
- [docs/specs/V1/aggregates/stock.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/stock.md)
- [docs/specs/V1/tasks/task-025-create-stock-base-and-operations.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/tasks/task-025-create-stock-base-and-operations.md)
- [docs/architecture/application-and-integrations.md](/Users/marcos/Projects/GarageFlow/docs/architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-012` — separação valida disponibilidade e reserva itens.
- `RN-013` — separação segue dupla confirmação de custódia.
- `RN-014` — disponibilidade nunca negativa.
- `RN-016` — operações de estoque e regra de liberação apenas para peças.
- `RN-020` — retomada após compra deve reservar e retomar separação.

Regras mandatórias desta task:
- reserva só ocorre quando transição de estado de separação for válida.
- `Supply` não pode sofrer operação de liberação de reserva no fluxo de retorno.
- sem parsing de mensagem para mapear HTTP.

## 5) Contratos e Interfaces
### 5.1 API pública
- Manter contratos existentes de separação:
  - `POST /separation-orders/{id}/reserve`
  - `POST /separation-orders/{id}/resume-after-purchase`
- Sem novos endpoints públicos obrigatórios nesta task.

### 5.2 Matriz de erro obrigatória
- separação inexistente -> `404`
- item de estoque inexistente -> `404`
- transição inválida de separação -> `409`
- estoque insuficiente para reserva -> `409`
- tentativa de liberar insumo -> `400`

### 5.3 Contratos internos
- `ReserveSeparationOrderHandler` deve orquestrar:
  1) carregar `SeparationOrder`
  2) reservar itens no `Stock`
  3) atualizar status da separação
- `ResumeSeparationOrderAfterPurchaseHandler` deve repetir a lógica de reserva antes de retomar status.

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- Sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- Manter invariantes já existentes de `SeparationOrder` e `Stock`.
- Não acoplar agregados diretamente.

### Application
- Evoluir handlers de separação para chamar operações de `Stock` por item.
- Garantir que operação parcial não deixe separação e estoque inconsistentes.

### Infrastructure
- Reutilizar `IStockRepository` e repositório de separação no mesmo scope transacional.

### API
- Manter endpoints e contratos existentes.
- Ajustar mapping de exceções para refletir conflitos reais de estoque.

### Tests
- Aplicação: cenário com reserva total, estoque insuficiente e retomada após compra.
- Integração: endpoints de separação refletindo impacto no estoque.

## 7) Arquivos a Criar/Alterar
### Alterar (esperado)
- `src/GarageFlow.Application/Stock/Handlers/ReserveSeparationOrderHandler.cs`
- `src/GarageFlow.Application/Stock/Handlers/ResumeSeparationOrderAfterPurchaseHandler.cs`
- `src/GarageFlow.Application/Stock/Handlers/WaitSeparationOrderPurchaseHandler.cs` (se necessário para consistência de fluxo)
- `src/GarageFlow.Api/Endpoints/Stock/SeparationOrdersEndpoints.cs`
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs` (se necessário)
- `tests/GarageFlow.Tests/Application/Stock/SeparationOrderHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/Stock/SeparationOrdersEndpointsTests.cs`
- `tests/GarageFlow.Tests/Integration/Stock/SeparationExecutionIntegrationEndpointsTests.cs` (se afetado)

### Criar (opcional, se necessário)
- `tests/GarageFlow.Tests/Application/Stock/SeparationStockReservationIntegrationTests.cs`

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] reserva em separação afeta estoque corretamente (`Part` e `Supply`).
- [ ] falta de estoque impede transição da separação com erro `409`.
- [ ] retomada após compra aplica nova reserva e retoma status.
- [ ] fluxo de liberação respeita regra de peça/insumo.

## 9) Estratégia de Testes
### Aplicação
- [ ] `ReserveSeparationOrder` reserva todos os itens e muda para `WaitingPickup`.
- [ ] `ReserveSeparationOrder` com estoque insuficiente retorna conflito sem mudança de status.
- [ ] `ResumeAfterPurchase` reserva novamente e muda para `WaitingPickup`.

### Integração
- [ ] `POST /separation-orders/{id}/reserve` retorna `200` e posição de estoque é atualizada.
- [ ] `POST /separation-orders/{id}/reserve` com estoque insuficiente retorna `409`.
- [ ] `POST /separation-orders/{id}/resume-after-purchase` retorna `200` e reaplica reserva.

## 10) Riscos e Mitigações
- Risco: inconsistência entre status da separação e estoque reservado.
  - Mitigação: orquestração única no handler e persistência transacional no mesmo `DbContext`.
- Risco: dupla reserva por repetição de chamada.
  - Mitigação: respeitar gate de status já existente na separação para idempotência comportamental.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Não criar endpoint novo sem necessidade.
- [ ] Garantir mapping de erro por tipo de exceção.
- [ ] Manter identificadores em inglês e mensagens em português.
- [ ] Respeitar paths mandatórios da task.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, sem conflito com canônico.

## Guardrails Não-Negociáveis
- Proibido implementar consumo definitivo de estoque nesta task.
- Proibido adicionar worker/outbox/event bus.
- Proibido parsing de `ex.Message` para semântica HTTP.

## Assumptions
- O agregado `Stock` criado na task-025 é a única fonte de saldo para reservas desta etapa.
- Ajustes de execução/consumo virão em task posterior dedicada.
