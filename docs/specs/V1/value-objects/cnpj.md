# CNPJ — Value Object

## Metadados
- Classe C#: `Cnpj`
- Bounded Context: Customers, Fornecedores
- Usado em: `Customer`, `Supplier`
- Arquivo: `GarageFlow.Domain/ValueObjects/Cnpj.cs`

## Responsabilidade
Representa o Cadastro Nacional da Pessoa Jurídica. Utilizado tanto para
clientes pessoa jurídica quanto para fornecedores, garantindo que apenas
CNPJs matematicamente válidos sejam registrados no sistema.

## Atributos
| Atributo | Tipo C# | Descrição |
|----------|---------|-----------|
| Value | string | CNPJ armazenado somente com dígitos (14 caracteres) |

## Invariantes
1. Value nunca pode ser nulo, vazio ou conter caracteres não numéricos
2. Value nunca pode representar um CNPJ com todos os dígitos iguais
3. Os dois dígitos verificadores devem ser válidos pelo algoritmo Mod 11

## Regras de Validação
- Remover pontos, barras e traços antes de validar
- Deve ter exatamente 14 dígitos numéricos após normalização
- CNPJs com todos os dígitos iguais são inválidos (ex: `11111111111111`)
- **Algoritmo Mod 11 — 1º dígito verificador:**
  - Pesos: `[5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]`
  - Multiplicar os 12 primeiros dígitos pelos pesos respectivos (esquerda→direita)
  - Somar os produtos; calcular `resto = soma % 11`
  - Se `resto < 2`: dígito = `0`; caso contrário: dígito = `11 - resto`
  - Comparar com o 13º dígito
- **Algoritmo Mod 11 — 2º dígito verificador:**
  - Pesos: `[6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]`
  - Multiplicar os 13 primeiros dígitos pelos pesos respectivos
  - Mesma fórmula de cálculo
  - Comparar com o 14º dígito

## Comportamentos
### Create(string value)
- Pré-condição: `value` não é nulo nem vazio
- Ação: remove formatação, valida comprimento, verifica dígitos iguais e executa Mod 11
- Pós-condição: instância contém CNPJ válido com exatamente 14 dígitos
- Exceção: `DomainException("CNPJ inválido")`

## Implementação C#
- Tipo: `public sealed record`
- Factory method: `public static Cnpj Create(string value)`
- Construtor: `private`
- Comparação: por valor (automática no record)

## Casos de Erro
| Entrada Inválida | Exceção |
|-----------------|---------|
| `null` | `DomainException("CNPJ inválido")` |
| `""` (vazio) | `DomainException("CNPJ inválido")` |
| `"1234567890123"` (13 dígitos) | `DomainException("CNPJ inválido")` |
| `"123456789012345"` (15 dígitos) | `DomainException("CNPJ inválido")` |
| `"11111111111111"` (todos iguais) | `DomainException("CNPJ inválido")` |
| CNPJ com dígito verificador incorreto | `DomainException("CNPJ inválido")` |
| CNPJ com máscara e dígito incorreto | `DomainException("CNPJ inválido")` |

## Testes Obrigatórios
- [ ] CNPJ válido sem formatação deve criar com sucesso
- [ ] CNPJ válido com máscara (`00.000.000/0000-00`) deve criar e armazenar sem formatação
- [ ] `null` deve lançar `DomainException("CNPJ inválido")`
- [ ] String vazia deve lançar `DomainException("CNPJ inválido")`
- [ ] CNPJ com menos de 14 dígitos deve lançar `DomainException("CNPJ inválido")`
- [ ] CNPJ com mais de 14 dígitos deve lançar `DomainException("CNPJ inválido")`
- [ ] CNPJ com todos os dígitos iguais deve lançar `DomainException("CNPJ inválido")`
- [ ] CNPJ com 1º dígito verificador incorreto deve lançar `DomainException("CNPJ inválido")`
- [ ] CNPJ com 2º dígito verificador incorreto deve lançar `DomainException("CNPJ inválido")`
