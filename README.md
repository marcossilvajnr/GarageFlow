# GarageFlow

Backend .NET para gestão de oficina mecânica, com foco em Ordem de Serviço, estoque e execução de ponta a ponta.

## Documentação
- Domínio canônico: `docs/Domain`
- Arquitetura (operação, qualidade e testes): `docs/architecture`
- Histórico evolutivo por tasks: `docs/specs`

## Pré-requisitos
- Docker Desktop instalado e em execução

## Passo a Passo Rápido (Professor/Banca)
1. Entre na pasta do projeto.
2. Configure as variáveis de ambiente:

Um arquivo `.env` com as credenciais do ambiente foi entregue junto com o projeto. Coloque-o na raiz do repositório (mesma pasta deste README) antes de subir o ambiente.

O arquivo `.env.example` no repositório serve apenas como referência da estrutura esperada.

3. Suba o ambiente:

```bash
docker compose up -d --build
```

4. Verifique se tudo subiu:

```bash
docker compose ps
```

5. Abra a API:
- Swagger UI: `http://localhost:8080/swagger`
- Swagger JSON: `http://localhost:8080/swagger/v1/swagger.json`

6. Teste autenticação JWT no Swagger:
- Faça `POST /auth/login` com um usuário de desenvolvimento, por exemplo:
  - Body JSON:

```json
{
  "username": "admin",
  "password": "admin123"
}
```
- Copie o valor de `accessToken` da resposta.
- Clique em `Authorize` no Swagger e cole:
  - `{seu_accessToken}`
- Chame um endpoint protegido de listagem sem ID (ex.: `GET /employees?page=1&pageSize=10`).

Exemplo de resposta esperada do `POST /auth/login`:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "role": "Administrative"
}
```

Matriz rápida de acesso por papel (endpoints críticos):
- `POST /service-orders` e `GET /employees`:
  - `FrontDesk`, `Administrative`
- `POST /service-orders/{id}/diagnostic/start`:
  - `Mechanic`, `Administrative`
- `POST /separation-orders/{id}/confirm-stockist-withdrawal`:
  - `Stockist`, `Administrative`
- `POST /stock/releases`:
  - apenas `Administrative`

Exemplo equivalente via `curl`:

```bash
# 1) Login e captura do token JWT
TOKEN=$(curl -s -X POST 'http://localhost:8080/auth/login' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "username": "admin",
    "password": "admin123"
  }' | jq -r '.accessToken')

# 2) Chamada autenticada no endpoint protegido de listagem
curl -X GET 'http://localhost:8080/employees?page=1&pageSize=10' \
  -H "accept: application/json" \
  -H "Authorization: Bearer $TOKEN"
```

Observação:
- A `ConnectionStrings__GarageFlow` é montada automaticamente pelo `docker-compose.yml` a partir das variáveis do `.env` — nenhuma configuração manual adicional é necessária.

## Validar que Funcionou
Teste de saúde da API:

```bash
curl http://localhost:8080/health
```

Rodar testes unitários (Domain + Application):

```bash
dotnet test --filter "FullyQualifiedName~Domain|FullyQualifiedName~Application"
```

Rodar testes de integração:

```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

Rodar testes E2E:

```bash
dotnet test --filter "FullyQualifiedName~E2E"
```

Rodar suíte completa:

```bash
dotnet test
```

Checklist mínimo de evidência para banca:
- `POST /auth/login` retornando `200` com token JWT.
- Chamada protegida sem token retornando `401`.
- Chamada protegida com role sem permissão retornando `403`.
- Fluxos E2E críticos passando com JWT real:
  - `dotnet test --filter "FullyQualifiedName~E2E"`
- Suíte completa verde:
  - `dotnet test`

## Comandos Úteis
Logs da API:

```bash
docker compose logs -f api
```

Parar ambiente:

```bash
docker compose down
```

Reset total (remove containers, rede e volume do banco):

```bash
docker compose down -v
docker compose up -d --build
```

## Troubleshooting Rápido
Porta `8080` ou `5432` ocupada:
- pare serviços locais que estejam usando essas portas;
- rode novamente `docker compose up -d --build`.

API não sobe após build:
- verificar logs com `docker compose logs -f api`.

Banco inconsistente:
- executar reset total (`docker compose down -v` + `docker compose up -d --build`).

## Observação Acadêmica
As credenciais acima são intencionalmente simples para facilitar reprodução da banca em ambiente local.
