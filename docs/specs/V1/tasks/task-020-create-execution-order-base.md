# Task-020 — Create ExecutionOrder Base

## 0) Metadata
- `task_id`: `task-020`
- `slug`: `create-execution-order-base`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-019-create-separation-order-base.md](task-019-create-separation-order-base.md)

## 1) Objetivo
Implementar a base funcional da `ExecutionOrder` com máquina de estados própria, comandos de ciclo de vida e contratos HTTP, preparando o contexto para integração explícita com `SeparationOrder` em task posterior.

## 2) Escopo
### In
- Implementar agregado `ExecutionOrder` ponta a ponta (Domain/Application/Infrastructure/API/Tests).
- Implementar máquina de estados:
  - `Pending -> Ready -> InExecution -> Completed`
- Implementar comandos de transição:
  - marcar pronta para início;
  - iniciar execução;
  - concluir execução.
- Implementar registro de tempo real de execução:
  - `StartedAt`, `CompletedAt`, `ActualTimeMinutes`.
- Expor endpoints de criação, consulta e transições da execução.

### Out
- Integração automática por evento com `SeparationOrder`.
- Dupla confirmação de custódia (já tratada na separação).
- Bloqueio por autorização/perfil de usuário.
- Fechamento automático da `ServiceOrder` quando todas execuções concluírem.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/specs/V1/aggregates/execution-order.md](../aggregates/execution-order.md)
- [docs/specs/V1/aggregates/separation-order.md](../aggregates/separation-order.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Regras de Negócio Aplicáveis (RN-xxx)
- `RN-008` — `ExecutionOrder` é criada a partir do fluxo da OS aprovada.
- `RN-009` — execução só inicia quando estiver pronta para início.
- `RN-010` — execução registra início/fim e calcula tempo real.

Regras mandatórias desta task:
- `StartExecution()` só pode ocorrer com `Status == Ready`.
- `CompleteExecution()` só pode ocorrer com `Status == InExecution`.
- `ActualTimeMinutes` deve ser calculado automaticamente na conclusão.
- `MarkReadyToStart()` deve ser idempotente (não lançar erro em chamada repetida).

## 5) Contratos e Interfaces
### 5.1 API pública
- `POST /execution-orders`
- `GET /execution-orders/{id}`
- `GET /execution-orders`
- `POST /execution-orders/{id}/mark-ready`
- `POST /execution-orders/{id}/start`
- `POST /execution-orders/{id}/complete`

### 5.2 Matriz de erro obrigatória
- criação:
  - payload inválido -> `400`
  - `serviceOrderId`/`serviceId` inválidos -> `400`
- comandos de transição:
  - execução inexistente -> `404`
  - transição inválida de estado -> `409`
  - `mechanicId` ausente/inválido em `start` -> `400`

Regras mandatórias:
- proibido parsing de `ex.Message` para definir HTTP status.
- mapear por tipo/causa da exceção.

### 5.3 Contratos internos
- Commands:
  - `CreateExecutionOrderCommand`
  - `MarkExecutionOrderReadyCommand`
  - `StartExecutionOrderCommand`
  - `CompleteExecutionOrderCommand`
- Queries:
  - `GetExecutionOrderByIdQuery`
  - `ListExecutionOrdersQuery`
- Repositório:
  - `IExecutionOrderRepository`

### 5.4 Erros de domínio
- Mensagens em português via `DomainErrorMessages`.
- sem strings inline em handlers/endpoints.

## 6) Plano Técnico por Camada
### Domain
- Implementar `ExecutionOrder`.
- Implementar `ExecutionOrderStatus`.
- Implementar transições e validações de pré-condição.

### Application
- Implementar commands/queries/handlers do fluxo de execução.
- Implementar mapeamento para DTOs.

### Infrastructure
- EF Core mapping do agregado.
- repositório EF da execução.
- migration para tabela de execução.

### API
- Endpoints REST da execução.
- DTOs de request/response por operação.

### Tests
- Domínio: transições válidas/inválidas e cálculo de tempo.
- Aplicação: handlers de sucesso/falha.
- Integração: contratos HTTP e códigos de erro.

## 7) Arquivos a Criar/Alterar
### Criar (esperado)
- [src/GarageFlow.Domain/Executions/ExecutionOrder.cs](../../../../src/GarageFlow.Domain/Executions/ExecutionOrder.cs)
- [src/GarageFlow.Domain/Executions/ExecutionOrderStatus.cs](../../../../src/GarageFlow.Domain/Executions/ExecutionOrderStatus.cs)
- [src/GarageFlow.Domain/Executions/IExecutionOrderRepository.cs](../../../../src/GarageFlow.Domain/Executions/IExecutionOrderRepository.cs)
- [src/GarageFlow.Application/Executions/Commands/CreateExecutionOrderCommand.cs](../../../../src/GarageFlow.Application/Executions/Commands/CreateExecutionOrderCommand.cs)
- [src/GarageFlow.Application/Executions/Commands/MarkExecutionOrderReadyCommand.cs](../../../../src/GarageFlow.Application/Executions/Commands/MarkExecutionOrderReadyCommand.cs)
- [src/GarageFlow.Application/Executions/Commands/StartExecutionOrderCommand.cs](../../../../src/GarageFlow.Application/Executions/Commands/StartExecutionOrderCommand.cs)
- [src/GarageFlow.Application/Executions/Commands/CompleteExecutionOrderCommand.cs](../../../../src/GarageFlow.Application/Executions/Commands/CompleteExecutionOrderCommand.cs)
- [src/GarageFlow.Application/Executions/Queries/GetExecutionOrderByIdQuery.cs](../../../../src/GarageFlow.Application/Executions/Queries/GetExecutionOrderByIdQuery.cs)
- [src/GarageFlow.Application/Executions/Queries/ListExecutionOrdersQuery.cs](../../../../src/GarageFlow.Application/Executions/Queries/ListExecutionOrdersQuery.cs)
- `src/GarageFlow.Application/Executions/Handlers/*`
- `src/GarageFlow.Application/Executions/DTOs/*`
- [src/GarageFlow.Infrastructure/Persistence/Configurations/Executions/ExecutionOrderConfiguration.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Configurations/Executions/ExecutionOrderConfiguration.cs)
- [src/GarageFlow.Infrastructure/Persistence/Repositories/ExecutionOrderRepository.cs](../../../../src/GarageFlow.Infrastructure/Persistence/Repositories/ExecutionOrderRepository.cs)
- `src/GarageFlow.Api/DTOs/Executions/*`
- `src/GarageFlow.Api/Endpoints/Executions/ExecutionOrdersEndpoints.cs`
- [tests/GarageFlow.Tests/Domain/Executions/ExecutionOrderTests.cs](../../../../tests/GarageFlow.Tests/Domain/Executions/ExecutionOrderTests.cs)
- [tests/GarageFlow.Tests/Application/Executions/ExecutionOrderHandlersTests.cs](../../../../tests/GarageFlow.Tests/Application/Executions/ExecutionOrderHandlersTests.cs)
- [tests/GarageFlow.Tests/Integration/Executions/ExecutionOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/Executions/ExecutionOrdersEndpointsTests.cs)

### Alterar (esperado)
- [src/GarageFlow.Domain/Shared/DomainErrorMessages.cs](../../../../src/GarageFlow.Domain/Shared/DomainErrorMessages.cs)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs)
- [src/GarageFlow.Infrastructure/DependencyInjection.cs](../../../../src/GarageFlow.Infrastructure/DependencyInjection.cs)
- [src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs](../../../../src/GarageFlow.Infrastructure/Persistence/GarageFlowDbContext.cs)
- `src/GarageFlow.Infrastructure/Persistence/Migrations/*`
- `src/GarageFlow.Api/Program.cs`

Contrato de arquivos:
- Caminhos definidos nesta seção são mandatórios.
- Mudanças fora da lista devem ser justificadas explicitamente na resposta final.

## 8) Critérios de Pronto
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` sem regressão.
- [ ] Fluxo `Pending -> Ready -> InExecution -> Completed` validado.
- [ ] `MarkReadyToStart()` idempotente validado.
- [ ] Cálculo de `ActualTimeMinutes` validado com precisão decimal.
- [ ] Endpoints funcionando com matriz de erro aderente.
- [ ] Migration criada para o contexto de execução.
- [ ] Schema validado pelos testes de integração da execução.

## 9) Estratégia de Testes
### Domínio
- [ ] criar execução válida.
- [ ] bloquear criação sem `serviceOrderId`.
- [ ] bloquear criação sem `serviceId`.
- [ ] marcar pronta a partir de `Pending`.
- [ ] `mark-ready` repetido não deve quebrar (idempotência).
- [ ] iniciar execução em `Ready` com `mechanicId` válido.
- [ ] bloquear início sem `mechanicId`.
- [ ] bloquear início fora de `Ready`.
- [ ] concluir execução em `InExecution`.
- [ ] bloquear conclusão fora de `InExecution`.
- [ ] validar cálculo de `ActualTimeMinutes`.

### Aplicação
- [ ] handlers com sucesso e conflitos de estado.

### Integração
- [ ] contratos dos endpoints (`200/201/400/404/409`).

## 10) Riscos e Mitigações
- Risco: misturar regra de separação no agregado de execução.
  - Mitigação: manter `ExecutionOrder` isolada nesta task; integração vem depois.
- Risco: comportamento inconsistente em chamadas repetidas de readiness.
  - Mitigação: tornar `mark-ready` explicitamente idempotente e cobrir em testes.

## 11) Checklist de Execução para IA
- [ ] Confirmar leitura dos documentos canônicos.
- [ ] Não inventar regra fora do canônico.
- [ ] Implementar vertical slice completo.
- [ ] Garantir mensagens de erro em português via catálogo central.
- [ ] Não fazer parsing de mensagem para status HTTP.
- [ ] Respeitar caminhos de arquivo da task.

## Regras de Interpretação
- **Regra de negócio**: obrigatória, não negociável.
- **Decisão de implementação**: ajustável, sem conflito com canônico.

## Guardrails Não-Negociáveis
- Proibido integrar automaticamente por evento com `SeparationOrder` nesta task.
- Proibido strings inline de erro.
- Proibido mapping HTTP por parsing de texto.

## Assumptions
- A integração `SeparationOrder -> ExecutionOrder` será tratada na `task-021`.
