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
2. Suba o ambiente (sem configuração prévia):

```bash
docker compose up -d --build
```

3. Verifique se tudo subiu:

```bash
docker compose ps
```

4. Abra a API:
- Swagger UI: `http://localhost:8080/swagger`
- Swagger JSON: `http://localhost:8080/swagger/v1/swagger.json`

5. Teste autenticação JWT no Swagger:
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
- No fluxo Docker, a `ConnectionStrings__GarageFlow` é montada automaticamente pelo `docker-compose.yml` com `Host=postgres`.
- Não é necessário exportar `ConnectionStrings__GarageFlow` no shell para rodar com Docker.

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
