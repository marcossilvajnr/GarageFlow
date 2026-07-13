# Task-033 — Govern Post-Custody Stock Exception Adjustments (Pre-JWT)

## 0) Metadata
- `task_id`: `task-033`
- `slug`: `govern-post-custody-stock-exception-adjustments`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-031-add-stock-manual-adjustment-audit-trail.md](task-031-add-stock-manual-adjustment-audit-trail.md)

## 1) Objetivo
Implementar governança de ajustes excepcionais de estoque após confirmação de custódia mecânica, preservando invariantes e rastreabilidade completa, **sem depender de JWT nesta etapa**.

## 2) Escopo
### In
- Definir e implementar regra explícita para ajuste excepcional pós-custódia:
  - justificativa obrigatória;
  - referência operacional obrigatória (`referenceId` + `referenceType`);
  - trilha auditável reforçada.
- Preservar bloqueio da devolução operacional padrão após `ConfirmMechanicReceipt`.
- Validar via domínio e camada de aplicação.

### Out
- Enforcement de autorização por perfil (`401/403`) nesta task.
- Reabrir devolução operacional padrão após custódia.
- Criar endpoints novos.

## 3) Contexto Canônico Obrigatório
- [docs/Domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/Domain/agregados.md](../../../domain/agregados.md)
- [docs/Domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/specs/V1/aggregates/stock.md](../aggregates/stock.md)
- [docs/specs/V1/aggregates/separation-order.md](../aggregates/separation-order.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-032` — devolução operacional total apenas antes de `ConfirmMechanicReceipt`.
- `RN-033` — ajuste manual administrativo com justificativa.

## 5) Contratos e Interfaces
### 5.1 API pública
- Reuso de `POST /stock/releases` para exceção operacional.

### 5.2 Matriz de erro obrigatória (pré-JWT)
- `400` para justificativa/referência ausente ou inválida.
- `404` para referências operacionais inexistentes (quando aplicável).
- `409` para conflito de quantidade/invariante.

## 6) Plano Técnico por Camada
### Domain
- Preservar invariantes de `Stock` e regra de devolução operacional.

### Application
- Validar referência obrigatória em ajuste pós-custódia.
- Marcar ajuste como excepcional no audit trail (dados suficientes para rastreio).

### Infrastructure
- Persistir metadado de exceção/referência operacional, se necessário.

### API
- Exigir campos obrigatórios para cenário de exceção (sem authz nesta etapa).

### Tests
- Cobrir cenários de exceção permitida e bloqueada por regra de domínio.

## 7) Arquivos a Criar/Alterar
### Alterar (esperado)
- [src/GarageFlow.Application/Stock/Handlers/ReleaseStockReservationHandler.cs](../../../../src/GarageFlow.Application/Stock/Handlers/ReleaseStockReservationHandler.cs)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs)
- `src/GarageFlow.Domain/Stock/*`
- [tests/GarageFlow.Tests/Application/Stock/StockHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/Stock/StockHandlersTests.cs)
- [tests/GarageFlow.Tests/Integration/Stock/StockEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Stock/StockEndpointsTests.cs)

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] ajuste excepcional pós-custódia exige justificativa + referência operacional.
- [ ] devolução operacional padrão continua bloqueada após `ConfirmMechanicReceipt`.

## 9) Estratégia de Testes
- [ ] exceção válida pós-custódia retorna sucesso.
- [ ] ausência de referência/justificativa retorna `400`.
- [ ] conflitos de quantidade retornam `409`.

## 10) Riscos e Mitigações
- Risco: contornar fluxo operacional padrão com ajuste manual genérico.
  - Mitigação: validação obrigatória de referência operacional + auditoria.

## 11) Checklist de Execução para IA
- [ ] Não conflitar com RN-032.
- [ ] Não abrir devolução operacional pós-custódia.
- [ ] Garantir governança excepcional sem depender de JWT.
