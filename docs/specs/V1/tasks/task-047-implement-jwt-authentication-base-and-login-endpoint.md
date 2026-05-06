# Task-047 — Implement JWT Authentication Base and Login Endpoint

## 0) Metadata
- `task_id`: `task-047`
- `slug`: `implement-jwt-authentication-base-and-login-endpoint`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-046-enforce-service-order-delivery-gate-and-extend-existing-e2e-flows.md`

## 1) Objetivo
Implementar a base de autenticação JWT do produto com endpoint de login funcional, emissão de token e claims mínimas para autorização por papel, com armazenamento seguro de senha (hash + salt).

## 2) Escopo
### In
- Criar endpoint `POST /auth/login`.
- Validar credenciais e emitir JWT.
- Definir claims mínimas (`sub`, `name`, `role`).
- Definir configuração canônica de JWT (`Issuer`, `Audience`, `Secret`, `Expiration`).
- Implementar armazenamento de senha com hash seguro (sem texto puro).
- Configurar integração do Swagger com JWT Bearer (`Authorize`) para teste de endpoints protegidos.

### Out
- Proteção de todos os endpoints por role (task seguinte).
- Migração completa de todos os testes para JWT real.
- Controle avançado de refresh token.

## 3) Contexto Canônico Obrigatório
- `docs/domain/linguagem-ubiqua.md`
- `docs/domain/regras-de-negocio.md`
- `docs/architecture/application-and-integrations.md`
- `docs/architecture/engineering-standards.md`
- `docs/architecture/testing-and-quality.md`

## 4) Regras de Negócio Aplicáveis
- Autenticação obrigatória para APIs administrativas.
- Papéis válidos para MVP: `Administrative`, `FrontDesk`, `Mechanic`, `Stockist`.
- Senhas nunca devem ser persistidas em texto puro.

## 5) Contratos e Interfaces
### 5.1 Endpoint
- `POST /auth/login`
- Request (MVP):
  - `username` (string)
  - `password` (string)
- Response:
  - `accessToken` (string)
  - `tokenType` (`Bearer`)
  - `expiresIn` (segundos)
  - `role` (string)

### 5.2 Matriz HTTP
- `200` login válido.
- `401` credenciais inválidas.
- `400` payload inválido.

## 6) Plano Técnico por Camada
### API
- Criar endpoint `/auth/login` com DTOs dedicados.
- Configurar `SwaggerGen` com `SecurityDefinition` para `Bearer JWT` e uso via botão `Authorize`.

### Application
- Criar caso de uso de autenticação (`LoginHandler`) com política MVP.

### Infrastructure
- Implementar gerador de token JWT.
- Carregar configuração de JWT via `Configuration`.
- Implementar hashing/verificação de senha **exclusivamente** com `Microsoft.AspNetCore.Identity.PasswordHasher<TUser>`.
- Não usar BCrypt, Argon2 ou libs alternativas nesta fase.

### Domain
- Sem novo agregado obrigatório nesta task.

## 7) Arquivos a Criar/Alterar
- `src/GarageFlow.Api/Endpoints/Auth/**`
- `src/GarageFlow.Api/DTOs/Auth/**`
- `src/GarageFlow.Application/Auth/**`
- `src/GarageFlow.Infrastructure/Auth/**`
- `src/GarageFlow.Api/Program.cs`
- `tests/GarageFlow.Tests/Integration/Auth/**`

## 8) Critérios de Pronto
- [ ] Login JWT funcional com emissão de token.
- [ ] Claims mínimas (`sub`, `name`, `role`) presentes no token.
- [ ] Senha persistida e validada via hash (sem plaintext).
- [ ] Matriz HTTP (`200/400/401`) validada.
- [ ] Swagger com botão `Authorize` aceitando token JWT Bearer para chamadas autenticadas.
- [ ] `dotnet build` verde.
- [ ] Testes de integração de login verdes.

## 9) Estratégia de Testes
- [ ] login válido retorna `200` e token.
- [ ] login inválido retorna `401`.
- [ ] payload inválido retorna `400`.
- [ ] validação de senha usa hash correto e rejeita senha incorreta.
- [ ] endpoint protegido pode ser chamado via Swagger após `Authorize` com token válido.

## 10) Riscos e Mitigações
- Risco: segredo JWT fraco em produção.
  - Mitigação: documentar segredo forte e variável obrigatória por ambiente.
- Risco: inconsistência de claims com políticas futuras.
  - Mitigação: padronizar desde já `role` e `sub`.
- Risco: adoção de múltiplas estratégias de hash no MVP.
  - Mitigação: padronizar oficialmente apenas `PasswordHasher<TUser>` nesta task.

## 11) Checklist de Execução para IA
- [ ] Implementar endpoint `/auth/login`.
- [ ] Garantir geração de JWT com claims mínimas.
- [ ] Validar contrato `200/400/401`.
- [ ] Configurar integração JWT no Swagger (`Authorize`).
- [ ] Criar testes de integração de login.
- [ ] Executar `dotnet build` e `dotnet test`.
