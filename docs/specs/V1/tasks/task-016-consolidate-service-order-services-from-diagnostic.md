# Task-016 — Consolidate ServiceOrder Services from Diagnostic

## 0) Metadata
- `task_id`: `task-016`
- `slug`: `consolidate-service-order-services-from-diagnostic`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-000-template.md`, `task-012-create-service-order-service-integration-frontdesk.md`, `task-015-create-diagnostic-base-and-service-selection.md`

## 1) Objetivo
Consolidar na `ServiceOrder` os serviços selecionados no diagnóstico, preservando rastreabilidade de origem (`FrontDesk` vs `Diagnostic`) e histórico de alterações para suportar as próximas etapas de orçamento.

## 2) Escopo
### In
- Implementar operação de consolidação após `Diagnostic.Completed`:
  - serviços selecionados no diagnóstico passam a integrar a lista oficial de serviços da OS.
- Preservar rastreabilidade por item:
  - `Source = Diagnostic`
  - `AddedByActorId = mechanicId do diagnóstico`
  - `AddedAt` com timestamp de consolidação.
- Manter serviços já existentes da recepção (`Source = FrontDesk`) sem perda de histórico.
- Implementar regra de deduplicação na consolidação:
  - se serviço do diagnóstico já estiver ativo na OS, não duplicar item.
- Registrar histórico em `ServiceHistory` para cada inclusão vinda do diagnóstico.
- Expor endpoint dedicado para consolidar:
  - `POST /service-orders/{id}/diagnostic/consolidate-services`
- Atualizar leitura da OS (`GET /service-orders/{id}` e `GET /service-orders`) com estado consolidado de serviços.

### Out
- Cálculo de orçamento (`Quote`).
- Aprovação/rejeição de orçamento.
- Movimentação de estoque, separação, execução ou compras.
- Eventos assíncronos/outbox.

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/domain/agregados.md)
- [docs/specs/V1/aggregates/service-order.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/service-order.md)
- [docs/specs/V1/aggregates/diagnostic.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/diagnostic.md)
- [docs/architecture/application-and-integrations.md](/Users/marcos/Projects/GarageFlow/docs/architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- RN-026: alterações no diagnóstico só enquanto `InProgress`.
- RN-028: diagnóstico não reabre após concluído.
- RN-029: rastreabilidade de alterações de serviço na OS (origem, ator, tempo).

Regras mandatórias desta task:
- consolidação só pode ocorrer com diagnóstico `Completed`.
- diagnóstico sem serviços não pode consolidar.
- não duplicar serviço ativo já existente na OS.
- não remover automaticamente serviço adicionado pela recepção nesta etapa.

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /service-orders/{id}/diagnostic/consolidate-services`
  - Request: sem body
  - Response: `200` com `ServiceOrderResponse` atualizado.

- `GET /service-orders/{id}`
  - Deve refletir lista consolidada de serviços e `serviceHistory`.

### 5.2 Matriz de erro obrigatória
- `POST /diagnostic/consolidate-services`
  - OS inexistente -> `404`
  - diagnóstico inexistente -> `409`
  - diagnóstico não concluído -> `409`
  - diagnóstico sem serviços -> `409`

Regras mandatórias:
- proibido parsing de `ex.Message` para mapear HTTP status.
- mapear erro por tipo/causa.

### 5.3 Contratos internos
- Command:
  - `ConsolidateDiagnosticServicesCommand`
- Handler:
  - `ConsolidateDiagnosticServicesHandler`
- Repositório:
  - `IServiceOrderRepository`

## 6) Plano Técnico por Camada
### Domain
- Evoluir `ServiceOrder` com método de negócio:
  - `ConsolidateDiagnosticServices()`
- Regras do método:
  - validar presença e status do diagnóstico;
  - iterar `Diagnostic.SelectedServices`;
  - incluir apenas serviços ainda não ativos na OS;
  - registrar histórico (`Added`, `Source.Diagnostic`, `actor = MechanicId`);
  - atualizar `UpdatedAt`.

### Application
- Criar command/handler de consolidação.
- Carregar OS, executar método de domínio, persistir e retornar DTO.

### Infrastructure
- Reaproveitar mapeamento atual de `Services` e `ServiceHistory`.
- Criar migration somente se surgir alteração estrutural necessária.

### API
- Adicionar endpoint em `ServiceOrdersEndpoints`.
- Mapear exceções para matriz de erro.

### Tests
- Domínio:
  - consolida serviços de diagnóstico concluído;
  - ignora duplicados já ativos;
  - falha sem diagnóstico;
  - falha com diagnóstico não concluído;
  - falha sem serviços.
- Aplicação:
  - handler sucesso/falhas.
- Integração:
  - endpoint de consolidação + leitura de OS consolidada.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- `src/GarageFlow.Application/ServiceOrders/Commands/ConsolidateDiagnosticServicesCommand.cs`
- `src/GarageFlow.Application/ServiceOrders/Handlers/ConsolidateDiagnosticServicesHandler.cs`
- `tests/GarageFlow.Tests/Domain/ServiceOrders/ServiceOrderDiagnosticConsolidationTests.cs`

### Alterar (esperado)
- `src/GarageFlow.Domain/ServiceOrders/ServiceOrder.cs`
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs`
- `src/GarageFlow.Application/DependencyInjection.cs`
- `src/GarageFlow.Api/Endpoints/ServiceOrders/ServiceOrdersEndpoints.cs`
- `src/GarageFlow.Application/ServiceOrders/DTOs/ServiceOrderDto.cs` (se necessário)
- `src/GarageFlow.Application/ServiceOrders/Handlers/ServiceOrderMapper.cs` (se necessário)
- `tests/GarageFlow.Tests/Application/ServiceOrders/DiagnosticHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs`

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] Endpoint `POST /service-orders/{id}/diagnostic/consolidate-services` funcional no Swagger.
- [ ] Serviços da OS refletem consolidação do diagnóstico concluído.
- [ ] Histórico de serviço registra inclusões de origem `Diagnostic`.
- [ ] Sem parsing de mensagem para HTTP status.

## 9) Estratégia de Testes
### Domínio
- [ ] Consolidar diagnóstico concluído com múltiplos serviços.
- [ ] Não duplicar serviço já ativo na OS.
- [ ] Erro sem diagnóstico.
- [ ] Erro com diagnóstico `InProgress`.
- [ ] Erro com diagnóstico concluído sem serviços.

### Aplicação
- [ ] Handler consolidando com sucesso.
- [ ] Handler retornando falhas conforme regra.

### Integração
- [ ] `POST /diagnostic/consolidate-services` com `200/404/409`.
- [ ] `GET /service-orders/{id}` após consolidação exibindo serviços e histórico corretos.

## 10) Riscos e Mitigações
- Risco: divergência entre serviços da recepção e diagnóstico.
  - Mitigação: política explícita de deduplicação sem remoção automática nesta task.
- Risco: dupla consolidação.
  - Mitigação: operação idempotente por regra de “não duplicar serviço ativo”.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Implementar apenas consolidação (sem quote/estoque/execução).
- [ ] Garantir rastreabilidade `Source/Actor/Timestamp` no histórico.
- [ ] Mapear erros sem parsing de mensagem.
- [ ] Executar build e testes.

## 12) Guardrails Não-Negociáveis
- Proibido implementar quote nesta task.
- Proibido remover serviços de recepção automaticamente.
- Proibido duplicar serviço ativo na OS.
- Proibido strings inline de erro.

## 13) Assumptions
- A decisão comercial de aceitar/ajustar orçamento ocorrerá na task seguinte.
- O cálculo financeiro será tratado em task específica de `Quote`.
