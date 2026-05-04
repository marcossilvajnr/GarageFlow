# Orçamento — Entidade

## Metadados
- Classe C#: `Quote`
- Bounded Context: Gestão de Ordens de Serviço
- Namespace: `GarageFlow.Domain.ServiceOrders`
- Arquivo: `GarageFlow.Domain/ServiceOrders/Quote.cs`

## Responsabilidade
Representa a proposta de custo da OS após o diagnóstico.
O orçamento calcula mão de obra, peças e insumos por serviço selecionado.

## Atributos
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| Id | `Guid` | Sim | Gerado automaticamente via `Guid.NewGuid()` |
| ServiceOrderId | `Guid` | Sim | Imutável após criação |
| Items | `IReadOnlyList<QuoteItem>` | Sim | Pelo menos 1 item |
| Version | `int` | Sim | Versão incremental dentro da OS |
| TotalAmount | `decimal` | Sim | Soma dos subtotais dos itens |
| Status | `QuoteStatus` | Sim | `Pending` -> `Approved` \| `Rejected` |
| GeneratedAt | `DateTime` | Sim | Definido em `Generate()` |
| ApprovedAt | `DateTime?` | Não | Definido em `Approve()` |
| RejectedAt | `DateTime?` | Não | Definido em `Reject()` |

### Enum QuoteStatus
`Pending | Approved | Rejected`

## Tipo Interno — QuoteItem
| Atributo | Tipo C# | Obrigatório | Regra |
|----------|---------|-------------|-------|
| ServiceId | `Guid` | Sim | Referência ao serviço do catálogo |
| ServiceName | `string` | Sim | Snapshot textual do serviço |
| LaborPrice | `decimal` | Sim | Vem de `Service.BasePrice` no momento do orçamento |
| PartsTotal | `decimal` | Sim | Soma de `Part.UnitPrice * Quantity` dos itens do serviço |
| SuppliesTotal | `decimal` | Sim | Soma de `Supply.UnitPrice * Quantity` dos itens do serviço |
| Subtotal | `decimal` | Sim | `LaborPrice + PartsTotal + SuppliesTotal` |

## Invariantes
1. `Items` nunca vazio
2. `TotalAmount` sempre igual à soma dos `Subtotal`
3. após `Approved`, orçamento não é reaberto
4. `ServiceOrderId` imutável
5. após `Rejected`, itens e valores também permanecem imutáveis
6. mudanças de escopo não editam versão existente; geram nova versão

## Métodos de Domínio

### Generate(Guid serviceOrderId, IEnumerable<QuoteItem> items)
- Pré-condição: `serviceOrderId` válido
- Pré-condição: lista com pelo menos 1 item
- Pré-condição: cada item com `ServiceId` válido e totais não negativos
- Ação: cria orçamento em `Pending`, calcula `TotalAmount`

### Approve()
- Pré-condição: `Status == Pending`
- Ação: define `Status = Approved` e `ApprovedAt`
- Exceção: `DomainException("Orçamento já foi aprovado")`

### Reject()
- Pré-condição: `Status == Pending`
- Ação: define `Status = Rejected` e `RejectedAt`
- Exceção: `DomainException("Orçamento já foi finalizado")`

## Eventos de Domínio
| Evento C# | Publicação oficial no boundary |
|-----------|-------------------------------|
| `QuoteGeneratedEvent` | Publicado por `ServiceOrder` ao gerar orçamento |
| `QuoteApprovedEvent` | Publicado por `ServiceOrder` ao aprovar orçamento |

`Quote` é entidade interna de `ServiceOrder` e não publica eventos de integração diretamente.

## Regra Canônica de Preço
- `LaborPrice` vem de `Service.BasePrice`.
- `UnitPrice` de peças e insumos é resolvido do catálogo no momento da geração.
- `ServiceItem` não armazena preço.

## Regras de Negócio Relacionadas
- [RN-007]: orçamento só após diagnóstico concluído
- [RN-008]: aprovação do orçamento cria execução
- [RN-031]: orçamento é imutável por versão e mudança gera nova versão

## Testes Obrigatórios
- [ ] gerar orçamento com item válido
- [ ] gerar orçamento sem item (erro)
- [ ] `LaborPrice` deve vir de `Service.BasePrice`
- [ ] `PartsTotal` calculado corretamente
- [ ] `SuppliesTotal` calculado corretamente
- [ ] `Subtotal = LaborPrice + PartsTotal + SuppliesTotal`
- [ ] `TotalAmount` soma os subtotais
- [ ] aprovar orçamento pendente
- [ ] aprovar orçamento já aprovado (erro)
- [ ] rejeitar orçamento pendente
- [ ] rejeitar orçamento já finalizado (erro)
- [ ] impedir edição de itens/valores após geração
