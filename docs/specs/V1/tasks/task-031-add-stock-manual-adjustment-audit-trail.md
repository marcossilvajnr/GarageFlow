# Task-031 — Add Stock Manual Adjustment Audit Trail

## 0) Metadata
- `task_id`: `task-031`
- `slug`: `add-stock-manual-adjustment-audit-trail`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-030-remove-double-stock-consumption-in-execution-flow.md](task-030-remove-double-stock-consumption-in-execution-flow.md)

## 1) Objetivo
Fortalecer rastreabilidade de ajustes manuais de estoque (`/stock/releases`) com trilha auditável completa para governança operacional.

## 2) Escopo
### In
- Registrar dados mínimos obrigatórios de auditoria por ajuste manual:
  - `performedBy` (ator)
  - `reason`
  - `occurredAt`
  - `referenceId` e/ou `referenceType` quando aplicável
- Garantir persistência e consulta desses dados no contexto de stock.
- Cobrir com testes de aplicação e integração.

### Out
- Implementar autorização por perfil (task seguinte).
- Alterar regras de separação/devolução operacional.

## 3) Contexto Canônico Obrigatório
- [docs/Domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/Domain/agregados.md](../../../domain/agregados.md)
- [docs/Domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/specs/V1/aggregates/stock.md](../aggregates/stock.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-016` — operações de estoque preservam invariantes.
- `RN-033` — ajuste manual exige justificativa e não substitui devolução operacional por separação.

Regra mandatória da task:
- todo ajuste manual precisa gerar trilha auditável completa e consistente.

## 5) Contratos e Interfaces
### 5.1 API pública
- Manter endpoint atual:
  - `POST /stock/releases`
- Se houver extensão de payload para auditoria, preservar compatibilidade e documentar claramente.

### 5.2 Matriz de erro obrigatória
- `400` para justificativa ausente/inválida.
- `404` para item de estoque inexistente.
- `409` para conflito de quantidade.

### 5.3 Contratos internos
- Modelo de persistência de movimento/ajuste deve conter metadados de auditoria.

## 6) Plano Técnico por Camada
### Domain
- Assegurar que release manual exija justificativa.

### Application
- Carregar e persistir metadados de auditoria em cada ajuste manual.

### Infrastructure
- Ajustar mapeamento EF e migrations caso necessário para campos de auditoria.

### API
- Garantir entrada de dados necessários ao audit trail.

### Tests
- Validar persistência e consulta dos metadados de auditoria.

## 7) Arquivos a Criar/Alterar
### Alterar (esperado)
- [src/GarageFlow.Application/Stock/Handlers/ReleaseStockReservationHandler.cs](../../../../src/GarageFlow.Application/Stock/Handlers/ReleaseStockReservationHandler.cs)
- `src/GarageFlow.Domain/Stock/*`
- `src/GarageFlow.Infrastructure/Persistence/Configurations/Stock/*`
- [tests/GarageFlow.Tests/Application/Stock/StockHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/Stock/StockHandlersTests.cs)
- [tests/GarageFlow.Tests/Integration/Stock/StockEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Stock/StockEndpointsTests.cs)

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] Ajuste manual gera trilha auditável completa.
- [ ] Testes validam auditoria persistida.

## 9) Estratégia de Testes
- [ ] ajuste manual válido persiste auditoria.
- [ ] ajuste sem justificativa retorna `400`.
- [ ] consulta do saldo/histórico mantém consistência após ajuste.

## 10) Riscos e Mitigações
- Risco: campo de auditoria parcial/incompleto.
  - Mitigação: validar obrigatoriedade no application layer + testes de integração.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos docs canônicos.
- [ ] Preservar semântica HTTP.
- [ ] Garantir audit trail obrigatório por ajuste.
