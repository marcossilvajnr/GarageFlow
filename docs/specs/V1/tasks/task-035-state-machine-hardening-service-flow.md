# Task-035 — State Machine Hardening (Service Flow)

## 0) Metadata
- `task_id`: `task-035`
- `slug`: `state-machine-hardening-service-flow`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-034-canonical-drift-check-and-alignment.md](task-034-canonical-drift-check-and-alignment.md)

## 1) Objetivo
Endurecer as máquinas de estado de `ServiceOrder`, `SeparationOrder`, `ExecutionOrder`, `Quote` e `PurchaseOrder`, removendo transições inválidas e garantindo respostas de conflito consistentes.

## 2) Escopo
### In
- Revisão de transições permitidas/proibidas nos agregados.
- Cobertura de testes para transições inválidas.
- Harmonização de exceções de domínio e status HTTP.

### Out
- Alterações de autenticação/autorização.
- Mudanças de modelo de dados não necessárias ao hardening.
