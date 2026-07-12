# Task-056 — Add Operational Service Order Listing

## 0) Metadata
- `task_id`: `task-056`
- `slug`: `add-operational-service-order-listing`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-055-add-external-quote-decision-webhook.md`

## 1) Objetivo
Criar uma listagem operacional de OS aderente ao enunciado da Fase 2, com ordenação por prioridade de status e exclusão de ordens finalizadas, entregues e recusadas.

## 2) Escopo
### In
- Criar `GET /service-orders/operational`.
- Reutilizar `PagedServiceOrderResponse`.
- Incluir `InExecution`, `Approved`, `WaitingApproval`, `InDiagnostic`, `Received`.
- Excluir `Finished`, `Delivered`, `Rejected`.
- Ordenar por prioridade operacional e por data de criação ascendente dentro do mesmo status.
- Preservar `GET /service-orders` atual sem alteração.

### Out
- Remover ou alterar `GET /service-orders`.
- Criar filtros avançados de status nesta task.
- Criar tela/UI.
- Alterar máquina de estados da OS.
- Alterar semantics de `Approved` ou `Rejected`.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- `docs/domain/regras-de-negocio.md`
- `docs/domain/linguagem-ubiqua.md`
- `docs/domain/agregados.md`
- `docs/specs/V1/aggregates/service-order.md`
- `docs/specs/V1/tasks/task-011-create-service-order-base.md`
- `docs/specs/V1/tasks/task-017-create-quote-from-consolidated-services.md`
- `docs/specs/V1/tasks/task-037-canonical-state-machine-conformance-gate-pre-jwt-e2e.md`
- `docs/specs/V1/tasks/task-046-enforce-service-order-delivery-gate-and-extend-existing-e2e-flows.md`
- `docs/architecture/engineering-standards.md`
- `docs/architecture/testing-and-quality.md`

## 4) Decisões Arquiteturais Já Tomadas
- Criar rota nova para evitar mudança de comportamento da listagem atual.
- A rota operacional inclui `Approved`.
- A rota operacional exclui `Rejected`, além de `Finished` e `Delivered`.
- A ordem operacional definida é:
  1. `InExecution`
  2. `Approved`
  3. `WaitingApproval`
  4. `InDiagnostic`
  5. `Received`
- Dentro do mesmo status, ordenar por `CreatedAt` ascendente.

## 5) Regras de Negócio Aplicáveis
- OS em execução tem maior prioridade operacional.
- OS aprovada deve aparecer como próxima fila de execução.
- OS aguardando aprovação, em diagnóstico e recebida permanecem visíveis.
- OS finalizada, entregue ou recusada não pertence à listagem operacional principal.
- Paginação deve ser aplicada sobre o conjunto já filtrado e ordenado.

## 6) Contratos e Interfaces
### 6.1 API pública
- Endpoint: `GET /service-orders/operational?page=1&pageSize=10`
- Query:
  - `page: int`
  - `pageSize: int`
- Response:
  - `PagedServiceOrderResponse`

### 6.2 Matriz HTTP obrigatória
- Query válida -> `200`
- `page <= 0` -> `400`
- `pageSize <= 0` -> `400`
- `pageSize` acima do limite atual -> `400`
- Sem token -> `401`
- Token sem perfil autorizado -> `403`

### 6.3 Contratos internos
- Criar query/handler:
  - `ListOperationalServiceOrdersQuery`
  - `ListOperationalServiceOrdersHandler`
- Criar método de repositório:
  - `ListOperationalAsync(page, pageSize, cancellationToken)`
- Reusar:
  - `PagedServiceOrderResult`
  - `ServiceOrderDto`
  - mapper atual de OS.

### 6.4 Regra de ordenação obrigatória
- Prioridade:
  - `InExecution` = 1
  - `Approved` = 2
  - `WaitingApproval` = 3
  - `InDiagnostic` = 4
  - `Received` = 5
- Segundo critério:
  - `CreatedAt` ascendente.
- Excluídos:
  - `Finished`
  - `Delivered`
  - `Rejected`

## 7) Plano Técnico por Camada
### Domain
- Sem alteração esperada.
- Não alterar enum de status.

### Application
- Criar query/handler específico para listagem operacional.
- Manter handler atual de listagem sem mudança de comportamento.

### Infrastructure
- Implementar filtro e ordenação no repositório EF.
- Garantir `TotalCount` calculado após filtro operacional.
- Garantir `Skip/Take` após ordenação operacional.

### API
- Adicionar rota `GET /service-orders/operational`.
- Reusar validação de paginação atual.
- Reusar `PagedServiceOrderResponse`.
- Garantir que a rota fixa não conflite com `/{id:guid}`.

### Tests
- Cobrir filtro, ordenação e paginação.
- Garantir que `GET /service-orders` continue com comportamento atual.

### Docs
- Atualizar docs de API/README apenas se houver seção de listagem de OS.
- Registrar justificativa para incluir `Approved` e excluir `Rejected`.

## 8) Arquivos a Criar/Alterar
- `src/GarageFlow.Api/ServiceOrders/Endpoints/ServiceOrdersEndpoints.cs`
- `src/GarageFlow.Application/ServiceOrders/Queries/ListOperationalServiceOrdersQuery.cs`
- `src/GarageFlow.Application/ServiceOrders/Handlers/ListOperationalServiceOrdersHandler.cs`
- `src/GarageFlow.Application/DependencyInjection.cs`
- `src/GarageFlow.Domain/ServiceOrders/IServiceOrderRepository.cs`
- `src/GarageFlow.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs`
- `tests/GarageFlow.Tests/Application/ServiceOrders/ServiceOrderHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs`
- `docs/specs/V1/aggregates/service-order.md` (somente se necessário registrar a visão operacional)

Contrato de arquivos:
- Mudanças fora desta lista devem ser justificadas explicitamente na resposta final.
- Não modificar `GET /service-orders` sem atualizar esta task.

## 9) Critérios de Pronto
- [ ] `GET /service-orders/operational` existe.
- [ ] Retorna `InExecution`, `Approved`, `WaitingApproval`, `InDiagnostic`, `Received`.
- [ ] Não retorna `Finished`, `Delivered`, `Rejected`.
- [ ] Ordena por prioridade operacional.
- [ ] Ordena mais antigas primeiro dentro do mesmo status.
- [ ] Paginação usa total filtrado.
- [ ] `GET /service-orders` continua sem mudança de comportamento.
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` verde.

## 10) Estratégia de Testes
### Domínio
- [ ] Sem novos testes obrigatórios.

### Aplicação
- [ ] handler retorna somente status operacionais;
- [ ] handler inclui `Approved`;
- [ ] handler exclui `Rejected`, `Finished`, `Delivered`;
- [ ] handler ordena por prioridade;
- [ ] handler ordena por `CreatedAt` ascendente dentro do mesmo status.

### Integração
- [ ] `GET /service-orders/operational` retorna `200`;
- [ ] resposta não contém finalizadas, entregues ou recusadas;
- [ ] resposta contém aprovadas;
- [ ] paginação válida funciona;
- [ ] paginação inválida retorna `400`;
- [ ] `GET /service-orders` atual continua verde nos testes existentes.

### E2E
- [ ] Não criar novo E2E obrigatório nesta task.

## 11) Riscos e Mitigações
- Risco: alterar a listagem atual e quebrar clientes existentes.
  - Mitigação: nova rota operacional separada.
- Risco: `Approved` ficar sem posição clara na ordenação.
  - Mitigação: task fixa `Approved` logo após `InExecution`.
- Risco: total de paginação contar itens excluídos.
  - Mitigação: calcular `TotalCount` após filtro operacional.
- Risco: ordenação em memória com grande volume.
  - Mitigação: implementar ordenação traduzível em EF quando possível.

## 12) Checklist de Execução para IA
- [ ] Ler documentos canônicos.
- [ ] Confirmar comportamento atual de `GET /service-orders`.
- [ ] Criar handler/query operacional separado.
- [ ] Adicionar método de repositório específico.
- [ ] Implementar filtro antes da contagem.
- [ ] Implementar ordenação antes da paginação.
- [ ] Não alterar endpoint atual de listagem.
- [ ] Testar inclusão de `Approved`.
- [ ] Testar exclusão de `Rejected`.
- [ ] Rodar `dotnet build`.
- [ ] Rodar `dotnet test`.
- [ ] Reportar evidências e ordem validada.

## 13) Evidência Esperada de Fechamento
- Comando `dotnet build` com resultado.
- Comando `dotnet test` com contagem de testes.
- Evidência de ordenação operacional.
- Evidência de exclusão de `Finished`, `Delivered`, `Rejected`.
- Evidência de inclusão de `Approved`.
