# Task-046 — Enforce Service Order Delivery Gate and Extend Existing E2E Flows

## 0) Metadata
- `task_id`: `task-046`
- `slug`: `enforce-service-order-delivery-gate-and-extend-existing-e2e-flows`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-045-e2e-customer-cancellation-at-latest-allowed-stage.md](task-045-e2e-customer-cancellation-at-latest-allowed-stage.md)

## 1) Objetivo
Fechar o fluxo operacional final da OS garantindo regra explícita de entrega (`Delivered`) e ajustar os E2E críticos já existentes para cobrir o encerramento completo até entrega, sem criar novos cenários E2E paralelos.

## 2) Escopo
### In
- Endurecer regra de transição para entrega da OS no domínio/aplicação/API.
- Garantir endpoint/ação de entrega com contrato HTTP consistente.
- Atualizar os E2E existentes dos fluxos felizes para irem até `Delivered`:
  - `task-043` (estoque suficiente)
  - `task-044` (falta de estoque com compra)
- Validar bloqueio de entrega antecipada (pré-condições não atendidas).

### Out
- Criação de novos arquivos de cenário E2E para entrega.
- Mudanças de autenticação JWT real.
- Novas features de domínio fora do fluxo de entrega final.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md)
- [docs/specs/V1/aggregates/execution-order.md](../aggregates/execution-order.md)
- [docs/specs/V1/aggregates/separation-order.md](../aggregates/separation-order.md)
- [docs/specs/V1/tasks/task-039-document-e2e-critical-flow-coverage.md](task-039-document-e2e-critical-flow-coverage.md)
- [docs/architecture/testing-and-quality.md](../../../architecture/testing-and-quality.md)

## 4) Regras de Negócio Aplicáveis
- `RN-003` — progressão de status da OS.
- Regras canônicas de encerramento de execução e conclusão da OS.
- Regra operacional de entrega apenas após conclusão efetiva do fluxo.

## 5) Contratos e Interfaces
### 5.1 Endpoint de entrega
- Garantir operação de entrega da OS (reuso de endpoint existente ou adição de endpoint dedicado canônico):
  - referência esperada: `POST /service-orders/{id}/deliver`
- Matriz de resposta:
  - `200` entrega concluída com sucesso;
  - `404` OS inexistente;
  - `409` transição inválida/fluxo ainda não concluído.

### 5.2 Pré-condições obrigatórias de entrega
- `ServiceOrder` em estado apto para entrega (após conclusão operacional).
- Todas `ExecutionOrder` vinculadas concluídas.
- Sem pendências abertas de separação/compra vinculadas ao fluxo da OS.

## 6) Plano Técnico por Camada
### Domain
- Revisar/agregar guard de entrega na entidade `ServiceOrder`.
- Garantir exceção de transição inválida específica quando necessário.

### Application
- Ajustar handler/comando de entrega para validar pré-condições cross-aggregate.

### API
- Garantir mapeamento HTTP consistente (`200/404/409`) sem parsing textual.

### Tests
- **Não criar novo cenário E2E dedicado**.
- Ajustar os cenários existentes:
  - `ServiceOrderSufficientStockE2ETests` deve terminar em `Delivered`.
  - `ServiceOrderStockShortagePurchaseE2ETests` deve terminar em `Delivered`.
- Incluir asserts negativos no próprio fluxo:
  - tentativa de entregar antes de finalizar execução -> `409`.

## 7) Arquivos a Criar/Alterar
- `src/GarageFlow.Domain/ServiceOrders/**`
- `src/GarageFlow.Application/ServiceOrders/**`
- `src/GarageFlow.Api/Endpoints/ServiceOrders/**`
- [tests/GarageFlow.Tests/E2E/ServiceOrders/ServiceOrderSufficientStockE2ETests.cs](../../../../tests/GarageFlow.Tests/E2E/ServiceOrders/ServiceOrderSufficientStockE2ETests.cs)
- [tests/GarageFlow.Tests/E2E/ServiceOrders/ServiceOrderStockShortagePurchaseE2ETests.cs](../../../../tests/GarageFlow.Tests/E2E/ServiceOrders/ServiceOrderStockShortagePurchaseE2ETests.cs)
- `tests/GarageFlow.Tests/Integration/ServiceOrders/**` (se necessário para cobertura de contrato)
- [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md) (somente se houver ajuste canônico de contrato)

## 8) Critérios de Pronto
- [ ] Entrega da OS bloqueada quando pré-condições não atendidas.
- [ ] Entrega da OS permitida quando fluxo está completo.
- [ ] E2E de `task-043` atualizado até `Delivered`.
- [ ] E2E de `task-044` atualizado até `Delivered`.
- [ ] Sem criação de novo arquivo E2E para esse objetivo.
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test --filter "FullyQualifiedName~E2E"` verde.
- [ ] `dotnet test` verde.

## 9) Estratégia de Testes
### Domínio
- [ ] garantir regra de entrega em estado inválido (erro).
- [ ] garantir entrega em estado válido (sucesso).

### Integração
- [ ] contrato HTTP da entrega (`200/404/409`).

### E2E (reuso de cenários existentes)
- [ ] fluxo de estoque suficiente com estado final `Delivered`.
- [ ] fluxo com compra/retomada com estado final `Delivered`.
- [ ] tentativa de entrega antecipada bloqueada dentro dos fluxos existentes.

## 10) Riscos e Mitigações
- Risco: introduzir nova rota/ação divergente da canônica.
  - Mitigação: validar primeiro contrato em [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md).
- Risco: regressão nos fluxos E2E já estáveis.
  - Mitigação: alterar incrementalmente os testes existentes e validar suíte E2E completa.
- Risco: inconsistência de estado entre OS e ordens filhas.
  - Mitigação: validação cross-aggregate explícita no handler de entrega.

## 11) Checklist de Execução para IA
- [ ] Confirmar contrato canônico de entrega antes de codar.
- [ ] Ajustar regra no domínio antes de tocar E2E.
- [ ] Reusar E2E existentes (`043` e `044`) sem criar novos cenários.
- [ ] Validar erros HTTP sem parsing de mensagem.
- [ ] Executar `dotnet build`.
- [ ] Executar `dotnet test --filter "FullyQualifiedName~E2E"`.
- [ ] Executar `dotnet test`.
