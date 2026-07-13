# Task-048 — Apply Role-Based Authorization Policies to Critical Endpoints

## 0) Metadata
- `task_id`: `task-048`
- `slug`: `apply-role-based-authorization-policies-to-critical-endpoints`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-047-implement-jwt-authentication-base-and-login-endpoint.md](task-047-implement-jwt-authentication-base-and-login-endpoint.md)

## 1) Objetivo
Aplicar autorização por papéis nos endpoints críticos, alinhando operações às responsabilidades de negócio e reduzindo risco de acesso indevido.

## 2) Escopo
### In
- Definir políticas de autorização por papel.
- Proteger endpoints críticos por contexto:
  - `Stock` (administrativo/estoquista)
  - `ServiceOrder` (atendimento/mecânico conforme ação)
  - `PurchaseOrder` e `SeparationOrder` (estoque/compras)
  - `ExecutionOrder` (mecânico)
- Manter endpoints públicos essenciais de observabilidade em `Development`.

### Out
- Fluxo de refresh token.
- Controle de permissões por recurso (ABAC).

## 3) Contexto Canônico Obrigatório
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/bounded-contexts.md](../../../domain/bounded-contexts.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis
- Operações administrativas só por perfil autorizado.
- Separação de responsabilidades por papel operacional.

## 5) Contratos e Interfaces
### 5.1 Política esperada (MVP)
- `Administrative`
- `FrontDesk`
- `Mechanic`
- `Stockist`

### 5.2 Matriz HTTP
- `401` sem token.
- `403` token válido sem permissão.
- `200/204` quando autorizado.

## 6) Plano Técnico por Camada
### API
- Aplicar `RequireAuthorization` por grupo/endpoint conforme papel.

### Application
- Sem mudança estrutural obrigatória.

### Tests
- Adicionar integração para `401/403` em endpoints críticos.

## 7) Arquivos a Criar/Alterar
- `src/GarageFlow.Api/Program.cs`
- `src/GarageFlow.Api/Endpoints/**`
- `tests/GarageFlow.Tests/Integration/**`

## 8) Critérios de Pronto
- [ ] Endpoints críticos protegidos por política adequada.
- [ ] Respostas `401/403` padronizadas para acesso inválido.
- [ ] Sem regressão nos fluxos autorizados.
- [ ] `dotnet build` verde.
- [ ] `dotnet test` verde.

## 9) Estratégia de Testes
- [ ] Sem token -> `401`.
- [ ] Papel incorreto -> `403`.
- [ ] Papel correto -> sucesso.

## 10) Riscos e Mitigações
- Risco: sobreproteger endpoint necessário para fluxo.
  - Mitigação: validar matriz por contexto com regras canônicas.
- Risco: quebra em testes existentes com `TestAuth`.
  - Mitigação: ajustar helpers de teste por role.

## 11) Checklist de Execução para IA
- [ ] Definir e aplicar políticas por endpoint crítico.
- [ ] Cobrir `401/403` em testes de integração.
- [ ] Validar fluxos felizes autorizados.
- [ ] Executar `dotnet build` e `dotnet test`.
