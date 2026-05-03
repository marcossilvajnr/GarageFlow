# CPF — Value Object

## Metadados
- Classe C#: `Cpf`
- Bounded Context: Customers
- Usado em: `Customer`
- Arquivo: `GarageFlow.Domain/ValueObjects/Cpf.cs`

## Responsabilidade
Representa o Cadastro de Pessoa Física de um cliente. Garante que apenas
CPFs matematicamente válidos sejam aceitos no sistema, eliminando dados
incorretos desde a criação do agregado.

## Atributos
| Atributo | Tipo C# | Descrição |
|----------|---------|-----------|
| Value | string | CPF armazenado somente com dígitos (11 caracteres) |

## Invariantes
1. Value nunca pode ser nulo, vazio ou conter caracteres não numéricos
2. Value nunca pode representar um CPF com todos os dígitos iguais
3. Os dois dígitos verificadores devem ser válidos pelo algoritmo Mod 11

## Regras de Validação
- Remover pontos e traços antes de validar
- Deve ter exatamente 11 dígitos numéricos após normalização
- CPFs com todos os dígitos iguais são inválidos (ex: `11111111111`)
- **Algoritmo Mod 11 — 1º dígito verificador:**
  - Multiplicar os 9 primeiros dígitos pelos pesos 10, 9, 8, 7, 6, 5, 4, 3, 2 (esquerda→direita)
  - Somar os produtos; calcular `resto = soma % 11`
  - Se `resto < 2`: dígito = `0`; caso contrário: dígito = `11 - resto`
  - Comparar com o 10º dígito
- **Algoritmo Mod 11 — 2º dígito verificador:**
  - Multiplicar os 10 primeiros dígitos pelos pesos 11, 10, 9, 8, 7, 6, 5, 4, 3, 2
  - Mesma fórmula de cálculo
  - Comparar com o 11º dígito

## Comportamentos
### Create(string value)
- Pré-condição: `value` não é nulo nem vazio
- Ação: remove formatação, valida comprimento, verifica dígitos iguais e executa Mod 11
- Pós-condição: instância contém CPF válido com exatamente 11 dígitos
- Exceção: `DomainException("CPF inválido")`

## Implementação C#
- Tipo: `public sealed record`
- Factory method: `public static Cpf Create(string value)`
- Construtor: `private`
- Comparação: por valor (automática no record)

## Casos de Erro
| Entrada Inválida | Exceção |
|-----------------|---------|
| `null` | `DomainException("CPF inválido")` |
| `""` (vazio) | `DomainException("CPF inválido")` |
| `"1234567890"` (10 dígitos) | `DomainException("CPF inválido")` |
| `"123456789012"` (12 dígitos) | `DomainException("CPF inválido")` |
| `"11111111111"` (todos iguais) | `DomainException("CPF inválido")` |
| CPF com dígito verificador incorreto | `DomainException("CPF inválido")` |

## Testes Obrigatórios
- [ ] CPF válido sem formatação deve criar com sucesso
- [ ] CPF válido com máscara (`000.000.000-00`) deve criar e armazenar sem formatação
- [ ] `null` deve lançar `DomainException("CPF inválido")`
- [ ] String vazia deve lançar `DomainException("CPF inválido")`
- [ ] CPF com menos de 11 dígitos deve lançar `DomainException("CPF inválido")`
- [ ] CPF com mais de 11 dígitos deve lançar `DomainException("CPF inválido")`
- [ ] CPF com todos os dígitos iguais deve lançar `DomainException("CPF inválido")`
- [ ] CPF com 1º dígito verificador incorreto deve lançar `DomainException("CPF inválido")`
- [ ] CPF com 2º dígito verificador incorreto deve lançar `DomainException("CPF inválido")`
