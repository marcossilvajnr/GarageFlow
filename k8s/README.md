# GarageFlow Kubernetes

Manifestos Kubernetes para demonstração local do GarageFlow.

## Recursos Criados
- Namespace `garageflow`.
- ConfigMap `garageflow-config` com configuração não sensível.
- Secret `garageflow-secret` com credenciais didáticas de desenvolvimento.
- PostgreSQL local de demonstração com `Deployment`, `Service` e `PersistentVolumeClaim`.
- Aplicação `garageflow-webhost` com `Deployment` e `Service`.
- HPA `garageflow-webhost` com escala por CPU e memória.

## Pré-requisitos
- Docker Desktop com Kubernetes habilitado, Kind ou cluster local equivalente.
- `kubectl` apontando para o cluster desejado.
- Imagem local `garageflow-api:latest` criada a partir do `Dockerfile`.

## Build Da Imagem
```bash
docker build -t garageflow-api:latest .
```

Se usar Kind:

```bash
kind load docker-image garageflow-api:latest
```

## Aplicar
```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/
kubectl get pods -n garageflow
kubectl get hpa -n garageflow
```

Se o cluster local acabou de subir, aplicar o namespace separadamente evita erro de namespace ainda não reconhecido pelo API server.

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

## Metrics Server Para HPA
O HPA precisa do metrics-server para exibir CPU/memória e tomar decisões reais de escala.

Em cluster local, aplique o manifesto oficial:

```bash
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

Em Docker Desktop ou clusters locais com certificado kubelet sem IP SAN, aplique também:

```bash
kubectl patch deployment metrics-server -n kube-system --type='json' -p='[{"op":"add","path":"/spec/template/spec/containers/0/args/-","value":"--kubelet-insecure-tls"}]'
kubectl rollout status deployment/metrics-server -n kube-system --timeout=120s
```

Validação:

```bash
kubectl top nodes
kubectl top pods -n garageflow
kubectl get hpa -n garageflow
```

## Limpeza
```bash
kubectl delete -f k8s/
```

O metrics-server é um add-on do cluster, não um recurso da aplicação GarageFlow. Se quiser removê-lo do cluster local:

```bash
kubectl delete -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

## Observações
- O banco PostgreSQL deste diretório é para desenvolvimento e demonstração local, não para produção.
- O `Secret` contém valores didáticos versionados para facilitar execução local. Em ambiente real, substitua por secrets gerenciados fora do Git.
- O HPA depende do metrics-server instalado no cluster para reportar métricas e escalar efetivamente.
- O workload Kubernetes executa a imagem `garageflow-api:latest`, que contém o `GarageFlow.WebHost` como composition root.
- A pipeline `GarageFlow CI/CD` também aplica estes manifests em um cluster Kind efêmero para evidência de deploy.
