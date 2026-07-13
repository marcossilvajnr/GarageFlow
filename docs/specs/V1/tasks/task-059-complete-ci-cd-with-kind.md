# Task-059 — Complete CI/CD With Kind

## 0) Metadata
- `task_id`: `task-059`
- `slug`: `complete-ci-cd-with-kind`
- `owner`: `Platform Team`
- `status`: `Ready`
- `depends_on`: [task-058-create-terraform-local-infrastructure.md](task-058-create-terraform-local-infrastructure.md)

## 1) Objetivo
Evoluir a pipeline manual do GitHub Actions para atender integralmente o requisito de CI/CD da Fase 2, mantendo baixo atrito operacional: build da aplicação, execução dos testes, build da imagem Docker, criação de cluster Kubernetes local no runner com Kind, deploy do banco, deploy da aplicação, aplicação dos manifests YAML e validação do `/health`.

O objetivo de entrega é deixar o último histórico bem-sucedido do workflow com evidência clara de todos os itens exigidos pelo enunciado.

## 2) Escopo
### In
- Evoluir o workflow manual existente para `.github/workflows/garageflow.yml`.
- Separar a pipeline em reusable workflows por stage.
- Manter `workflow_dispatch` como gatilho principal.
- Executar build da solução.
- Executar testes automatizados.
- Executar testes unitários/integração no stage `Quality`, excluindo E2E.
- Executar E2E em stage dedicado com PostgreSQL service.
- Gerar relatórios já existentes de qualidade, cobertura, segurança e breakdown.
- Buildar imagem Docker `garageflow-api:ci`.
- Criar cluster Kind temporário dentro do runner GitHub Actions.
- Carregar a imagem Docker no cluster Kind.
- Aplicar manifests Kubernetes de `/k8s`.
- Subir banco PostgreSQL de demonstração no cluster.
- Subir `GarageFlow.WebHost` no cluster.
- Instalar metrics-server no cluster Kind para evidenciar HPA, se viável no tempo da pipeline.
- Validar pods, services, HPA e `/health`.
- Publicar resumo executivo da etapa de deploy no Job Summary.
- Publicar artifacts de evidência, quando útil.

### Out
- Deploy automático em cloud.
- Uso obrigatório de AWS Academy na pipeline.
- EKS, ECR, RDS ou IAM nesta task.
- Terraform cloud/remote state nesta task.
- Gatilhos automáticos por `push` ou `pull_request`, salvo decisão explícita futura.
- Alterar regras de domínio ou código funcional da aplicação.
- Publicar imagem Docker em registry remoto.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- `.github/workflows/garageflow.yml`
- `.github/workflows/garageflow-quality.yml`
- `.github/workflows/garageflow-e2e.yml`
- `.github/workflows/garageflow-build.yml`
- `.github/workflows/garageflow-deploy-kind.yml`
- `.github/scripts/generate-coverage-summary.sh`
- `.github/scripts/generate-security-summary.sh`
- `.github/scripts/generate-test-breakdown.sh`
- `.github/scripts/generate-executive-summary.sh`
- [Dockerfile](../../../../Dockerfile)
- [README.md](../../../../README.md)
- [k8s/README.md](../../../../k8s/README.md)
- [k8s/namespace.yaml](../../../../k8s/namespace.yaml)
- [k8s/configmap.yaml](../../../../k8s/configmap.yaml)
- [k8s/secret.yaml](../../../../k8s/secret.yaml)
- [k8s/postgres.yaml](../../../../k8s/postgres.yaml)
- [k8s/webhost.yaml](../../../../k8s/webhost.yaml)
- [k8s/hpa.yaml](../../../../k8s/hpa.yaml)
- [infra/README.md](../../../../infra/README.md)
- [docs/architecture/ci.md](../../../architecture/ci.md)
- [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md)
- [docs/specs/V1/tasks/task-014-create-manual-quality-and-security-pipeline.md](task-014-create-manual-quality-and-security-pipeline.md)
- [docs/specs/V1/tasks/task-057-create-kubernetes-manifests.md](task-057-create-kubernetes-manifests.md)
- [docs/specs/V1/tasks/task-058-create-terraform-local-infrastructure.md](task-058-create-terraform-local-infrastructure.md)

## 4) Decisões Arquiteturais Já Tomadas
- O workflow oficial é `GarageFlow CI/CD`.
- O workflow é manual por custo-benefício e rastreabilidade para banca.
- O workflow deve ficar separado em quatro stages/jobs claros: `Quality`, `E2E`, `Build` e `Deploy Kind`.
- O projeto já possui [Dockerfile](../../../../Dockerfile) executando `GarageFlow.WebHost`.
- Os manifests Kubernetes oficiais ficam em `/k8s`.
- O deploy local validado usa Kind.
- A pipeline não deve depender de credenciais temporárias da AWS Academy para o caminho obrigatório.
- AWS/EKS pode ser demonstrado separadamente em vídeo, mas a evidência obrigatória de CI/CD deve ser reproduzível no GitHub Actions.

## 5) Regras de Infra Aplicáveis
- A pipeline deve falhar se build, testes, Docker build, deploy Kubernetes ou health check falharem.
- O stage `Quality` deve rodar testes unitários/integração sem exigir infraestrutura E2E.
- O stage `E2E` deve fornecer PostgreSQL e variáveis E2E necessárias para os fluxos críticos.
- O cluster Kind no runner é efêmero e deve ser criado pela própria pipeline.
- O banco de dados deve ser aplicado no cluster como parte do deploy.
- O WebHost deve aguardar disponibilidade do Postgres conforme manifest da task 58.
- Não commitar secrets reais.
- O Secret Kubernetes versionado continua sendo didático/local para demonstração.
- A etapa de metrics-server pode ser marcada como evidência de HPA; se instável no runner, documentar objetivamente a decisão de mantê-la ou removê-la do caminho crítico.

## 6) Contratos e Interfaces
### 6.1 Workflow
Arquivo:
```text
.github/workflows/garageflow.yml
```

Reusable workflows:
```text
.github/workflows/garageflow-quality.yml
.github/workflows/garageflow-e2e.yml
.github/workflows/garageflow-build.yml
.github/workflows/garageflow-deploy-kind.yml
```

Trigger mínimo:
```yaml
on:
  workflow_dispatch:
```

### 6.2 Etapas obrigatórias do CI/CD
- Checkout.
- Setup .NET.
- Restore.
- Build.
- Testes com cobertura.
- E2E com PostgreSQL service em stage dedicado.
- Relatórios existentes.
- Docker build.
- Setup Kind.
- Criar cluster Kind.
- Carregar imagem no Kind.
- Aplicar manifests Kubernetes.
- Aguardar rollout do Postgres.
- Aguardar rollout do WebHost.
- Validar recursos com `kubectl get`.
- Validar `/health`.
- Publicar resumo no Job Summary.

### 6.3 Comandos base esperados
```bash
docker build -t garageflow-api:ci .
kind create cluster --name garageflow-ci
kind load docker-image garageflow-api:ci --name garageflow-ci
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/
kubectl rollout status deployment/garageflow-postgres -n garageflow --timeout=180s
kubectl rollout status deployment/garageflow-webhost -n garageflow --timeout=240s
kubectl port-forward service/garageflow-webhost 18080:8080 -n garageflow &
curl --fail http://127.0.0.1:18080/health
```

### 6.4 Imagem de CI
O workflow pode usar uma tag própria de CI:
```text
garageflow-api:ci
```

Se usar essa tag, o manifest deve ser ajustado em runtime no pipeline ou a imagem deve ser tagueada também como:
```text
garageflow-api:latest
```

Preferência 80/20:
```bash
docker build -t garageflow-api:latest -t garageflow-api:ci .
kind load docker-image garageflow-api:latest --name garageflow-ci
```

### 6.5 Saídas de evidência
Gerar no Job Summary:
- status de build/testes;
- status de Docker build;
- pods no namespace `garageflow`;
- services no namespace `garageflow`;
- HPA no namespace `garageflow`;
- resposta do `/health`;
- link para artifacts existentes.

## 7) Plano Técnico por Camada
### Domain
- Sem alterações.

### Application
- Sem alterações funcionais.

### Infrastructure
- Sem alterações funcionais.

### API/WebHost
- Sem alterações funcionais.

### Docker
- Reusar [Dockerfile](../../../../Dockerfile).
- Buildar imagem durante o workflow.
- Carregar imagem local no Kind.

### Kubernetes
- Reusar manifests `/k8s`.
- Aguardar rollout de Postgres e WebHost.
- Validar readiness real.
- Instalar metrics-server se o custo/tempo for aceitável.

### Terraform
- Não executar Terraform obrigatoriamente nesta pipeline.
- A task 58 já cobre IaC local com Terraform.
- Esta task foca o deploy CI/CD reproduzível no runner.

### CI/CD
- Evoluir workflow existente sem perder relatórios atuais.
- Separar o workflow em quatro jobs/stages: `quality`, `e2e`, `build` e `deploy-kind`.
- Configurar `quality` para excluir E2E do comando de testes.
- Configurar `e2e` com PostgreSQL service e credenciais E2E didáticas.
- Fazer o job `e2e` depender do job `quality`.
- Fazer o job `build` depender do job `e2e`.
- Fazer o job `deploy` depender do job `build`.
- Publicar a imagem Docker do stage `Build` como artifact.
- Fazer o stage `Deploy Kind` baixar a imagem Docker, executar `docker load` e carregá-la no Kind.
- Usar `needs` para garantir a ordem `Quality -> E2E -> Build -> Deploy Kind`.
- Manter `workflow_dispatch` apenas no orquestrador `garageflow.yml`.
- Usar `workflow_call` nos workflows de stage.
- Publicar evidência no Job Summary.
- Garantir cleanup com `if: always()` quando aplicável.
- Nomear os jobs como `Quality`, `E2E`, `Build` e `Deploy Kind` para leitura clara no histórico do GitHub Actions.

### Docs
- Atualizar [docs/architecture/ci.md](../../../architecture/ci.md) com o novo escopo da pipeline.
- Atualizar [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md) se necessário.
- Atualizar [README.md](../../../../README.md) com instrução curta de execução e evidência esperada.

## 8) Arquivos a Criar/Alterar
- `.github/workflows/garageflow.yml`
- `.github/workflows/garageflow-quality.yml`
- `.github/workflows/garageflow-e2e.yml`
- `.github/workflows/garageflow-build.yml`
- `.github/workflows/garageflow-deploy-kind.yml`
- [docs/architecture/ci.md](../../../architecture/ci.md)
- [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md) (se necessário)
- [README.md](../../../../README.md) (se necessário)
- `.github/scripts/*` (somente se for útil para reduzir complexidade do workflow)

Contrato de arquivos:
- Mudanças fora desta lista devem ser justificadas explicitamente na resposta final.
- Não alterar código C# nesta task sem justificar necessidade operacional.

## 9) Critérios de Pronto
- [ ] Workflow continua manual via `workflow_dispatch`.
- [ ] Build da aplicação passa na pipeline.
- [ ] Testes automatizados passam na pipeline.
- [ ] Quality exclui E2E e passa sem PostgreSQL service.
- [ ] E2E passa em stage dedicado com PostgreSQL service.
- [ ] Relatórios atuais continuam sendo gerados.
- [ ] Imagem Docker é buildada no stage `Build`.
- [ ] Imagem Docker é publicada como artifact do workflow.
- [ ] Imagem Docker é carregada no stage `Deploy Kind`.
- [ ] Cluster Kind é criado na pipeline.
- [ ] Imagem Docker é carregada no cluster Kind.
- [ ] Banco PostgreSQL é aplicado no Kubernetes pela pipeline.
- [ ] Aplicação WebHost é aplicada no Kubernetes pela pipeline.
- [ ] `kubectl rollout status` passa para Postgres e WebHost.
- [ ] `/health` retorna sucesso na pipeline.
- [ ] HPA é aplicado e exibido na evidência.
- [ ] Job Summary mostra evidência do deploy Kubernetes.
- [ ] Último run bem-sucedido do GitHub Actions cobre todos os requisitos do enunciado de CI/CD.

## 10) Estratégia de Testes
### Local
- [ ] Validar sintaxe YAML do workflow, quando ferramenta disponível.
- [ ] Rodar localmente comandos equivalentes:
  - `docker build`;
  - `kind create cluster`;
  - `kind load docker-image`;
  - `kubectl apply`;
  - `kubectl rollout status`;
  - `curl /health`.

### Pipeline
- [ ] Executar `GarageFlow CI/CD` manualmente no GitHub Actions.
- [ ] Confirmar que o job de qualidade passa.
- [ ] Confirmar que o job Docker/Kubernetes passa.
- [ ] Confirmar artifacts de qualidade.
- [ ] Confirmar Job Summary com evidências de deploy.
- [ ] Confirmar que o último run verde é o run completo.

### Regressão
- [ ] Garantir que nenhum endpoint/API foi alterado.
- [ ] Garantir que os manifests `/k8s` continuam rodando localmente.
- [ ] Garantir que Docker Compose não foi afetado.

## 11) Riscos e Mitigações
- Risco: tempo da pipeline aumentar muito.
  - Mitigação: manter trigger manual e evitar matriz paralela.
- Risco: Kind no GitHub Actions falhar por ambiente do runner.
  - Mitigação: usar actions estáveis de setup Kind ou comandos oficiais simples.
- Risco: imagem do manifest não bater com imagem buildada.
  - Mitigação: buildar também `garageflow-api:latest`.
- Risco: WebHost iniciar antes do Postgres.
  - Mitigação: manter `initContainer` de espera do Postgres no manifest.
- Risco: metrics-server flakey no runner.
  - Mitigação: HPA deve ser aplicado; métricas reais podem ser evidência complementar se estável.
- Risco: vulnerabilidade conhecida de dependência quebrar a pipeline.
  - Mitigação: manter comportamento atual de warning se essa for a decisão vigente; tratar upgrade em task separada.
- Risco: confundir CD local em Kind com deploy cloud.
  - Mitigação: documentar que a pipeline valida entrega Kubernetes reproduzível; AWS/EKS fica como demonstração opcional/futura.

## 12) Checklist de Execução para IA
- [ ] Ler documentos canônicos.
- [ ] Preservar relatório de qualidade existente.
- [ ] Decidir se adiciona novo job ou estende job atual.
- [ ] Adicionar Docker build.
- [ ] Adicionar setup/criação de cluster Kind.
- [ ] Carregar imagem no Kind.
- [ ] Aplicar manifests Kubernetes.
- [ ] Aguardar rollouts.
- [ ] Validar `/health`.
- [ ] Coletar `kubectl get pods`, `kubectl get svc`, `kubectl get hpa`.
- [ ] Publicar evidência no Job Summary.
- [ ] Atualizar documentação.
- [ ] Validar workflow localmente no limite possível.
- [ ] Orientar execução manual do workflow no GitHub.

## 13) Evidência Esperada de Fechamento
- Diff do workflow com job/etapas de Docker e Kubernetes.
- Saída local equivalente, se rodada.
- Link ou identificação do último run bem-sucedido do GitHub Actions, quando disponível.
- Job Summary contendo:
  - build/test status;
  - Docker build status;
  - pods;
  - services;
  - HPA;
  - resposta do `/health`.
- Confirmação de que o último run verde cobre os requisitos:
  - build da aplicação;
  - testes automatizados;
  - build da imagem Docker;
  - deploy no cluster Kubernetes;
  - deploy do banco de dados;
  - aplicação dos manifests YAML no cluster.
