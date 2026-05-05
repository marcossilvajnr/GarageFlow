# Task-013 — Setup Docker Infrastructure and Local Runbook

## 0) Metadata
- `task_id`: `task-013`
- `slug`: `setup-docker-infrastructure-and-local-runbook`
- `owner`: `Platform Team`
- `status`: `Ready`
- `depends_on`: `task-000-template.md`

## 1) Objetivo
Provisionar infraestrutura local padronizada com Docker para executar a API GarageFlow e dependências com um único comando, incluindo runbook operacional no `README.md` da raiz do repositório.

## 2) Escopo
### In
- Criar `Dockerfile` multi-stage para `GarageFlow.Api`.
- Criar `.dockerignore` para reduzir contexto de build.
- Criar `docker-compose.yml` com serviços:
  - `api` (GarageFlow.Api)
  - `postgres` (banco de dados)
- Configurar variáveis de ambiente via `.env.example`.
- Garantir que a API conecte no Postgres via variável de ambiente.
- Garantir endpoint de documentação (`/swagger`) disponível com ambiente Docker ativo.
- Criar `README.md` na raiz com instruções de execução local (Docker e local sem Docker).
- Incluir comandos de validação operacional (`build`, `up`, `logs`, `down`, `tests`).

### Out
- Implementação de autenticação/autorização JWT.
- Mudanças de domínio, regras RN, agregados ou contratos REST funcionais.
- Migração para orquestradores externos (Kubernetes, ECS, etc.).
- Observabilidade avançada (OpenTelemetry, stack de métricas distribuídas).

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/architecture/architecture-overview.md](/Users/marcos/Projects/GarageFlow/docs/architecture/architecture-overview.md)
- [docs/architecture/application-and-integrations.md](/Users/marcos/Projects/GarageFlow/docs/architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)
- Enunciado de referência da entrega: `/Users/marcos/Documents/Postech - Arq/Tech  Challenge/15SOAT - Fase 1 - Tech Challenge.pdf`

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- Não há novas regras de negócio nesta task.
- Esta task é exclusivamente de infraestrutura de execução e operação local.

## 5) Contratos e Interfaces
### 5.1 API pública (comportamento esperado)
- `GET /swagger` e `GET /swagger/v1/swagger.json` devem responder com ambiente Docker ativo.
- Endpoints funcionais já existentes devem manter compatibilidade de contrato.

### 5.2 Infra contracts
- Serviço `api` depende de `postgres` via `depends_on` + `healthcheck` no compose.
- Connection string da API deve ser resolvida por variável de ambiente no container.
- Porta HTTP da API deve ser exposta para host para acesso ao Swagger.

### 5.3 Erros operacionais mapeados
- Postgres indisponível: API deve registrar erro de startup/conexão com mensagem observável em logs.
- Variável obrigatória ausente: falha explícita de inicialização (sem fallback silencioso).

## 6) Plano Técnico por Camada
### Domain
- Sem alterações.

### Application
- Sem alterações funcionais.

### Infrastructure
- Garantir configuração de `DbContext` orientada por `ConnectionStrings__DefaultConnection` (ou chave equivalente já usada no projeto).
- Garantir compatibilidade com ambiente containerizado.

### API
- Garantir binding em `0.0.0.0` no container.
- Garantir Swagger habilitado no ambiente de execução definido para compose.

### Tests
- Adicionar/atualizar validação operacional no runbook para execução de testes automatizados após stack subir.

## 7) Arquivos a Criar/Alterar
### Criar (mandatório)
- `Dockerfile`
- `.dockerignore`
- `docker-compose.yml`
- `.env.example`
- `README.md`

### Alterar (esperado, se necessário)
- `src/GarageFlow.Api/appsettings.json`
- `src/GarageFlow.Api/appsettings.Development.json`
- `src/GarageFlow.Api/Program.cs`
- `src/GarageFlow.Infrastructure/DependencyInjection.cs`

Regra mandatória:
- Não alterar regras de domínio para acomodar infraestrutura.

## 8) Critérios de Pronto
- [ ] `docker compose up -d --build` sobe `api` e `postgres` sem erro.
- [ ] `docker compose ps` mostra ambos os serviços em estado saudável/rodando.
- [ ] `http://localhost:<porta-api>/swagger` abre com sucesso.
- [ ] `http://localhost:<porta-api>/swagger/v1/swagger.json` responde `200`.
- [ ] `dotnet build` sem erros após alterações.
- [ ] `dotnet test` sem regressão.
- [ ] `README.md` raiz contém passo a passo mínimo para:
  - subir stack
  - aplicar migrations (quando aplicável na estratégia adotada)
  - validar Swagger
  - derrubar stack

## 9) Estratégia de Testes
### Infra smoke
- [ ] Subir stack com build limpo.
- [ ] Validar health do Postgres.
- [ ] Validar disponibilidade do Swagger.

### Regressão de aplicação
- [ ] Executar suíte de testes existente.
- [ ] Garantir que nenhum endpoint existente perdeu comportamento esperado após containerização.

## 10) Riscos e Mitigações
- Risco: API iniciar antes do banco aceitar conexões.
  - Mitigação: `healthcheck` + `depends_on` condicional por saúde.
- Risco: divergência de connection string entre local e Docker.
  - Mitigação: centralizar configuração via variáveis de ambiente e documentar no `README.md`.
- Risco: Swagger indisponível em ambiente containerizado.
  - Mitigação: garantir configuração explícita de ambiente e middleware de Swagger no profile de desenvolvimento local da stack.

## 11) Checklist de Execução para IA
- [ ] Ler task completa antes de alterar código.
- [ ] Confirmar estratégia de connection string por env var.
- [ ] Criar Dockerfile multi-stage com imagem final enxuta.
- [ ] Criar compose com `api` + `postgres` + volume persistente.
- [ ] Criar `.env.example` com variáveis mínimas.
- [ ] Criar/atualizar `README.md` raiz com runbook completo.
- [ ] Rodar build, subir compose, validar Swagger, rodar testes.

## 12) Guardrails Não-Negociáveis
- Proibido alterar contratos de API funcional por causa de Docker.
- Proibido introduzir regra de domínio nova nesta task.
- Proibido deixar execução local dependente de configuração implícita não documentada.
- Proibido commitar credenciais reais; usar placeholders em `.env.example`.

## 13) Assumptions
- O projeto continuará usando PostgreSQL como banco padrão de execução local.
- A estratégia de migrations seguirá o padrão já adotado no repositório.
- JWT e autorização serão tratados em task própria, separada da infraestrutura Docker.
