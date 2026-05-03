# RENAVAM — Value Object

## Metadados
- Classe C#: `Renavam`
- Bounded Context: Customers
- Usado em: `Vehicle`
- Arquivo: `GarageFlow.Domain/ValueObjects/Renavam.cs`

## Responsabilidade
Representa o Registro Nacional de Veículos Automotores. Identifica
unicamente um veículo junto ao DETRAN. Suporta o formato antigo (9 dígitos)
para veículos registrados antes da padronização nacional, e o formato atual
(11 dígitos) com validação completa de dígito verificador.

## Atributos
| Atributo | Tipo C# | Descrição |
|----------|---------|-----------|
| Value | string | RENAVAM armazenado somente com dígitos (9 ou 11 caracteres) |

## Invariantes
1. Value nunca pode ser nulo, vazio ou conter caracteres não numéricos
2. Value deve ter exatamente 9 ou 11 dígitos
3. RENAVAM de 11 dígitos deve ter dígito verificador válido pelo algoritmo Mod 11

## Regras de Validação
- Remover espaços antes de validar
- Deve ter exatamente 9 ou 11 dígitos numéricos
- **RENAVAM de 9 dígitos:** validação apenas de comprimento e dígitos numéricos
  (formato legado — oficinas possuem veículos antigos com esse registro)
- **RENAVAM de 11 dígitos — algoritmo Mod 11:**
  - Pesos: `[3, 2, 9, 8, 7, 6, 5, 4, 3, 2]` aplicados aos 10 primeiros dígitos (esquerda→direita)
  - Somar os produtos; calcular `resto = soma % 11`
  - Se `resto < 2`: dígito verificador = `0`; caso contrário: dígito = `11 - resto`
  - Comparar com o 11º (último) dígito
- Qualquer comprimento fora de 9 ou 11 é inválido (ex: 10 dígitos é inválido)

## Comportamentos
### Create(string value)
- Pré-condição: `value` não é nulo nem vazio
- Ação: remove espaços, valida comprimento (9 ou 11) e, se 11 dígitos, executa Mod 11
- Pós-condição: instância contém RENAVAM válido somente com dígitos
- Exceção: `DomainException("RENAVAM inválido")`

## Implementação C#
- Tipo: `public sealed record`
- Factory method: `public static Renavam Create(string value)`
- Construtor: `private`
- Comparação: por valor (automática no record)

## Casos de Erro
| Entrada Inválida | Exceção |
|-----------------|---------|
| `null` | `DomainException("RENAVAM inválido")` |
| `""` (vazio) | `DomainException("RENAVAM inválido")` |
| `"12345678"` (8 dígitos) | `DomainException("RENAVAM inválido")` |
| `"1234567890"` (10 dígitos) | `DomainException("RENAVAM inválido")` |
| `"123456789012"` (12 dígitos) | `DomainException("RENAVAM inválido")` |
| `"1234567ABC"` (letras) | `DomainException("RENAVAM inválido")` |
| RENAVAM de 11 dígitos com dígito verificador incorreto | `DomainException("RENAVAM inválido")` |

## Testes Obrigatórios
- [ ] RENAVAM de 9 dígitos válido deve criar com sucesso
- [ ] RENAVAM de 11 dígitos com dígito verificador correto deve criar com sucesso
- [ ] `null` deve lançar `DomainException("RENAVAM inválido")`
- [ ] String vazia deve lançar `DomainException("RENAVAM inválido")`
- [ ] RENAVAM com menos de 9 dígitos deve lançar `DomainException("RENAVAM inválido")`
- [ ] RENAVAM com 10 dígitos deve lançar `DomainException("RENAVAM inválido")`
- [ ] RENAVAM com mais de 11 dígitos deve lançar `DomainException("RENAVAM inválido")`
- [ ] RENAVAM de 11 dígitos com dígito verificador incorreto deve lançar `DomainException("RENAVAM inválido")`
