# GarageFlow

Backend em .NET para gestão de oficina, com arquitetura em camadas e documentação canônica em `docs/`.

## Documentação
- Domínio canônico: `docs/Domain`
- Arquitetura de referência: `docs/architecture`
- Trilhas evolutivas: `docs/specs`

## Pré-requisitos
- Docker + Docker Compose
- .NET SDK 10.0 (para execução local sem Docker e comandos EF)

## Execução com Docker
1. Criar arquivo de ambiente:
   - `cp .env.example .env`
   - editar `.env` com credenciais reais locais (nunca versionar)
2. Subir ambiente:
   - `docker compose up -d --build`
3. Verificar serviços:
   - `docker compose ps`
4. Acessar Swagger:
   - `http://localhost:8080/swagger`
   - `http://localhost:8080/swagger/v1/swagger.json`
5. Logs da API (se necessário):
   - `docker compose logs -f api`
6. Derrubar ambiente:
   - `docker compose down`

## Migrations manuais (quando aplicável)
Se precisar aplicar migrations manualmente, use:

```bash
dotnet ef database update \
  --project src/GarageFlow.Infrastructure/GarageFlow.Infrastructure.csproj \
  --startup-project src/GarageFlow.Api/GarageFlow.Api.csproj
```

Se não tiver o CLI do EF:

```bash
dotnet tool install --global dotnet-ef
```

## Automacao de banco no startup
- A API agora aplica migrations automaticamente ao iniciar.
- Se o schema estiver faltando (ex.: `relation "service_orders" does not exist`), basta reiniciar a API.
- Para desabilitar esse comportamento (ex.: ambiente de teste), use:

```bash
export Database__AutoMigrateOnStartup=false
```

## Operacoes de banco via API (somente Development)
Endpoints disponiveis no Swagger quando rodando em `Development`:
- `POST /dev/database/migrate` -> aplica migrations pendentes
- `POST /dev/database/clean` -> remove o banco (destrutivo)
- `POST /dev/database/reset` -> remove e recria o banco com migrations

Para operacoes destrutivas (`clean` e `reset`), envie:

```json
{ "confirm": true }
```

## Execução local sem Docker
1. Garantir PostgreSQL ativo e credenciais válidas.
2. Definir connection string:

```bash
export ConnectionStrings__GarageFlow="Host=localhost;Port=5432;Database=garageflow;Username=<seu_usuario>;Password=<sua_senha>"
```

3. Executar API:

```bash
dotnet run --project src/GarageFlow.Api/GarageFlow.Api.csproj
```

4. Acessar Swagger:
   - `http://localhost:5007/swagger`

## Build e testes
```bash
dotnet build
dotnet test
```

## Pipeline Manual de Qualidade e Segurança
Workflow:
- `.github/workflows/manual-quality-gate.yml`

Execução:
1. GitHub -> `Actions` -> `Manual Quality Gate`
2. `Run workflow`

Evidências geradas:
- dashboard executivo visual no `Summary` do workflow;
- relatório visual de testes no run do GitHub Actions;
- cobertura de testes (`coverage-summary.md` + `coverage-html.tar.gz`);
- vulnerabilidades de dependências (`security-report.json` + `security-report.md`);
- contagem de testes por tipo (`test-breakdown.json` + `test-breakdown.md`).
