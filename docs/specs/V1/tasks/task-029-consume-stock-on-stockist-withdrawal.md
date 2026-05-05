# Task-029 — Consume Stock on Stockist Withdrawal

## 0) Metadata
- `task_id`: `task-029`
- `slug`: `consume-stock-on-stockist-withdrawal`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-025-create-stock-base-and-operations.md`, `task-026-integrate-separation-order-with-stock-reservation-and-release.md`, `task-028-implement-separation-order-total-return-before-mechanic-receipt.md`

## 1) Objetivo
Realizar a baixa física de estoque (`Stock.Consume`) no momento de `ConfirmStockistWithdrawal`, garantindo que a custódia da `SeparationOrder` e o saldo de estoque fiquem consistentes no mesmo fluxo.

## 2) Escopo
### In
- Orquestrar consumo de estoque na confirmação de retirada do estoquista:
  - `POST /separation-orders/{id}/confirm-stockist-withdrawal`
- Consumir itens de `Part` e `Supply` associados à `SeparationOrder`.
- Garantir que o consumo use exatamente as quantidades reservadas da separação.
- Manter consistência transacional entre:
  - consumo no `Stock`;
  - transição da separação para `Separated`;
  - registro de custódia (`StockistId`, `ConfirmedByStockistAt`).
- Cobrir com testes de aplicação e integração.

### Out
- Reversão automática de consumo após `ConfirmStockistWithdrawal`.
- Alteração de autorização por perfil nesta task (depende da esteira de autenticação/autorização ativa).
- Worker/outbox/event bus.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/Domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/Domain/regras-de-negocio.md)
- [docs/Domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/Domain/agregados.md)
- [docs/Domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/Domain/linguagem-ubiqua.md)
- [docs/Domain/bounded-contexts.md](/Users/marcos/Projects/GarageFlow/docs/Domain/bounded-contexts.md)
- [docs/specs/V1/aggregates/separation-order.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/separation-order.md)
- [docs/specs/V1/aggregates/stock.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/stock.md)
- [docs/specs/V1/tasks/task-026-integrate-separation-order-with-stock-reservation-and-release.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/tasks/task-026-integrate-separation-order-with-stock-reservation-and-release.md)
- [docs/specs/V1/tasks/task-028-implement-separation-order-total-return-before-mechanic-receipt.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/tasks/task-028-implement-separation-order-total-return-before-mechanic-receipt.md)
- [docs/architecture/application-and-integrations.md](/Users/marcos/Projects/GarageFlow/docs/architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-013` — separação exige dupla confirmação de custódia.
- `RN-014` — `AvailableQuantity` nunca negativa.
- `RN-016` — operações de estoque preservam invariantes.
- `RN-032` — devolução operacional total ocorre apenas antes de `ConfirmMechanicReceipt`.
- `RN-033` — ajuste manual de release exige justificativa e não substitui devolução operacional por separação.

Regras mandatórias desta task:
- baixa de estoque deve ocorrer na retirada física confirmada pelo estoquista;
- consumo deve usar a quantidade reservada da `SeparationOrder`;
- se falhar consumo de qualquer item, `ConfirmStockistWithdrawal` não conclui;
- sem parsing de mensagem para mapear HTTP.

## 5) Contratos e Interfaces
### 5.1 API pública
- Manter contrato existente:
  - `POST /separation-orders/{id}/confirm-stockist-withdrawal`
- Sem criação de endpoint novo nesta task.

### 5.2 Matriz de erro obrigatória
- separação inexistente -> `404`
- estoque do item inexistente -> `404`
- transição inválida de separação -> `409`
- quantidade reservada insuficiente para consumo -> `409`
- `stockistId` inválido -> `400`

### 5.3 Contratos internos
- Evoluir `ConfirmSeparationStockistWithdrawalHandler` para orquestrar:
  1) carregar `SeparationOrder`;
  2) validar status e custódia;
  3) consumir estoque de `Parts` e `Supplies`;
  4) confirmar retirada do estoquista.
- Repositórios/portas esperadas:
  - `ISeparationOrderRepository`
  - `IStockRepository`

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- Sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- Reutilizar invariantes de `Stock.Consume`.
- Preservar invariantes de `SeparationOrder` para confirmação de custódia.

### Application
- Evoluir `ConfirmSeparationStockistWithdrawalHandler` para consumo + confirmação no mesmo fluxo.
- Garantir atomicidade lógica: ou consome tudo e confirma retirada, ou não confirma.

### Infrastructure
- Reuso de repositórios no mesmo scope transacional.
- Sem nova tabela nesta task.

### API
- Manter endpoint de confirmação de retirada.
- Ajustar mapping de exceções para `400/404/409` conforme tipo.

### Tests
- Aplicação: fluxo feliz, estoque insuficiente, separação em status inválido, separação ausente.
- Integração: endpoint de retirada refletindo baixa real no estoque.

## 7) Arquivos a Criar/Alterar
### Alterar (esperado)
- `src/GarageFlow.Application/Stock/Handlers/ConfirmSeparationStockistWithdrawalHandler.cs`
- `src/GarageFlow.Api/Endpoints/Stock/SeparationOrdersEndpoints.cs` (se necessário para mapping de exceções)
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs` (se necessário)
- `tests/GarageFlow.Tests/Application/Stock/SeparationOrderHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/Stock/SeparationOrdersEndpointsTests.cs`

### Criar (opcional, se necessário)
- `tests/GarageFlow.Tests/Integration/Stock/SeparationStockConsumptionEndpointsTests.cs`

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] `ConfirmStockistWithdrawal` consome estoque reservado de todos os itens da separação.
- [ ] falha de consumo impede mudança para `Separated`.
- [ ] endpoint retorna códigos HTTP conforme matriz de erro.
- [ ] fluxo de devolução total (task-028) permanece consistente após essa mudança.

## 9) Estratégia de Testes
### Aplicação
- [ ] confirmar retirada com estoque reservado suficiente consome estoque e muda status para `Separated`.
- [ ] confirmar retirada com estoque reservado insuficiente retorna conflito e mantém separação sem custódia confirmada.
- [ ] confirmar retirada em status inválido retorna conflito.
- [ ] confirmar retirada em separação inexistente retorna não encontrado.

### Integração
- [ ] `POST /separation-orders/{id}/confirm-stockist-withdrawal` retorna `200` no fluxo feliz e reflete novo saldo no `GET /stock/{itemType}/{itemId}`.
- [ ] `POST /separation-orders/{id}/confirm-stockist-withdrawal` retorna `409` quando consumo não pode ser aplicado.
- [ ] `POST /separation-orders/{id}/confirm-stockist-withdrawal` retorna `404` quando dependência obrigatória não existe.

## 10) Riscos e Mitigações
- Risco: confirmar retirada sem baixa real de estoque.
  - Mitigação: orquestrar consumo no mesmo handler da confirmação de retirada.
- Risco: consumo parcial em caso de falha no meio do processo.
  - Mitigação: só confirmar custódia após processar todos os itens com sucesso.
- Risco: conflito com devolução total da task-028.
  - Mitigação: manter devolução total como operação explícita, com validação de elegibilidade e quantidades.

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
- Proibido implementar devolução parcial nesta task.
- Proibido remover o gate de dupla custódia da separação.
- Proibido usar parsing de `ex.Message` para semântica HTTP.

## Assumptions
- Esta task altera o momento da baixa para retirada física confirmada (`ConfirmStockistWithdrawal`).
- O fluxo de execução (`ExecutionOrder`) continua dependente da conclusão de separação.
- Autorização por papel será aplicada em task específica de autenticação/autorização.
