# GarageFlow — Bounded Contexts

## Visão Geral
O domínio do GarageFlow é dividido em 8 contextos delimitados.
Cada contexto tem responsabilidade única e se comunica com os demais por eventos de integração.

---

## Mapa de Contextos

```mermaid
graph TD
    subgraph BC1[Gestão de Clientes]
        Cliente
        Veiculo
    end

    subgraph BC2[Catálogo]
        Servico
        Peca
        Insumo
    end

    subgraph BC3[Fornecedores]
        Fornecedor
    end

    subgraph BC4[Gestão de Pessoas]
        Funcionario
    end

    subgraph BC5[Gestão de Ordens de Serviço]
        OrdemDeServico
        Diagnostico
        Orcamento
    end

    subgraph BC6[Produção]
        OrdemDeExecucao
    end

    subgraph BC7[Gestão de Estoque]
        Estoque
        OrdemDeSeparacao
    end

    subgraph BC8[Compras]
        OrdemDeCompra
    end

    BC1 -->|CustomerId, VehicleId| BC5
    BC2 -->|ServiceId + composição| BC5
    BC2 -->|PartId, SupplyId| BC7
    BC3 -->|SupplierId| BC8
    BC5 -->|QuoteApprovedEvent| BC6
    BC6 -->|ExecutionOrderCreatedEvent| BC7
    BC6 -->|ExecutionOrderCompletedEvent| BC5
    BC7 -->|SeparationOrderCompletedEvent| BC6
    BC7 -->|InsufficientStockEvent| BC8
    BC8 -->|PurchaseOrderCompletedEvent| BC7
```

---

## 1. Gestão de Clientes

Responsabilidade: cadastro e histórico de clientes e veículos.

Agregados:
- `Customer` (CPF/CNPJ)
- `Vehicle` (LicensePlate/Renavam)

Regras críticas:
- CPF/CNPJ únicos
- Placa/RENAVAM únicos
- veículo pertence a um único cliente
- remoção lógica (soft delete)

Comunicações:
- fornece `CustomerId` e `VehicleId` para Gestão de Ordens de Serviço

---

## 2. Catálogo

Responsabilidade: definição de serviços, peças e insumos ofertados.

Agregados:
- `Service`
- `Part`
- `Supply`

Regras críticas:
- serviço, peça e insumo são desativáveis (soft delete)
- `Service` mantém composição pré-definida de peças e insumos
- composição de serviço só aceita itens não duplicados e quantidades > 0
- `BasePrice` do serviço é a fonte de preço de mão de obra no orçamento
- `Part` mantém `Code` e `Sku` como identificadores de catálogo
- `Supply` mantém `Code`, `UnitOfMeasure`, `BaseCost` e pode referenciar `PreferredSupplierId`

Comunicações:
- fornece `ServiceId` e composição para Gestão de Ordens de Serviço
- fornece `PartId` e `SupplyId` para Gestão de Estoque

---

## 3. Fornecedores

Responsabilidade: cadastro de fornecedores de peças e insumos.

Agregado:
- `Supplier`

Regras críticas:
- CNPJ único
- remoção lógica (soft delete)

Comunicações:
- fornece `SupplierId` para Compras

---

## 4. Gestão de Pessoas

Responsabilidade: cadastro e ciclo de vida de funcionários internos do sistema.

Agregado:
- `Employee`

Regras críticas:
- CPF/CNPJ únicos no contexto de funcionários
- cargo obrigatório
- remoção lógica (soft delete)

Comunicações:
- fornece identidade e papel para fluxos operacionais do sistema

---

## 5. Gestão de Ordens de Serviço

Responsabilidade: controle do ciclo de vida da OS, diagnóstico e orçamento.

Agregados/entidades:
- `ServiceOrder` (raiz)
- `Diagnostic` (entidade interna)
- `Quote` (entidade interna)
- `ServiceItem` (value object interno)

Status da OS:

```mermaid
stateDiagram-v2
    [*] --> Received
    Received --> InDiagnostic : StartDiagnostic
    InDiagnostic --> WaitingApproval : CompleteDiagnostic
    WaitingApproval --> InExecution : ApproveQuote
    InExecution --> Finished : All services completed
    Finished --> Delivered : RegisterDelivery
```

Regras críticas:
- `CustomerId` e `VehicleId` imutáveis
- mecânico seleciona serviços no diagnóstico
- peças e insumos não são cadastrados manualmente no diagnóstico
- após `Diagnostic.Completed`, não há reabertura
- `Quote` calcula:
  - `LaborPrice` via `Service.BasePrice`
  - `PartsTotal` e `SuppliesTotal` via preços de catálogo no momento da geração
- `ServiceItem` é snapshot estrutural (sem preço)

Comunicações:
- consome `CustomerId` e `VehicleId` de Gestão de Clientes
- consome `ServiceId` e composição do Catálogo
- publica `QuoteApprovedEvent` para Produção
- consome `ExecutionOrderCompletedEvent` para progresso da OS

---

## 6. Produção

Responsabilidade: execução de serviços pelos mecânicos.

Agregado:
- `ExecutionOrder`

Status:

```mermaid
stateDiagram-v2
    [*] --> Pending
    Pending --> Ready : SeparationOrderCompletedEvent
    Ready --> InExecution : StartExecution
    InExecution --> Completed : CompleteExecution
```

Regras críticas:
- criado automaticamente ao aprovar orçamento (1 por serviço)
- só inicia execução após separação concluída
- registra tempo real da execução

Comunicações:
- consome `QuoteApprovedEvent`
- publica `ExecutionOrderCreatedEvent`
- publica `ExecutionOrderReadyEvent`
- publica `ExecutionOrderCompletedEvent`
- consome `SeparationOrderCompletedEvent`

---

## 7. Gestão de Estoque

Responsabilidade: controle de saldo e separação física para execução.

Agregados:
- `Stock`
- `SeparationOrder`

Status da separação:

```mermaid
stateDiagram-v2
    [*] --> Pending
    Pending --> WaitingPurchase : no stock
    Pending --> WaitingPickup : stock available
    WaitingPurchase --> WaitingPickup : PurchaseOrderCompletedEvent
    WaitingPickup --> Separated : Stockist withdrawal
    Separated --> Completed : Mechanic receipt
```

Regras críticas:
- separação criada automaticamente por execução
- separação mantém listas separadas de peças e insumos
- cancelamento antes da execução:
  - peça pode retornar ao estoque
  - insumo não retorna após separação
- `AvailableQuantity` nunca negativa

Comunicações:
- consome `ExecutionOrderCreatedEvent`
- publica `SeparationOrderCompletedEvent`
- publica `InsufficientStockEvent`
- consome `PurchaseOrderCompletedEvent`

---

## 8. Compras

Responsabilidade: reposição de estoque por ordem de compra.

Agregado:
- `PurchaseOrder`

Status:

```mermaid
stateDiagram-v2
    [*] --> Created
    Created --> Started : Administrative starts
    Started --> Completed : Stockist confirms receipt
```

Regras críticas:
- geração automática quando há insuficiência
- fornecedor obrigatório para iniciar
- conclusão aciona retomada automática de separações pendentes

Comunicações:
- consome `InsufficientStockEvent`
- consome `SupplierId`
- publica `PurchaseOrderCompletedEvent`

---

## Diagrama de Comunicações entre Contextos

```mermaid
graph TD
    BC1[Gestão de Clientes] -->|CustomerId, VehicleId| BC5[Gestão de OS]
    BC2[Catálogo] -->|ServiceId + composição| BC5
    BC2 -->|PartId, SupplyId| BC7[Gestão de Estoque]
    BC3[Fornecedores] -->|SupplierId| BC8[Compras]
    BC5 -->|QuoteApprovedEvent| BC6[Produção]
    BC6 -->|ExecutionOrderCreatedEvent| BC7
    BC6 -->|ExecutionOrderCompletedEvent| BC5
    BC7 -->|SeparationOrderCompletedEvent| BC6
    BC7 -->|InsufficientStockEvent| BC8
    BC8 -->|PurchaseOrderCompletedEvent| BC7
```
