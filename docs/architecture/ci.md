# CI/CD (Continuous Integration and Delivery)

## Objetivo
Definir a esteira manual de CI/CD do GarageFlow para geração de evidências técnicas de qualidade, segurança e deploy Kubernetes.

## Escopo Atual
- Execução manual via GitHub Actions (`workflow_dispatch`).
- Build da solução.
- Execução de testes automatizados.
- Geração de relatórios de cobertura.
- Scan de vulnerabilidades de dependências.
- Build da imagem Docker da aplicação.
- Criação de cluster Kubernetes temporário com Kind no runner.
- Deploy do banco PostgreSQL e do `GarageFlow.WebHost` no cluster Kind.
- Aplicação dos manifests Kubernetes de `/k8s`.
- Validação de rollout, HPA e `/health`.
- Resumo executivo visual da execução.

## Workflow Oficial
- Arquivo: `.github/workflows/garageflow.yml`
- Nome no GitHub Actions: `GarageFlow CI/CD`

## Stages
- `Quality`: build, testes unitários/integração sem E2E, cobertura, segurança, breakdown e relatório executivo.
- `E2E`: fluxos críticos ponta a ponta com PostgreSQL service dedicado.
- `Build`: build da imagem Docker e publicação da imagem como artifact do workflow.
- `Deploy Kind`: cluster Kind, carga da imagem Docker, deploy Kubernetes, banco, HPA e health check.

## Arquivos
- `.github/workflows/garageflow.yml`: orquestrador manual com `workflow_dispatch`.
- `.github/workflows/garageflow-quality.yml`: reusable workflow do stage `Quality`.
- `.github/workflows/garageflow-e2e.yml`: reusable workflow do stage `E2E`.
- `.github/workflows/garageflow-build.yml`: reusable workflow do stage `Build`.
- `.github/workflows/garageflow-deploy-kind.yml`: reusable workflow do stage `Deploy Kind`.

## Como Executar
1. Abrir o repositório no GitHub.
2. Ir em `Actions`.
3. Selecionar `GarageFlow CI/CD`.
4. Clicar em `Run workflow`.

## Evidências Geradas
- Job Summary com indicadores consolidados.
- Resultado visual de testes (TRX).
- Evidência de E2E com PostgreSQL real no stage `E2E`.
- Evidência de build da imagem Docker.
- Evidência de deploy Kubernetes com pods, services, HPA e health check.
- Artefatos baixáveis:
  - `artifacts/executive/executive-summary.md`
  - `artifacts/coverage/coverage-summary.md`
  - `artifacts/coverage/coverage-html.tar.gz`
  - `artifacts/security/security-report.json`
  - `artifacts/security/security-report.md`
  - `artifacts/test-breakdown/test-breakdown.json`
  - `artifacts/test-breakdown/test-breakdown.md`
  - `artifacts/kubernetes/kubernetes-deploy-summary.md`

## Evidência Kubernetes na CI/CD
O stage `Deploy Kind` valida:
- carga da imagem Docker `garageflow-api` produzida no stage `Build`;
- criação de cluster Kind efêmero;
- carga da imagem no cluster;
- aplicação dos manifests em `/k8s`;
- rollout do PostgreSQL;
- rollout do WebHost;
- HPA aplicado;
- health check via port-forward.

## Escopo Da Esteira
- A pipeline não cria recursos AWS, EKS, ECR, RDS ou IAM.
- O deploy cloud pode ser adicionado sem substituir o caminho reproduzível em Kind.
- SonarQube remoto não faz parte da CI atual; a análise Sonar está documentada como fluxo local opcional em `operations-and-quality.md`.
- O relatório de vulnerabilidades da CI emite warnings para pacotes high/critical; upgrades de dependências devem ser tratados separadamente.

## Evidência JWT e RBAC na CI
Itens mínimos para trilha de autenticação/autorização:
- suíte de testes verde com cenários de autenticação;
- evidência de E2E crítico com JWT real;
- validação de rotas protegidas com comportamento `401/403` conforme ausência de token e falta de papel;
- rastreabilidade em resumo executivo da execução.

## Critério de Uso
- A esteira manual é o padrão operacional por custo-benefício e rastreabilidade.
- Automações adicionais (ex.: `push`/`pull_request`) podem ser adotadas conforme necessidade operacional.
