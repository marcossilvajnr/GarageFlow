# Task-054 — Add Service Order Status Read Model Endpoint

## 0) Metadata
- `task_id`: `task-054`
- `slug`: `add-service-order-status-read-model-endpoint`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-053-extend-service-order-opening-with-initial-services.md](task-053-extend-service-order-opening-with-initial-services.md)

## 1) Objetivo
Criar uma consulta dedicada de status da OS que retorne o status interno e uma label em português aderente ao enunciado da Fase 2.

## 2) Escopo
### In
- Criar `GET /service-orders/{id}/status`.
- Retornar status interno da Application e label pública em português.
- Criar mapper explícito de status para label.
- Preservar status extras do domínio (`Approved`, `Rejected`) no contrato.
- Cobrir todos os status conhecidos em testes.

### Out
- Remover ou renomear status existentes.
- Alterar `GET /service-orders/{id}`.
- Traduzir enum interno para strings diferentes no restante da API.
- Criar persistência ou read model materializado.
- Ocultar `Approved` ou `Rejected` do contrato público.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/domain/linguagem-ubiqua.md](../../../domain/linguagem-ubiqua.md)
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/specs/V1/aggregates/service-order.md](../aggregates/service-order.md)
- [docs/specs/V1/tasks/task-037-canonical-state-machine-conformance-gate-pre-jwt-e2e.md](task-037-canonical-state-machine-conformance-gate-pre-jwt-e2e.md)
- [docs/specs/V1/tasks/task-046-enforce-service-order-delivery-gate-and-extend-existing-e2e-flows.md](task-046-enforce-service-order-delivery-gate-and-extend-existing-e2e-flows.md)
- [docs/architecture/architecture-overview.md](../../../architecture/architecture-overview.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)

## 4) Decisões Arquiteturais Já Tomadas
- A consulta dedicada será adicionada além do `GET /service-orders/{id}`.
- O contrato deve expor status interno e label pública.
- Labels em português atendem leitura da banca sem enfraquecer o domínio.
- `Approved` e `Rejected` continuam existindo e devem ser mapeados.
- A API deve usar enum da Application, não Domain.

## 5) Regras de Negócio Aplicáveis
- O status retornado deve refletir o estado atual persistido da OS.
- Todos os status da máquina atual devem ter label pública.
- Status desconhecido não deve cair silenciosamente em label genérica.
- A consulta não altera estado.

## 6) Contratos e Interfaces
### 6.1 API pública
- Endpoint: `GET /service-orders/{id}/status`
- Request:
  - route `id: Guid`
- Response sugerido:
  - `serviceOrderId: Guid`
  - `status: ServiceOrderStatus`
  - `label: string`
  - `updatedAt: DateTime?`

### 6.2 Labels obrigatórias
- `Received` -> `Recebida`
- `InDiagnostic` -> `Diagnóstico`
- `WaitingApproval` -> `Aguardando Aprovação`
- `Approved` -> `Orçamento aprovado`
- `Rejected` -> `Orçamento recusado`
- `InExecution` -> `Execução`
- `Finished` -> `Finalizada`
- `Delivered` -> `Entregue`

### 6.3 Matriz HTTP obrigatória
- OS existente -> `200`
- OS inexistente -> `404`
- `id` inválido por rota não-Guid -> comportamento padrão do ASP.NET
- Sem token -> `401`
- Token sem perfil autorizado -> `403`

### 6.4 Contratos internos
- Criar query/handler se necessário:
  - `GetServiceOrderStatusQuery`
  - `GetServiceOrderStatusHandler`
- Criar DTO de Application se necessário:
  - `ServiceOrderStatusDto`
- Criar response de API:
  - `ServiceOrderStatusResponse`
- Criar mapper de label em API ou Application, mantendo API sem Domain.

## 7) Plano Técnico por Camada
### Domain
- Sem alteração esperada.

### Application
- Preferir handler dedicado para retornar apenas dados de status.
- Reusar `IServiceOrderRepository.GetByIdAsync`.
- Garantir que o DTO use `GarageFlow.Application.ServiceOrders.Enums.ServiceOrderStatus`.

### Infrastructure
- Sem migration esperada.
- Se otimizar query, não quebrar contrato do repositório atual sem justificativa.

### API
- Adicionar rota antes de rotas que possam conflitar.
- Adicionar DTO de response e mapper de label.
- Manter authorization compatível com consulta de OS.

### Tests
- Testar mapper de todos os status.
- Testar contrato HTTP de OS existente e inexistente.

### Docs
- Atualizar documentação de API se houver seção de contratos de OS.

## 8) Arquivos a Criar/Alterar
- [src/GarageFlow.Api/ServiceOrders/Endpoints/ServiceOrdersEndpoints.cs](../../../../src/GarageFlow.Api/ServiceOrders/Endpoints/ServiceOrdersEndpoints.cs)
- [src/GarageFlow.Api/ServiceOrders/DTOs/ServiceOrderStatusResponse.cs](../../../../src/GarageFlow.Api/ServiceOrders/DTOs/ServiceOrderStatusResponse.cs)
- `src/GarageFlow.Api/ServiceOrders/Mappers/ServiceOrderStatusLabelMapper.cs` (ou local equivalente com justificativa)
- [src/GarageFlow.Application/ServiceOrders/Queries/GetServiceOrderStatusQuery.cs](../../../../src/GarageFlow.Application/ServiceOrders/Queries/GetServiceOrderStatusQuery.cs) (se handler dedicado for criado)
- [src/GarageFlow.Application/ServiceOrders/Handlers/GetServiceOrderStatusHandler.cs](../../../../src/GarageFlow.Application/ServiceOrders/Handlers/GetServiceOrderStatusHandler.cs) (se handler dedicado for criado)
- [src/GarageFlow.Application/DependencyInjection.cs](../../../../src/GarageFlow.Application/DependencyInjection.cs)
- `tests/GarageFlow.Tests/Application/ServiceOrders/**`
- [tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs](../../../../tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs)

Contrato de arquivos:
- Mudanças fora desta lista devem ser justificadas explicitamente na resposta final.
- Não criar mapper em Infrastructure.

## 9) Critérios de Pronto
- [ ] `GET /service-orders/{id}/status` retorna `200` com status e label.
- [ ] Todos os status têm label testada.
- [ ] `Approved` e `Rejected` aparecem corretamente quando a OS estiver nesses estados.
- [ ] OS inexistente retorna `404`.
- [ ] Endpoint respeita autenticação/autorização.
- [ ] `GET /service-orders/{id}` permanece inalterado.
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` verde.
- [ ] API não importa Domain nem Infrastructure.

## 10) Estratégia de Testes
### Domínio
- [ ] Sem novos testes obrigatórios, salvo alteração de status.

### Aplicação
- [ ] handler retorna status correto para OS existente;
- [ ] handler falha para OS inexistente;
- [ ] `Approved` e `Rejected` são preservados.

### Integração
- [ ] `GET /service-orders/{id}/status` retorna `200`;
- [ ] response contém `serviceOrderId`, `status`, `label`, `updatedAt`;
- [ ] OS inexistente retorna `404`;
- [ ] mapper cobre todos os status públicos.

### E2E
- [ ] Não criar novo E2E obrigatório nesta task.

## 11) Riscos e Mitigações
- Risco: esconder `Approved`/`Rejected` para caber no enunciado.
  - Mitigação: expor status interno e label pública.
- Risco: mapper incompleto quebrar futuro status.
  - Mitigação: switch exaustivo e teste para todos os valores.
- Risco: nova rota conflitar com `/{id:guid}`.
  - Mitigação: usar rota `/{id:guid}/status`.

## 12) Checklist de Execução para IA
- [ ] Confirmar status atuais na Application.
- [ ] Criar response dedicado.
- [ ] Criar mapper de label explícito.
- [ ] Testar todos os labels.
- [ ] Não importar Domain na API.
- [ ] Não alterar `GET /service-orders/{id}`.
- [ ] Rodar `dotnet build`.
- [ ] Rodar `dotnet test`.
- [ ] Reportar evidências.

## 13) Evidência Esperada de Fechamento
- Comando `dotnet build` com resultado.
- Comando `dotnet test` com contagem de testes.
- Exemplo de response para status aderente ao enunciado.
- Evidência de label para `Approved` e `Rejected`.
