# Peça — Agregado Raiz

## Metadados
- Classe C#: `Part`
- Bounded Context: Catálogo
- Namespace: `GarageFlow.Domain.Catalog`
- Arquivo: `GarageFlow.Domain/Catalog/Part.cs`

## Responsabilidade
Representa um componente físico discreto do catálogo da oficina. Ao contrário
dos insumos, peças são contadas em unidades inteiras e podem ser devolvidas
ao estoque caso o serviço seja cancelado antes da execução. Nunca é deletada
fisicamente.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| Name | `string` | Sim | Não nulo, não vazio, máximo 200 caracteres |
| Code | `string` | Sim | Não nulo, não vazio, máximo 50 caracteres; único no sistema |
| UnitPrice | `decimal` | Sim | Maior ou igual a zero |
| IsActive | `bool` | Sim | Padrão `true`; nunca volta a `true` após `Deactivate()` |
| CreatedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Create()` |

## Invariantes
1. `Code` é único no sistema — duas peças não podem ter o mesmo código
2. `Name` nunca pode ser nulo ou vazio
3. `UnitPrice` nunca pode ser negativo
4. `IsActive` nunca retorna a `true` após ter sido definido como `false`

> **Enforcement de unicidade:**
> A unicidade de código é garantida por índice único no banco.
> Violações de unicidade devem ser traduzidas para `DomainException` em português.

## Métodos de Domínio

### Create(string name, string code, decimal unitPrice)
- Pré-condição: `name` não nulo/vazio (máx 200 chars); `code` não nulo/vazio (máx 50 chars); `unitPrice >= 0`
- Ação: aplica `trim` em `name` e `code` (somente bordas) e cria a instância com `Id = Guid.NewGuid()`, `CreatedAt = DateTime.UtcNow`, `IsActive = true`
- Pós-condição: instância válida e ativa no catálogo
- Evento emitido: nenhum
- Exceções:
  - `DomainException("Nome da peça inválido")` — se `name` for nulo ou vazio
  - `DomainException("Código da peça inválido")` — se `code` for nulo ou vazio
  - `DomainException("Preço não pode ser negativo")` — se `unitPrice < 0`

### UpdatePrice(decimal newPrice)
- Pré-condição: `newPrice >= 0`
- Ação: atualiza `UnitPrice`
- Pós-condição: `UnitPrice == newPrice`
- Evento emitido: nenhum
- Exceção: `DomainException("Preço não pode ser negativo")`

### Deactivate()
- Pré-condição: `IsActive == true`
- Ação: define `IsActive = false`
- Pós-condição: peça inativa; operação irreversível
- Evento emitido: nenhum
- Exceção: `DomainException("Peça já está inativa")`

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| — | Nenhum evento de domínio (CRUD de catálogo) |

## Regras de Negócio Relacionadas
- [RN-005]: Peças podem ser devolvidas ao estoque se o serviço for cancelado antes da execução
- [RN-016]: Operação de liberação devolve peças reservadas ao estoque disponível
- [RN-023]: Peças nunca são deletadas fisicamente — apenas desativadas
- [RN-025]: Diferença entre peça (discreta, devolvível) e insumo (consumível, não devolvível)

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
- [ ] Criar peça válida deve criar com sucesso
- [ ] `Name` nulo deve lançar `DomainException("Nome da peça inválido")`
- [ ] `Name` vazio deve lançar `DomainException("Nome da peça inválido")`
- [ ] `Code` nulo deve lançar `DomainException("Código da peça inválido")`
- [ ] `Code` vazio deve lançar `DomainException("Código da peça inválido")`
- [ ] `Name` e `Code` com espaços nas bordas devem ser normalizados com `trim`
- [ ] `UnitPrice` negativo no `Create()` deve lançar `DomainException("Preço não pode ser negativo")`
- [ ] `UpdatePrice(0)` deve atualizar com sucesso
- [ ] `UpdatePrice(-1)` deve lançar `DomainException("Preço não pode ser negativo")`
- [ ] Desativar peça ativa deve definir `IsActive = false`
- [ ] Desativar peça já inativa deve lançar `DomainException("Peça já está inativa")`
