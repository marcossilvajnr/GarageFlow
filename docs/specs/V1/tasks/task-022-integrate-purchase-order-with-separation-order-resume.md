# Task-022 — Integrate PurchaseOrder With SeparationOrder Resume

## 0) Metadata
- `task_id`: `task-022`
- `slug`: `integrate-purchase-order-with-separation-order-resume`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-019-create-separation-order-base.md](task-019-create-separation-order-base.md), [task-021-create-purchase-order-base.md](task-021-create-purchase-order-base.md)

## 1) Objetivo
Implementar a integração de aplicação entre `PurchaseOrder` e `SeparationOrder` para suportar `RN-020`: conclusão de compra deve permitir retomada de separações que estavam em `WaitingPurchase`.

## 2) Escopo
### In
- Implementar fluxo de integração na camada de aplicação para retomar separações vinculadas após conclusão de compra.
- Definir contrato explícito entre `PurchaseOrder` e `SeparationOrder` via Application Service/Handler (sem acoplamento direto entre agregados).
- Garantir atualização consistente de status:
  - `SeparationOrder: WaitingPurchase -> WaitingPickup` (ou fluxo canônico vigente do agregado).
- Expor endpoint de conclusão de compra com efeito de retomada explícito no retorno/estado.
- Cobrir o fluxo com testes de aplicação e integração.

### Out
- Integração com `ExecutionOrder` (fica para `task-023`).
- Movimentação real de estoque (`Stock.Replenish`/`Stock.Reserve`) completa.
- Orquestração assíncrona com worker/outbox.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/specs/V1/aggregates/purchase-order.md](../aggregates/purchase-order.md)
- [docs/specs/V1/aggregates/separation-order.md](../aggregates/separation-order.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-017` — falta de estoque gera compra.
- `RN-018` — compra é gerada automaticamente.
- `RN-019` — fornecedor antes do início.
- `RN-020` — conclusão da compra retoma separações em espera.

Regras mandatórias desta task:
- Retomada só ocorre quando `PurchaseOrder.Status == Completed`.
- Retomada só ocorre para separações listadas em `PurchaseOrder.SeparationOrderIds`.
- Se separação não estiver em estado compatível de retomada, retornar erro de domínio controlado (`409`/`400` conforme matriz).
- Sem chamada de agregado para agregado diretamente: integração fica em handler/service de aplicação.

## 5) Contratos e Interfaces
### 5.1 API pública
- manter endpoint existente: `POST /purchase-orders/{id}/complete`
- comportamento estendido: conclusão da compra deve também executar retomada das separações vinculadas.

### 5.2 Matriz de erro obrigatória
- conclusão:
  - ordem inexistente -> `404`
  - transição inválida da compra -> `409`
  - separação vinculada inexistente -> `404`
  - separação em estado inválido para retomada -> `409`

Regras mandatórias:
- proibido parsing de `ex.Message` para definir HTTP status.
- mapear por tipo/causa da exceção.

### 5.3 Contratos internos
- Orquestrador/handler de aplicação:
  - concluir compra;
  - carregar separações vinculadas;
  - aplicar `ResumeAfterPurchase` em cada separação;
  - persistir transação lógica da operação.

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- sem acoplamento direto entre `PurchaseOrder` e `SeparationOrder`.
- manter invariantes existentes dos dois agregados.

### Application
- evoluir handler de conclusão de compra para orquestrar retomada das separações.
- definir ordem de execução e tratamento de falhas consistente.

### Infrastructure
- garantir repositórios necessários no fluxo orquestrado.
- persistência consistente na mesma unidade de trabalho.

### API
- manter contrato REST estável.
- refletir corretamente erros de integração de estado.

### Tests
- Aplicação: cenários de sucesso e falha da retomada.
- Integração: `complete purchase` refletindo retomada de separação.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- [tests/GarageFlow.Tests/Application/Purchasing/PurchaseOrderSeparationIntegrationTests.cs](../../../../tests/GarageFlow.Tests/Application/Purchasing/PurchaseOrderSeparationIntegrationTests.cs)
- [tests/GarageFlow.Tests/Integration/Purchasing/PurchaseOrderSeparationIntegrationEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Purchasing/PurchaseOrderSeparationIntegrationEndpointsTests.cs)

### Alterar (esperado)
- [src/GarageFlow.Application/Purchasing/Handlers/CompletePurchaseOrderHandler.cs](../../../../src/GarageFlow.Application/Purchasing/Handlers/CompletePurchaseOrderHandler.cs)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs)
- `src/GarageFlow.Api/Endpoints/Purchasing/PurchaseOrdersEndpoints.cs` (apenas mapping de erro/comportamento)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs) (se necessário, somente mensagens canônicas)
- [tests/GarageFlow.Tests/Integration/Purchasing/PurchaseOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Purchasing/PurchaseOrdersEndpointsTests.cs)

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] `POST /purchase-orders/{id}/complete` conclui compra e retoma separações vinculadas válidas.
- [ ] Tratamento de erro aderente para separação inexistente/estado inválido.
- [ ] Sem acoplamento direto entre agregados.

## 9) Estratégia de Testes
### Aplicação
- [ ] concluir compra com separação vinculada em `WaitingPurchase` retoma com sucesso.
- [ ] concluir compra com separação inexistente falha com erro mapeado.
- [ ] concluir compra com separação em estado inválido falha com erro mapeado.

### Integração
- [ ] endpoint `complete` retorna `200` e separação muda de status no fluxo feliz.
- [ ] endpoint `complete` retorna `404` para vínculo inexistente.
- [ ] endpoint `complete` retorna `409` para conflito de estado.

## 10) Riscos e Mitigações
- Risco: partial update entre compra concluída e separação não retomada.
  - Mitigação: tratar operação como unidade lógica única na aplicação.
- Risco: acoplamento indevido de domínio.
  - Mitigação: integração somente via camada de aplicação.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Implementar integração na camada de aplicação, não no domínio entre agregados.
- [ ] Garantir mensagens de erro em português via catálogo central.
- [ ] Não fazer parsing de mensagem para status HTTP.
- [ ] Respeitar caminhos de arquivo da task.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, sem conflito com canônico.

## Guardrails Não-Negociáveis
- Proibido integrar `ExecutionOrder` nesta task.
- Proibido implementar worker/outbox.
- Proibido strings inline de erro.
- Proibido mapping HTTP por parsing de texto.

## Assumptions
- A integração `SeparationOrder -> ExecutionOrder` será tratada na `task-023`.
