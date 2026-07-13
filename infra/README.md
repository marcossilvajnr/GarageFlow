# GarageFlow Terraform

Infraestrutura como Código local do GarageFlow.

Esta pasta usa Terraform para provisionar um cluster Kubernetes local com Kind. Os workloads da aplicação e do banco continuam descritos nos manifests oficiais em `../k8s`, mantendo uma divisão simples:

- `infra/`: cria e destrói o cluster Kubernetes local.
- `k8s/`: aplica recursos da aplicação, PostgreSQL, ConfigMap, Secret, Service e HPA.

## Pré-requisitos
- Docker Desktop em execução.
- Terraform instalado.
- Kind instalado.
- kubectl instalado.
- Imagem local da aplicação criada a partir do `Dockerfile`.

Validação rápida:

```bash
terraform version
kind version
kubectl version --client
docker version
```

## Provisionar O Cluster
Na raiz do repositório:

```bash
cd infra
terraform init
terraform fmt -check -recursive
terraform validate
terraform plan
terraform apply
```

O Terraform cria um cluster Kind chamado `garageflow` e configura o contexto `kind-garageflow` no `kubectl`.

Valide:

```bash
kind get clusters
kubectl config current-context
kubectl get nodes
```

## Build E Carga Da Imagem
Em outro terminal, na raiz do repositório:

```bash
docker build -t garageflow-api:latest .
kind load docker-image garageflow-api:latest --name garageflow
```

O `kind load` é necessário porque o cluster Kind roda em containers Docker próprios e precisa receber a imagem local explicitamente.

## Aplicar Banco E Aplicação
Na raiz do repositório:

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/
kubectl get pods -n garageflow
kubectl get hpa -n garageflow
```

Os manifests aplicam:

- namespace `garageflow`;
- PostgreSQL de demonstração;
- ConfigMap e Secret locais;
- Deployment e Service do `GarageFlow.WebHost`;
- HPA da aplicação.

## Acessar Health E Swagger
```bash
kubectl port-forward service/garageflow-webhost 8080:8080 -n garageflow
```

Em outro terminal:

```bash
curl http://localhost:8080/health
```

Swagger:

```text
http://localhost:8080/swagger
```

## Metrics Server
O HPA precisa do metrics-server para exibir CPU/memória reais.

```bash
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
kubectl patch deployment metrics-server -n kube-system --type='json' -p='[{"op":"add","path":"/spec/template/spec/containers/0/args/-","value":"--kubelet-insecure-tls"}]'
kubectl rollout status deployment/metrics-server -n kube-system --timeout=120s
kubectl top nodes
kubectl top pods -n garageflow
kubectl get hpa -n garageflow
```

## Destruir Infraestrutura
Remova primeiro os workloads da aplicação:

```bash
kubectl delete -f k8s/
```

Depois destrua o cluster local:

```bash
cd infra
terraform destroy
```

## Observações
- Nenhum recurso cloud é criado.
- O banco PostgreSQL é local e didático, adequado para demonstração.
- O estado Terraform local não deve ser commitado.
- O arquivo `.terraform.lock.hcl`, quando gerado, deve ser versionado para travar versões de providers. Esta implementação usa apenas o recurso built-in `terraform_data`.
- Os workloads continuam em `/k8s`; o Terraform provisiona o cluster local. Essa divisão mantém os manifests Kubernetes legíveis e reutilizáveis pela CI/CD.
- AWS/EKS pode ser adicionado sem remover este caminho local reproduzível.
