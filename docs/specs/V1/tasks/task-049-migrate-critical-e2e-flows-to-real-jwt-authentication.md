# Task-049 — Migrate Critical E2E Flows to Real JWT Authentication

## 0) Metadata
- `task_id`: `task-049`
- `slug`: `migrate-critical-e2e-flows-to-real-jwt-authentication`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-048-apply-role-based-authorization-policies-to-critical-endpoints.md](task-048-apply-role-based-authorization-policies-to-critical-endpoints.md)

## 1) Objetivo
Migrar os fluxos E2E críticos para autenticação JWT real, removendo dependência de autenticação fake no caminho principal de validação ponta a ponta.

## 2) Escopo
### In
- E2E deve obter token real via `POST /auth/login`.
- E2E deve chamar endpoints protegidos com `Bearer token`.
- Cobrir os 3 fluxos críticos já existentes com autenticação real:
  - estoque suficiente;
  - ruptura com compra;
  - cancelamento no último estágio permitido.

### Out
- Novos cenários E2E não críticos.
- Testes de refresh token.

## 3) Contexto Canônico Obrigatório
- [docs/specs/V1/tasks/task-039-document-e2e-critical-flow-coverage.md](task-039-document-e2e-critical-flow-coverage.md)
- [docs/architecture/testing-and-quality.md](../../../architecture/testing-and-quality.md)
- [docs/architecture/ci.md](../../../architecture/ci.md)
- [docs/specs/V1/tasks/task-047-implement-jwt-authentication-base-and-login-endpoint.md](task-047-implement-jwt-authentication-base-and-login-endpoint.md)

## 4) Regras de Negócio Aplicáveis
- Acesso a operações deve respeitar papel operacional do ator.

## 5) Contratos e Interfaces
### 5.1 Login
- `POST /auth/login` para obtenção de token.
- credenciais devem ser validadas contra senha com hash (sem plaintext).

### 5.2 Proteção
- Endpoints críticos devem responder `401/403` conforme ausência/perfil.

## 6) Plano Técnico por Camada
### Tests (principal)
- Atualizar infraestrutura E2E para solicitar tokens por papel.
- Remover/neutralizar dependência de `TestAuthHandler` no caminho E2E principal.
- Ajustar builders/utilitários E2E para header `Authorization`.

### API/Application/Infrastructure
- Ajustes mínimos apenas se necessário para suportar E2E real JWT.

## 7) Arquivos a Criar/Alterar
- `tests/GarageFlow.Tests/E2E/Infrastructure/**`
- `tests/GarageFlow.Tests/E2E/ServiceOrders/**`
- `tests/GarageFlow.Tests/E2E/Builders/**`

## 8) Critérios de Pronto
- [ ] Fluxos E2E críticos executando com login/token real.
- [ ] Sem uso de auth fake no caminho principal desses E2E.
- [ ] Cobertura de falha por ausência/erro de token em ao menos um cenário.
- [ ] `dotnet test --filter "FullyQualifiedName~E2E"` verde.

## 9) Estratégia de Testes
- [ ] Login por papel e uso de token em chamadas protegidas.
- [ ] Assert de `401/403` em chamadas sem/perfil incorreto.
- [ ] Fluxos críticos completos mantidos verdes.

## 10) Riscos e Mitigações
- Risco: instabilidade por expiração de token.
  - Mitigação: token de teste com expiração suficiente e relógio consistente.
- Risco: acoplamento excessivo de E2E com detalhes de auth.
  - Mitigação: centralizar login/token em fixture/helper.

## 11) Checklist de Execução para IA
- [ ] Implementar login real em fixture E2E.
- [ ] Aplicar token por papel nos cenários críticos.
- [ ] Remover dependência de auth fake no caminho E2E principal.
- [ ] Executar E2E completo e registrar evidência.
