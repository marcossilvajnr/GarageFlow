# Orçamento — Entidade

## Metadados
- Classe C#: `Quote`
- Bounded Context: Gestão de Ordens de Serviço
- Namespace: `GarageFlow.Domain.ServiceOrders`
- Arquivo: `GarageFlow.Domain/ServiceOrders/Quote.cs`

## Responsabilidade
Representa a proposta de custo gerada automaticamente após a conclusão do
diagnóstico. Lista os serviços a executar com seus preços e calcula o valor
total. A aprovação do cliente sobre o orçamento é o gatilho que inicia a
execução dos serviços. Não é um agregado raiz — pertence ao ciclo de vida
da `ServiceOrder`.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| ServiceOrderId | `Guid` | Sim | Imutável após criação |
| Items | `IReadOnlyList<QuoteItem>` | Sim | Pelo menos 1 item; imutável após geração |
| TotalAmount | `decimal` | Sim | Calculado automaticamente como soma dos `Subtotal` de cada item |
| Status | `QuoteStatus` | Sim | Inicia como `Pending`; avança para `Approved` |
| GeneratedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Generate()` |
| ApprovedAt | `DateTime?` | Não | Nulo até aprovação; definido no `Approve()` |

> **Enum `QuoteStatus`:**
> ```
> Pending, Approved
> ```

> **Tipo de suporte `QuoteItem`** (`sealed record`):
>
> | Atributo | Tipo C# | Descrição |
> |----------|---------|-----------|
> | ServiceId | `Guid` | Referência ao `Service` do catálogo |
> | ServiceName | `string` | Nome do serviço no momento da geração (snapshot) |
> | UnitPrice | `decimal` | Preço unitário no momento da geração (snapshot) |
> | Quantity | `int` | Quantidade de vezes que o serviço é executado |
> | Subtotal | `decimal` | Calculado: `UnitPrice * Quantity` |
>
> `ServiceName` e `UnitPrice` são snapshots — refletem o valor no momento
da geração do orçamento, independente de alterações posteriores no catálogo.

## Invariantes
1. `Items` nunca pode ser vazio — orçamento sem serviços não é válido
2. `TotalAmount` é sempre a soma dos `Subtotal` de todos os `Items`
3. `TotalAmount >= 0`
4. `ApprovedAt` só pode ser definido quando `Status == Approved`
5. `ServiceOrderId` é imutável após a criação
6. `Items` é imutável após a geração — um orçamento não pode ter itens adicionados ou removidos após ser criado

## Métodos de Domínio

### Generate(Guid serviceOrderId, IEnumerable<QuoteItem> items)
- Pré-condição: `serviceOrderId` não é `Guid.Empty`; `items` contém pelo menos 1 item
- Pré-condição: cada `QuoteItem` deve ter:
  - `ServiceId != Guid.Empty`
  - `ServiceName` não nulo/não vazio após `trim`
  - `Quantity > 0`
  - `UnitPrice >= 0`
- Ação: cria a instância com `Id = Guid.NewGuid()`, `Status = Pending`, `GeneratedAt = DateTime.UtcNow`; calcula `TotalAmount = items.Sum(i => i.Subtotal)`
- Pós-condição: orçamento gerado, pendente de aprovação; `TotalAmount` calculado
- Publicação oficial de integração: feita pelo `ServiceOrder` (`QuoteGeneratedEvent`)
- Exceções:
  - `DomainException("Id da ordem de serviço inválido")` — se `serviceOrderId` for `Guid.Empty`
  - `DomainException("Orçamento deve ter pelo menos um item")` — se `items` for vazio
  - `DomainException("Item do orçamento inválido")` — se qualquer item violar as regras de campos

### Approve()
- Pré-condição: `Status == Pending`
- Ação: define `Status = Approved`, `ApprovedAt = DateTime.UtcNow`
- Pós-condição: orçamento aprovado; `ApprovedAt` definido
- Publicação oficial de integração: feita pelo `ServiceOrder` (`QuoteApprovedEvent`)
- Exceção: `DomainException("Orçamento já foi aprovado")`

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| — | Entidade interna não publica eventos de integração diretamente |

> `QuoteGeneratedEvent` e `QuoteApprovedEvent` são eventos oficiais do ciclo da OS e
> são publicados pelo Aggregate Root `ServiceOrder`.

## Regras de Negócio Relacionadas
- [RN-007]: O orçamento só pode ser gerado após o diagnóstico ser concluído
- [RN-008]: A aprovação do orçamento cria automaticamente uma `ExecutionOrder` por serviço

## Dependências
- Value Objects: nenhum
- Agregados: `ServiceOrder` (entidade pertencente à OS); `Service` (referenciado por `ServiceId` em cada `QuoteItem`)

## Implementação C#
- Construtor privado
- Factory method estático `Generate()`
- Propriedades com `private set`
- Exceções sempre via `DomainException`
- Normalização textual: aplicar `trim` nas bordas em entradas de texto do agregado

## Testes Obrigatórios
- [ ] `Generate()` com lista de itens válida deve criar com `Status = Pending`
- [ ] `Generate()` com lista vazia deve lançar `DomainException("Orçamento deve ter pelo menos um item")`
- [ ] `Generate()` com `QuoteItem` inválido deve lançar `DomainException("Item do orçamento inválido")`
- [ ] `TotalAmount` deve ser a soma correta dos `Subtotal` de todos os itens
- [ ] `Subtotal` de cada `QuoteItem` deve ser `UnitPrice * Quantity`
- [ ] `Approve()` em orçamento `Pending` deve definir `Status = Approved` e `ApprovedAt`
- [ ] `Approve()` em orçamento já aprovado deve lançar `DomainException("Orçamento já foi aprovado")`
- [ ] `ApprovedAt` deve ser `null` antes do `Approve()` e ter valor após
- [ ] `GeneratedAt` deve ser definido automaticamente no `Generate()`
- [ ] `Items` não deve ser modificável após a geração
