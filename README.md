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

## Migrations (quando aplicável)
A aplicação não cria schema automaticamente. Para aplicar migrations:

```bash
dotnet ef database update \
  --project src/GarageFlow.Infrastructure/GarageFlow.Infrastructure.csproj \
  --startup-project src/GarageFlow.Api/GarageFlow.Api.csproj
```

Se não tiver o CLI do EF:

```bash
dotnet tool install --global dotnet-ef
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
