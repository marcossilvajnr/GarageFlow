# Application and Integrations

## Fluxos Críticos

### 1) Criação e acompanhamento de Ordem de Serviço
- A aplicação identifica cliente e veículo.
- Cria a OS, inicia diagnóstico, gera orçamento e envia para aprovação.
- Acompanha status da OS no fluxo canônico:
  `Received -> InDiagnostic -> WaitingApproval -> InExecution -> Finished -> Delivered`.

### 2) Fluxo de estoque e separação
- Ao criar Ordem de Execução, cria Ordem de Separação.
- Se há saldo: reserva itens e segue para retirada.
- Se não há saldo: segue para compra e posterior retomada.

### 3) Fluxo de compras
- Compra é gerada automaticamente quando o estoque é insuficiente.
- Administrativo seleciona fornecedor e inicia compra.
- Conclusão de compra atualiza estoque e retoma separações pendentes.

## Orquestrações de Aplicação Obrigatórias

### RN-009 — Dependência da separação para execução
- `ExecutionOrder` só inicia após separação concluída.
- Ao receber `SeparationOrderCompletedEvent`, a camada de aplicação chama
  `ExecutionOrder.MarkReadyToStart()` de forma idempotente.

### RN-020 — Retomada automática após compra
- Ao receber `PurchaseOrderCompletedEvent`, a camada de aplicação executa:
  `Stock.Replenish() -> Stock.Reserve() -> SeparationOrder.ResumeAfterPurchase()`.
- O fluxo deve ser transacional no contexto do caso de uso.

## Ports e Adapters
- Entrada:
  - HTTP (Minimal API / REST endpoints)
  - endpoint de autenticação para emissão de token (`/auth/login`)
- Saída:
  - EF Core (persistência)
  - Identity/JWT (autenticação e autorização)
  - Relógio do sistema (abstração para tempo)

As integrações entre módulos são tratadas por handlers da camada de aplicação.

Convenções de segurança na borda HTTP:
- validação de credenciais e emissão de JWT ocorrem na camada de aplicação com adapters de infraestrutura.
- autorização é aplicada na API por políticas/roles e nunca por parsing textual de mensagens.
- handlers de aplicação recebem identidade já autenticada (claims) e aplicam regras de autorização por caso de uso quando necessário.

## Contratos Internos de Integração
Os eventos de integração seguem o catálogo canônico definido em
`docs/Domain/agregados.md`, na seção **Eventos de Integração Canônicos**.

Convenções:
- Handlers de aplicação são responsáveis por orquestração entre módulos.
- Repositório é sempre por agregado raiz.
- Cada caso de uso fecha sua unidade transacional.
