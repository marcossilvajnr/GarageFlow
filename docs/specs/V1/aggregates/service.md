# Serviço — Agregado Raiz

## Metadados
- Classe C#: `Service`
- Bounded Context: Catálogo
- Namespace: `GarageFlow.Domain.Catalog`
- Arquivo: `GarageFlow.Domain/Catalog/Service.cs`

## Responsabilidade
Representa um tipo de serviço oferecido pela oficina, com preço base e tempo
médio estimado de execução. Define o catálogo de trabalhos disponíveis para
compor orçamentos nas Ordens de Serviço. Nunca é deletado fisicamente.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| Name | `string` | Sim | Não nulo, não vazio, máximo 200 caracteres |
| Description | `string?` | Não | Opcional; máximo 500 caracteres se informado |
| BasePrice | `decimal` | Sim | Maior ou igual a zero |
| EstimatedTimeMinutes | `int` | Sim | Maior que zero |
| IsActive | `bool` | Sim | Padrão `true`; nunca volta a `true` após `Deactivate()` |
| CreatedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Create()` |

## Invariantes
1. `Name` nunca pode ser nulo ou vazio
2. `BasePrice` nunca pode ser negativo
3. `EstimatedTimeMinutes` nunca pode ser zero ou negativo
4. `IsActive` nunca retorna a `true` após ter sido definido como `false`

## Métodos de Domínio

### Create(string name, string? description, decimal basePrice, int estimatedTimeMinutes)
- Pré-condição: `name` não nulo/vazio (máx 200 chars); `basePrice >= 0`; `estimatedTimeMinutes > 0`
- Ação: aplica `trim` em `name` e `description` (somente bordas) e cria a instância com `Id = Guid.NewGuid()`, `CreatedAt = DateTime.UtcNow`, `IsActive = true`
- Pós-condição: instância válida e ativa no catálogo
- Evento emitido: nenhum
- Exceções:
  - `DomainException("Nome do serviço inválido")` — se `name` for nulo ou vazio
  - `DomainException("Preço não pode ser negativo")` — se `basePrice < 0`
  - `DomainException("Tempo estimado deve ser maior que zero")` — se `estimatedTimeMinutes <= 0`

### UpdatePrice(decimal newPrice)
- Pré-condição: `newPrice >= 0`
- Ação: atualiza `BasePrice`
- Pós-condição: `BasePrice == newPrice`
- Evento emitido: nenhum
- Exceção: `DomainException("Preço não pode ser negativo")`

### UpdateEstimatedTime(int minutes)
- Pré-condição: `minutes > 0`
- Ação: atualiza `EstimatedTimeMinutes`
- Pós-condição: `EstimatedTimeMinutes == minutes`
- Evento emitido: nenhum
- Exceção: `DomainException("Tempo estimado deve ser maior que zero")`

### Deactivate()
- Pré-condição: `IsActive == true`
- Ação: define `IsActive = false`
- Pós-condição: serviço inativo; operação irreversível
- Evento emitido: nenhum
- Exceção: `DomainException("Serviço já está inativo")`

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| — | Nenhum evento de domínio (CRUD de catálogo) |

## Regras de Negócio Relacionadas
- [RN-024]: Tempo médio estimado é atualizado manualmente pelo administrativo — nunca calculado automaticamente
- [RN-023]: Serviços nunca são deletados fisicamente — apenas desativados

## Implementação C#
- Construtor privado
- Factory method estático `Create()`
- Propriedades com `private set`
- Exceções sempre via `DomainException`
- Normalização textual: aplicar `trim` nas bordas em entradas de texto do agregado

## Dependências
- Value Objects: nenhum
- Agregados: nenhum

## Testes Obrigatórios
- [ ] Criar serviço válido com `Description` deve criar com sucesso
- [ ] Criar serviço válido sem `Description` (`null`) deve criar com sucesso
- [ ] `Name` nulo deve lançar `DomainException("Nome do serviço inválido")`
- [ ] `Name` vazio deve lançar `DomainException("Nome do serviço inválido")`
- [ ] `Name` e `Description` com espaços nas bordas devem ser normalizados com `trim`
- [ ] `BasePrice` negativo no `Create()` deve lançar `DomainException("Preço não pode ser negativo")`
- [ ] `EstimatedTimeMinutes` igual a zero no `Create()` deve lançar `DomainException("Tempo estimado deve ser maior que zero")`
- [ ] `EstimatedTimeMinutes` negativo no `Create()` deve lançar `DomainException("Tempo estimado deve ser maior que zero")`
- [ ] `UpdatePrice(0)` deve atualizar com sucesso
- [ ] `UpdatePrice(-1)` deve lançar `DomainException("Preço não pode ser negativo")`
- [ ] `UpdateEstimatedTime(30)` deve atualizar com sucesso
- [ ] `UpdateEstimatedTime(0)` deve lançar `DomainException("Tempo estimado deve ser maior que zero")`
- [ ] Desativar serviço ativo deve definir `IsActive = false`
- [ ] Desativar serviço já inativo deve lançar `DomainException("Serviço já está inativo")`
