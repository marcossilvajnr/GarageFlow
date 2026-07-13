# Task-014 — Create Manual Quality and Security Pipeline

## 0) Metadata
- `task_id`: `task-014`
- `slug`: `create-manual-quality-and-security-pipeline`
- `owner`: `Platform Team`
- `status`: `Ready`
- `depends_on`: [task-000-template.md](task-000-template.md), [task-013-setup-docker-infrastructure-and-local-runbook.md](task-013-setup-docker-infrastructure-and-local-runbook.md)

## 1) Objetivo
Implementar uma pipeline manual no GitHub Actions para gerar evidências apresentáveis de qualidade e segurança, com baixo consumo de minutos, sem gatilhos automáticos de `push`/`pull_request`.

## 2) Escopo
### In
- Criar workflow manual (`workflow_dispatch`) para:
  - build da solução;
  - execução de testes;
  - relatório de cobertura de testes;
  - relatório de vulnerabilidades de dependências;
  - contagem de testes por tipo (`Domain`, `Application`, `Integration`).
- Publicar relatórios em:
  - Job Summary (Markdown);
  - artifacts baixáveis do workflow.
- Garantir que a pipeline finalize com status claro (`OK/NOK`) por etapa.
- Documentar baseline operacional e da esteira em documento dedicado de arquitetura.
- Publicar visão visual dos resultados diretamente na execução do GitHub Actions.

### Out
- Gatilhos automáticos por `push` ou `pull_request`.
- Deploy automático em qualquer ambiente.
- Integração com ferramentas pagas externas de segurança.
- SAST avançado (CodeQL) nesta task.

## 3) Contexto Canônico Obrigatório
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)
- [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md)
- [docs/specs/V1/tasks/task-013-setup-docker-infrastructure-and-local-runbook.md](task-013-setup-docker-infrastructure-and-local-runbook.md)
- [README.md](../../../../README.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- Não há alteração de regras de negócio.
- Escopo exclusivamente de engenharia/esteira.

## 5) Contratos e Interfaces
### 5.1 Workflow público
- Arquivo obrigatório:
  - `.github/workflows/manual-quality-gate.yml`
- Trigger obrigatório:
  - `workflow_dispatch`.

### 5.4 Contrato documental da task
- Documento dedicado obrigatório em arquitetura:
  - [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md)
- O documento deve cobrir exclusivamente estado vigente:
  - execução Docker local;
  - pipeline manual de qualidade/segurança;
  - formato dos relatórios e evidências.

### 5.2 Relatórios obrigatórios
- Vulnerabilidades:
  - comando base: `dotnet list package --vulnerable --include-transitive --format json`.
  - saídas: `security-report.json` + [security-report.md](../../../../artifacts/security/security-report.md).
- Cobertura:
  - coleta com `XPlat Code Coverage`.
  - saídas: relatório consolidado (Markdown) + HTML (artifact compactado).
- Test breakdown:
  - contagem por tipo:
    - `Domain`
    - `Application`
    - `Integration`
  - saídas: `test-breakdown.json` + [test-breakdown.md](../../../../artifacts/test-breakdown/test-breakdown.md).
- Test report visual:
  - publicar resultado de testes no formato visual do GitHub Actions (aba de checks/report).
  - manter arquivo de resultado (`.trx`) como evidência técnica.
- Anotações de segurança:
  - expor vulnerabilidades `high/critical` como warnings no run para leitura imediata.

### 5.3 Critérios de falha da pipeline
- Build com erro -> `NOK`.
- Testes com falha -> `NOK`.
- Falha na geração dos relatórios obrigatórios -> `NOK`.

## 6) Plano Técnico por Camada
### Domain
- Sem alterações.

### Application
- Sem alterações funcionais.

### Infrastructure
- Sem alterações funcionais.

### API
- Sem alterações funcionais.

### DevOps/CI
- Criar workflow manual com jobs sequenciais:
  - `build_and_test`
  - `coverage_report`
  - `security_report`
  - `test_breakdown_report`
- Publicar artifacts de cada relatório.
- Adicionar `concurrency` para evitar execuções manuais duplicadas concorrentes.
- Gerar dashboard executivo no `Job Summary` com KPIs de:
  - build/testes;
  - cobertura;
  - vulnerabilidades;
  - contagem de testes por tipo.
- Publicar relatório visual de testes a partir do `.trx`.
- Emitir warnings de segurança a partir do relatório de vulnerabilidades.

### Tests
- Reutilizar suíte existente sem criar novos testes nesta task.

## 7) Arquivos a Criar/Alterar
### Criar (mandatório)
- `.github/workflows/manual-quality-gate.yml`

### Criar (scripts auxiliares, se necessário)
- `.github/scripts/generate-test-breakdown.sh`
- `.github/scripts/generate-security-summary.sh`
- `.github/scripts/generate-coverage-summary.sh`
- `.github/scripts/generate-executive-summary.sh`

### Alterar (esperado, se necessário)
- [README.md](../../../../README.md) (seção de execução manual da pipeline e leitura dos relatórios)
- [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md) (detalhes operacionais e de qualidade)
- [docs/architecture/README.md](../../../architecture/README.md) (incluir nova seção na ordem de leitura)

## 8) Critérios de Pronto
- [ ] Workflow aparece no GitHub Actions com execução manual disponível.
- [ ] Build e testes executam com sucesso na pipeline.
- [ ] Relatório de vulnerabilidades gerado em JSON e Markdown.
- [ ] Relatório de cobertura gerado e anexado como artifact.
- [ ] Breakdown de testes por tipo gerado e anexado como artifact.
- [ ] Job Summary contém visão executiva dos 3 blocos (qualidade, cobertura, segurança).
- [ ] Job Summary contém dashboard visual com KPIs consolidados.
- [ ] Resultado de testes é exibido visualmente no GitHub Actions.
- [ ] Vulnerabilidades high/critical aparecem como warnings no run.
- [ ] Documento [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md) atualizado e aderente ao estado atual da esteira.

## 9) Estratégia de Testes
### Pipeline validation
- [ ] Rodar workflow manualmente uma vez após merge.
- [ ] Validar artifacts baixáveis e conteúdo dos arquivos.
- [ ] Validar clareza do Job Summary para uso em apresentação.

### Regressão
- [ ] Confirmar que nenhuma alteração de domínio/API foi introduzida.

## 10) Riscos e Mitigações
- Risco: consumo excessivo de minutos do GitHub Actions.
  - Mitigação: trigger manual apenas + sem matrizes paralelas desnecessárias.
- Risco: contagem de testes por tipo inconsistente por filtro inadequado.
  - Mitigação: usar filtros explícitos por namespace/pasta e documentar regra.
- Risco: relatório de cobertura não consolidado de forma legível.
  - Mitigação: publicar resumo Markdown e artifact HTML.

## 11) Checklist de Execução para IA
- [ ] Confirmar trigger apenas `workflow_dispatch`.
- [ ] Implementar execução de build/test sem mudar código funcional.
- [ ] Gerar e publicar relatório de vulnerabilidades.
- [ ] Gerar e publicar cobertura com resumo legível.
- [ ] Gerar e publicar breakdown de testes por tipo.
- [ ] Garantir publicação visual de resultados de teste (`.trx`).
- [ ] Garantir dashboard executivo no Job Summary.
- [ ] Garantir warnings de segurança no run para severidade alta/crítica.
- [ ] Validar sintaxe YAML do workflow.
- [ ] Atualizar README se necessário com instruções de uso.
- [ ] Atualizar documentação dedicada de operações e qualidade em `docs/architecture`.

## 12) Guardrails Não-Negociáveis
- Proibido adicionar trigger automático por `push`/`pull_request` nesta task.
- Proibido alterar regras de domínio.
- Proibido falhar silenciosamente quando relatório obrigatório não for gerado.
- Proibido dependência de ferramenta paga externa para gerar evidências.

## 13) Assumptions
- O repositório continuará em plano GitHub Free no curto prazo.
- A nomenclatura de testes por tipo seguirá o padrão atual do projeto.
- A evolução para pipeline automática será tratada em task futura.
