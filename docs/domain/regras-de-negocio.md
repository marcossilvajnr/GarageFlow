# GarageFlow — Regras de Negócio e Invariantes

## Visão Geral
Este documento descreve as regras de negócio críticas do domínio GarageFlow.
Estas regras são invariantes e não podem ser violadas.

---

## 1. Regras da Ordem de Serviço

### RN-001 — Imutabilidade do Cliente
`CustomerId` nunca pode ser alterado após a criação da OS.

### RN-002 — Imutabilidade do Veículo
`VehicleId` nunca pode ser alterado após a criação da OS.

### RN-003 — Progressão de Status
A OS só pode avançar na sequência definida, sem pular ou reverter status.

Sequência obrigatória:
`Received -> InDiagnostic -> WaitingApproval -> Approved -> InExecution -> Finished -> Delivered`

Fluxo alternativo de decisão:
`WaitingApproval -> Rejected`

### RN-004 — Finalização da OS
A OS só pode ir para `Finished` quando:
`CompletedServices == TotalServices`

### RN-005 — Cancelamento de Serviço em Execução
Serviços em execução ou concluídos não podem ser cancelados.

---

## 2. Regras do Diagnóstico

### RN-006 — Diagnóstico Único
Uma OS só pode ter um diagnóstico ativo por vez.

### RN-007 — Orçamento após Diagnóstico
O orçamento só pode ser gerado após diagnóstico concluído.

### RN-026 — Alteração de Serviços no Diagnóstico
Serviços só podem ser adicionados ou removidos do diagnóstico enquanto `Status == InProgress`.
Após `Completed`, nenhuma alteração é permitida.
Cada serviço pode ser selecionado no máximo uma vez no diagnóstico.

### RN-027 — Cadastro de Serviço pelo Mecânico
O mecânico não tem permissão para cadastrar serviços no catálogo.
Se um serviço necessário não existir, o fluxo é comunicar o Administrativo para cadastro.

### RN-028 — Diagnóstico não pode ser reaberto
Após `Completed`, o diagnóstico não volta para `InProgress`.
Se faltar serviço, o cadastro deve ocorrer antes da conclusão do diagnóstico.

### RN-029 — Rastreabilidade de Serviços da OS
Toda alteração de serviços da OS deve ser rastreável com autoria, origem e tempo.
Cada inclusão/remoção de serviço registra:
- origem da operação (`FrontDesk` ou `Diagnostic`)
- ator da operação
- data/hora da operação
- motivo da remoção (quando aplicável)

### RN-030 — Congelamento dos Serviços após Diagnóstico
Após conclusão do diagnóstico, a composição de serviços da OS fica congelada para geração do orçamento.
Qualquer solicitação de mudança do cliente deve retornar ao atendimento para novo ciclo de ajuste e novo orçamento.

### RN-031 — Orçamento Imutável por Versão
Após gerado, o orçamento não pode ter itens ou valores alterados.
As únicas transições permitidas para uma versão são:
- `WaitingCustomerApproval -> CustomerApproved`
- `WaitingCustomerApproval -> CustomerRejected`
Mudanças de escopo geram nova versão de orçamento, preservando histórico.

---

## 3. Regras da Ordem de Execução

### RN-008 — Criação Automática
Uma `ExecutionOrder` é criada automaticamente para cada serviço aprovado no orçamento.
Não há criação manual.

### RN-009 — Dependência da Ordem de Separação
A execução só inicia após separação concluída.

Implementação canônica:
- `ExecutionOrder` nasce em `Pending`
- `StartExecution()` exige `Status == Ready`
- ao receber `SeparationOrderCompletedEvent`, a camada de aplicação chama `MarkReadyToStart()`
- `MarkReadyToStart()` é idempotente

### RN-010 — Registro de Tempo
`ActualTimeMinutes` é calculado com precisão decimal em `CompleteExecution()`:
`(CompletedAt - StartedAt).TotalMinutes`

---

## 4. Regras da Ordem de Separação

### RN-011 — Criação Automática
Uma `SeparationOrder` é criada automaticamente para cada `ExecutionOrder`.

### RN-012 — Verificação Automática de Estoque
Ao criar separação:
- com estoque: reserva e segue para `WaitingPickup`
- sem estoque: gera compra e segue para `WaitingPurchase`

### RN-013 — Dupla Confirmação de Custódia
A separação só é concluída com duas confirmações:
1. Estoquista confirma retirada física
2. Mecânico confirma recebimento

---

## 5. Regras do Estoque

### RN-014 — Quantidade Disponível Nunca Negativa
`AvailableQuantity >= 0` é invariante.

### RN-015 — Três Tipos de Quantidade
- `TotalQuantity`
- `AvailableQuantity`
- `ReservedQuantity`

### RN-016 — Operações de Estoque
- Reservar: `Available--`, `Reserved++`
- Baixar: `Reserved--`, `Total--`
- Liberar: `Reserved--`, `Available++`

### RN-032 — Devolução Total Vinculada à Ordem de Separação
A devolução de itens retirados do estoque é permitida somente antes de `ConfirmMechanicReceipt`.

Regras obrigatórias:
- a devolução deve referenciar obrigatoriamente a `SeparationOrder` de origem;
- a devolução é total da ordem (todos os itens de peças e insumos);
- após `ConfirmMechanicReceipt`, devolução é bloqueada no fluxo operacional;
- toda devolução valida que nenhuma quantidade devolvida ultrapassa a quantidade retirada.

### RN-033 — Liberação Manual de Reserva de Estoque
A liberação manual de reserva via operação de estoque é permitida para peças e insumos como ajuste operacional.

Regras obrigatórias:
- a operação exige justificativa obrigatória;
- a operação é restrita ao perfil `Administrative`;
- a operação não substitui o fluxo de devolução total vinculado à `SeparationOrder`.

### RN-017 — Geração de Ordem de Compra
Falta de estoque gera `PurchaseOrder` automaticamente.

---

## 6. Regras da Ordem de Compra

### RN-018 — Geração Automática
`PurchaseOrder` é sempre gerada pelo sistema.

### RN-019 — Seleção de Fornecedor
Fornecedor é obrigatório antes de iniciar compra.

### RN-020 — Retomada da Ordem de Separação
Ao concluir compra, a aplicação orquestra automaticamente:
`Stock.Replenish() -> Stock.Reserve() -> SeparationOrder.ResumeAfterPurchase()`

---

## 7. Regras de Clientes e Veículos

### RN-021 — Identificação Única de Cliente
CPF e CNPJ são únicos no sistema.

### RN-022 — Identificação Única de Veículo
Placa e RENAVAM são únicos no sistema.

### RN-023 — Soft Delete
Clientes, veículos, serviços, peças, insumos, fornecedores e funcionários não são deletados fisicamente.
A remoção é lógica (`IsActive = false`).

---

## 8. Regras de Catálogo e Orçamento

### RN-024 — Tempo Estimado de Serviço
O tempo estimado de serviço é definido e atualizado manualmente pelo Administrativo.

### RN-025 — Diferença entre Peça e Insumo
| | Peça | Insumo |
|-|------|--------|
| Natureza | Componente discreto | Material consumível |
| Medida | Unidade inteira | Quantidade variável |
| Devolução ao estoque | Sim (antes de `ConfirmMechanicReceipt`) | Sim (antes de `ConfirmMechanicReceipt`) |

### Regra vigente — Composição de Serviço no Catálogo
`Service` mantém listas pré-definidas de peças e insumos para execução:
- `Parts` (com `PartId`, `PartName`, `Quantity`)
- `Supplies` (com `SupplyId`, `SupplyName`, `Quantity`, `Unit`)

Regras obrigatórias da composição:
- não permite item duplicado do mesmo tipo por serviço
- `Quantity` deve ser maior que zero

### Regra vigente — Diagnóstico seleciona serviços
No diagnóstico, o mecânico seleciona serviços de catálogo.
Peças e insumos não são inseridos manualmente no diagnóstico.
Cada serviço selecionado aparece no máximo uma vez (sem quantidade no diagnóstico).

### Regra vigente — Snapshot e preço no orçamento
- `ServiceItem` copia dados estruturais do serviço (serviço, peças, insumos e quantidades).
- `ServiceItem` não armazena preço.
- `LaborPrice` do `QuoteItem` vem de `Service.BasePrice` no momento de geração do orçamento.
- `PartsTotal` usa `Part.UnitPrice` e `SuppliesTotal` usa `Supply.BaseCost`, sempre no momento da geração do orçamento.
- orçamento gerado é imutável; alterações de escopo exigem nova versão.

---

## Resumo de Invariantes Críticos

| Código | Invariante |
|--------|-----------|
| RN-001 | `CustomerId` imutável após criação da OS |
| RN-002 | `VehicleId` imutável após criação da OS |
| RN-003 | Status da OS segue sequência obrigatória |
| RN-004 | OS finaliza só quando todos os serviços forem concluídos |
| RN-005 | Serviço em execução ou concluído não pode ser cancelado |
| RN-009 | Execução só inicia após separação concluída |
| RN-010 | Tempo real registrado com precisão decimal |
| RN-013 | Dupla confirmação de custódia na separação |
| RN-014 | `AvailableQuantity` nunca negativa |
| RN-018 | Ordem de compra sempre automática |
| RN-019 | Fornecedor obrigatório antes de iniciar compra |
| RN-020 | Retomada automática da separação por compra concluída |
| RN-021 | CPF e CNPJ únicos por cliente |
| RN-022 | Placa e RENAVAM únicos por veículo |
| RN-023 | Soft delete nas entidades de cadastro |
| RN-026 | Diagnóstico só altera serviços em `InProgress` |
| RN-027 | Mecânico não cadastra serviço no catálogo |
| RN-028 | Diagnóstico concluído não pode ser reaberto |
| RN-029 | Alterações de serviços na OS são rastreáveis por origem/ator/tempo |
| RN-030 | Serviços da OS ficam congelados após diagnóstico concluído |
| RN-031 | Orçamento é imutável por versão; mudança gera nova versão |
| RN-032 | Devolução de retirada exige ordem de separação, ocorre apenas antes de `ConfirmMechanicReceipt` e é total |
| RN-033 | Liberação manual de reserva exige justificativa e é restrita ao perfil `Administrative` |
