# Operations and Quality

## Objetivo
Esta seção define o baseline operacional e de qualidade do GarageFlow para execução local, validação técnica e evolução da esteira.

## Escopo Atual
- Execução local com Docker Compose.
- Verificação de disponibilidade da API via `health` e `swagger`.
- Esteira manual de qualidade e segurança no GitHub Actions.
- Relatórios de cobertura, vulnerabilidades de dependências e contagem de testes por tipo.

## Operação Local (Docker)
Artefatos oficiais:
- `Dockerfile`
- `docker-compose.yml`
- `.env.example`
- `README.md` (raiz)

Princípios operacionais:
- configuração por variáveis de ambiente;
- credenciais reais apenas em `.env` local não versionado;
- banco PostgreSQL com healthcheck;
- API exposta para validação de contrato (`swagger`).

## Qualidade e Segurança (Pipeline Manual)
A pipeline de qualidade e segurança é executada sob demanda (`workflow_dispatch`) para reduzir custo e manter evidências auditáveis.

Evidências mínimas esperadas por execução:
- build da solução;
- execução de testes automatizados;
- cobertura consolidada de testes;
- relatório de vulnerabilidades de dependências (`dotnet list package --vulnerable --include-transitive`);
- contagem de testes por tipo (`Domain`, `Application`, `Integration`).

Saídas obrigatórias:
- resumo executivo no Job Summary;
- artifacts baixáveis com relatórios completos.
- dashboard visual no Job Summary com KPIs consolidados.
- resultado visual de testes no run do GitHub Actions (a partir de `.trx`).

## Segurança de Dependências
A governança de pacotes segue abordagem incremental:
- identificar vulnerabilidades via relatório automatizado;
- priorizar correção por severidade e criticidade técnica;
- registrar exceções de curto prazo com justificativa e prazo de revisão.

## Evolução da Esteira
A evolução deve ser incremental e orientada a custo-benefício:
1. manual on-demand (estado atual);
2. automação seletiva por branch/release;
3. controles adicionais de segurança e qualidade quando houver necessidade operacional.

Regras de evolução:
- não introduzir custo recorrente sem evidência de ganho;
- preservar rastreabilidade dos relatórios;
- manter compatibilidade com os contratos públicos da API.

## Cobertura E2E Crítica (Pre-JWT)
Objetivo:
- demonstrar o sistema ponta a ponta com três fluxos de maior valor antes da fase de JWT.

Fluxos obrigatórios:
1. OS ponta a ponta com estoque suficiente.
2. OS com falta de estoque, compra e retomada da separação.
3. OS com cancelamento no último estágio permitido.

Evidências mínimas por fluxo:
- respostas HTTP esperadas por etapa crítica;
- estado final consistente de `ServiceOrder`, `SeparationOrder`, `PurchaseOrder` (quando houver) e `ExecutionOrder`;
- ids rastreáveis dos agregados principais no teste.

Estados-alvo por fluxo:
- Fluxo 1:
  - `ServiceOrder`: `Approved -> InExecution -> Finished`
  - `SeparationOrder`: `Pending -> WaitingPickup -> Separated -> Completed`
  - `ExecutionOrder`: `Pending -> Ready -> InExecution -> Completed`
- Fluxo 2:
  - `SeparationOrder`: `Pending -> WaitingPurchase -> WaitingPickup -> Separated -> Completed`
  - `PurchaseOrder`: `Created -> Started -> Completed`
  - `ExecutionOrder`: `Ready -> InExecution -> Completed`
- Fluxo 3:
  - cancelamento aceito no limite canônico e bloqueio de avanço inválido após cancelamento.

Limites desta fase:
- sem validação de token real JWT;
- uso de autenticação de teste permitido;
- foco em integridade de fluxo e máquina de estado.
