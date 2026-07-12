# GarageFlow
Backend .NET para gestão de oficina mecânica, com foco em Ordem de Serviço, estoque, compras e execução ponta a ponta.

Modelagem de domínio canônica: `docs/domain`.

## Documentação
- Domínio: `docs/domain`
- Arquitetura e qualidade: `docs/architecture`
- Especificações por task: `docs/specs`

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
- `tools/rest-client/maintenance-requests.http`
- `tools/rest-client/demo-service-order-with-purchase-requests.http`

Configuração:
- Variáveis de execução ficam em `tools/rest-client/.env`.
- O fluxo já consome `API_HTTP_PORT`, `API_USERNAME` e `API_PASSWORD` via `{{$dotenv ...}}`.

Sequência sugerida:
1. Abra `maintenance-requests.http` e execute `POST /dev/database/reset` (`confirm: true`).
2. Abra `demo-service-order-with-purchase-requests.http`.
3. Execute as requests de cima para baixo.

Esse fluxo cobre:
- cadastro base
- abertura da OS
- diagnóstico com serviço adicional do mecânico
- separação com falta de estoque
- geração e conclusão de compra
- retomada da separação
- execução e entrega final

## Validação rápida
Health:

```bash
curl http://localhost:8080/health
```

## Testes automatizados
Somente E2E:

```bash
dotnet test --filter "FullyQualifiedName~E2E"
```

Suíte completa:

```bash
dotnet test
```

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
