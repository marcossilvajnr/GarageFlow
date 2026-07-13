# Task-041 — Apply End-to-End Observability and State Transition Logging

## 0) Metadata
- `task_id`: `task-041`
- `slug`: `apply-end-to-end-observability-and-state-transition-logging`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: [task-040-fix-execution-serviceorder-state-sync-gap.md](task-040-fix-execution-serviceorder-state-sync-gap.md)

## 1) Objetivo
Aplicar observabilidade em todo o sistema com logs estruturados de nível `Info` para criação de entidades, fluxos operacionais e todas as mudanças de estado das máquinas de estado canônicas.

## 2) Escopo
### In
- Logar criação de entidades de catálogo/base e operacionais.
- Logar operações de fluxo (criação, atualização operacional e ações de processo).
- Logar toda transição de estado em agregados com máquina de estado.
- Definir padrão único de estrutura dos logs para rastreabilidade.

### Out
- Alteração de regra de domínio ou mudança de comportamento funcional.
- Introdução de JWT real.
- Implementação de tracing distribuído externo nesta task.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)
- [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md)
- [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md)
- [docs/specs/V1/aggregates/separation-order.md](../aggregates/separation-order.md)
- [docs/specs/V1/aggregates/purchase-order.md](../aggregates/purchase-order.md)
- [docs/specs/V1/aggregates/execution-order.md](../aggregates/execution-order.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-003`, `RN-007`, `RN-009`, `RN-011`, `RN-012`, `RN-017`, `RN-020`.
- Não alterar as regras; apenas tornar os fluxos observáveis.

## 5) Contratos e Interfaces
### 5.1 Escopo de agregados
- Catálogo/base: `Customer`, `Vehicle`, `Service`, `Part`, `Supply`, `Employee`, `Supplier`.
- Operacionais: `ServiceOrder`, `Diagnostic`, `Quote`, `SeparationOrder`, `PurchaseOrder`, `ExecutionOrder`, `Stock`.

### 5.2 Padrão mínimo de log estruturado
- `aggregate` e `aggregateId`.
- `action` ou `transition`.
- `oldState` e `newState` quando houver transição.
- `actorId` e `correlationId` quando disponíveis no contexto da operação.

### 5.3 Regras de registro
- Logs de criação com identificador gerado e contexto operacional.
- Logs de fluxo em pontos de entrada/saída dos handlers de aplicação.
- Logs de transição de estado somente em transições válidas efetivadas.

## 6) Plano Técnico por Camada
### Domain
- Sem mudança de regra de negócio; apenas suportar rastreabilidade de transições.

### Application
- Instrumentar handlers de criação e operações com logs `Info` estruturados.

### Infrastructure
- Garantir propagação de `correlationId` no pipeline quando disponível.

### API
- Não alterar contratos HTTP; somente enriquecer observabilidade dos fluxos.

### Tests
- Adicionar testes para validar emissão de logs nos fluxos críticos e em transições de estado.

## 7) Arquivos a Criar/Alterar
- `src/GarageFlow.Application/**/Handlers/*`
- `src/GarageFlow.Api/**`
- `tests/GarageFlow.Tests/Application/**`
- `tests/GarageFlow.Tests/Integration/**`
- [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md) (se necessário para guia operacional)

## 8) Critérios de Pronto
- [x] Logs `Info` estruturados implementados para criação de entidades em todo o sistema.
- [x] Logs `Info` estruturados implementados para fluxos operacionais críticos.
- [x] Todas as transições de estado relevantes registradas com `oldState` e `newState`.
- [x] Sem alteração de regra de domínio.
- [x] Suíte de testes verde sem regressão funcional.

## 9) Estratégia de Testes
### Aplicação/Integração
- [x] Validar logs de criação para agregados de catálogo/base.
- [x] Validar logs de fluxo para agregados operacionais.
- [x] Validar logs de transição de estado em `ServiceOrder`, `SeparationOrder`, `PurchaseOrder` e `ExecutionOrder`.
- [x] Executar `dotnet test` para garantir ausência de regressões.

## 10) Evidência de Execução
- Interceptor de persistência criado para logs estruturados de criação, atualização e transição de estado.
- Middleware HTTP criado para correlação (`X-Correlation-ID`) e contexto de ator.
- Teste automatizado adicionado para validar emissão de logs de criação e transição de estado.
- `dotnet test` executado com sucesso.
