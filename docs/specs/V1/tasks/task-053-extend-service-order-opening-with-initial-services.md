# Task-053 — Extend Service Order Opening with Initial Services

## 0) Metadata
- `task_id`: `task-053`
- `slug`: `extend-service-order-opening-with-initial-services`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-052-link-employee-at-aggregate-creation.md`

## 1) Objetivo
Evoluir a abertura de Ordem de Serviço para aceitar serviços iniciais no payload, preservando compatibilidade com o contrato atual e mantendo peças como consequência da composição dos serviços.

## 2) Escopo
### In
- Estender `POST /service-orders` com campo opcional `serviceIds`.
- Manter criação atual quando `serviceIds` for `null` ou vazio.
- Criar OS já com serviços ativos e histórico quando serviços forem informados.
- Validar serviços informados sem introduzir dependência direta da API com Domain ou Infrastructure.
- Documentar que peças entram na OS indiretamente pela composição dos serviços.

### Out
- Receber `partIds` diretamente no payload da OS.
- Alterar o fluxo de orçamento, estoque ou composição de serviços.
- Criar nova rota de abertura paralela.
- Quebrar payloads já aceitos por `POST /service-orders`.
- Implementar integração com fornecedor externo ou catálogo externo de peças.

## 3) Contexto Canônico Obrigatório
Antes de implementar, ler obrigatoriamente:
- `docs/domain/regras-de-negocio.md`
- `docs/domain/linguagem-ubiqua.md`
- `docs/domain/agregados.md`
- `docs/specs/V1/aggregates/service-order.md`
- `docs/specs/V1/aggregates/service.md`
- `docs/specs/V1/aggregates/part.md`
- `docs/specs/V1/tasks/task-011-create-service-order-base.md`
- `docs/specs/V1/tasks/task-012-create-service-order-service-integration-frontdesk.md`
- `docs/architecture/architecture-overview.md`
- `docs/architecture/engineering-standards.md`

## 4) Decisões Arquiteturais Já Tomadas
- A rota `POST /service-orders` deve ser estendida, não substituída.
- `serviceIds` é opcional para preservar compatibilidade.
- Peças não entram como `partIds` na abertura; peças são derivadas da composição dos serviços selecionados.
- A API continua dependendo apenas da Application para este fluxo.
- O histórico de serviços deve ser preservado como trilha operacional da OS.

## 5) Regras de Negócio Aplicáveis
- A OS deve continuar nascendo em `Received`.
- Cliente e veículo devem existir.
- O veículo informado deve pertencer ao cliente informado.
- O atendente/front desk deve ser um ator válido para abertura de OS.
- Serviços iniciais devem existir e estar aptos para uso no fluxo da OS.
- Serviços duplicados no payload não devem gerar itens duplicados na OS.
- Cada serviço inicial ativo deve gerar histórico de adição.

## 6) Contratos e Interfaces
### 6.1 API pública
- Endpoint: `POST /service-orders`
- Request compatível:
  - `customerId: Guid`
  - `vehicleId: Guid`
  - `frontDeskEmployeeId: Guid`
  - `serviceIds?: Guid[]`
- Response:
  - `201 Created`
  - `ServiceOrderResponse`
  - header/location ou corpo deve continuar permitindo identificar a OS criada via `id`.

### 6.2 Matriz HTTP obrigatória
- Dados válidos sem `serviceIds` -> `201`
- Dados válidos com `serviceIds` -> `201`
- Cliente inexistente -> `404`
- Veículo inexistente -> `404`
- Veículo de outro cliente -> `400`
- Atendente inválido/inexistente -> erro já padronizado pelo fluxo atual
- Serviço inexistente -> `404`
- Serviço duplicado no payload -> `400` ou `409`, desde que a escolha seja documentada no teste e não dependa de parsing textual
- Serviço indisponível/inativo -> `409`
- Payload com `Guid.Empty` em `serviceIds` -> `400`

### 6.3 Contratos internos
- Atualizar `CreateServiceOrderCommand` com `IReadOnlyList<Guid>? ServiceIds`.
- Atualizar `CreateServiceOrderHandler` para validar e adicionar serviços iniciais.
- Reusar regras existentes de `ServiceOrder.AddService`.
- Reusar repositório/porta de serviços já existente para validação dos serviços.
- Não criar nova entidade para peças nesta task.

## 7) Plano Técnico por Camada
### Domain
- Preferir reuso de `ServiceOrder.AddService`.
- Não alterar o modelo de peças da OS.
- Só alterar domínio se uma regra existente impedir abertura com serviços iniciais.

### Application
- Estender command e handler de criação.
- Validar `serviceIds` antes de persistir.
- Garantir que serviços iniciais gerem `Services` e `ServiceHistory`.
- Manter retorno `ServiceOrderDto`.

### Infrastructure
- Sem migration esperada.
- Sem novo repositório esperado.
- Ajustar queries/includes apenas se necessário para retornar serviços/histórico após criação.

### API
- Estender `CreateServiceOrderRequest` sem remover propriedades atuais.
- Mapear `serviceIds` para o command.
- Manter responses e authorization atuais.

### Tests
- Adicionar testes de aplicação e integração para criação com serviços iniciais.
- Preservar testes existentes de criação sem serviços.

### Docs
- Atualizar README ou docs operacionais somente se houver seção de contrato de OS.
- Registrar explicitamente que peças entram via composição dos serviços.

## 8) Arquivos a Criar/Alterar
- `src/GarageFlow.Api/ServiceOrders/DTOs/CreateServiceOrderRequest.cs`
- `src/GarageFlow.Api/ServiceOrders/Endpoints/ServiceOrdersEndpoints.cs`
- `src/GarageFlow.Application/ServiceOrders/Commands/CreateServiceOrderCommand.cs`
- `src/GarageFlow.Application/ServiceOrders/Handlers/CreateServiceOrderHandler.cs`
- `tests/GarageFlow.Tests/Application/ServiceOrders/ServiceOrderHandlersTests.cs`
- `tests/GarageFlow.Tests/Integration/ServiceOrders/ServiceOrdersEndpointsTests.cs`
- `docs/specs/V1/aggregates/service-order.md` (somente se for necessário registrar a decisão sobre serviços iniciais)

Contrato de arquivos:
- Mudanças fora desta lista devem ser justificadas explicitamente na resposta final.
- Não criar estrutura alternativa para abertura de OS sem atualizar esta task antes.

## 9) Critérios de Pronto
- [ ] `POST /service-orders` sem `serviceIds` mantém comportamento atual.
- [ ] `POST /service-orders` com `serviceIds` cria OS com serviços ativos.
- [ ] Serviços iniciais aparecem no response.
- [ ] Histórico de adição é criado para cada serviço inicial.
- [ ] Peças permanecem derivadas da composição dos serviços.
- [ ] Contratos HTTP da matriz obrigatória estão cobertos.
- [ ] `dotnet build` sem erros.
- [ ] `dotnet test` verde.
- [ ] API não importa Domain nem Infrastructure.

## 10) Estratégia de Testes
### Domínio
- [ ] Reusar cobertura de `ServiceOrder.AddService`.
- [ ] Adicionar teste de domínio apenas se nova regra for introduzida.

### Aplicação
- [ ] criação sem serviços preserva comportamento atual;
- [ ] criação com um serviço retorna DTO com serviço ativo;
- [ ] criação com múltiplos serviços retorna histórico correspondente;
- [ ] serviço inexistente falha;
- [ ] serviço duplicado falha de forma estável;
- [ ] veículo de outro cliente continua falhando.

### Integração
- [ ] `POST /service-orders` sem `serviceIds` retorna `201`;
- [ ] `POST /service-orders` com `serviceIds` retorna `201` com serviços;
- [ ] serviço inexistente retorna `404`;
- [ ] serviço duplicado retorna erro esperado;
- [ ] `GET /service-orders/{id}` após criação retorna serviços e histórico.

### E2E
- [ ] Não criar novo E2E obrigatório nesta task.
- [ ] Se algum E2E existente usar abertura de OS, garantir que continue verde.

## 11) Riscos e Mitigações
- Risco: duplicar conceito de peças diretamente na OS.
  - Mitigação: manter peças derivadas de serviços e documentar a decisão.
- Risco: quebrar clientes/testes atuais.
  - Mitigação: `serviceIds` opcional e testes de compatibilidade.
- Risco: validação inconsistente de serviços duplicados.
  - Mitigação: definir erro esperado em teste, sem parsing textual.
- Risco: burlar histórico operacional.
  - Mitigação: exigir `ServiceHistory` para cada adição inicial.

## 12) Checklist de Execução para IA
- [ ] Ler os documentos canônicos listados.
- [ ] Confirmar contrato atual antes de alterar DTO.
- [ ] Implementar compatibilidade retroativa primeiro.
- [ ] Reusar `ServiceOrder.AddService`.
- [ ] Não adicionar `PartIds`.
- [ ] Não importar Domain na API.
- [ ] Não decidir HTTP por texto de exceção.
- [ ] Rodar `dotnet build`.
- [ ] Rodar `dotnet test`.
- [ ] Reportar evidência e arquivos alterados.

## 13) Evidência Esperada de Fechamento
- Comando `dotnet build` com resultado.
- Comando `dotnet test` com contagem de testes.
- Evidência de `POST /service-orders` sem serviços.
- Evidência de `POST /service-orders` com serviços.
- Nota explícita: peças entram pela composição dos serviços.
