# GarageFlow
Backend .NET para gestão de oficina mecânica, com foco em Ordem de Serviço, estoque, compras e execução ponta a ponta.

Modelagem de domínio canônica: `docs/domain`.

## Documentação
- Domínio: `docs/domain`
- Arquitetura e qualidade: `docs/architecture`
- Especificações por task: `docs/specs`

## Operação E Infraestrutura
O projeto está organizado para execução local, validação por repositório e demonstração dos fluxos operacionais:

- APIs de Ordem de Serviço disponíveis no contrato HTTP.
- `GarageFlow.WebHost` extraído como composition root.
- Docker Compose executando o WebHost.
- Kubernetes local com aplicação, PostgreSQL, HPA, ConfigMap e Secret.
- Terraform local provisionando cluster Kind.
- CI/CD manual no GitHub Actions com stages `Quality`, `E2E`, `Build` e `Deploy Kind`.

AWS/EKS e SonarQube remoto não fazem parte do caminho local padrão. SonarQube existe como fluxo local opcional.

## Pré-requisitos
- Docker Desktop em execução
- VS Code com extensão REST Client (para a demo guiada)
- Arquivo `.env` na raiz do repositório
- Arquivo `.env` em `tools/rest-client/` para executar os `.http`

## Subir o ambiente
1. Garanta um arquivo `.env` na raiz do repositório.
2. Suba os serviços:

```bash
docker compose up -d --build
docker compose ps
```

O serviço Docker Compose continua chamado `api` para manter compatibilidade com os comandos e arquivos REST Client já existentes, mas a imagem executa `GarageFlow.WebHost`, que é o composition root da aplicação.

## Endpoints de acesso
- Swagger UI: `http://localhost:8080/swagger`
- Swagger JSON: `http://localhost:8080/swagger/v1/swagger.json`
- Health: `http://localhost:8080/health`

## Demo operacional (recomendado para apresentação)
Arquivos REST Client:
- [tools/rest-client/maintenance-requests.http](tools/rest-client/maintenance-requests.http)
- [tools/rest-client/demo-service-order-with-purchase-requests.http](tools/rest-client/demo-service-order-with-purchase-requests.http)
- [tools/rest-client/demo-phase-2-service-orders.http](tools/rest-client/demo-phase-2-service-orders.http)

Configuração:
- Variáveis de execução ficam em `tools/rest-client/.env`.
- O fluxo consome `API_HTTP_PORT`, `API_USERNAME`, `API_PASSWORD`, `EXTERNAL_USERNAME` e `EXTERNAL_PASSWORD` via `{{$dotenv ...}}`.

Sequência sugerida:
1. Abra `maintenance-requests.http` e execute `POST /dev/database/reset` (`confirm: true`).
2. Abra `demo-service-order-with-purchase-requests.http`.
3. Execute as requests de cima para baixo.

O fluxo ponta a ponta cobre:
- cadastro base
- abertura da OS
- diagnóstico com serviço adicional do mecânico
- separação com falta de estoque
- geração e conclusão de compra
- retomada da separação
- execução e entrega final

Para demonstrar especificamente as APIs de OS adicionadas, use `demo-phase-2-service-orders.http`. Ele separa o setup pré-gravação dos blocos de demonstração e evidencia:
- abertura de OS com serviços iniciais
- consulta dedicada de status com label pública
- aprovação externa de orçamento
- listagem operacional ordenada por prioridade
- exclusão lógica de OS entregue da fila operacional

## Validação rápida
Health:

```bash
curl http://localhost:8080/health
```

## Kubernetes
Os manifestos Kubernetes ficam em `k8s/`.

```bash
docker build -t garageflow-api:latest .
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/
kubectl port-forward service/garageflow-webhost 8080:8080 -n garageflow
```

Detalhes de recursos criados, validação e limpeza estão em [k8s/README.md](k8s/README.md). A visão canônica de infraestrutura e deploy está em [docs/architecture/deployment-and-infrastructure.md](docs/architecture/deployment-and-infrastructure.md).

## Terraform
A infraestrutura como código local fica em `infra/`.

```bash
cd infra
terraform init
terraform plan
terraform apply
```

O Terraform provisiona um cluster Kubernetes local com Kind. Depois disso, a imagem Docker e os manifests Kubernetes continuam sendo aplicados conforme o runbook em [infra/README.md](infra/README.md).

## Testes automatizados
Somente E2E:

```bash
dotnet test --filter "FullyQualifiedName~E2E"
```

Suíte completa:

```bash
dotnet test
```

## CI/CD
O workflow oficial é `GarageFlow CI/CD`, executado manualmente no GitHub Actions.

Ele valida:
- build da solução;
- testes automatizados;
- relatórios de cobertura, segurança e breakdown;
- build e empacotamento da imagem Docker;
- deploy em cluster Kind efêmero;
- aplicação dos manifests Kubernetes;
- deploy do banco PostgreSQL;
- deploy do `GarageFlow.WebHost`;
- validação de HPA e `/health`.

Detalhes estão em [docs/architecture/ci.md](docs/architecture/ci.md) e em [docs/architecture/deployment-and-infrastructure.md](docs/architecture/deployment-and-infrastructure.md).

## Comandos úteis
Logs da aplicação:

```bash
docker compose logs -f api
```

Parar ambiente:

```bash
docker compose down
```

Reset total (com volume do banco):

```bash
docker compose down -v
docker compose up -d --build
```

## Troubleshooting rápido
- `ECONNRESET` no REST Client:
1. aguarde a API estabilizar após reset/migrate
2. valide `GET /health`
3. faça login novamente para obter novo token

- API não sobe:
1. verifique `docker compose logs -f api`
2. se necessário, faça reset total (`docker compose down -v`)
