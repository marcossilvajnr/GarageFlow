# Estoque — Agregado Raiz

## Metadados
- Classe C#: `Stock`
- Bounded Context: Gestão de Estoque
- Namespace: `GarageFlow.Domain.Stock`
- Arquivo: `GarageFlow.Domain/Stock/Stock.cs`

## Responsabilidade
Controla a disponibilidade de uma peça ou insumo específico no estoque da
oficina. Mantém três quantidades complementares: total físico, disponível
para reserva e reservada para ordens em andamento. Garante que a quantidade
disponível nunca seja negativa e aciona alertas de reposição quando cai
abaixo do mínimo configurado.

> **Invariante das três quantidades (comentário obrigatório na classe):**
> ```
> AvailableQuantity = TotalQuantity - ReservedQuantity
> Toda operação que altere ReservedQuantity ou TotalQuantity
> deve recalcular AvailableQuantity e verificar o invariante.
> ```

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| ItemId | `Guid` | Sim | Imutável após criação; referencia `Part.Id` ou `Supply.Id` |
| ItemType | `StockItemType` | Sim | Imutável após criação; enum `Part` ou `Supply` |
| TotalQuantity | `decimal` | Sim | Quantidade física total; maior ou igual a zero |
| AvailableQuantity | `decimal` | Sim | Livre para novas reservas; maior ou igual a zero (RN-014) |
| ReservedQuantity | `decimal` | Sim | Bloqueada para ordens em andamento; maior ou igual a zero |
| MinimumQuantity | `decimal` | Sim | Gatilho de reposição; maior ou igual a zero |
| CreatedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Create()` |
| UpdatedAt | `DateTime?` | Não | Nulo na criação; atualizado em toda operação de mutação de estado |

> **Enum `StockItemType`:**
> ```
> Part, Supply
> ```

## Invariantes
1. `AvailableQuantity >= 0` sempre — nunca pode ser negativo (RN-014)
2. `ReservedQuantity >= 0` sempre
3. `TotalQuantity >= 0` sempre
4. `AvailableQuantity = TotalQuantity - ReservedQuantity` após toda operação (RN-015)
5. `MinimumQuantity >= 0`
6. `ItemId` e `ItemType` são imutáveis após a criação

## Métodos de Domínio

### Create(Guid itemId, StockItemType itemType, decimal initialQuantity, decimal minimumQuantity)
- Pré-condição: `itemId` não é `Guid.Empty`; `initialQuantity >= 0`; `minimumQuantity >= 0`
- Ação: cria com `TotalQuantity = AvailableQuantity = initialQuantity`, `ReservedQuantity = 0`; define `CreatedAt`
- Pós-condição: estoque inicializado com invariante satisfeito
- Evento emitido: nenhum
- Exceções:
  - `DomainException("Id do item inválido")` — se `itemId` for `Guid.Empty`
  - `DomainException("Quantidade inicial inválida")` — se `initialQuantity < 0`
  - `DomainException("Quantidade mínima inválida")` — se `minimumQuantity < 0`

### Reserve(decimal quantity)
- Pré-condição: `quantity > 0`; `AvailableQuantity >= quantity`
- Ação: `AvailableQuantity -= quantity`; `ReservedQuantity += quantity`; `UpdatedAt = DateTime.UtcNow`
- Pós-condição: invariante satisfeito; se `AvailableQuantity < MinimumQuantity` após a reserva, emite também `InsufficientStockEvent`
- Eventos emitidos: `PartsReservedEvent`; condicionalmente `InsufficientStockEvent`
- Exceção: `DomainException("Estoque insuficiente")`

### Consume(decimal quantity)
- Pré-condição: `quantity > 0`; `ReservedQuantity >= quantity`
- Ação: `ReservedQuantity -= quantity`; `TotalQuantity -= quantity`; `UpdatedAt = DateTime.UtcNow`
  (`AvailableQuantity` não se altera pois a quantidade já estava reservada)
- Pós-condição: baixa física do estoque; invariante satisfeito
- Evento emitido: `StockUpdatedEvent`
- Exceção: `StockQuantityConflictException("Quantidade reservada insuficiente")`

### Release(decimal quantity, string reason, string performedBy, Guid? referenceId = null, string? referenceType = null)
- Pré-condição: `quantity > 0`; `ReservedQuantity >= quantity`
- Pré-condição: `reason` obrigatório; `performedBy` obrigatório
- Ação: `ReservedQuantity -= quantity`; `AvailableQuantity += quantity`; `UpdatedAt = DateTime.UtcNow`
- Pós-condição: quantidade reservada devolvida à disponibilidade; invariante satisfeito (RN-016)
- Evento emitido: `StockUpdatedEvent`
- Exceções:
  - `DomainException("Motivo da liberação é obrigatório")`
  - `DomainException("Responsável pela liberação é obrigatório")`
  - `StockQuantityConflictException("Quantidade reservada insuficiente")`

### Entry(decimal quantity, string? reason = null, Guid? referenceId = null)
- Pré-condição: `quantity > 0`
- Ação: `TotalQuantity += quantity`; `AvailableQuantity += quantity`; `UpdatedAt = DateTime.UtcNow`
- Pós-condição: estoque incrementado; invariante satisfeito
- Evento emitido: `StockUpdatedEvent`
- Exceção: `DomainException("Quantidade da operação inválida")`

### Adjust(decimal quantityDelta, string reason, Guid? referenceId = null)
- Pré-condição: `quantityDelta != 0`; `reason` obrigatório
- Ação: ajusta `TotalQuantity` pelo delta informado e recalcula disponibilidade
- Pós-condição: invariante preservado
- Evento emitido: `StockUpdatedEvent`
- Exceções:
  - `DomainException("Quantidade de ajuste inválida")`
  - `DomainException("Motivo do ajuste é obrigatório")`
  - `StockQuantityConflictException("Ajuste de estoque quebra invariante")`

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| `PartsReservedEvent` | Ao reservar quantidade para uma ordem |
| `InsufficientStockEvent` | Ao reservar e `AvailableQuantity` cair abaixo de `MinimumQuantity` |
| `StockUpdatedEvent` | Ao atualizar quantidades do estoque (baixa, liberação ou reposição) |

## Regras de Negócio Relacionadas
- [RN-012]: Verificação automática de estoque ao criar Ordem de Separação
- [RN-014]: `AvailableQuantity` nunca pode ser menor que zero
- [RN-015]: Três quantidades distintas: `TotalQuantity`, `AvailableQuantity`, `ReservedQuantity`
- [RN-016]: Operações de estoque incluem Reservar (`Reserve`), Consumir (`Consume`), Liberar (`Release`), Entrada (`Entry`) e Ajuste (`Adjust`)
- [RN-017]: `InsufficientStockEvent` desencadeia geração automática de Ordem de Compra

## Implementação C#
- Construtor privado
- Factory method estático `Create()`
- Propriedades com `private set`
- Exceções sempre via `DomainException`

## Dependências
- Value Objects: nenhum
- Agregados: `Part` ou `Supply` (referenciados por `ItemId` + `ItemType`)

## Testes Obrigatórios
- [ ] Criar estoque com quantidade inicial válida deve criar com sucesso e satisfazer o invariante
- [ ] Criar estoque com `initialQuantity` negativa deve lançar `DomainException("Quantidade inicial inválida")`
- [ ] `Reserve(quantity)` com `AvailableQuantity` suficiente deve decrementar disponível, incrementar reservado e emitir `PartsReservedEvent`
- [ ] `Reserve(quantity)` com `AvailableQuantity` insuficiente deve lançar `DomainException("Estoque insuficiente")`
- [ ] `Reserve(quantity)` que leve `AvailableQuantity` abaixo de `MinimumQuantity` deve emitir `PartsReservedEvent` e `InsufficientStockEvent`
- [ ] `Consume(quantity)` após reserva válida deve decrementar reservado e total, manter disponível e emitir `StockUpdatedEvent`
- [ ] `Consume(quantity)` com `ReservedQuantity` insuficiente deve lançar `StockQuantityConflictException`
- [ ] `Release(quantity, reason, performedBy)` deve devolver quantidade à disponível e emitir `StockUpdatedEvent`
- [ ] `Release(quantity, reason, performedBy)` com `reason` vazio deve lançar `DomainException`
- [ ] `Release(quantity, reason, performedBy)` maior que `ReservedQuantity` deve lançar `StockQuantityConflictException`
- [ ] `Entry(quantity)` deve incrementar total e disponível e emitir `StockUpdatedEvent`
- [ ] `Adjust(quantityDelta, reason)` deve manter invariante e emitir `StockUpdatedEvent`
- [ ] `AvailableQuantity` nunca deve ser negativo após qualquer operação (invariante)
- [ ] `UpdatedAt` deve ser atualizado em toda operação de mutação
