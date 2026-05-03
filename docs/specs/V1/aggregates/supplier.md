# Fornecedor — Agregado Raiz

## Metadados
- Classe C#: `Supplier`
- Bounded Context: Fornecedores
- Namespace: `GarageFlow.Domain.Suppliers`
- Arquivo: `GarageFlow.Domain/Suppliers/Supplier.cs`

## Responsabilidade
Representa um fornecedor de peças e insumos. Identificado de forma única por
CNPJ. Fornece o `SupplierId` referenciado pelas Ordens de Compra no contexto
de Compras. Nunca é deletado fisicamente.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| Name | `string` | Sim | Não nulo, não vazio, máximo 200 caracteres |
| Cnpj | `Cnpj` | Sim | Value Object válido; único no sistema |
| Email | `Email` | Sim | Value Object válido |
| PhoneNumber | `PhoneNumber` | Sim | Value Object válido |
| Address | `Address` | Sim | Value Object válido |
| IsActive | `bool` | Sim | Padrão `true`; nunca volta a `true` após `Deactivate()` |
| CreatedAt | `DateTime` | Sim | Definido como `DateTime.UtcNow` no `Create()` |

## Invariantes
1. `Cnpj` é único no sistema — dois fornecedores não podem ter o mesmo CNPJ
2. `Name` nunca pode ser nulo ou vazio
3. `IsActive` nunca retorna a `true` após ter sido definido como `false`
4. `Cnpj` nunca pode ser alterado após a criação

> **Enforcement de unicidade:**
> A unicidade de CNPJ é garantida por índice único no banco.
> Violações de unicidade devem ser traduzidas para `DomainException` em português.

## Métodos de Domínio

### Create(string name, Cnpj cnpj, Email email, PhoneNumber phone, Address address)
- Pré-condição: `name` não nulo/vazio (máx 200 chars); todos os VOs válidos
- Ação: aplica `trim` em `name` (somente bordas) e cria a instância com `Id = Guid.NewGuid()`, `CreatedAt = DateTime.UtcNow`, `IsActive = true`
- Pós-condição: instância válida e ativa
- Evento emitido: `SupplierCreatedEvent`
- Exceção: `DomainException("Nome do fornecedor inválido")`

### Deactivate()
- Pré-condição: `IsActive == true`
- Ação: define `IsActive = false`
- Pós-condição: fornecedor inativo; operação irreversível
- Evento emitido: `SupplierDeactivatedEvent`
- Exceção: `DomainException("Fornecedor já está inativo")`

## Eventos de Domínio
| Evento C# | Quando é emitido |
|-----------|-----------------|
| `SupplierCreatedEvent` | Ao cadastrar um novo fornecedor |
| `SupplierDeactivatedEvent` | Ao desativar um fornecedor |

## Regras de Negócio Relacionadas
- [RN-019]: O administrativo seleciona o fornecedor antes de iniciar uma Ordem de Compra
- [RN-023]: Fornecedores nunca são deletados fisicamente — apenas desativados

## Implementação C#
- Construtor privado
- Factory method estático `Create()`
- Propriedades com `private set`
- Exceções sempre via `DomainException`
- Normalização textual: aplicar `trim` nas bordas em entradas de texto do agregado

## Dependências
- Value Objects: `Cnpj`, `Email`, `PhoneNumber`, `Address`
- Agregados: nenhum

## Testes Obrigatórios
- [ ] Criar fornecedor válido deve criar com sucesso e emitir `SupplierCreatedEvent`
- [ ] `Name` nulo deve lançar `DomainException("Nome do fornecedor inválido")`
- [ ] `Name` vazio deve lançar `DomainException("Nome do fornecedor inválido")`
- [ ] `Name` com mais de 200 caracteres deve lançar `DomainException("Nome do fornecedor inválido")`
- [ ] `Name` com espaços nas bordas deve ser normalizado com `trim`
- [ ] Desativar fornecedor ativo deve definir `IsActive = false` e emitir `SupplierDeactivatedEvent`
- [ ] Desativar fornecedor já inativo deve lançar `DomainException("Fornecedor já está inativo")`
- [ ] `CreatedAt` deve ser definido automaticamente no `Create()`
- [ ] `Id` deve ser gerado automaticamente no `Create()`
