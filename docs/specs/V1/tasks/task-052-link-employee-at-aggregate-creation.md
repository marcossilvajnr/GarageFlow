# Task-052 — Link Employee at Aggregate Creation / Start

## 0) Metadata
- `task_id`: `task-052`
- `slug`: `link-employee-at-aggregate-creation`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: [task-051-protect-administrative-cruds-with-jwt-administrative-role.md](task-051-protect-administrative-cruds-with-jwt-administrative-role.md)

## 1) Objetivo
Padronizar o vínculo lógico de ator operacional (`Employee`) no início dos fluxos de negócio, garantindo rastreabilidade mínima por ID compatível com o papel esperado, sem depender de resolução do usuário logado nesta fase.

## 2) Escopo
### In
- Mapear agregados/fluxos que exigem vínculo de funcionário.
- Definir o vínculo no momento de criação do agregado ou no primeiro `Start` canônico quando o agregado é gerado automaticamente.
- Validar `employeeId` informado na entrada:
  - funcionário existe;
  - funcionário está ativo;
  - funcionário possui role compatível com o fluxo.
- Ajustar contratos de API/commands/handlers para receber e persistir os IDs de vínculo inicial.
- Atualizar testes de integração/e2e afetados.

### Out
- Resolver ator via sessão/JWT (`CurrentActorService`) para estas operações.
- Auditoria avançada de troca de responsável (motivo, trilha específica).
- Refactor amplo de autorização além do necessário para manter contratos funcionais.

## 3) Contexto Canônico Obrigatório
- [docs/domain/agregados.md](../../../domain/agregados.md)
- [docs/domain/regras-de-negocio.md](../../../domain/regras-de-negocio.md)
- [docs/architecture/engineering-standards.md](../../../architecture/engineering-standards.md)
- [docs/architecture/application-and-integrations.md](../../../architecture/application-and-integrations.md)

## 4) Mapa de Vínculos Obrigatórios
### 4.1 ServiceOrder
- Vínculo obrigatório na criação: `FrontDeskEmployeeId`.
- Role compatível: `Attendant` (e, quando aplicável ao produto, `Administrative`).

### 4.2 Diagnostic (entidade interna da ServiceOrder)
- Vínculo obrigatório no início (`StartDiagnostic`): `MechanicId`.
- Role compatível: `Mechanic` (ou `Administrative`, se regra de operação permitir).

### 4.3 ExecutionOrder
- Vínculo obrigatório no início (`StartExecution`): `MechanicId`.
- Role compatível: `Mechanic`.

### 4.4 SeparationOrder
- Vínculo obrigatório de retirada: `StockistId` em `ConfirmStockistWithdrawal`.
- Vínculo de mecânico: derivado da `ExecutionOrder.MechanicId` (não duplicar campo obrigatório se já existir relação canônica suficiente).

### 4.5 PurchaseOrder
- Vínculo obrigatório no primeiro passo humano do fluxo:
  - `AssignSupplier`: registra `AssignedSupplierByEmployeeId`.
  - `Start`: registra `StartedByEmployeeId`.
  - `Complete`: registra `CompletedByEmployeeId`.
- Role compatível: `Stockist` (ou `Administrative` conforme política vigente).

## 5) Contratos e Interfaces
### 5.1 API pública
Ajustar requests/commands para que o vínculo inicial seja informado na operação de criação/início do fluxo quando ainda não existir vínculo persistido.

Matriz de erro mínima para operações com vínculo de employee:
- `400` — `employeeId` inválido (`Guid.Empty`) ou role incompatível.
- `404` — funcionário não encontrado.
- `409` — conflito de estado do agregado (quando já houver vínculo definido e fluxo não permitir redefinição).

### 5.2 Regras de contrato
- Não exigir `employeeId` repetidamente em etapas subsequentes quando o vínculo já estiver definido no agregado.
- Se a etapa depender de vínculo já definido e ele não existir, retornar erro de validação de fluxo (`400`/`409`, conforme regra existente do agregado).

## 6) Plano Técnico por Camada
### Domain
- Garantir campos de vínculo e invariantes de criação/início para os agregados/entidades mapeados.
- Evitar duplicidade de vínculo quando já existe relação canônica suficiente (ex.: `SeparationOrder` -> `ExecutionOrder`).

### Application
- Ajustar commands/handlers para receber e validar `employeeId` compatível no ponto de criação/início.
- Reusar `IEmployeeRepository` para validações de existência/ativo/role.

### Infrastructure
- Ajustar mapeamentos EF/Core caso necessário para novos campos de vínculo.
- Criar migration somente se houver alteração de esquema.

### API
- Ajustar DTOs/endpoints de criação/início para refletir os novos vínculos.
- Preservar mapeamento HTTP sem parsing textual de mensagens.

### Tests
- Atualizar/Adicionar testes para:
  - sucesso com `employeeId` válido e role compatível;
  - erro por funcionário inexistente;
  - erro por funcionário inativo;
  - erro por role incompatível.

## 7) Arquivos a Criar/Alterar
### Alterar (mínimo esperado)
- `src/GarageFlow.Domain/ServiceOrders/**`
- `src/GarageFlow.Domain/Executions/**`
- `src/GarageFlow.Domain/Stock/**`
- `src/GarageFlow.Domain/Purchasing/**`
- `src/GarageFlow.Application/ServiceOrders/**`
- `src/GarageFlow.Application/Executions/**`
- `src/GarageFlow.Application/Stock/**`
- `src/GarageFlow.Application/Purchasing/**`
- `src/GarageFlow.Api/Endpoints/**`
- `src/GarageFlow.Api/DTOs/**`
- `tests/GarageFlow.Tests/Integration/**`
- `tests/GarageFlow.Tests/E2E/**` (se necessário)

## 8) Critérios de Pronto
- [ ] Vínculos de employee definidos no início de cada fluxo mapeado.
- [ ] Validações de existência/ativo/role aplicadas nos pontos de criação/início.
- [ ] Sem dependência de usuário logado para resolver ator nesta task.
- [ ] Endpoints e contratos atualizados com matriz de erro consistente.
- [ ] Build e testes verdes.

## 9) Riscos e Mitigações
- Risco: ampliar escopo para autorização por sessão nesta mesma task.
  - Mitigação: manter estritamente vínculo lógico por ID e validação de employee.
- Risco: duplicar vínculo de mecânico entre `ExecutionOrder` e `SeparationOrder`.
  - Mitigação: priorizar derivação via relação canônica quando já disponível.

## 10) Checklist de Execução para IA
- [ ] Confirmar mapa final de agregados que exigem vínculo inicial.
- [ ] Ajustar domínio + aplicação + API para entrada/persistência do vínculo.
- [ ] Cobrir validações de employee compatível por role.
- [ ] Executar `dotnet build` e `dotnet test`.
