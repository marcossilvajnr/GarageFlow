# Task-039 — Document E2E Critical Flow Coverage

## 0) Metadata
- `task_id`: `task-039`
- `slug`: `document-e2e-critical-flow-coverage`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: `task-038-setup-e2e-test-infrastructure.md`

## 1) Objetivo
Documentar oficialmente os fluxos E2E críticos que demonstram o sistema funcionando ponta a ponta para apresentação do projeto.

## 2) Escopo
### In
- Definir os 3 fluxos E2E prioritários com pré-condições, passos, estado esperado e evidências.
- Definir critérios de aceite de cada fluxo e checklist de observabilidade.
- Registrar limites e não objetivos da suíte pré-JWT.

### Out
- Implementação de código de produção.
- Implementação dos testes E2E (fica para tasks 043-045).
- Setup de infraestrutura E2E em banco real (fica para task-042).
- Observabilidade global de fluxos e transições (fica para task-041).

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- `docs/domain/agregados.md`
- `docs/domain/regras-de-negocio.md`
- `docs/domain/linguagem-ubiqua.md`
- `docs/specs/V1/aggregates/service-order.md`
- `docs/specs/V1/aggregates/separation-order.md`
- `docs/specs/V1/aggregates/purchase-order.md`
- `docs/specs/V1/aggregates/execution-order.md`

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-003`, `RN-007`, `RN-009`, `RN-011`, `RN-012`, `RN-017`, `RN-020`.

## 5) Contratos e Interfaces
### 5.1 Fluxos documentados obrigatórios
- Fluxo A: OS ponta a ponta com estoque suficiente.
- Fluxo B: OS com falta de estoque e compra + retomada.
- Fluxo C: OS com cancelamento do cliente no último estágio permitido.

### 5.2 Evidências mínimas por fluxo
- Status de entidades críticas no final.
- Verificação de consistência de estoque/custódia quando aplicável.
- HTTP esperado por etapa crítica.

## 6) Plano Técnico por Camada
### Docs
- Criar/atualizar seção canônica em `docs/architecture/operations-and-quality.md` para cobertura E2E crítica.

## 7) Arquivos a Criar/Alterar
- `docs/architecture/operations-and-quality.md`
- Referência cruzada em `docs/specs/V1/tasks/` se necessário.

## 8) Critérios de Pronto
- [x] Documento publicado com os 3 fluxos completos.
- [x] Critérios de aceite por fluxo definidos.
- [x] Mapeamento para tasks 041-045 explícito.

## 9) Estratégia de Testes
- [x] Não aplicável (task documental).

## 10) Riscos e Mitigações
- Risco: ambiguidade de cenário entre equipes.
  - Mitigação: passos e estados esperados explícitos.

## 11) Checklist de Execução para IA
- [x] Não inventar regra fora do canônico.
- [x] Especificar fluxos com linguagem operacional objetiva.

## 12) Evidência de Execução
- Documento criado:
  - `docs/architecture/operations-and-quality.md` (seção `Cobertura E2E Crítica (Pre-JWT)`)
- Conteúdo coberto:
  - Fluxo A: OS com estoque suficiente.
  - Fluxo B: OS com falta de estoque + compra + retomada.
  - Fluxo C: cancelamento no último estágio permitido.
- Mapeamento explícito para implementação:
  - `task-041` (observabilidade), `task-042` (setup real DB), `task-043`, `task-044`, `task-045`.
