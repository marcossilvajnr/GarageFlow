# CI (Continuous Integration)

## Objetivo
Definir a esteira de Integração Contínua do GarageFlow para geração de evidências técnicas de qualidade e segurança.

## Escopo Atual
- Execução manual via GitHub Actions (`workflow_dispatch`).
- Build da solução.
- Execução de testes automatizados.
- Geração de relatórios de cobertura.
- Scan de vulnerabilidades de dependências.
- Resumo executivo visual da execução.

## Workflow Oficial
- Arquivo: `.github/workflows/manual-quality-gate.yml`
- Nome no GitHub Actions: `Manual Quality Gate`

## Como Executar
1. Abrir o repositório no GitHub.
2. Ir em `Actions`.
3. Selecionar `Manual Quality Gate`.
4. Clicar em `Run workflow`.

## Evidências Geradas
- Job Summary com indicadores consolidados.
- Resultado visual de testes (TRX).
- Artefatos baixáveis:
  - `artifacts/executive/executive-summary.md`
  - `artifacts/coverage/coverage-summary.md`
  - `artifacts/coverage/coverage-html.tar.gz`
  - `artifacts/security/security-report.json`
  - `artifacts/security/security-report.md`
  - `artifacts/test-breakdown/test-breakdown.json`
  - `artifacts/test-breakdown/test-breakdown.md`

## Critério de Uso
- A esteira manual é o baseline atual por custo-benefício.
- Automações adicionais (ex.: `push`/`pull_request`) podem ser adotadas em fase posterior.
