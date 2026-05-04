# Ordem de Separação — Agregado Raiz

## Metadados
- Classe C#: `SeparationOrder`
- Tipo: Agregado Raiz
- Bounded Context: Gestão de Estoque
- Namespace: `GarageFlow.Domain.Stock`
- Arquivo: `GarageFlow.Domain/Stock/SeparationOrder.cs`

## Responsabilidade
Representa a separação física de materiais necessários para uma `ExecutionOrder`,
com listas distintas de peças e insumos.
Controla reserva, retomada após compra, retirada pelo estoquista e
confirmação de recebimento pelo mecânico.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| ExecutionOrderId | `Guid` | Sim | Imutável após criação |
| Status | `SeparationOrderStatus` | Sim | Fluxo com/sem compra, sem salto de estado |
| Parts | `IReadOnlyList<SeparationPartItem>` | Sim | Pode iniciar vazio; sem duplicidade por `PartId` |
| Supplies | `IReadOnlyList<SeparationSupplyItem>` | Sim | Pode iniciar vazio; sem duplicidade por `SupplyId` |
| StockistId | `Guid?` | Não | Nulo até confirmação do estoquista |
| ConfirmedByStockistAt | `DateTime?` | Não | Nulo até confirmação do estoquista |
| ConfirmedByMechanicAt | `DateTime?` | Não | Nulo até confirmação do mecânico |
| CreatedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Create()` |

> **Enum `SeparationOrderStatus`:**
> ```
> Pending, WaitingPurchase, WaitingPickup, Separated, Completed
> ```

## Tipos Internos

### SeparationPartItem
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| PartId | `Guid` | Sim | Referência da peça no catálogo |
| PartName | `string` | Sim | Snapshot textual da peça |
| Quantity | `int` | Sim | Maior que zero |
| IsReserved | `bool` | Sim | Indica reserva de estoque |

### SeparationSupplyItem
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| SupplyId | `Guid` | Sim | Referência do insumo no catálogo |
| SupplyName | `string` | Sim | Snapshot textual do insumo |
| Quantity | `decimal` | Sim | Maior que zero |
| Unit | `SupplyUnit` | Sim | Unidade canônica de medida |
| IsReserved | `bool` | Sim | Indica reserva de estoque |

## Invariantes
1. `ExecutionOrderId` nunca pode ser alterado após criação.
2. A separação deve conter pelo menos um item no total (`Parts` ou `Supplies`).
3. `Parts` não aceita `PartId` duplicado.
4. `Supplies` não aceita `SupplyId` duplicado.
5. Fluxos válidos:
   - Com estoque: `Pending -> WaitingPickup -> Separated -> Completed`
   - Sem estoque: `Pending -> WaitingPurchase -> WaitingPickup -> Separated -> Completed`
6. `Completed` exige dupla confirmação de custódia (estoquista + mecânico).
7. `ConfirmedByMechanicAt` só pode ser definido após `ConfirmedByStockistAt`.

## Origem dos Itens
As listas `Parts` e `Supplies` são construídas a partir dos `ServiceItem` da OS,
que por sua vez são snapshots do catálogo no momento do diagnóstico.

## Métodos de Domínio

### Create(Guid executionOrderId, IEnumerable<SeparationPartItem> parts, IEnumerable<SeparationSupplyItem> supplies)
- Pré-condição: `executionOrderId != Guid.Empty`
- Pré-condição: pelo menos um item entre `parts` e `supplies`
- Pré-condição: itens válidos (`Id` válido, nome não vazio, quantidade > 0)
- Ação:
  - cria instância com `Status = Pending`
  - inicializa confirmações e custodiante como `null`
  - define `CreatedAt = DateTime.UtcNow`
- Pós-condição: ordem criada e pronta para decisão de reserva/compra
- Evento emitido: `SeparationOrderCreatedEvent`
- Exceções:
  - `DomainException("Ordem de Execução é obrigatória")`
  - `DomainException("Separação deve ter pelo menos um item")`
  - `DomainException("Item de separação inválido")`

### Reserve()
- Pré-condição: `Status == Pending`
- Ação:
  - define `IsReserved = true` para todas as peças e insumos
  - define `Status = WaitingPickup`
- Pós-condição: materiais reservados e aguardando retirada
- Evento emitido: nenhum de integração (`PartsReservedEvent` é publicado por `Stock`)
- Exceção: `DomainException("Separação não está Pendente")`

### WaitForPurchase()
- Pré-condição: `Status == Pending`
- Ação: define `Status = WaitingPurchase`
- Pós-condição: separação aguardando reposição
- Evento emitido: nenhum de integração (`InsufficientStockEvent` é publicado por `Stock`)
- Exceção: `DomainException("Separação não está Pendente")`

### ResumeAfterPurchase()
- Pré-condição: `Status == WaitingPurchase`
- Ação:
  - define `IsReserved = true` para todas as peças e insumos
  - define `Status = WaitingPickup`
- Pós-condição: separação retomada com materiais reservados
- Exceção: `DomainException("Separação não está Aguardando Compra")`

### ConfirmStockistWithdrawal(Guid stockistId)
- Pré-condição: `Status == WaitingPickup`
- Pré-condição: `stockistId != Guid.Empty`
- Pré-condição: todos os itens com `IsReserved == true`
- Ação:
  - define `StockistId = stockistId`
  - define `ConfirmedByStockistAt = DateTime.UtcNow`
  - define `Status = Separated`
- Pós-condição: retirada física confirmada
- Exceções:
  - `DomainException("Separação não está Aguardando Retirada")`
  - `DomainException("Estoquista é obrigatório")`
  - `DomainException("Itens da separação ainda não foram reservados")`

### ConfirmMechanicReceipt()
- Pré-condição: `Status == Separated`
- Pré-condição: `ConfirmedByStockistAt` possui valor
- Ação:
  - define `ConfirmedByMechanicAt = DateTime.UtcNow`
  - define `Status = Completed`
- Pós-condição: custódia concluída
- Evento emitido: `SeparationOrderCompletedEvent`
- Exceções:
  - `DomainException("Aguardando confirmação do estoquista")`
  - `DomainException("Separação não está Separada")`

## Política de Cancelamento
Quando houver cancelamento antes do início da execução:
- peças reservadas podem retornar ao estoque;
- insumos não retornam ao estoque após a separação.

A reposição/devolução física é orquestrada na camada de aplicação,
respeitando o status da `SeparationOrder` e o tipo de item.

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| `SeparationOrderCreatedEvent` | Ao criar a ordem de separação |
| `SeparationOrderCompletedEvent` | Ao confirmar recebimento pelo mecânico |

## Regras de Negócio Relacionadas
- [RN-011]: criada automaticamente para cada `ExecutionOrder`
- [RN-012]: verificação automática de estoque na criação
- [RN-013]: dupla confirmação de custódia
- [RN-020]: retomada após compra concluída

## Dependências
- Agregados externos referenciados por ID: `ExecutionOrder`
- Integra com: `Stock` (reserva/disponibilidade) e `PurchaseOrder` (retomada)

## Testes Obrigatórios
- [ ] criar válida com peças e/ou insumos
- [ ] criar sem executionOrderId (erro)
- [ ] criar sem itens (erro)
- [ ] criar com item inválido (erro)
- [ ] criar com peça duplicada (erro)
- [ ] criar com insumo duplicado (erro)
- [ ] reservar estoque disponível
- [ ] marcar aguardando compra
- [ ] retomar após compra
- [ ] confirmar retirada do estoquista
- [ ] confirmar retirada sem itens reservados (erro)
- [ ] confirmar recebimento do mecânico
- [ ] confirmar recebimento antes do estoquista (erro)
- [ ] aplicar política de cancelamento por tipo de item
