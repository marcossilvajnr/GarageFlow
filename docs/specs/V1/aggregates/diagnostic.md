# Diagnóstico — Entidade

## Metadados
- Classe C#: `Diagnostic`
- Bounded Context: Gestão de Ordens de Serviço
- Namespace: `GarageFlow.Domain.ServiceOrders`
- Arquivo: `GarageFlow.Domain/ServiceOrders/Diagnostic.cs`

## Responsabilidade
Representa a análise técnica do veículo realizada por um mecânico no início
de uma Ordem de Serviço. É a entidade que formaliza o problema diagnosticado
e habilita a geração do orçamento. Uma OS só pode ter um diagnóstico ativo
por vez (RN-006). Não é um agregado raiz — pertence ao ciclo de vida da
`ServiceOrder`.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| ServiceOrderId | `Guid` | Sim | Imutável após criação |
| MechanicId | `Guid` | Sim | Mecânico responsável pelo diagnóstico |
| Description | `string?` | Não | Nula ao iniciar; obrigatória ao completar (máx 2000 chars) |
| Status | `DiagnosticStatus` | Sim | Inicia como `InProgress`; avança para `Completed` |
| StartedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Start()` |
| CompletedAt | `DateTime?` | Não | Nulo até completar; definido no `Complete()` |

> **Enum `DiagnosticStatus`:**
> ```
> InProgress, Completed
> ```

## Invariantes
1. `ServiceOrderId` é imutável após a criação
2. `CompletedAt` só pode ser definido quando `Status == Completed`
3. `Description` é obrigatória ao completar — não pode ser nula ou vazia no `Complete()`
4. Após `Complete()`, nenhum atributo pode ser alterado

## Métodos de Domínio

### Start(Guid serviceOrderId, Guid mechanicId)
- Pré-condição: `serviceOrderId` não é `Guid.Empty`; `mechanicId` não é `Guid.Empty`
- Ação: cria a instância com `Id = Guid.NewGuid()`, `Status = InProgress`, `StartedAt = DateTime.UtcNow`
- Pós-condição: diagnóstico em andamento; `CompletedAt` e `Description` são `null`
- Publicação oficial de integração: feita pelo `ServiceOrder` (`DiagnosticStartedEvent`)
- Exceções:
  - `DomainException("Id da ordem de serviço inválido")` — se `serviceOrderId` for `Guid.Empty`
  - `DomainException("Id do mecânico inválido")` — se `mechanicId` for `Guid.Empty`

### Complete(string description)
- Pré-condição: `Status == InProgress`; `description` não nula e não vazia
- Ação: aplica `trim` em `description` (somente bordas) e define `Status = Completed`, `CompletedAt = DateTime.UtcNow`, `Description = description`
- Pós-condição: diagnóstico concluído; `CompletedAt` e `Description` definidos
- Publicação oficial de integração: feita pelo `ServiceOrder` (`DiagnosticCompletedEvent`)
- Exceções:
  - `DomainException("Diagnóstico já foi concluído")` — se `Status != InProgress`
  - `DomainException("Descrição do diagnóstico é obrigatória")` — se `description` for nula ou vazia

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| — | Entidade interna não publica eventos de integração diretamente |

> `DiagnosticStartedEvent` e `DiagnosticCompletedEvent` são eventos oficiais do ciclo da OS e
> são publicados pelo Aggregate Root `ServiceOrder`.

## Regras de Negócio Relacionadas
- [RN-006]: Uma OS só pode ter um diagnóstico ativo por vez
- [RN-007]: O orçamento só pode ser gerado após o diagnóstico ser concluído

## Implementação C#
- Construtor privado
- Factory method estático `Start()`
- Propriedades com `private set`
- Exceções sempre via `DomainException`
- Normalização textual: aplicar `trim` nas bordas em entradas de texto do agregado

## Dependências
- Value Objects: nenhum
- Agregados: `ServiceOrder` (entidade pertencente à OS)

## Testes Obrigatórios
- [ ] `Start()` com IDs válidos deve criar diagnóstico com `Status = InProgress`
- [ ] `Start()` com `serviceOrderId` igual a `Guid.Empty` deve lançar `DomainException("Id da ordem de serviço inválido")`
- [ ] `Start()` com `mechanicId` igual a `Guid.Empty` deve lançar `DomainException("Id do mecânico inválido")`
- [ ] `Complete(description)` com descrição válida deve definir `Status = Completed` e `CompletedAt`
- [ ] `Complete(null)` deve lançar `DomainException("Descrição do diagnóstico é obrigatória")`
- [ ] `Complete("")` deve lançar `DomainException("Descrição do diagnóstico é obrigatória")`
- [ ] `Complete()` em diagnóstico já concluído deve lançar `DomainException("Diagnóstico já foi concluído")`
- [ ] `StartedAt` deve ser definido automaticamente no `Start()`
- [ ] `CompletedAt` deve ser `null` antes do `Complete()` e ter valor após
