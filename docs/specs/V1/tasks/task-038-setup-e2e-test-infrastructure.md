# Task-038 — Setup E2E Test Infrastructure

## 0) Metadata
- `task_id`: `task-038`
- `slug`: `setup-e2e-test-infrastructure`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: `task-037-canonical-state-machine-conformance-gate-pre-jwt-e2e.md`

## 1) Objetivo
Criar a infraestrutura base de testes E2E para executar fluxos completos da API de forma reprodutível e isolada.

## 2) Escopo
### In
- Criar base técnica para suíte E2E em projeto de testes.
- Configurar fixture para inicialização da API e isolamento de banco.
- Padronizar builders/helpers de seed para cenários de OS, estoque, compra, separação e execução.
- Definir convenção de organização dos testes E2E por fluxo.

### Out
- Implementar JWT/Auth.
- Implementar todos os fluxos E2E completos nesta task.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- `docs/domain/agregados.md`
- `docs/domain/regras-de-negocio.md`
- `docs/domain/linguagem-ubiqua.md`
- `docs/architecture/architecture-overview.md`
- `docs/architecture/application-and-integrations.md`
- `docs/architecture/engineering-standards.md`

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-003` — progressão de status da OS.
- `RN-009` — execução só inicia após separação concluída.
- `RN-012` — verificação de estoque na separação.

## 5) Contratos e Interfaces
### 5.1 API pública
- Sem novos endpoints nesta task.
- Reutilizar endpoints já existentes para orquestração dos fluxos.

### 5.2 Contratos internos
- Fixture E2E compartilhada.
- Helpers de criação de dados canônicos para cenários críticos.

### 5.3 Erros de domínio
- Sem alteração de catálogo nesta task.

## 6) Plano Técnico por Camada
### Domain
- Sem mudança funcional.

### Application
- Sem mudança funcional.

### Infrastructure
- Garantir isolamento de execução de testes E2E por cenário/suíte.

### API
- Sem novos contratos.

### Tests
- Criar estrutura base E2E reutilizável.

## 7) Arquivos a Criar/Alterar
- `tests/GarageFlow.Tests/E2E/**` (nova estrutura dedicada)
- `tests/GarageFlow.Tests/Fixtures/**` (se necessário para compartilhamento)
- `tests/GarageFlow.Tests/Helpers/**` (seed/builders E2E)

## 8) Critérios de Pronto
- [x] Infra E2E criada e executável localmente.
- [x] Fixture e helpers reutilizáveis disponíveis.
- [x] Execução de teste “smoke” E2E verde.
- [x] Sem impacto regressivo na suíte atual.

## 9) Estratégia de Testes
### Integração/E2E
- [x] 1 cenário smoke ponta a ponta mínimo executando com a nova infraestrutura.

## 10) Riscos e Mitigações
- Risco: instabilidade por dependência de estado compartilhado.
  - Mitigação: isolamento explícito de dados por teste.

## 11) Checklist de Execução para IA
- [x] Confirmar leitura dos docs canônicos.
- [x] Evitar mudanças funcionais de domínio.
- [x] Entregar infraestrutura E2E pronta para as próximas tasks.

## 12) Evidência de Execução
- Estrutura criada:
  - `tests/GarageFlow.Tests/E2E/Infrastructure/`
  - `tests/GarageFlow.Tests/E2E/Builders/`
  - `tests/GarageFlow.Tests/E2E/Smoke/`
- Teste smoke implementado:
  - `Smoke_CreateAndGetCustomer_ShouldWorkEndToEnd`
- Comandos executados:
  - `dotnet test --filter "FullyQualifiedName~E2E"` -> `Passed 1/1`
  - `dotnet build` -> sucesso
  - `dotnet test` -> `Passed 835/835`
