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

## Estratégia de Testes E2E
Objetivo:
- validar fluxos ponta a ponta críticos do produto com foco em integridade de processo, aderência às máquinas de estado e consistência entre contratos HTTP e estado final dos agregados.

Escopo de cobertura crítica:
1. OS ponta a ponta com estoque suficiente.
2. OS com falta de estoque, compra e retomada da separação.
3. OS com cancelamento no último estágio permitido.

Princípios:
- determinismo: dados e pré-condições previsíveis por cenário;
- rastreabilidade: cada execução deve manter identificadores de negócio auditáveis;
- isolamento: cenários independentes, sem acoplamento implícito entre execuções;
- legibilidade: passos, expectativa e resultado descritos de forma operacional.

Padrão de validação por cenário:
- validação de resposta HTTP nas etapas críticas;
- validação das transições de estado esperadas em cada processo envolvido;
- validação de consistência final de processo (sem estados conflitantes entre agregados relacionados).

Checklist corporativo de evidência por fluxo:
- identificadores de entidades-chave: `ServiceOrder`, `SeparationOrder`, `ExecutionOrder`, `PurchaseOrder` (quando aplicável);
- transições de estado esperadas por agregado crítico;
- respostas HTTP esperadas nas etapas críticas;
- condição final de consistência do processo.

Referência de estados-alvo dos fluxos críticos:
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

Critérios mínimos de aceitação de um fluxo E2E:
- cenário executável ponta a ponta sem intervenção manual fora do processo definido;
- validação explícita de HTTP e estados de negócio;
- resultado reproduzível no ambiente de execução definido para testes.

Forma de observação e evidência:
- os cenários podem ser observados por testes automatizados e/ou inspeção de endpoints;
- Swagger, execução de testes e outras ferramentas equivalentes são meios válidos de evidência, desde que preservem rastreabilidade e critérios de aceitação.
