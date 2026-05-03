# GarageFlow — Regras de Negócio e Invariantes

## Visão Geral

Este documento descreve todas as regras de negócio críticas
do domínio GarageFlow. Estas regras são invariantes — nunca
podem ser violadas independente do contexto ou situação.

---

## 1. Regras da Ordem de Serviço

### RN-001 — Imutabilidade do Cliente
ClienteId nunca pode ser alterado após a criação da OS.

**Motivo:** A OS é um contrato com um cliente específico.
Trocar o cliente após a criação geraria prejuízo financeiro
e responsabilidade legal.

**Implementação:**
- ClienteId é definido na criação e não expõe setter
- Qualquer tentativa de alteração lança DomainException

---

### RN-002 — Imutabilidade do Veículo
VeiculoId nunca pode ser alterado após a criação da OS.

**Motivo:** O serviço é realizado em um veículo específico.
Trocar o veículo após o início dos trabalhos significa que
o serviço foi feito no carro errado.

**Implementação:**
- VeiculoId é definido na criação e não expõe setter
- Qualquer tentativa de alteração lança DomainException

---

### RN-003 — Progressão de Status
A OS só pode avançar na sequência definida de status.
Nenhum status pode ser pulado ou revertido.

**Sequência obrigatória:**
Recebida → Em Diagnóstico → Aguardando Aprovação
→ Em Execução → Finalizada → Entregue

**Implementação:**
- Cada transição verifica o status atual antes de avançar
- Transições inválidas lançam DomainException

---

### RN-004 — Finalização da OS
A OS só pode ir para Finalizada quando todos os serviços
forem concluídos.

**Regra:**
ServicosConcluidos == TotalServicos

**Implementação:**
- OS mantém contador TotalServicos e ServicosConcluidos
- Ao receber `ExecutionOrderCompletedEvent`, incrementa o contador
- Verifica automaticamente se pode finalizar

---

### RN-005 — Cancelamento de Serviço
Serviços só podem ser cancelados antes de serem executados.
Serviços Em Execução ou Concluídos não podem ser cancelados.

**Motivo:** Serviço iniciado já consumiu mão de obra.
O cliente deve pagar pelos serviços executados.

---

## 2. Regras do Diagnóstico

### RN-006 — Diagnóstico Único
Uma OS só pode ter um diagnóstico ativo por vez.

---

### RN-007 — Orçamento após Diagnóstico
O orçamento só pode ser gerado após o diagnóstico
ser concluído.

---

## 3. Regras da Ordem de Execução

### RN-008 — Criação Automática
Uma Ordem de Execução é criada automaticamente para
cada serviço ao aprovar o orçamento.
Nunca é criada manualmente.

---

### RN-009 — Dependência da Ordem de Separação
A Ordem de Execução só vai para Em Execução após
a Ordem de Separação correspondente estar Concluída.

**Motivo:** O mecânico não pode executar o serviço
sem ter as peças e insumos em mãos.

**Implementação:**
- `ExecutionOrder` nasce em `Pending` e só pode iniciar em `Ready`
- `StartExecution()` exige `Status == Ready`
- Ao receber `SeparationOrderCompletedEvent`, o Application Service chama `MarkReadyToStart()` no `ExecutionOrder`
- `MarkReadyToStart()` é idempotente (duplicidade de evento não quebra o fluxo)
- `Completed` da separação pressupõe dupla confirmação de custódia concluída (estoquista + mecânico)

---

### RN-010 — Registro de Tempo
Ao iniciar a execução, IniciadoEm é registrado.
Ao concluir, ConcluidoEm é registrado e
TempoRealMinutos é calculado automaticamente.

**Cálculo:**
TempoRealMinutos = (ConcluidoEm - IniciadoEm).TotalMinutes

**Implementação:**
- `TempoRealMinutos` é armazenado com precisão decimal (sem truncamento para inteiro)
- O cálculo é executado no momento da conclusão da execução

---

## 4. Regras da Ordem de Separação

### RN-011 — Criação Automática
Uma Ordem de Separação é criada automaticamente
para cada Ordem de Execução.
Nunca é criada manualmente.

---

### RN-012 — Verificação Automática de Estoque
Ao ser criada, a Ordem de Separação verifica
automaticamente a disponibilidade de todas as peças
e insumos necessários.

**Cenário 1 — Tem estoque:**
Reserva as peças → Status: Aguardando Retirada

**Cenário 2 — Sem estoque:**
Gera Ordem de Compra → Status: Aguardando Compra

---

### RN-013 — Dupla Confirmação de Custódia
A entrega de peças ao mecânico requer duas confirmações:
1. Estoquista confirma a retirada física do estoque
2. Mecânico confirma o recebimento das peças

**Motivo:** Rastreabilidade completa para evitar
que peças "sumam" sem registro.

---

## 5. Regras do Estoque

### RN-014 — Quantidade Disponível Nunca Negativa
QuantidadeDisponivel nunca pode ser menor que zero.

**Implementação:**
QuantidadeDisponivel = QuantidadeTotal - QuantidadeReservada
QuantidadeDisponivel >= 0 (invariante)

---

### RN-015 — Três Tipos de Quantidade
O estoque mantém três quantidades distintas:

| Campo | Descrição |
|-------|-----------|
| QuantidadeTotal | Total físico disponível |
| QuantidadeDisponivel | Livre para novos pedidos |
| QuantidadeReservada | Bloqueada para ordens em andamento |

---

### RN-016 — Operações de Estoque
Existem três operações distintas no estoque:

| Operação | Quando | Efeito |
|----------|--------|--------|
| Reservar | Ordem de Separação criada com estoque disponível | Disponivel-- / Reservada++ |
| Baixar | Estoquista retira fisicamente | Reservada-- / Total-- |
| Liberar | Serviço cancelado antes da execução | Reservada-- / Disponivel++ |

---

### RN-017 — Geração de Ordem de Compra
Quando a quantidade disponível é insuficiente para
atender uma Ordem de Separação, uma Ordem de Compra
é gerada automaticamente pelo sistema.

---

## 6. Regras da Ordem de Compra

### RN-018 — Geração Automática
A Ordem de Compra é sempre gerada automaticamente
pelo sistema. Nunca é criada manualmente.

---

### RN-019 — Seleção de Fornecedor
O administrativo deve selecionar um fornecedor
antes de iniciar a Ordem de Compra.

---

### RN-020 — Retomada da Ordem de Separação
Ao ser concluída, a Ordem de Compra dispara
automaticamente a retomada da Ordem de Separação
que estava Aguardando Compra.

**Implementação:**
- A retomada automática é orquestrada pelo Application Service
- O gatilho é `PurchaseOrderCompletedEvent`
- A ordem de execução é: `Stock.Replenish()`, `Stock.Reserve()` e depois `SeparationOrder.ResumeAfterPurchase()`

---

## 7. Regras de Clientes e Veículos

### RN-021 — Identificação Única de Cliente
CPF é único para pessoas físicas.
CNPJ é único para pessoas jurídicas.
Não podem existir dois clientes com o mesmo CPF ou CNPJ.

**Implementação:**
- Unicidade garantida por índice único no banco
- Violação de índice traduzida para `DomainException` em português

---

### RN-022 — Identificação Única de Veículo
Placa e RENAVAM são únicos no sistema.
Não podem existir dois veículos com a mesma placa ou RENAVAM.

**Implementação:**
- Unicidade garantida por índice único no banco
- Violação de índice traduzida para `DomainException` em português

---

### RN-023 — Soft Delete
Clientes, veículos, serviços, peças e insumos
nunca são deletados fisicamente.
São apenas desativados (soft delete).

**Motivo:** Preservação do histórico de OSs anteriores.

---

## 8. Regras de Catálogo

### RN-024 — Tempo Médio Estimado
O tempo médio estimado de execução de um serviço
é definido e atualizado manualmente pelo administrativo.
Não é calculado automaticamente.

---

### RN-025 — Diferença entre Peça e Insumo

| | Peça | Insumo |
|-|------|--------|
| Natureza | Componente discreto | Material consumível |
| Medida | Unidade inteira | Quantidade variável |
| Devolução ao estoque | Sim (se não usada) | Não |

---

## Resumo de Invariantes Críticos

| Código | Invariante |
|--------|-----------|
| RN-001 | ClienteId imutável após criação da OS |
| RN-002 | VeiculoId imutável após criação da OS |
| RN-003 | Status da OS segue sequência obrigatória |
| RN-004 | OS finaliza só quando todos serviços concluídos |
| RN-005 | Serviço Em Execução ou Concluído não pode ser cancelado |
| RN-009 | Ordem de Execução só inicia após Ordem de Separação Concluída |
| RN-010 | Tempo real é registrado com precisão decimal |
| RN-013 | Dupla confirmação de custódia de peças |
| RN-014 | QuantidadeDisponivel nunca negativa |
| RN-018 | Ordem de compra sempre automática |
| RN-019 | Fornecedor obrigatório antes de iniciar compra |
| RN-020 | Retomada automática da separação por evento de compra concluída |
| RN-021 | CPF e CNPJ únicos por cliente |
| RN-022 | Placa e RENAVAM únicos por veículo |
| RN-023 | Soft delete em todas as entidades |
