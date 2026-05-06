# Task-037 — Canonical State-Machine Conformance Gate (Pre JWT/E2E)

## 0) Metadata
- `task_id`: `task-037`
- `slug`: `canonical-state-machine-conformance-gate-pre-jwt-e2e`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: `task-036-finalize-purchase-separation-execution-chain.md`

## 1) Objetivo
Executar uma auditoria final de aderência entre documentação canônica e implementação das máquinas de estado críticas, fechando divergências de regra/transição/erro HTTP antes da etapa de JWT e E2E.

## 2) Escopo
### In
- Auditoria de conformidade para:
  - `ServiceOrder`
  - `Diagnostic`
  - `Quote`
  - `SeparationOrder`
  - `ExecutionOrder`
  - `PurchaseOrder`
- Verificação de transições válidas e inválidas (estado de origem, comando/método, estado de destino).
- Verificação de mapeamento de erros por endpoint (`400/404/409`) sem parsing de mensagem.
- Correções pontuais em código e/ou docs canônicas para eliminar drift.

### Out
- Implementação de JWT/Auth.
- Criação de suíte E2E completa.
- Novas features de domínio fora de aderência canônica.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- `docs/domain/agregados.md`
- `docs/domain/regras-de-negocio.md`
- `docs/domain/linguagem-ubiqua.md`
- `docs/domain/bounded-contexts.md`
- `docs/specs/V1/aggregates/service-order.md`
- `docs/specs/V1/aggregates/quote.md`
- `docs/specs/V1/aggregates/separation-order.md`
- `docs/specs/V1/aggregates/execution-order.md`
- `docs/specs/V1/aggregates/purchase-order.md`
- `docs/architecture/engineering-standards.md`

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-003` — progressão de status da OS.
- `RN-006` — diagnóstico único.
- `RN-007` — orçamento após diagnóstico.
- `RN-008` — criação automática de execução por serviço aprovado.
- `RN-009` — execução só inicia após separação concluída.
- `RN-010` — registro de tempo real da execução.
- `RN-011` — criação automática de separação por execução.
- `RN-012` — verificação automática de estoque na separação.
- `RN-013` — dupla confirmação de custódia.
- `RN-017` — geração de ordem de compra por insuficiência.
- `RN-020` — retomada automática após compra.
- `RN-026` — alteração de diagnóstico só em `InProgress`.
- `RN-028` — diagnóstico não reabre após concluído.
- `RN-029` — rastreabilidade de alterações de serviço da OS.
- `RN-030` — congelamento de serviços após diagnóstico concluído.
- `RN-031` — orçamento imutável por versão.

## 5) Contratos e Interfaces

### 5.1 API pública (escopo de auditoria)
Endpoints críticos a validar:
- `POST /service-orders/{id}/diagnostic/start`
- `POST /service-orders/{id}/diagnostic/services`
- `DELETE /service-orders/{id}/diagnostic/services/{serviceId}`
- `POST /service-orders/{id}/diagnostic/complete`
- `POST /service-orders/{id}/quote/generate`
- `POST /service-orders/{id}/quote/accept`
- `POST /service-orders/{id}/quote/reject`
- `POST /separation-orders/{id}/reserve`
- `POST /separation-orders/{id}/wait-purchase`
- `POST /separation-orders/{id}/resume-after-purchase`
- `POST /separation-orders/{id}/confirm-stockist-withdrawal`
- `POST /separation-orders/{id}/confirm-mechanic-receipt`
- `POST /purchase-orders/{id}/start`
- `POST /purchase-orders/{id}/complete`
- `POST /execution-orders/{id}/start`
- `POST /execution-orders/{id}/complete`

Matriz de erro obrigatória:
- Transição de estado inválida -> `409`
- Pré-condição de domínio/validação de entrada -> `400`
- Entidade inexistente -> `404`
- Proibido decidir status HTTP por parsing textual de mensagem.

### 5.2 Contratos internos
- Exceções de transição:
  - `InvalidServiceOrderStatusTransitionException`
  - `InvalidSeparationOrderStatusTransitionException`
  - `InvalidExecutionOrderStatusTransitionException`
  - `InvalidPurchaseOrderStatusTransitionException`
- Exceções de pré-condição de fluxo:
  - `DiagnosticNotInProgressException`
  - `DiagnosticAlreadyStartedException`
  - `QuoteAlreadyDecidedException`
  - `SeparationOrderCustodyPreconditionException`
- Erros canônicos centralizados em `DomainErrorMessages`.

### 5.3 Erros de domínio
- Mensagens em português.
- Identificadores de código em inglês.
- Sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada

### Domain
- Auditar guards de estado em cada agregado crítico.
- Ajustar apenas quando houver divergência objetiva com canônico.
- Garantir consistência entre status enums, métodos de transição e exceções lançadas.

### Application
- Validar orquestrações cross-aggregate:
  - `PurchaseOrder.Complete` -> reserva de estoque -> `SeparationOrder.ResumeAfterPurchase`
  - `ConfirmMechanicReceipt` -> `ExecutionOrder.MarkReadyToStart`
- Garantir que não há bypass de estado por fluxo alternativo.

### Infrastructure
- Sem mudança estrutural obrigatória.
- Apenas ajustes necessários se algum comportamento de persistência quebrar regra de estado (escopo mínimo).

### API
- Revisar endpoint por endpoint:
  - conflito de estado sempre `409`;
  - not found sempre `404`;
  - erro de validação/domínio sempre `400`.
- Padronizar `ProblemDetails` em conflitos de estado quando houver inconsistência.

### Tests
- Consolidar cobertura de transições inválidas e felizes para os 6 processos críticos.
- Adicionar testes faltantes para cada gap encontrado na auditoria.
- Garantir evidência com `dotnet test` verde.

## 7) Arquivos a Criar/Alterar

Escopo esperado (ajustar somente se necessário):
- `src/GarageFlow.Domain/**` (agregados/exceções/status)
- `src/GarageFlow.Application/**` (handlers de orquestração)
- `src/GarageFlow.Api/Endpoints/**`
- `tests/GarageFlow.Tests/Domain/**`
- `tests/GarageFlow.Tests/Application/**`
- `tests/GarageFlow.Tests/Integration/**`
- `docs/domain/**` e `docs/specs/V1/aggregates/**` (somente para corrigir drift)

Contrato de arquivos:
- Mudanças fora do escopo acima exigem justificativa explícita.
- Sem criação de feature paralela.

## 8) Critérios de Pronto
- [x] Matriz canônica de estados vs implementação concluída para os 6 processos.
- [x] Todas as divergências classificadas e corrigidas (código ou doc).
- [x] Mapeamento HTTP `400/404/409` consistente nos endpoints críticos.
- [x] `dotnet build` sem erros.
- [x] `dotnet test` verde.
- [x] Sem drift aberto entre doc canônica e código nos fluxos auditados.

## 9) Estratégia de Testes

### Domínio
- [x] Transições válidas por agregado crítico.
- [x] Transições inválidas por status com exceção específica.

### Aplicação
- [x] Orquestrações `Purchase -> Separation -> Execution` sem bypass.
- [x] Regras de gate entre processos preservadas.

### Integração
- [x] Endpoints de transição retornando HTTP correto (`400/404/409`).
- [x] Fluxos encadeados com e sem compra mantendo estados esperados.

## 10) Riscos e Mitigações
- Risco: correções em um fluxo quebrarem regressivamente outro encadeamento.
  - Mitigação: rodar suíte completa e priorizar testes de integração do encadeamento.
- Risco: correção ficar só no código e não atualizar docs (ou vice-versa).
  - Mitigação: fechamento obrigatório com dupla validação doc <-> código.
- Risco: ambiguidade residual sobre regra canônica.
  - Mitigação: prevalece `docs/domain/*`; ajustar `docs/specs` para refletir canônico.

## 11) Checklist de Execução para IA
- [x] Ler integralmente os docs canônicos listados.
- [x] Gerar matriz de estados por agregado (canônico vs código).
- [x] Corrigir primeiro conflitos de regra, depois nomenclatura.
- [x] Validar mapeamento HTTP sem parsing de mensagem.
- [x] Manter mensagens em português e identificadores em inglês.
- [x] Executar `dotnet build`.
- [x] Executar `dotnet test`.
- [x] Entregar evidência final com lista de drifts resolvidos.

## 12) Evidência de Execução (2026-05-05)

### 12.1 Matriz de conformidade (resumo)
- `ServiceOrder`: transições e gates de diagnóstico/orçamento aderentes.
- `Diagnostic`: `InProgress -> Completed` com bloqueio de reabertura aderente.
- `Quote`: decisão única (`WaitingCustomerApproval -> CustomerApproved/CustomerRejected`) aderente.
- `SeparationOrder`: fluxo `Pending -> WaitingPurchase/WaitingPickup -> Separated -> DeliveredToMechanic` aderente.
- `ExecutionOrder`: `Pending -> ReadyToStart -> InProgress -> Completed` aderente, com gate de separação.
- `PurchaseOrder`: `Pending -> InProgress -> Completed` aderente, incluindo retomada de separação.

### 12.2 Verificações de contrato HTTP
- Endpoints críticos auditados:
  - `/service-orders/{id}/diagnostic/*`
  - `/service-orders/{id}/quote/*`
  - `/separation-orders/{id}/*`
  - `/purchase-orders/{id}/*`
  - `/execution-orders/{id}/*`
- Resultado:
  - transição inválida mapeada para `409`;
  - entidade inexistente mapeada para `404`;
  - validações de entrada/domínio mapeadas para `400`;
  - sem uso de parsing textual de mensagem para decidir status HTTP.

### 12.3 Comandos e resultado
- `dotnet build`:
  - sucesso, `0` erros.
  - warnings conhecidos de vulnerabilidade de pacote (`NU1903`) já rastreados fora deste escopo.
- `dotnet test`:
  - sucesso, `834/834` testes passando.

### 12.4 Drift resolvido
- Task normalizada como gate de conformidade pré-JWT/E2E.
- Sem drift aberto entre máquinas de estado canônicas e implementação nos fluxos auditados.
