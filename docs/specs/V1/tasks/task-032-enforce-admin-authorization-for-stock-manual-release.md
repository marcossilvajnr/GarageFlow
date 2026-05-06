# Task-032 — Enforce Admin Authorization for Stock Manual Release

## 0) Metadata
- `task_id`: `task-032`
- `slug`: `enforce-admin-authorization-for-stock-manual-release`
- `owner`: `Domain Team`
- `status`: `Blocked (JWT/Auth Pending)`
- `depends_on`: `task-031-add-stock-manual-adjustment-audit-trail.md`, `task-auth-jwt-foundation (to be created)`

## 1) Objetivo
Aplicar autorização por perfil para garantir que apenas `Administrative` execute ajuste manual em `/stock/releases`.

## 2) Escopo
### In
- Proteger `POST /stock/releases` com política/role apropriada.
- Garantir retorno `401` e `403` com autenticação/autorização ativa.
- Manter regras de validação e conflitos já existentes.

### Out
- Implementar foundation de identidade/JWT nesta task.
- Alterar regras de separação/devolução operacional.

## 3) Contexto Canônico Obrigatório
- [docs/Domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/Domain/regras-de-negocio.md)
- [docs/Domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/Domain/agregados.md)
- [docs/specs/V1/aggregates/stock.md](/Users/marcos/Projects/GarageFlow/docs/specs/V1/aggregates/stock.md)
- [docs/architecture/application-and-integrations.md](/Users/marcos/Projects/GarageFlow/docs/architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-033` — ajuste manual permitido ao Administrativo, com justificativa.

## 5) Contratos e Interfaces
### 5.1 API pública
- Endpoint alvo:
  - `POST /stock/releases`

### 5.2 Matriz de erro obrigatória
- `401` para não autenticado.
- `403` para autenticado sem papel `Administrative`.
- `400/404/409` preservados para validação/domínio.

## 6) Plano Técnico por Camada
### Domain
- Sem mudança de regra de domínio.

### Application
- Sem alteração semântica de use case.

### Infrastructure/API
- Configurar política de autorização e aplicar no endpoint.
- Garantir claims/role mapping consistente com padrão já adotado no projeto.

### Tests
- Integração cobrindo `401`, `403` e `200` conforme papel.

## 7) Arquivos a Criar/Alterar
### Alterar (esperado)
- `src/GarageFlow.Api/Endpoints/Stock/StockEndpoints.cs`
- `src/GarageFlow.Api/Program.cs` (ou wiring equivalente de authz)
- `tests/GarageFlow.Tests/Integration/Stock/StockEndpointsTests.cs`

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] endpoint manual de release protegido por perfil `Administrative`.
- [ ] testes comprovam `401/403/200`.

## 9) Estratégia de Testes
- [ ] release manual sem token retorna `401`.
- [ ] release manual com papel sem permissão retorna `403`.
- [ ] release manual com papel administrativo retorna sucesso conforme regras de domínio.

## 10) Riscos e Mitigações
- Risco: brecha de autorização por endpoint sem policy.
  - Mitigação: testes de integração específicos por perfil.

## 11) Checklist de Execução para IA
- [ ] Confirmar JWT/Auth ativo antes de iniciar.
- [ ] Não quebrar contratos de erro existentes.
- [ ] Validar `401/403/200` em integração.
