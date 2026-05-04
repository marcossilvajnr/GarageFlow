# Serviço — Agregado Raiz

## Metadados
- Classe C#: `Service`
- Bounded Context: Catálogo
- Namespace: `GarageFlow.Domain.Catalog`
- Arquivo: `GarageFlow.Domain/Catalog/Service.cs`

## Responsabilidade
Representa um tipo de serviço oferecido pela oficina, com preço base,
tempo estimado e composição pré-definida de peças e insumos.
Essa composição é usada no diagnóstico e no orçamento da OS.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| Name | `string` | Sim | Não nulo, não vazio, máximo 200 caracteres |
| Description | `string?` | Não | Opcional; máximo 500 caracteres se informado |
| BasePrice | `decimal` | Sim | Maior ou igual a zero |
| EstimatedTimeMinutes | `int` | Sim | Maior que zero |
| IsActive | `bool` | Sim | Padrão `true`; não volta para `true` após `Deactivate()` |
| Parts | `IReadOnlyList<ServicePartItem>` | Sim | Pode iniciar vazio; sem duplicidade por `PartId` |
| Supplies | `IReadOnlyList<ServiceSupplyItem>` | Sim | Pode iniciar vazio; sem duplicidade por `SupplyId` |
| CreatedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Create()` |

## Tipos Internos

### ServicePartItem
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| PartId | `Guid` | Sim | Não pode ser `Guid.Empty` |
| PartName | `string` | Sim | Não nulo e não vazio |
| Quantity | `int` | Sim | Maior que zero |

### ServiceSupplyItem
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| SupplyId | `Guid` | Sim | Não pode ser `Guid.Empty` |
| SupplyName | `string` | Sim | Não nulo e não vazio |
| Quantity | `decimal` | Sim | Maior que zero |
| Unit | `SupplyUnit` | Sim | Enum canônico |

### Enum SupplyUnit
`Liter | Milliliter | Gram | Kilogram | Unit`

## Invariantes
1. `Name` nunca pode ser nulo ou vazio
2. `BasePrice` nunca pode ser negativo
3. `EstimatedTimeMinutes` nunca pode ser zero ou negativo
4. `IsActive` nunca retorna a `true` após `Deactivate()`
5. `Parts` não aceita `PartId` duplicado
6. `Supplies` não aceita `SupplyId` duplicado
7. Itens de composição exigem quantidade maior que zero

## Métodos de Domínio

### Create(string name, string? description, decimal basePrice, int estimatedTimeMinutes)
- Pré-condição: `name` válido; `basePrice >= 0`; `estimatedTimeMinutes > 0`
- Ação: cria instância ativa com listas `Parts` e `Supplies` vazias
- Exceções:
  - `DomainException("Nome do serviço inválido")`
  - `DomainException("Preço não pode ser negativo")`
  - `DomainException("Tempo estimado deve ser maior que zero")`

### AddPart(Guid partId, string partName, int quantity)
- Pré-condição: `partId` válido, `partName` válido, `quantity > 0`
- Pré-condição: `partId` não pode estar duplicado na lista
- Exceções:
  - `DomainException("Peça já adicionada ao serviço")`
  - `DomainException("Quantidade deve ser maior que zero")`

### RemovePart(Guid partId)
- Pré-condição: `partId` existente na lista
- Exceção: `DomainException("Peça não encontrada no serviço")`

### AddSupply(Guid supplyId, string supplyName, decimal quantity, SupplyUnit unit)
- Pré-condição: `supplyId` válido, `supplyName` válido, `quantity > 0`
- Pré-condição: `supplyId` não pode estar duplicado na lista
- Exceções:
  - `DomainException("Insumo já adicionado ao serviço")`
  - `DomainException("Quantidade deve ser maior que zero")`

### RemoveSupply(Guid supplyId)
- Pré-condição: `supplyId` existente na lista
- Exceção: `DomainException("Insumo não encontrado no serviço")`

### UpdatePrice(decimal newPrice)
- Pré-condição: `newPrice >= 0`
- Exceção: `DomainException("Preço não pode ser negativo")`

### UpdateEstimatedTime(int minutes)
- Pré-condição: `minutes > 0`
- Exceção: `DomainException("Tempo estimado deve ser maior que zero")`

### Deactivate()
- Pré-condição: `IsActive == true`
- Exceção: `DomainException("Serviço já está inativo")`

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| — | Nenhum evento de integração (cadastro de catálogo) |

## Regras de Negócio Relacionadas
- [RN-023]: Soft delete para serviços de catálogo
- [RN-024]: Tempo estimado é mantido manualmente
- [RN-027]: Mecânico não cadastra serviços

## Dependências
- Value Objects: nenhum
- Agregados: referencia `Part` e `Supply` por ID na composição

## Testes Obrigatórios
- [ ] adicionar peça válida
- [ ] adicionar peça duplicada (erro)
- [ ] adicionar peça com quantidade zero (erro)
- [ ] remover peça existente
- [ ] remover peça inexistente (erro)
- [ ] adicionar insumo válido
- [ ] adicionar insumo duplicado (erro)
- [ ] adicionar insumo com quantidade zero (erro)
- [ ] remover insumo existente
- [ ] remover insumo inexistente (erro)
- [ ] desativar serviço já inativo (erro)
