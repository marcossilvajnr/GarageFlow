# Task-034 — Canonical Drift Check and Alignment

## 0) Metadata
- `task_id`: `task-034`
- `slug`: `canonical-drift-check-and-alignment`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-033-govern-post-custody-stock-exception-adjustments.md`

## 1) Objetivo
Executar varredura de drift entre documentação canônica e implementação, corrigindo divergências de regra/nomenclatura/status/eventos sem alterar escopo funcional.

## 2) Escopo
### In
- Comparar `docs/Domain/*`, `docs/specs/V1/aggregates/*` e código atual.
- Corrigir inconsistências identificadas no código e/ou docs canônicas.
- Consolidar critérios de erro (`400/404/409`) em fluxos críticos.

### Out
- Criar feature nova.
- Alterar contratos públicos sem justificativa canônica.
