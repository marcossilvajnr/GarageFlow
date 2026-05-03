# Telefone — Value Object

## Metadados
- Classe C#: `PhoneNumber`
- Bounded Context: Customers, Fornecedores
- Usado em: `Customer`, `Supplier`
- Arquivo: `GarageFlow.Domain/ValueObjects/PhoneNumber.cs`

## Responsabilidade
Representa o número de telefone de um cliente ou fornecedor no formato
brasileiro. Suporta telefones fixos (8 dígitos) e celulares (9 dígitos),
ambos precedidos pelo DDD de 2 dígitos.

## Atributos
| Atributo | Tipo C# | Descrição |
|----------|---------|-----------|
| Value | string | Telefone armazenado somente com dígitos (10 ou 11 caracteres) |

## Invariantes
1. Value nunca pode ser nulo ou vazio
2. Value deve ter exatamente 10 dígitos (fixo) ou 11 dígitos (celular)
3. O DDD (2 primeiros dígitos) deve estar entre 11 e 99
4. Celular deve começar com dígito 9 (após o DDD); fixo com dígitos 2–8

## Regras de Validação
- Remover todos os caracteres não numéricos antes de validar
- Deve ter entre 10 e 11 dígitos após normalização
- **DDD:** primeiros 2 dígitos, valor entre 11 e 99 (DDDs 00 a 10 são inválidos)
- **Telefone fixo (10 dígitos):** número com 8 dígitos iniciando com 2, 3, 4, 5, 6, 7 ou 8
- **Celular (11 dígitos):** número com 9 dígitos iniciando obrigatoriamente com 9

## Comportamentos
### Create(string value)
- Pré-condição: `value` não é nulo nem vazio
- Ação: remove caracteres não numéricos, valida comprimento, DDD e dígito inicial do número
- Pós-condição: instância contém telefone válido somente com dígitos
- Exceção: `DomainException("Telefone inválido")`

## Implementação C#
- Tipo: `public sealed record`
- Factory method: `public static PhoneNumber Create(string value)`
- Construtor: `private`
- Comparação: por valor (automática no record)

## Casos de Erro
| Entrada Inválida | Exceção |
|-----------------|---------|
| `null` | `DomainException("Telefone inválido")` |
| `""` (vazio) | `DomainException("Telefone inválido")` |
| `"119876543"` (9 dígitos) | `DomainException("Telefone inválido")` |
| `"1198765432101"` (13 dígitos) | `DomainException("Telefone inválido")` |
| `"0198765432"` (DDD 01) | `DomainException("Telefone inválido")` |
| `"0098765432"` (DDD 00) | `DomainException("Telefone inválido")` |
| `"1101234567"` (fixo começando com 1) | `DomainException("Telefone inválido")` |
| `"11087654321"` (celular não começando com 9) | `DomainException("Telefone inválido")` |

## Testes Obrigatórios
- [ ] Celular válido com 11 dígitos deve criar com sucesso
- [ ] Telefone fixo válido com 10 dígitos deve criar com sucesso
- [ ] Telefone com formatação `(11) 98765-4321` deve normalizar e criar com sucesso
- [ ] `null` deve lançar `DomainException("Telefone inválido")`
- [ ] String vazia deve lançar `DomainException("Telefone inválido")`
- [ ] Telefone com menos de 10 dígitos deve lançar `DomainException("Telefone inválido")`
- [ ] Telefone com mais de 11 dígitos deve lançar `DomainException("Telefone inválido")`
- [ ] DDD `00` deve lançar `DomainException("Telefone inválido")`
- [ ] DDD `01` deve lançar `DomainException("Telefone inválido")`
- [ ] Celular sem o dígito 9 inicial deve lançar `DomainException("Telefone inválido")`
