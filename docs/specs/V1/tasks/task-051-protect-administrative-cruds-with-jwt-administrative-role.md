# Task-051 — Protect Administrative CRUD APIs with JWT Role `Administrative`

## 0) Metadata
- `task_id`: `task-051`
- `slug`: `protect-administrative-cruds-with-jwt-administrative-role`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-050-finalize-jwt-delivery-docs-and-evidence-package.md`

## 1) Objetivo
Restringir os CRUDs administrativos para acesso apenas de usuários autenticados com role `Administrative`, mantendo o restante dos fluxos operacionais sem mudanças de comportamento.

## 2) Escopo
### In
- Aplicar `RequireAuthorization("Administrative")` nos endpoints de CRUD administrativo.
- Garantir respostas HTTP corretas:
  - `401 Unauthorized` para requisições sem JWT válido.
  - `403 Forbidden` para JWT válido sem role `Administrative`.
- Atualizar testes de integração/e2e afetados por autenticação/autorização.
- Atualizar documentação de operação/arquitetura sobre o escopo de proteção administrativa.

### Out
- Vínculo estrutural `AuthUser <-> Employee`.
- Resolver ator operacional por token para fluxos de OS/diagnóstico/separação/execução/compra.
- Inclusão de novos campos de autoria nos agregados.
- Alterações de contratos de request para remover `ActorId/MechanicId/StockistId`.

## 3) Contexto Canônico Obrigatório
- `docs/domain/agregados.md`
- `docs/domain/regras-de-negocio.md`
- `docs/architecture/engineering-standards.md`
- `docs/architecture/operations-and-quality.md`

## 4) Regras de Negócio Aplicáveis
- Governança de acesso administrativo via JWT + role.
- Não alterar regras de domínio operacionais existentes.

## 5) Contratos e Interfaces
### 5.1 Políticas
- Reutilizar policy já existente `Administrative` no `Program.cs`.

### 5.2 Endpoints-alvo
- CRUDs administrativos de cadastro/base (ex.: funcionários, fornecedores, serviços, peças, insumos, clientes, veículos) conforme contrato atual da API.
- Se houver dúvida em endpoint híbrido, priorizar não quebrar fluxo operacional e registrar decisão na resposta final.

### 5.3 Matriz mínima de erro
- Sem token: `401`.
- Token com role não administrativa: `403`.
- Token administrativo: acesso normal ao endpoint.

## 6) Plano Técnico por Camada
### API
- Ajustar endpoints com `RequireAuthorization("Administrative")` no grupo/rota apropriada.

### Application/Domain
- Sem alterações funcionais obrigatórias para esta task.

### Tests
- Ajustar/Adicionar testes de integração para validar `401/403/2xx` por role.

## 7) Arquivos a Criar/Alterar
### Alterar (mínimo esperado)
- `src/GarageFlow.Api/Endpoints/**`
- `tests/GarageFlow.Tests/Integration/**`
- `tests/GarageFlow.Tests/E2E/**` (se aplicável)
- `docs/architecture/operations-and-quality.md` (se necessário)

## 8) Critérios de Pronto
- [ ] Todos os CRUDs administrativos relevantes protegidos por `Administrative`.
- [ ] Testes cobrindo cenários `401`, `403` e sucesso com admin.
- [ ] Sem regressão nos fluxos operacionais fora do escopo.
- [ ] Build e testes verdes.

## 9) Riscos e Mitigações
- Risco: bloquear endpoints operacionais por engano.
  - Mitigação: aplicar proteção apenas em rotas de CRUD administrativo.
- Risco: quebra de testes existentes por mudança de auth.
  - Mitigação: ajustar fixtures/login por role nos testes.

## 10) Débito Técnico Registrado
Fica explicitamente fora desta task (backlog):
- identidade operacional completa (`AuthUser` vinculado a `Employee`);
- rastreabilidade de autoria ponta a ponta nos fluxos de operação.
