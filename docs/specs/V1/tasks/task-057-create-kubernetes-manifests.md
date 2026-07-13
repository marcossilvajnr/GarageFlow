# Task-057 — Create Kubernetes Manifests

## 0) Metadata
- `task_id`: `task-057`
- `slug`: `create-kubernetes-manifests`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-056-add-operational-service-order-listing.md](task-056-add-operational-service-order-listing.md)

## 1) Objetivo
Criar os manifestos Kubernetes em `/k8s` para executar o GarageFlow em cluster local ou compatível, contemplando aplicação, banco de dados, configuração, segredo e escalabilidade horizontal conforme o enunciado da Fase 2.

## 2) Escopo
### In
- Criar pasta `/k8s`.
- Criar manifests YAML para `Namespace`, `ConfigMap`, `Secret`, `Deployment`, `Service` e `HorizontalPodAutoscaler`.
- Publicar o `GarageFlow.WebHost` como workload principal da aplicação.
- Incluir PostgreSQL como dependência de desenvolvimento/demonstração.
- Documentar instalação e validação do metrics-server para o HPA em cluster local.
- Documentar comandos de aplicação, validação, acesso ao Swagger e limpeza.

### Out
- Provisionar cluster Kubernetes real em cloud.
- Criar Terraform nesta task.
- Criar pipeline CI/CD nesta task.
- Usar Helm, Kustomize, service mesh, ingress controller obrigatório ou operador de Postgres.
- Criar estratégia de alta disponibilidade real para banco de dados.
- Alterar código da aplicação.
- Versionar manifesto próprio do metrics-server como parte do domínio da aplicação.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [Dockerfile](../../../../Dockerfile)
- [docker-compose.yml](../../../../docker-compose.yml)
- [README.md](../../../../README.md)
- [docs/architecture/architecture-overview.md](../../../architecture/architecture-overview.md)
- [docs/architecture/operations-and-quality.md](../../../architecture/operations-and-quality.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)
- [docs/specs/V1/tasks/task-013-setup-docker-infrastructure-and-local-runbook.md](task-013-setup-docker-infrastructure-and-local-runbook.md)
- [docs/specs/V1/tasks/task-050-finalize-jwt-delivery-docs-and-evidence-package.md](task-050-finalize-jwt-delivery-docs-and-evidence-package.md)

## 4) Decisões Arquiteturais Já Tomadas
- O executável principal é `GarageFlow.WebHost`, composition root da aplicação.
- O serviço Docker Compose continua chamado `api` por compatibilidade, mas o container executa o WebHost.
- A porta HTTP interna da aplicação em container é `8080`.
- A aplicação depende de `ConnectionStrings__GarageFlow`.
- Swagger deve estar disponível em ambiente de desenvolvimento para demonstração.
- Secrets sensíveis não devem ficar em `ConfigMap`.

## 5) Regras de Infra Aplicáveis
- A aplicação deve iniciar somente após banco estar disponível quando possível.
- Configuração não sensível deve ir para `ConfigMap`.
- Configuração sensível deve ir para `Secret`.
- O HPA deve escalar a aplicação por CPU e/ou memória.
- O banco em Kubernetes é recurso de demonstração local, não desenho final de produção.
- Os manifests devem ser simples e aplicáveis com `kubectl apply -f k8s/`.

## 6) Contratos e Interfaces
### 6.1 Recursos Kubernetes obrigatórios
- `Namespace`: ambiente isolado para `garageflow`.
- `ConfigMap`: ambiente, URLs e parâmetros não sensíveis.
- `Secret`: credenciais do Postgres e JWT secret.
- `Deployment`: aplicação `garageflow-webhost`.
- `Service`: exposição interna da aplicação.
- `Deployment` ou `StatefulSet`: PostgreSQL para demo local.
- `Service`: exposição interna do PostgreSQL.
- `HorizontalPodAutoscaler`: escala do `garageflow-webhost`.

### 6.2 Variáveis mínimas da aplicação
- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_URLS=http://+:8080`
- `ConnectionStrings__GarageFlow`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SecretKey`

### 6.3 Acesso esperado para validação
- `kubectl port-forward service/garageflow-webhost 8080:8080 -n garageflow`
- `GET http://localhost:8080/health`
- `GET http://localhost:8080/swagger`

## 7) Plano Técnico por Camada
### Docker
- Reusar a imagem gerada pelo [Dockerfile](../../../../Dockerfile) atual.
- Documentar tag esperada, por exemplo `garageflow-api:latest`, salvo decisão diferente durante a execução.

### Kubernetes
- Criar manifests claros, pequenos e independentes.
- Definir labels consistentes para app e banco.
- Configurar readiness/liveness quando houver endpoint adequado.
- Configurar requests/limits para permitir HPA.
- Documentar metrics-server como add-on do cluster necessário para leitura real de CPU/memória pelo HPA.

### Application/API
- Sem alteração esperada.
- Não mudar rotas, autenticação ou Swagger nesta task.

### Docs
- Criar [k8s/README.md](../../../../k8s/README.md) com comandos de build da imagem, apply, port-forward, health, swagger e cleanup.
- Atualizar [README.md](../../../../README.md) raiz somente se necessário para apontar para `/k8s`.

### Tests
- Validar YAML e comandos `kubectl` quando ambiente local existir.
- Se cluster local não estiver disponível, executar validações estáticas e registrar limitação objetivamente.

## 8) Arquivos a Criar/Alterar
- [k8s/namespace.yaml](../../../../k8s/namespace.yaml)
- [k8s/configmap.yaml](../../../../k8s/configmap.yaml)
- [k8s/secret.yaml](../../../../k8s/secret.yaml)
- [k8s/postgres.yaml](../../../../k8s/postgres.yaml)
- [k8s/webhost.yaml](../../../../k8s/webhost.yaml)
- [k8s/hpa.yaml](../../../../k8s/hpa.yaml)
- [k8s/README.md](../../../../k8s/README.md)
- [README.md](../../../../README.md) (somente ponte para documentação Kubernetes, se necessário)

Contrato de arquivos:
- Mudanças fora desta lista devem ser justificadas explicitamente na resposta final.
- Não alterar código C# nesta task sem justificar necessidade operacional.

## 9) Critérios de Pronto
- [ ] `/k8s` existe.
- [ ] Manifests incluem `Deployment`, `Service`, `ConfigMap`, `Secret` e `HPA`.
- [ ] Aplicação usa `GarageFlow.WebHost`.
- [ ] Porta interna da aplicação é `8080`.
- [ ] Connection string aponta para serviço PostgreSQL dentro do cluster.
- [ ] Swagger e health são acessíveis via `port-forward`.
- [ ] Metrics-server está documentado como pré-requisito para métricas reais do HPA.
- [ ] `kubectl apply -f k8s/` documentado.
- [ ] `kubectl delete -f k8s/` documentado.
- [ ] [k8s/README.md](../../../../k8s/README.md) explica recursos criados.

## 10) Estratégia de Testes
### Estática
- [ ] `kubectl apply --dry-run=client -f k8s/` quando `kubectl` estiver disponível.
- [ ] `kubectl kustomize` não é obrigatório, pois a task não usa Kustomize.
- [ ] Revisar YAML para ausência de secrets em `ConfigMap`.

### Integração local
- [ ] Build da imagem Docker local.
- [ ] Aplicar manifests em cluster local.
- [ ] Verificar `kubectl get pods -n garageflow`.
- [ ] Verificar `kubectl get hpa -n garageflow`.
- [ ] Verificar `kubectl top nodes` com metrics-server instalado.
- [ ] Verificar `kubectl top pods -n garageflow` com metrics-server instalado.
- [ ] Verificar HPA sem `TARGETS <unknown>` quando metrics-server estiver saudável.
- [ ] Port-forward do serviço da aplicação.
- [ ] `curl http://localhost:8080/health`.
- [ ] Abrir `http://localhost:8080/swagger`.

### E2E
- [ ] Não criar novo E2E obrigatório nesta task.

## 11) Riscos e Mitigações
- Risco: cluster local não disponível no ambiente do executor.
  - Mitigação: entregar manifests e validação estática, registrando limitação.
- Risco: imagem local não existir no cluster.
  - Mitigação: documentar build/tag e, se usar `kind`, documentar `kind load docker-image`.
- Risco: HPA não funcionar sem metrics-server.
  - Mitigação: documentar dependência do metrics-server para observação real de escala.
- Risco: Postgres em `Deployment` ser confundido com produção.
  - Mitigação: documentar que é banco para desenvolvimento/demonstração.
- Risco: segredo real versionado.
  - Mitigação: usar valores locais/didáticos e documentar substituição por secret real fora do Git.

## 12) Checklist de Execução para IA
- [ ] Ler documentos canônicos.
- [ ] Confirmar porta e variáveis atuais do Docker/Compose.
- [ ] Criar `/k8s`.
- [ ] Criar namespace.
- [ ] Criar configmap sem dados sensíveis.
- [ ] Criar secret com dados sensíveis didáticos.
- [ ] Criar recurso PostgreSQL.
- [ ] Criar deployment/service do WebHost.
- [ ] Criar HPA com requests/limits compatíveis.
- [ ] Criar [k8s/README.md](../../../../k8s/README.md).
- [ ] Validar YAML com `kubectl` se disponível.
- [ ] Registrar limitações de ambiente, se houver.

## 13) Evidência Esperada de Fechamento
- Lista de arquivos criados em `/k8s`.
- Saída de validação YAML ou justificativa se `kubectl`/cluster não estiver disponível.
- Evidência de `kubectl get pods -n garageflow`, se possível.
- Evidência de `kubectl top nodes`, `kubectl top pods -n garageflow` e HPA com métricas reais, se metrics-server estiver instalado.
- Evidência de `GET /health`, se possível.
- Evidência de Swagger acessível via port-forward, se possível.
