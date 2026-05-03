# GarageFlow — Catálogo Canônico de Value Objects

## Objetivo
Este documento descreve os value objects canônicos do domínio no estado atual.

## Padrão Canônico de Implementação
Todos os VOs seguem:
- `record` imutável
- factory estático `Create()`
- construtor privado
- igualdade por valor
- validações com `DomainException` em português

## Catálogo
| VO (PT) | Classe (EN) | Uso Canônico | Bounded Context |
|---|---|---|---|
| CPF | `Cpf` | `Customer` | Customers |
| CNPJ | `Cnpj` | `Customer`, `Supplier` | Customers, Fornecedores |
| E-mail | `Email` | `Customer`, `Supplier` | Customers, Fornecedores |
| Telefone | `PhoneNumber` | `Customer`, `Supplier` | Customers, Fornecedores |
| Endereço | `Address` | `Customer`, `Supplier` | Customers, Fornecedores |
| Placa | `LicensePlate` | `Vehicle` | Customers |
| RENAVAM | `Renavam` | `Vehicle` | Customers |

## Regras Canônicas de Normalização
- Campos de texto em agregados: aplicar `trim` nas bordas e preservar espaços internos.
- VOs com documento/identificador aplicam normalização específica do próprio VO (máscara, dígitos e case), conforme contrato de validação.

## Invariantes por VO
### Cpf
- Sempre 11 dígitos válidos pelo algoritmo oficial.
- Nunca aceita valor nulo, vazio, não numérico ou com dígitos verificadores inválidos.

### Cnpj
- Sempre 14 dígitos válidos pelo algoritmo oficial.
- Nunca aceita valor nulo, vazio, não numérico, todos dígitos iguais ou dígitos verificadores inválidos.

### Email
- Formato de e-mail obrigatório.
- Não aceita valor nulo, vazio ou fora do formato esperado.

### PhoneNumber
- Somente dígitos válidos para telefone.
- Não aceita valor nulo, vazio ou quantidade inválida de dígitos.

### Address
- Campos obrigatórios mínimos preenchidos.
- Não aceita composição incompleta para endereço válido.

### LicensePlate
- Formato de placa válido conforme padrão aceito no domínio.
- Não aceita valor nulo, vazio ou fora do padrão.

### Renavam
- Sempre 11 dígitos válidos pelo algoritmo oficial.
- Não aceita valor nulo, vazio, não numérico ou dígitos verificadores inválidos.

## Decisões Vigentes
- `Money` não é VO; valores monetários permanecem `decimal` nos agregados.
- `Name` não é VO; nomes permanecem `string` validadas nos agregados.

## Unicidade Relacionada
Quando um VO participa de regra de unicidade de agregado (ex.: `Cpf`, `Cnpj`, `LicensePlate`, `Renavam`), a garantia é por índice único no banco com tradução de violação para `DomainException` em português.
