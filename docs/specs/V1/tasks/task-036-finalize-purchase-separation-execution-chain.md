# Task-036 — Finalize Purchase -> Separation -> Execution Chain

## 0) Metadata
- `task_id`: `task-036`
- `slug`: `finalize-purchase-separation-execution-chain`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-035-state-machine-hardening-service-flow.md`

## 1) Objetivo
Fechar a cadeia operacional completa de compra, separação e execução, garantindo retomada sem bypass após reposição e consistência de estado ponta a ponta.

## 2) Escopo
### In
- Validar e corrigir integração `PurchaseOrder` -> `SeparationOrder` (resume) -> `ExecutionOrder`.
- Cobrir cenários felizes e conflitos principais em integração.

### Out
- JWT/Auth.
- E2E completos.
