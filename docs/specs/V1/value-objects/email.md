# E-mail — Value Object

## Metadados
- Classe C#: `Email`
- Bounded Context: Customers, Fornecedores
- Usado em: `Customer`, `Supplier`
- Arquivo: `GarageFlow.Domain/ValueObjects/Email.cs`

## Responsabilidade
Representa o endereço de e-mail de um cliente ou fornecedor. Garante
estrutura mínima válida e normalização para minúsculas, permitindo
comparação e busca consistentes no sistema.

## Atributos
| Atributo | Tipo C# | Descrição |
|----------|---------|-----------|
| Value | string | E-mail normalizado em minúsculas (máx 320 caracteres) |

## Invariantes
1. Value nunca pode ser nulo ou vazio
2. Value deve conter exatamente um caractere `@`
3. Value deve respeitar a estrutura `local@domínio` com domínio contendo pelo menos um ponto

## Regras de Validação
- Normalizar para minúsculas antes de validar e armazenar
- Não pode ser nulo ou vazio
- Deve conter exatamente um `@`
- Deve ter pelo menos um caractere antes do `@`
- A parte do domínio (após `@`) deve conter pelo menos um ponto
- Deve ter pelo menos dois caracteres após o último ponto do domínio
- Comprimento máximo: 320 caracteres (padrão RFC 5321)

## Comportamentos
### Create(string value)
- Pré-condição: `value` não é nulo nem vazio
- Ação: converte para minúsculas e valida a estrutura do e-mail
- Pós-condição: instância contém e-mail válido em minúsculas
- Exceção: `DomainException("E-mail inválido")`

## Implementação C#
- Tipo: `public sealed record`
- Factory method: `public static Email Create(string value)`
- Construtor: `private`
- Comparação: por valor (automática no record)

## Casos de Erro
| Entrada Inválida | Exceção |
|-----------------|---------|
| `null` | `DomainException("E-mail inválido")` |
| `""` (vazio) | `DomainException("E-mail inválido")` |
| `"semArroba"` | `DomainException("E-mail inválido")` |
| `"dois@@arroba.com"` | `DomainException("E-mail inválido")` |
| `"@dominio.com"` (sem local) | `DomainException("E-mail inválido")` |
| `"usuario@semPonto"` | `DomainException("E-mail inválido")` |
| `"usuario@dominio.c"` (TLD < 2 chars) | `DomainException("E-mail inválido")` |
| E-mail com mais de 320 caracteres | `DomainException("E-mail inválido")` |

## Testes Obrigatórios
- [ ] E-mail válido em minúsculas deve criar com sucesso
- [ ] E-mail com letras maiúsculas deve normalizar para minúsculas e criar com sucesso
- [ ] `null` deve lançar `DomainException("E-mail inválido")`
- [ ] String vazia deve lançar `DomainException("E-mail inválido")`
- [ ] E-mail sem `@` deve lançar `DomainException("E-mail inválido")`
- [ ] E-mail com múltiplos `@` deve lançar `DomainException("E-mail inválido")`
- [ ] E-mail sem parte local (começa com `@`) deve lançar `DomainException("E-mail inválido")`
- [ ] E-mail sem ponto no domínio deve lançar `DomainException("E-mail inválido")`
- [ ] E-mail com TLD de 1 caractere deve lançar `DomainException("E-mail inválido")`
- [ ] E-mail com mais de 320 caracteres deve lançar `DomainException("E-mail inválido")`
