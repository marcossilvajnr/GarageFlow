# Veículo — Agregado Raiz

## Metadados
- Classe C#: `Vehicle`
- Bounded Context: Gestão de Clientes
- Namespace: `GarageFlow.Domain.Customers`
- Arquivo: `GarageFlow.Domain/Customers/Vehicle.cs`

## Responsabilidade
Representa um veículo cadastrado na oficina e pertencente a um cliente.
Identificado de forma única por placa e RENAVAM. `CustomerId` é imutável —
trocar o dono após o cadastro não é uma operação permitida. Nunca é deletado
fisicamente.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| CustomerId | `Guid` | Sim | Imutável após criação; deve referenciar um `Customer` existente |
| LicensePlate | `LicensePlate` | Sim | Value Object válido; único no sistema |
| Renavam | `Renavam` | Sim | Value Object válido; único no sistema |
| Brand | `string` | Sim | Não nulo, não vazio, máximo 50 caracteres |
| Model | `string` | Sim | Não nulo, não vazio, máximo 100 caracteres |
| Year | `int` | Sim | Entre 1900 e `DateTime.UtcNow.Year + 1` |
| IsActive | `bool` | Sim | Padrão `true`; nunca volta a `true` após `Deactivate()` |
| CreatedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Create()` |

## Invariantes
1. `CustomerId` nunca pode ser alterado após a criação (RN-002)
2. `LicensePlate` é única no sistema — dois veículos não podem ter a mesma placa (RN-022)
3. `Renavam` é único no sistema — dois veículos não podem ter o mesmo RENAVAM (RN-022)
4. `Year` deve estar entre 1900 e o ano corrente + 1 (avaliado em `DateTime.UtcNow.Year`)
5. `IsActive` nunca retorna a `true` após ter sido definido como `false`

> **Enforcement de unicidade:**
> A unicidade de placa/RENAVAM é garantida por índice único no banco.
> Violações de unicidade devem ser traduzidas para `DomainException` em português.

## Métodos de Domínio

### Create(Guid customerId, LicensePlate licensePlate, Renavam renavam, string brand, string model, int year)
- Pré-condição: `customerId` não é `Guid.Empty`; todos os VOs válidos; `brand` e `model` não nulos/vazios; `year` no intervalo válido
- Ação: aplica `trim` em `brand` e `model` (somente bordas) e cria a instância com `Id = Guid.NewGuid()`, `CreatedAt = DateTime.UtcNow`, `IsActive = true`
- Pós-condição: instância válida e ativa associada ao cliente
- Evento emitido: `VehicleCreatedEvent`
- Exceções:
  - `DomainException("Id do cliente inválido")` — se `customerId` for `Guid.Empty`
  - `DomainException("Marca inválida")` — se `brand` for nulo ou vazio
  - `DomainException("Modelo inválido")` — se `model` for nulo ou vazio
  - `DomainException("Ano do veículo inválido")` — se `year` estiver fora do intervalo

### Deactivate()
- Pré-condição: `IsActive == true`
- Ação: define `IsActive = false`
- Pós-condição: veículo inativo; operação irreversível
- Evento emitido: `VehicleDeactivatedEvent`
- Exceção: `DomainException("Veículo já está inativo")`

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| `VehicleCreatedEvent` | Ao cadastrar um novo veículo |
| `VehicleDeactivatedEvent` | Ao desativar um veículo |

## Regras de Negócio Relacionadas
- [RN-002]: `CustomerId` é imutável após a criação — o veículo pertence ao cliente original
- [RN-022]: Placa e RENAVAM são únicos no sistema
- [RN-023]: Veículos nunca são deletados fisicamente — apenas desativados

## Implementação C#
- Construtor privado
- Factory method estático `Create()`
- Propriedades com `private set`
- Exceções sempre via `DomainException`
- Normalização textual: aplicar `trim` nas bordas em entradas de texto do agregado

## Dependências
- Value Objects: `LicensePlate`, `Renavam`
- Agregados: `Customer` (referenciado por `CustomerId`)

## Testes Obrigatórios
- [ ] Criar veículo válido deve criar com sucesso e emitir `VehicleCreatedEvent`
- [ ] `customerId` igual a `Guid.Empty` deve lançar `DomainException("Id do cliente inválido")`
- [ ] `Brand` nulo deve lançar `DomainException("Marca inválida")`
- [ ] `Model` vazio deve lançar `DomainException("Modelo inválido")`
- [ ] `Brand` e `Model` com espaços nas bordas devem ser normalizados com `trim`
- [ ] `Year` igual a 1899 deve lançar `DomainException("Ano do veículo inválido")`
- [ ] `Year` igual a `DateTime.UtcNow.Year + 2` deve lançar `DomainException("Ano do veículo inválido")`
- [ ] `Year` igual a `DateTime.UtcNow.Year + 1` deve criar com sucesso
- [ ] Desativar veículo ativo deve definir `IsActive = false` e emitir `VehicleDeactivatedEvent`
- [ ] Desativar veículo já inativo deve lançar `DomainException("Veículo já está inativo")`
