# Task Template — Execução Assistida por IA

## Metadados
- Task ID: `task-XXX`
- Título: `<slug-da-feature>`
- Status: `Draft | Ready | In Progress | Done`
- Dono: `<nome>`
- Data de criação: `YYYY-MM-DD`
- Última atualização: `YYYY-MM-DD`

## 1) Objetivo
Descrever em uma frase o resultado funcional esperado.

## 2) Escopo
### In
- `<item 1>`
- `<item 2>`

### Out
- `<item fora de escopo 1>`
- `<item fora de escopo 2>`

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- `docs/domain/regras-de-negocio.md`
- `docs/domain/linguagem-ubiqua.md`
- `docs/domain/agregados.md` (fonte canônica de eventos de integração)
- `docs/architecture/architecture-overview.md`
- `docs/architecture/application-and-integrations.md`
- `docs/architecture/engineering-standards.md`

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-xxx` — `<regra obrigatória>`
- `RN-yyy` — `<regra obrigatória>`

## 5) Contratos e Interfaces
### 5.1 API pública (se aplicável)
- Endpoint: `<METHOD /route>`
- Request: `<campos obrigatórios/opcionais>`
- Response: `<campos>`
- Códigos HTTP esperados: `<200/201/400/404/409/...>`

Matriz de erro obrigatória por endpoint:
- Cenário: `<ex.: duplicidade de documento>`
- Origem do erro: `<ex.: DomainException com código estável>`
- HTTP esperado: `<ex.: 409>`
- Regra: proibido decidir status HTTP por parsing textual de mensagem.

### 5.2 Contratos internos
- Commands/Queries: `<nomes>`
- Eventos (se aplicável): referenciar apenas `docs/domain/agregados.md`.
- Repositórios/portas: `<interfaces>`

### 5.3 Erros de domínio
- Mensagens em português.
- Falhas de unicidade: traduzir para `DomainException`.

## 6) Plano Técnico por Camada
### Domain
- `<agregado/VO/exceções/invariantes>`

### Application
- `<commands/queries/handlers/orquestração>`

### Infrastructure
- `<EF mapping/repositórios/índices/migrations>`

### API
- `<endpoints/DTOs/validação/políticas>`

### Tests
- `<domínio/aplicação/integração>`

## 7) Arquivos a Criar/Alterar
Listar caminhos obrigatórios, agrupando por camada:
- `src/...`
- `tests/...`

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.
- Não é permitido criar estrutura alternativa de pastas sem atualização prévia da task.

## 8) Critérios de Pronto
- [ ] Build da solução sem erros.
- [ ] Testes previstos implementados e verdes.
- [ ] Regras RN aplicáveis cobertas.
- [ ] Contratos HTTP e erros aderentes à task.
- [ ] Sem violar dependências entre camadas.

## 9) Estratégia de Testes
### Domínio
- [ ] `<cenário>`

### Aplicação
- [ ] `<cenário>`

### Integração
- [ ] `<cenário>`

## 10) Riscos e Mitigações
- Risco: `<risco>`
  - Mitigação: `<ação>`

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Implementar por vertical slice (Domain -> Application -> Infrastructure -> API -> Tests).
- [ ] Garantir mensagens de erro em português.
- [ ] Traduzir violação de unicidade para `DomainException`.
- [ ] Não fazer parsing de texto de mensagem para decidir status HTTP.
- [ ] Validar contratos de entrada na borda (ex.: `page > 0`, `pageSize > 0` e limites máximos quando aplicável).
- [ ] Respeitar estritamente os caminhos de arquivo definidos na task.
- [ ] Atualizar documentação canônica se houver mudança de regra de negócio.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, desde que não conflite com o canônico.

## Guardrails Não-Negociáveis
- Proibido parsing de `ex.Message` para decidir semântica de transporte.
- Proibido retornar `404` para qualquer erro sem distinção da causa de domínio.
- Proibido usar strings inline para mensagens de erro.
- Proibido alterar escopo `Out` sem registrar justificativa e impacto.
