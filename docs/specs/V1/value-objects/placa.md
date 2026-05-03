# Placa — Value Object

## Metadados
- Classe C#: `LicensePlate`
- Bounded Context: Customers
- Usado em: `Vehicle`
- Arquivo: `GarageFlow.Domain/ValueObjects/LicensePlate.cs`

## Responsabilidade
Representa a placa de identificação de um veículo. Suporta os dois formatos
vigentes no Brasil — padrão antigo e Mercosul — garantindo que apenas
placas bem formadas sejam associadas a veículos no sistema.

## Atributos
| Atributo | Tipo C# | Descrição |
|----------|---------|-----------|
| Value | string | Placa armazenada em maiúsculas sem hífen (7 caracteres) |

## Invariantes
1. Value nunca pode ser nulo ou vazio
2. Value deve ter exatamente 7 caracteres alfanuméricos em maiúsculas
3. Value deve corresponder ao formato antigo ou ao formato Mercosul

## Regras de Validação
- Normalizar para maiúsculas e remover hífens antes de validar
- Aceitar apenas letras A–Z e dígitos 0–9
- **Formato antigo:** 3 letras + 4 dígitos — regex: `^[A-Z]{3}[0-9]{4}$`
- **Formato Mercosul:** 3 letras + 1 dígito + 1 letra + 2 dígitos — regex: `^[A-Z]{3}[0-9][A-Z][0-9]{2}$`
- Qualquer outro padrão é inválido

## Comportamentos
### Create(string value)
- Pré-condição: `value` não é nulo nem vazio
- Ação: converte para maiúsculas, remove hífens e valida contra os dois formatos
- Pós-condição: instância contém placa normalizada com 7 caracteres, sem hífen
- Exceção: `DomainException("Placa inválida")`

## Implementação C#
- Tipo: `public sealed record`
- Factory method: `public static LicensePlate Create(string value)`
- Construtor: `private`
- Comparação: por valor (automática no record)

## Casos de Erro
| Entrada Inválida | Exceção |
|-----------------|---------|
| `null` | `DomainException("Placa inválida")` |
| `""` (vazio) | `DomainException("Placa inválida")` |
| `"ABC123"` (6 caracteres) | `DomainException("Placa inválida")` |
| `"ABC12345"` (8 caracteres) | `DomainException("Placa inválida")` |
| `"ABC1@34"` (caractere especial) | `DomainException("Placa inválida")` |
| `"1BC1234"` (começa com dígito) | `DomainException("Placa inválida")` |
| `"ABCD123"` (4 letras no início) | `DomainException("Placa inválida")` |

## Testes Obrigatórios
- [ ] Placa no formato antigo válido (`ABC1234`) deve criar com sucesso
- [ ] Placa no formato Mercosul válido (`ABC1D23`) deve criar com sucesso
- [ ] Placa com letras minúsculas (`abc1234`) deve normalizar para maiúsculas e criar com sucesso
- [ ] Placa com hífen (`ABC-1234`) deve remover hífen e criar com sucesso
- [ ] `null` deve lançar `DomainException("Placa inválida")`
- [ ] String vazia deve lançar `DomainException("Placa inválida")`
- [ ] Placa com formato inválido deve lançar `DomainException("Placa inválida")`
- [ ] Placa com caracteres especiais deve lançar `DomainException("Placa inválida")`
