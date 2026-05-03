# Cliente — Agregado Raiz

## Metadados
- Classe C#: `Customer`
- Bounded Context: Gestão de Clientes
- Namespace: `GarageFlow.Domain.Customers`
- Arquivo: `GarageFlow.Domain/Customers/Customer.cs`

## Responsabilidade
Representa um cliente da oficina, podendo ser pessoa física (identificado por CPF)
ou jurídica (identificado por CNPJ). É o agregado raiz do contexto de clientes —
todo veículo pertence a um `Customer`. Nunca é deletado fisicamente.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| Name | `string` | Sim | Não nulo, não vazio, máximo 200 caracteres |
| Document | `Cpf` ou `Cnpj` | Sim | Exatamente um dos dois; único no sistema |
| Email | `Email` | Sim | Value Object válido |
| PhoneNumber | `PhoneNumber` | Sim | Value Object válido |
| Address | `Address` | Sim | Value Object válido |
| IsActive | `bool` | Sim | Padrão `true`; nunca volta a `true` após `Deactivate()` |
| CreatedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Create()` |

> **Nota de implementação:** `Document` é uma união de `Cpf` e `Cnpj`.
> Em C#, representar como `object Document` com propriedades `Cpf? Cpf` e
> `Cnpj? Cnpj` (exatamente uma não-nula), ou via type hierarchy
> (`CustomerDocument` abstrato com `CpfDocument` e `CnpjDocument`).

## Invariantes
1. `Document` é único no sistema — dois clientes não podem ter o mesmo CPF ou CNPJ (RN-021)
2. `Name` nunca pode ser nulo ou vazio
3. `IsActive` nunca retorna a `true` após ter sido definido como `false`
4. `Document` nunca pode ser alterado após a criação

> **Enforcement de unicidade:**
> A unicidade de CPF/CNPJ é garantida por índice único no banco.
> Violações de unicidade devem ser traduzidas para `DomainException` em português.

## Métodos de Domínio

### Create(string name, Cpf | Cnpj document, Email email, PhoneNumber phone, Address address)
- Pré-condição: `name` não nulo/vazio (máx 200 chars); todos os VOs válidos
- Ação: aplica `trim` em `name` (somente bordas) e cria a instância com `Id = Guid.NewGuid()`, `CreatedAt = DateTime.UtcNow`, `IsActive = true`
- Pós-condição: instância válida e ativa
- Evento emitido: `CustomerCreatedEvent`
- Exceção: `DomainException("Nome do cliente inválido")`

### Deactivate()
- Pré-condição: `IsActive == true`
- Ação: define `IsActive = false`
- Pós-condição: cliente inativo; operação irreversível
- Evento emitido: `CustomerDeactivatedEvent`
- Exceção: `DomainException("Cliente já está inativo")`

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| `CustomerCreatedEvent` | Ao criar um novo cliente |
| `CustomerDeactivatedEvent` | Ao desativar um cliente |

## Regras de Negócio Relacionadas
- [RN-021]: CPF é único para PF; CNPJ é único para PJ — dois clientes não podem compartilhar documento
- [RN-023]: Clientes nunca são deletados fisicamente — apenas desativados

## Implementação C#
- Construtor privado
- Factory method estático `Create()`
- Propriedades com `private set`
- Exceções sempre via `DomainException`
- Normalização textual: aplicar `trim` nas bordas em entradas de texto do agregado

## Dependências
- Value Objects: `Cpf`, `Cnpj`, `Email`, `PhoneNumber`, `Address`
- Agregados: nenhum

## Testes Obrigatórios
- [ ] Criar cliente válido com CPF deve criar com sucesso e emitir `CustomerCreatedEvent`
- [ ] Criar cliente válido com CNPJ deve criar com sucesso e emitir `CustomerCreatedEvent`
- [ ] `Name` nulo deve lançar `DomainException("Nome do cliente inválido")`
- [ ] `Name` vazio deve lançar `DomainException("Nome do cliente inválido")`
- [ ] `Name` com mais de 200 caracteres deve lançar `DomainException("Nome do cliente inválido")`
- [ ] `Name` com espaços nas bordas deve ser normalizado com `trim`
- [ ] Desativar cliente ativo deve definir `IsActive = false` e emitir `CustomerDeactivatedEvent`
- [ ] Desativar cliente já inativo deve lançar `DomainException("Cliente já está inativo")`
- [ ] `CreatedAt` deve ser definido automaticamente no `Create()`
- [ ] `Id` deve ser gerado automaticamente no `Create()`
