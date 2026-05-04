# Diagnóstico — Entidade

## Metadados
- Classe C#: `Diagnostic`
- Bounded Context: Gestão de Ordens de Serviço
- Namespace: `GarageFlow.Domain.ServiceOrders`
- Arquivo: `GarageFlow.Domain/ServiceOrders/Diagnostic.cs`

## Responsabilidade
Representa a análise técnica inicial da OS.
No diagnóstico, o mecânico seleciona serviços do catálogo.
Peças e insumos são derivados automaticamente desses serviços.
As alterações no diagnóstico atualizam a composição rastreável de serviços da OS com origem `Diagnostic`.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| ServiceOrderId | `Guid` | Sim | Imutável após criação |
| MechanicId | `Guid` | Sim | Mecânico responsável |
| Description | `string?` | Não | Obrigatória para concluir diagnóstico |
| SelectedServices | `IReadOnlyList<Guid>` | Sim | Deve conter pelo menos 1 serviço ao concluir; cada `ServiceId` pode aparecer uma única vez |
| StartedAt | `DateTime` | Sim | Definido no `Start()` |
| CompletedAt | `DateTime?` | Não | Definido no `Complete()` |
| Status | `DiagnosticStatus` | Sim | `InProgress` ou `Completed` |

### Enum DiagnosticStatus
`InProgress | Completed`

## Invariantes
1. `ServiceOrderId` é imutável
2. `SelectedServices` não aceita duplicidade (máximo 1 ocorrência por serviço)
3. após `Completed`, não há mutação de serviços
4. diagnóstico concluído exige descrição e ao menos 1 serviço
5. diagnóstico não registra quantidade por serviço; a seleção é unitária por `ServiceId`
6. serviços selecionados no diagnóstico refletem os serviços ativos da OS antes do congelamento

## Métodos de Domínio

### Start(Guid serviceOrderId, Guid mechanicId)
- Pré-condição: IDs válidos
- Ação: inicia em `InProgress` com lista `SelectedServices` vazia

### AddService(Guid serviceId)
- Pré-condição: `Status == InProgress`
- Pré-condição: `serviceId` não duplicado
- Exceções:
  - `DomainException("Diagnóstico já foi concluído")`
  - `DomainException("Serviço já adicionado ao diagnóstico")`

### RemoveService(Guid serviceId)
- Pré-condição: `Status == InProgress`
- Pré-condição: `serviceId` existe na lista
- Pré-condição: `SelectedServices.Count > 1`
- Exceções:
  - `DomainException("Diagnóstico já foi concluído")`
  - `DomainException("Serviço não encontrado no diagnóstico")`
  - `DomainException("Diagnóstico deve ter pelo menos um serviço")`

### Complete(string description)
- Pré-condição: `Status == InProgress`
- Pré-condição: descrição não nula/não vazia
- Pré-condição: `SelectedServices` não vazio
- Ação: define `Status = Completed`, `CompletedAt = DateTime.UtcNow`, `Description = description`
- Ação adicional no boundary (`ServiceOrder`): congela serviços ativos para orçamento
- Exceções:
  - `DomainException("Diagnóstico já foi concluído")`
  - `DomainException("Descrição do diagnóstico é obrigatória")`
  - `DomainException("Diagnóstico deve ter pelo menos um serviço")`

## Eventos de Domínio
| Evento C# | Publicação oficial no boundary |
|-----------|-------------------------------|
| `DiagnosticStartedEvent` | Publicado por `ServiceOrder` ao iniciar diagnóstico |
| `DiagnosticCompletedEvent` | Publicado por `ServiceOrder` ao concluir diagnóstico |

`Diagnostic` é entidade interna de `ServiceOrder` e não publica eventos de integração diretamente.

## Regras de Negócio Relacionadas
- [RN-006]: diagnóstico único por OS
- [RN-007]: orçamento só após diagnóstico concluído
- [RN-026]: alteração de serviços só em `InProgress`
- [RN-028]: diagnóstico não pode ser reaberto
- [RN-029]: alterações de serviços da OS devem ser rastreáveis
- [RN-030]: após `Completed`, serviços da OS ficam congelados para orçamento

## Dependências
- Agregados: `ServiceOrder` (entidade interna)
- Catálogo: `Service` por `ServiceId` na seleção

## Testes Obrigatórios
- [ ] adicionar serviço com diagnóstico `InProgress`
- [ ] adicionar serviço com diagnóstico `Completed` (erro)
- [ ] adicionar serviço duplicado (erro)
- [ ] remover serviço existente
- [ ] remover único serviço (erro)
- [ ] remover serviço inexistente (erro)
- [ ] completar sem serviços (erro)
- [ ] completar sem descrição (erro)
- [ ] completar diagnóstico já concluído (erro)
