# Insumo — Agregado Raiz

## Metadados
- Classe C#: `Supply`
- Bounded Context: Catálogo
- Namespace: `GarageFlow.Domain.Catalog`
- Arquivo: `GarageFlow.Domain/Catalog/Supply.cs`

## Responsabilidade
Representa um material consumível do catálogo da oficina. Ao contrário das
peças, insumos são medidos em quantidades variáveis (litros, gramas etc.) e
nunca retornam ao estoque após o uso — são descartados ao final do serviço.
Nunca é deletado fisicamente.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| Name | `string` | Sim | Não nulo, não vazio, máximo 200 caracteres |
| Code | `string` | Sim | Não nulo, não vazio, máximo 50 caracteres; único no sistema |
| UnitPrice | `decimal` | Sim | Maior ou igual a zero |
| Unit | `SupplyUnit` | Sim | Um dos valores do enum canônico; deve ser definido na criação |
| IsActive | `bool` | Sim | Padrão `true`; nunca volta a `true` após `Deactivate()` |
| CreatedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Create()` |

> **Enum `SupplyUnit`:**
> ```
> Liter, Milliliter, Gram, Kilogram, Unit
> ```

## Invariantes
1. `Code` é único no sistema — dois insumos não podem ter o mesmo código
2. `Name` nunca pode ser nulo ou vazio
3. `UnitPrice` nunca pode ser negativo
4. `Unit` deve ser um valor válido do enum
5. `IsActive` nunca retorna a `true` após ter sido definido como `false`

> **Enforcement de unicidade:**
> A unicidade de código é garantida por índice único no banco.
> Violações de unicidade devem ser traduzidas para `DomainException` em português.

## Métodos de Domínio

### Create(string name, string code, decimal unitPrice, SupplyUnit unit)
- Pré-condição: `name` não nulo/vazio (máx 200 chars); `code` não nulo/vazio (máx 50 chars); `unitPrice >= 0`; `unit` é valor válido do enum
- Ação: aplica `trim` em `name` e `code` (somente bordas) e cria a instância com `Id = Guid.NewGuid()`, `CreatedAt = DateTime.UtcNow`, `IsActive = true`
- Pós-condição: instância válida e ativa no catálogo
- Evento emitido: nenhum
- Exceções:
  - `DomainException("Nome do insumo inválido")` — se `name` for nulo ou vazio
  - `DomainException("Código do insumo inválido")` — se `code` for nulo ou vazio
  - `DomainException("Preço não pode ser negativo")` — se `unitPrice < 0`
  - `DomainException("Unidade de medida inválida")` — se `unit` não for valor válido do enum

### UpdatePrice(decimal newPrice)
- Pré-condição: `newPrice >= 0`
- Ação: atualiza `UnitPrice`
- Pós-condição: `UnitPrice == newPrice`
- Evento emitido: nenhum
- Exceção: `DomainException("Preço não pode ser negativo")`

### Deactivate()
- Pré-condição: `IsActive == true`
- Ação: define `IsActive = false`
- Pós-condição: insumo inativo; operação irreversível
- Evento emitido: nenhum
- Exceção: `DomainException("Insumo já está inativo")`

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| — | Nenhum evento de domínio (CRUD de catálogo) |

## Regras de Negócio Relacionadas
- [RN-023]: Insumos nunca são deletados fisicamente — apenas desativados
- [RN-025]: Insumos são consumíveis e não retornam ao estoque após uso (diferente de peças)

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
- [ ] Criar insumo válido com `SupplyUnit.Liter` deve criar com sucesso
- [ ] Criar insumo válido com `SupplyUnit.Gram` deve criar com sucesso
- [ ] `Name` nulo deve lançar `DomainException("Nome do insumo inválido")`
- [ ] `Name` vazio deve lançar `DomainException("Nome do insumo inválido")`
- [ ] `Code` nulo deve lançar `DomainException("Código do insumo inválido")`
- [ ] `Name` e `Code` com espaços nas bordas devem ser normalizados com `trim`
- [ ] `UnitPrice` negativo no `Create()` deve lançar `DomainException("Preço não pode ser negativo")`
- [ ] `SupplyUnit` com valor inválido de enum deve lançar `DomainException("Unidade de medida inválida")`
- [ ] `UpdatePrice(0)` deve atualizar com sucesso
- [ ] `UpdatePrice(-1)` deve lançar `DomainException("Preço não pode ser negativo")`
- [ ] Desativar insumo ativo deve definir `IsActive = false`
- [ ] Desativar insumo já inativo deve lançar `DomainException("Insumo já está inativo")`
