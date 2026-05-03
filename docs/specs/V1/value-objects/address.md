# Endereço — Value Object

## Metadados
- Classe C#: `Address`
- Bounded Context: Customers, Fornecedores
- Usado em: `Customer`, `Supplier`
- Arquivo: `GarageFlow.Domain/ValueObjects/Address.cs`

## Responsabilidade
Representa o endereço físico de um cliente ou fornecedor no Brasil.
Agrupa logradouro, número, complemento, bairro, cidade, estado e CEP
em um único objeto imutável, garantindo que cada campo respeite seus limites.

## Atributos
| Atributo | Tipo C# | Descrição |
|----------|---------|-----------|
| Street | string | Logradouro (máx 200 caracteres) |
| Number | string | Número do imóvel (máx 10 caracteres) |
| Complement | string? | Complemento, opcional (máx 100 caracteres) |
| Neighborhood | string | Bairro (máx 100 caracteres) |
| City | string | Cidade (máx 100 caracteres) |
| State | string | UF brasileira em maiúsculas (exatamente 2 caracteres) |
| ZipCode | string | CEP armazenado somente com dígitos (8 caracteres) |

## Invariantes
1. Street, Number, Neighborhood e City nunca podem ser nulos ou vazios
2. State deve ser exatamente 2 letras maiúsculas e corresponder a uma UF brasileira válida
3. ZipCode deve ter exatamente 8 dígitos numéricos
4. Complement, quando informado, não pode exceder 100 caracteres

## Regras de Validação
- **Street:** não nulo, não vazio, máximo 200 caracteres
- **Number:** não nulo, não vazio, máximo 10 caracteres
- **Complement:** opcional (`null` permitido); se informado, máximo 100 caracteres
- **Neighborhood:** não nulo, não vazio, máximo 100 caracteres
- **City:** não nulo, não vazio, máximo 100 caracteres
- **State:** deve ser uma das 27 UFs brasileiras:
  `AC, AL, AP, AM, BA, CE, DF, ES, GO, MA, MT, MS, MG, PA, PB, PR, PE, PI, RJ, RN, RS, RO, RR, SC, SP, SE, TO`
  Normalizar para maiúsculas antes de validar
- **ZipCode:** remover hífen antes de validar; deve ter exatamente 8 dígitos numéricos

## Comportamentos
### Create(string street, string number, string? complement, string neighborhood, string city, string state, string zipCode)
- Pré-condição: todos os campos obrigatórios são não nulos e não vazios
- Ação: valida cada campo individualmente; normaliza State para maiúsculas e remove hífen do ZipCode
- Pós-condição: instância contém endereço válido com todos os campos normalizados
- Exceções individuais por campo (ver Casos de Erro)

## Implementação C#
- Tipo: `public sealed record`
- Factory method: `public static Address Create(string street, string number, string? complement, string neighborhood, string city, string state, string zipCode)`
- Construtor: `private`
- Comparação: por valor (automática no record)

## Casos de Erro
| Entrada Inválida | Exceção |
|-----------------|---------|
| `street` nulo ou vazio | `DomainException("Logradouro inválido")` |
| `street` com mais de 200 caracteres | `DomainException("Logradouro inválido")` |
| `number` nulo ou vazio | `DomainException("Número inválido")` |
| `number` com mais de 10 caracteres | `DomainException("Número inválido")` |
| `complement` com mais de 100 caracteres | `DomainException("Complemento inválido")` |
| `neighborhood` nulo ou vazio | `DomainException("Bairro inválido")` |
| `neighborhood` com mais de 100 caracteres | `DomainException("Bairro inválido")` |
| `city` nulo ou vazio | `DomainException("Cidade inválida")` |
| `city` com mais de 100 caracteres | `DomainException("Cidade inválida")` |
| `state` não correspondente a uma UF válida | `DomainException("UF inválida")` |
| `zipCode` com caracteres não numéricos (após remover hífen) | `DomainException("CEP inválido")` |
| `zipCode` com comprimento diferente de 8 dígitos | `DomainException("CEP inválido")` |

## Testes Obrigatórios
- [ ] Endereço completo válido deve criar com sucesso
- [ ] Endereço sem complemento (`null`) deve criar com sucesso
- [ ] CEP com hífen (`00000-000`) deve normalizar e criar com sucesso
- [ ] State em minúsculas (`sp`) deve normalizar para maiúsculas e criar com sucesso
- [ ] `street` nulo deve lançar `DomainException("Logradouro inválido")`
- [ ] `street` vazio deve lançar `DomainException("Logradouro inválido")`
- [ ] `street` com mais de 200 caracteres deve lançar `DomainException("Logradouro inválido")`
- [ ] `number` nulo deve lançar `DomainException("Número inválido")`
- [ ] `complement` com mais de 100 caracteres deve lançar `DomainException("Complemento inválido")`
- [ ] `neighborhood` nulo deve lançar `DomainException("Bairro inválido")`
- [ ] `city` nulo deve lançar `DomainException("Cidade inválida")`
- [ ] `state` com valor inexistente (ex: `"XX"`) deve lançar `DomainException("UF inválida")`
- [ ] `zipCode` com 7 dígitos deve lançar `DomainException("CEP inválido")`
- [ ] `zipCode` com letras deve lançar `DomainException("CEP inválido")`
