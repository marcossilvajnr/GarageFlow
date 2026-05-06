# Task-050 — Finalize JWT Delivery Docs and Evidence Package

## 0) Metadata
- `task_id`: `task-050`
- `slug`: `finalize-jwt-delivery-docs-and-evidence-package`
- `owner`: `Domain Team`
- `status`: `Done`
- `depends_on`: `task-049-migrate-critical-e2e-flows-to-real-jwt-authentication.md`

## 1) Objetivo
Consolidar documentação e evidências finais da trilha JWT para entrega acadêmica, garantindo replicabilidade e rastreabilidade para banca.

## 2) Escopo
### In
- Atualizar `README` com passo a passo de login JWT e testes protegidos.
- Atualizar docs canônicas de arquitetura:
  - estratégia de auth;
  - políticas por papel;
  - execução E2E com JWT.
- Consolidar checklist final de evidências.

### Out
- Novas features funcionais.
- Mudanças estruturais de domínio.

## 3) Contexto Canônico Obrigatório
- `docs/architecture/application-and-integrations.md`
- `docs/architecture/engineering-standards.md`
- `docs/architecture/testing-and-quality.md`
- `docs/architecture/ci.md`
- `README.md`

## 4) Regras de Negócio Aplicáveis
- Segurança mínima para operações administrativas.
- Rastreabilidade de acesso por perfil.

## 5) Contratos e Interfaces
### 5.1 Documentação de contrato
- `POST /auth/login` com exemplos de request/response.
- Matriz de acesso por papel para endpoints críticos.

### 5.2 Evidências obrigatórias
- comandos de execução local;
- evidência de testes E2E com JWT real;
- evidência da pipeline manual.

## 6) Plano Técnico por Camada
### Docs
- Ajustar README para fluxo final de execução e validação JWT.
- Atualizar docs de arquitetura com seção de autenticação/autorização.

### Quality
- Rodar suíte de validação final e registrar números.

## 7) Arquivos a Criar/Alterar
- `README.md`
- `docs/architecture/application-and-integrations.md`
- `docs/architecture/engineering-standards.md`
- `docs/architecture/testing-and-quality.md`
- `docs/architecture/ci.md`
- `docs/specs/V1/tasks/**` (somente evidências finais da trilha)

## 8) Critérios de Pronto
- [x] README final orientado para banca com JWT incluído.
- [x] Docs canônicas alinhadas com implementação JWT.
- [x] Evidências de execução e testes registradas.
- [x] `dotnet test` verde.

## 9) Estratégia de Testes
- [x] Executar suíte completa e capturar números finais.
- [x] Confirmar E2E críticos com JWT real.

## 10) Riscos e Mitigações
- Risco: drift entre docs e código no fechamento.
  - Mitigação: revisão cruzada final doc <-> implementação.
- Risco: instruções confusas para banca.
  - Mitigação: fluxo único e objetivo no README.

## 11) Checklist de Execução para IA
- [x] Atualizar README com fluxo JWT.
- [x] Atualizar docs canônicas de auth/policies.
- [x] Consolidar evidências finais.
- [x] Executar `dotnet test`.

## 12) Evidência de Execução (Fechamento)
- Comando E2E:
  - `dotnet test --filter "FullyQualifiedName~E2E" --nologo`
  - resultado: `Passed: 3, Failed: 0`
- Comando suíte completa:
  - `dotnet test --nologo`
  - resultado: `Passed: 847, Failed: 0`
- Evidência funcional de auth:
  - `POST /auth/login` com credencial válida retorna `200` e token JWT.
  - endpoint protegido sem token retorna `401`.
  - endpoint protegido com token de role sem permissão retorna `403`.
