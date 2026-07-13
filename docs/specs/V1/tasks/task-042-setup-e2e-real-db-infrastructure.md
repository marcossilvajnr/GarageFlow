# Task-042 — Setup E2E Real DB Infrastructure

## 0) Metadata
- `task_id`: `task-042`
- `slug`: `setup-e2e-real-db-infrastructure`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-041-apply-end-to-end-observability-and-state-transition-logging.md](task-041-apply-end-to-end-observability-and-state-transition-logging.md)

## 1) Objetivo
Adaptar a suíte E2E para execução contra banco PostgreSQL real, mantendo previsibilidade de dados por cenário.

## 2) Escopo
### In
- Criar infraestrutura E2E dedicada para banco real.
- Isolar execução E2E em collection sem paralelismo.
- Padronizar reset de banco antes de cada cenário E2E crítico.
- Definir configuração de connection string para E2E real via ambiente.

### Out
- Alteração de regra de domínio.
- Introdução de JWT real.
- Expansão de escopo para além da suíte E2E crítica.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md)
- [docs/specs/V1/tasks/task-038-setup-e2e-test-infrastructure.md](task-038-setup-e2e-test-infrastructure.md)
- [docs/specs/V1/tasks/task-039-document-e2e-critical-flow-coverage.md](task-039-document-e2e-critical-flow-coverage.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-003` — progressão de status da OS.
- `RN-009` — execução só inicia após separação concluída.
- `RN-013` — dupla confirmação de custódia na separação.

## 5) Contratos e Interfaces
### 5.1 API pública
- Reutilizar endpoints existentes dos fluxos E2E.
- Reutilizar endpoints de manutenção de banco para reset controlado em ambiente de desenvolvimento.

### 5.2 Contratos internos
- Factory E2E dedicada para banco real.
- Variável de ambiente para connection string E2E real (`E2E_REAL_DB_CONNECTION`, com fallback para `ConnectionStrings__GarageFlow`).

### 5.3 Segurança operacional
- Operações destrutivas de banco restritas ao ambiente de desenvolvimento.
- Reset explícito por cenário para evitar estado residual.

## 6) Plano Técnico por Camada
### Domain
- Sem mudança funcional.

### Application
- Sem mudança funcional.

### Infrastructure
- Configurar bootstrap E2E para banco real com migrations no startup.

### API
- Reutilizar endpoints de manutenção de banco para `reset` durante setup de cenário E2E.

### Tests
- Aplicar collection E2E não paralela para evitar concorrência no mesmo banco.
- Garantir limpeza/recriação de banco no início de cada teste E2E crítico.

## 7) Arquivos a Criar/Alterar
- `tests/GarageFlow.Tests/E2E/Infrastructure/**`
- `tests/GarageFlow.Tests/E2E/Smoke/**`
- `tests/GarageFlow.Tests/E2E/ServiceOrders/**`
- [README.md](../../../../README.md) (se necessário para runbook de execução E2E com banco real)

## 8) Critérios de Pronto
- [ ] E2E executa contra PostgreSQL real sem usar SQLite em memória.
- [ ] Reset do banco aplicado por cenário E2E crítico.
- [ ] Execução E2E estável sem concorrência entre cenários.
- [ ] Sem regressão na suíte completa de testes.

## 9) Estratégia de Testes
### Integração/E2E
- [ ] Executar `dotnet test --filter "FullyQualifiedName~E2E"` validando persistência real.
- [ ] Executar `dotnet test` validando ausência de regressão global.
