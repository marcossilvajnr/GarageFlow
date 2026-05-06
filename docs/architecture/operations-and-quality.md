# Operations

## Objetivo
Esta seção define o baseline operacional do GarageFlow para execução local, validação técnica e evolução da esteira.

## Escopo Atual
- Execução local com Docker Compose.
- Verificação de disponibilidade da API via `health` e `swagger`.
- Execução manual da esteira de qualidade e segurança no GitHub Actions.
- Referência operacional para evidências e artefatos de execução.

## Operação Local (Docker)
Artefatos oficiais:
- `Dockerfile`
- `docker-compose.yml`
- `README.md` (raiz)

Princípios operacionais:
- configuração por variáveis de ambiente;
- configuração padrão pronta no `docker-compose.yml` para execução local imediata;
- banco PostgreSQL com healthcheck;
- API exposta para validação de contrato (`swagger`).

## Pipeline Manual (Operação)
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

## Governança e Evolução da Esteira
A evolução deve ser incremental e orientada a custo-benefício:
1. manual on-demand (estado atual);
2. automação seletiva por branch/release;
3. controles adicionais quando houver necessidade operacional.

Regras:
- não introduzir custo recorrente sem evidência de ganho;
- preservar rastreabilidade dos relatórios;
- manter compatibilidade com os contratos públicos da API.

## Referências Relacionadas
- Estratégia de testes e qualidade: `docs/architecture/testing-and-quality.md`
