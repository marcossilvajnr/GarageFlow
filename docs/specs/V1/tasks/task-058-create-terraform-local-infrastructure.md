# Task-058 — Create Terraform Local Infrastructure

## 0) Metadata
- `task_id`: `task-058`
- `slug`: `create-terraform-local-infrastructure`
- `owner`: `Platform Team`
- `status`: `Ready`
- `depends_on`: [task-057-create-kubernetes-manifests.md](task-057-create-kubernetes-manifests.md)

## 1) Objetivo
Criar a camada de Infraestrutura como Código em `/infra` usando Terraform, conforme requisito obrigatório da Fase 2, para provisionar de forma reproduzível a infraestrutura local do GarageFlow: cluster Kubernetes local e banco de dados de demonstração.

O objetivo desta task é entregar um caminho 80/20 aderente ao enunciado: simples o suficiente para rodar na máquina do avaliador/desenvolvedor, mas explícito sobre o papel do Terraform no provisionamento.

## 2) Escopo
### In
- Criar pasta `/infra`.
- Criar arquivos Terraform para provisionamento local.
- Provisionar ou preparar um cluster Kubernetes local usando Terraform.
- Provisionar o banco de dados de demonstração via recursos Kubernetes gerenciados pelo Terraform ou pela aplicação dos manifests existentes.
- Integrar com os manifests Kubernetes já criados em `/k8s`.
- Documentar `terraform init`, `terraform plan`, `terraform apply` e `terraform destroy`.
- Documentar pré-requisitos locais e limitações conhecidas.
- Produzir evidência de execução ou registrar limitação objetiva do ambiente.

### Out
- Provisionar cloud real obrigatória.
- Criar ambiente produtivo.
- Criar banco gerenciado em cloud.
- Criar registry remoto de imagens.
- Criar pipeline CI/CD nesta task.
- Alterar código C# da aplicação.
- Substituir os manifests Kubernetes da task 057 por Helm/Kustomize.
- Criar política avançada de secrets para produção.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [Dockerfile](../../../../Dockerfile)
- [docker-compose.yml](../../../../docker-compose.yml)
- [README.md](../../../../README.md)
- [k8s/README.md](../../../../k8s/README.md)
- [k8s/namespace.yaml](../../../../k8s/namespace.yaml)
- [k8s/configmap.yaml](../../../../k8s/configmap.yaml)
- [k8s/secret.yaml](../../../../k8s/secret.yaml)
- [k8s/postgres.yaml](../../../../k8s/postgres.yaml)
- [k8s/webhost.yaml](../../../../k8s/webhost.yaml)
- [k8s/hpa.yaml](../../../../k8s/hpa.yaml)
- [docs/specs/V1/tasks/task-013-setup-docker-infrastructure-and-local-runbook.md](task-013-setup-docker-infrastructure-and-local-runbook.md)
- [docs/specs/V1/tasks/task-014-create-manual-quality-and-security-pipeline.md](task-014-create-manual-quality-and-security-pipeline.md)
- [docs/specs/V1/tasks/task-057-create-kubernetes-manifests.md](task-057-create-kubernetes-manifests.md)

## 4) Decisões Arquiteturais Já Tomadas
- A aplicação executável principal é `GarageFlow.WebHost`.
- A imagem local esperada é `garageflow-api:latest`.
- Os manifests Kubernetes oficiais do projeto ficam em `/k8s`.
- O banco PostgreSQL em Kubernetes é de desenvolvimento/demonstração local.
- O namespace Kubernetes da aplicação é `garageflow`.
- O HPA depende do metrics-server para métricas reais.
- A Fase 2 exige Terraform em `/infra`.
- Para esta entrega, o alvo principal é infraestrutura local reprodutível, não cloud.

## 5) Regras de Infra Aplicáveis
- Terraform deve ser usado para provisionar infraestrutura, não para substituir código da aplicação.
- O estado Terraform local deve ser tratável em ambiente de desenvolvimento.
- O executor deve evitar commitar arquivos de estado sensíveis ou derivados:
  - `.terraform/`
  - `*.tfstate`
  - `*.tfstate.backup`
  - `.terraform.lock.hcl` deve ser versionado se gerado, salvo decisão explícita contrária.
- Secrets reais não devem ser versionados.
- Valores didáticos podem ser usados apenas para ambiente local, com aviso claro.
- A solução deve ser desmontável com `terraform destroy` quando tecnicamente possível.

## 6) Contratos e Interfaces
### 6.1 Estrutura esperada
```text
infra/
  README.md
  main.tf
  variables.tf
  outputs.tf
  versions.tf
```

Arquivos adicionais são permitidos se simplificarem a implementação:
```text
infra/
  terraform.tfvars.example
  providers.tf
  kind-cluster.yaml
```

### 6.2 Abordagem recomendada
Usar Terraform para criar um cluster Kubernetes local com Kind.

Fluxo esperado:
```text
terraform init
terraform plan
terraform apply
docker build -t garageflow-api:latest .
kind load docker-image garageflow-api:latest --name <cluster>
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/
kubectl port-forward service/garageflow-webhost 8080:8080 -n garageflow
```

Se a implementação usar Terraform também para aplicar recursos Kubernetes, documentar claramente:
- quais recursos são criados pelo Terraform;
- quais continuam sendo aplicados via `kubectl apply -f k8s/`;
- por que essa divisão foi escolhida.

### 6.3 Alternativa aceitável
Se Kind não for viável no ambiente, usar Terraform com provider Kubernetes apontando para o contexto local `docker-desktop`, desde que a limitação seja documentada objetivamente.

Essa alternativa atende parcialmente o requisito, pois provisiona recursos no cluster, mas não provisiona o cluster em si. Se escolhida, a documentação deve explicar a diferença.

### 6.4 Outputs mínimos
O Terraform deve expor outputs úteis para validação:
- nome do cluster ou contexto utilizado;
- namespace da aplicação;
- comando sugerido de port-forward;
- URL local esperada do health check;
- URL local esperada do Swagger.

## 7) Plano Técnico por Camada
### Terraform
- Definir providers e versões em `versions.tf`.
- Criar recursos necessários para cluster local ou conexão com cluster local.
- Preferir variáveis para nomes e portas:
  - `cluster_name`
  - `namespace`
  - `app_image`
  - `app_port`
- Criar outputs legíveis.
- Garantir que `terraform validate` passe.

### Kubernetes
- Reusar os manifests de `/k8s` como fonte operacional principal.
- Não duplicar desnecessariamente YAML complexo dentro de Terraform se isso piorar manutenção.
- Garantir que o fluxo final consiga subir:
  - namespace;
  - PostgreSQL;
  - WebHost;
  - HPA.

### Docker
- Reusar o [Dockerfile](../../../../Dockerfile) atual.
- Documentar build da imagem antes do deploy.
- Se usar Kind, documentar carga da imagem no cluster.

### Banco de Dados
- Banco esperado: PostgreSQL de demonstração local.
- Pode ser provisionado pelos manifests de [k8s/postgres.yaml](../../../../k8s/postgres.yaml) ou por recursos Terraform equivalentes.
- Não criar banco cloud nesta task.

### Application/API
- Sem alteração esperada.
- Não mudar rotas, autenticação, migrations, Swagger ou composition root nesta task.

### Docs
- Criar [infra/README.md](../../../../infra/README.md) com:
  - visão geral;
  - pré-requisitos;
  - comandos de execução;
  - comandos de validação;
  - comandos de limpeza;
  - explicação do que Terraform provisiona;
  - limites do ambiente local.
- Atualizar [README.md](../../../../README.md) raiz com ponte para `/infra`, se necessário.

### Tests
- Validar Terraform com:
  - `terraform fmt -check -recursive`
  - `terraform init`
  - `terraform validate`
  - `terraform plan`
- Validar infraestrutura aplicada quando o ambiente permitir.

## 8) Arquivos a Criar/Alterar
- [infra/README.md](../../../../infra/README.md)
- [infra/main.tf](../../../../infra/main.tf)
- [infra/variables.tf](../../../../infra/variables.tf)
- [infra/outputs.tf](../../../../infra/outputs.tf)
- [infra/versions.tf](../../../../infra/versions.tf)
- [infra/terraform.tfvars.example](../../../../infra/terraform.tfvars.example) (opcional, recomendado)
- [infra/kind-cluster.yaml](../../../../infra/kind-cluster.yaml) (opcional, se usar Kind)
- `.gitignore` (se necessário para ignorar estado Terraform)
- [README.md](../../../../README.md) (somente ponte para documentação Terraform, se necessário)

Contrato de arquivos:
- Mudanças fora desta lista devem ser justificadas explicitamente na resposta final.
- Não alterar código C# nesta task sem justificar necessidade operacional.

## 9) Critérios de Pronto
- [ ] `/infra` existe.
- [ ] Terraform possui providers e versões declaradas.
- [ ] Terraform possui variáveis e outputs documentados.
- [ ] `terraform fmt -check -recursive` passa.
- [ ] `terraform validate` passa após `terraform init`.
- [ ] `terraform plan` executa ou limitação de ambiente é registrada objetivamente.
- [ ] Cluster Kubernetes local é provisionado via Terraform ou a alternativa local é justificada.
- [ ] Banco PostgreSQL de demonstração é provisionado/aplicado como parte do fluxo documentado.
- [ ] Aplicação pode ser publicada no Kubernetes usando a infraestrutura provisionada.
- [ ] [infra/README.md](../../../../infra/README.md) explica `init`, `plan`, `apply`, validação e `destroy`.
- [ ] README raiz aponta para `/infra` se ainda não houver ponte.

## 10) Estratégia de Testes
### Estática
- [ ] `terraform fmt -check -recursive infra`
- [ ] `terraform validate`
- [ ] Revisar `.gitignore` para impedir commit de estado local.
- [ ] Revisar ausência de secrets reais versionados.

### Integração local
- [ ] `terraform init`
- [ ] `terraform plan`
- [ ] `terraform apply`
- [ ] Build da imagem Docker `garageflow-api:latest`.
- [ ] Se Kind for usado: `kind load docker-image garageflow-api:latest --name <cluster>`.
- [ ] Aplicar manifests Kubernetes.
- [ ] `kubectl get pods -n garageflow`.
- [ ] `kubectl get hpa -n garageflow`.
- [ ] `kubectl port-forward service/garageflow-webhost 8080:8080 -n garageflow`.
- [ ] `curl http://localhost:8080/health`.

### Destruição
- [ ] `terraform destroy`.
- [ ] Confirmar remoção do cluster ou dos recursos provisionados.
- [ ] Registrar qualquer recurso que precise de limpeza manual.

### E2E
- [ ] Não criar novo E2E de aplicação nesta task.

## 11) Riscos e Mitigações
- Risco: Kind não estar instalado na máquina do avaliador.
  - Mitigação: documentar instalação e pré-requisito; manter comandos claros.
- Risco: Docker Desktop Kubernetes e Kind confundirem contextos.
  - Mitigação: documentar `kubectl config current-context` e nome do cluster.
- Risco: Terraform duplicar responsabilidade dos manifests `/k8s`.
  - Mitigação: escolher uma divisão explícita e documentada.
- Risco: banco em Kubernetes ser interpretado como produção.
  - Mitigação: declarar que é demo local; produção exigiria desenho próprio.
- Risco: estado Terraform ser commitado.
  - Mitigação: revisar `.gitignore` e não commitar `*.tfstate`.
- Risco: ambiente do executor não permitir criação de cluster local.
  - Mitigação: entregar validações estáticas e registrar a limitação com comandos tentados.

## 12) Checklist de Execução para IA
- [ ] Ler documentos canônicos.
- [ ] Confirmar se `terraform`, `docker`, `kubectl` e `kind` estão disponíveis.
- [ ] Definir abordagem final: Kind preferencial ou provider Kubernetes local justificado.
- [ ] Criar `/infra`.
- [ ] Criar arquivos Terraform mínimos.
- [ ] Criar [infra/README.md](../../../../infra/README.md).
- [ ] Atualizar `.gitignore` se necessário.
- [ ] Atualizar README raiz se necessário.
- [ ] Rodar `terraform fmt`.
- [ ] Rodar `terraform init`.
- [ ] Rodar `terraform validate`.
- [ ] Rodar `terraform plan`.
- [ ] Se possível, rodar `terraform apply`.
- [ ] Se possível, publicar app no cluster provisionado.
- [ ] Se possível, validar `/health` e Swagger.
- [ ] Registrar evidências e limitações.

## 13) Evidência Esperada de Fechamento
- Lista de arquivos criados em `/infra`.
- Saída de `terraform fmt -check -recursive infra`.
- Saída de `terraform init`.
- Saída de `terraform validate`.
- Saída de `terraform plan`.
- Evidência de `terraform apply`, se possível.
- Evidência de cluster/contexto Kubernetes provisionado ou utilizado.
- Evidência de banco PostgreSQL aplicado/provisionado.
- Evidência de `kubectl get pods -n garageflow`, se possível.
- Evidência de `GET /health`, se possível.
- Evidência de `terraform destroy` ou instrução clara de limpeza.
