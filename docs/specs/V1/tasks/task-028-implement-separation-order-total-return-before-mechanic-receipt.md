# Task-028 — Implement SeparationOrder Total Return Before Mechanic Receipt

## 0) Metadata
- `task_id`: `task-028`
- `slug`: `implement-separation-order-total-return-before-mechanic-receipt`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: [task-019-create-separation-order-base.md](task-019-create-separation-order-base.md), [task-025-create-stock-base-and-operations.md](task-025-create-stock-base-and-operations.md), [task-026-integrate-separation-order-with-stock-reservation-and-release.md](task-026-integrate-separation-order-with-stock-reservation-and-release.md), [task-027-consume-stock-on-execution-completion.md](task-027-consume-stock-on-execution-completion.md)

## 1) Objetivo
Implementar devolução operacional total de itens da `SeparationOrder` antes de `ConfirmMechanicReceipt`, com rastreabilidade por ordem de separação e consistência no `Stock`.

## 2) Escopo
### In
- Implementar comando de devolução total vinculada à `SeparationOrder`.
- Permitir devolução somente quando a separação ainda não foi confirmada pelo mecânico.
- Devolver todos os itens da ordem (peças e insumos) em operação única.
- Validar quantidades de devolução contra quantidades retiradas/reservadas da separação.
- Atualizar status da separação conforme regra de negócio pós-devolução.
- Cobrir com testes de aplicação e integração.

### Out
- Devolução parcial por item.
- Definição de autorização por perfil nesta task (depende da esteira de autenticação/autorização ativa).
- Worker/outbox/event bus.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/Domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/Domain/agregados.md](../../../domain/agregados.md)
- [docs/Domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/Domain/bounded-contexts.md](../../../domain/bounded-contexts.md)
- [docs/specs/V1/aggregates/separation-order.md](../aggregates/separation-order.md)
- [docs/specs/V1/aggregates/stock.md](../aggregates/stock.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-013` — separação exige dupla confirmação de custódia.
- `RN-014` — `AvailableQuantity` nunca negativa.
- `RN-016` — operações de estoque preservam invariantes.
- `RN-032` — devolução operacional:
  - permitida somente antes de `ConfirmMechanicReceipt`;
  - vinculada obrigatoriamente à `SeparationOrder`;
  - total para todos os itens da ordem (peças e insumos);
  - bloqueada após `ConfirmMechanicReceipt`;
  - não pode exceder quantidade retirada.
- `RN-033` — ajuste manual de liberação de reserva:
  - permitido para peças e insumos;
  - exige justificativa obrigatória;
  - restrito ao perfil `Administrative`;
  - não substitui o fluxo de devolução total por `SeparationOrder`.

## 5) Contratos e Interfaces
### 5.1 API pública
- Novo endpoint:
  - `POST /separation-orders/{id}/return-total`

### 5.2 Matriz de erro obrigatória
- separação inexistente -> `404`
- separação já confirmada pelo mecânico -> `409`
- estado inválido para devolução -> `409`
- inconsistência de quantidade para devolução -> `409`

### 5.3 Contratos internos
- Novo comando e handler:
  - `ReturnSeparationOrderTotalCommand`
  - `ReturnSeparationOrderTotalHandler`
- Repositórios/portas:
  - `ISeparationOrderRepository`
  - `IStockRepository`

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- Sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- Evoluir `SeparationOrder` com método de retorno total coerente com status.
- Preservar invariantes de custódia e rastreabilidade.

### Application
- Orquestrar devolução total no handler:
  1) carregar `SeparationOrder`
  2) validar janela de devolução
  3) devolver todos os itens no `Stock`
  4) atualizar estado da separação

### Infrastructure
- Reuso de repositórios existentes no mesmo scope transacional.

### API
- Expor endpoint `POST /separation-orders/{id}/return-total`.
- Mapear exceções para `404/409` sem parsing textual.

### Tests
- Aplicação: fluxo feliz, bloqueio por estado, bloqueio após recebimento do mecânico.
- Integração: endpoint novo e reflexo em posição de estoque.

## 7) Arquivos a Criar/Alterar
### Alterar (esperado)
- [src/GarageFlow.Domain/Stock/SeparationOrder.cs](../../../../src/GarageFlow.Domain/Stock/SeparationOrder.cs)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs) (se necessário)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs) (se necessário)
- [src/GarageFlow.Application/Stock/Handlers/ReleaseStockReservationHandler.cs](../../../../src/GarageFlow.Application/Stock/Handlers/ReleaseStockReservationHandler.cs) (alinhamento RN-033)
- `src/GarageFlow.Api/Endpoints/Stock/SeparationOrdersEndpoints.cs`
- [tests/GarageFlow.Tests/Application/Stock/SeparationOrderHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/Stock/SeparationOrderHandlersTests.cs)
- [tests/GarageFlow.Tests/Integration/Stock/SeparationOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Stock/SeparationOrdersEndpointsTests.cs)
- [tests/GarageFlow.Tests/Application/Stock/StockHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/Stock/StockHandlersTests.cs) (alinhamento RN-033)
- [tests/GarageFlow.Tests/Integration/Stock/StockEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Stock/StockEndpointsTests.cs) (alinhamento RN-033)

### Criar (esperado)
- [src/GarageFlow.Application/Stock/Commands/ReturnSeparationOrderTotalCommand.cs](../../../../src/GarageFlow.Application/Stock/Commands/ReturnSeparationOrderTotalCommand.cs)
- [src/GarageFlow.Application/Stock/Handlers/ReturnSeparationOrderTotalHandler.cs](../../../../src/GarageFlow.Application/Stock/Handlers/ReturnSeparationOrderTotalHandler.cs)

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] devolução total antes de `ConfirmMechanicReceipt` funcionando para peças e insumos.
- [ ] devolução bloqueada após `ConfirmMechanicReceipt`.
- [ ] endpoint retorna códigos HTTP conforme matriz de erro.
- [ ] liberação manual via `/stock/releases` exige justificativa para peças e insumos.

## 9) Estratégia de Testes
### Aplicação
- [ ] `ReturnTotal` em separação elegível devolve todos os itens.
- [ ] `ReturnTotal` em separação já recebida por mecânico retorna conflito.
- [ ] `ReturnTotal` em separação inexistente retorna not found.

### Integração
- [ ] `POST /separation-orders/{id}/return-total` retorna `200` e corrige saldo de estoque.
- [ ] retorno após `ConfirmMechanicReceipt` retorna `409`.
- [ ] `POST /stock/releases` para peça/insumo sem `reason` retorna `400`.
- [ ] `POST /stock/releases` para peça/insumo com `reason` retorna `200`.

## 10) Riscos e Mitigações
- Risco: devolução parcial não intencional em caso de falha.
  - Mitigação: operação única e transacional para todos os itens.
- Risco: devolução fora da janela de custódia.
  - Mitigação: gate explícito por status e confirmação do mecânico.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Implementar com consistência transacional.
- [ ] Não fazer parsing de mensagem para mapear HTTP.
- [ ] Manter identificadores em inglês e mensagens em português.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, sem conflito com canônico.

## Guardrails Não-Negociáveis
- Proibido implementar devolução parcial nesta task.
- Proibido permitir devolução após `ConfirmMechanicReceipt`.
- Proibido usar ajuste manual para contornar regras de devolução total da `SeparationOrder`.

## Assumptions
- A devolução total pode retornar a separação para estado operacional anterior definido no domínio (a ser implementado conforme invariantes existentes).
- O fluxo de consumo em execução (task-027) permanece inalterado e será acionado apenas quando não houver devolução.
