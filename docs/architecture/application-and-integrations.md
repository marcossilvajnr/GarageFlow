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
As integrações entre módulos são tratadas por handlers da camada de aplicação. A regra geral de Ports e Adapters, incluindo responsabilidades de entrada e saída, está em `docs/architecture/clean-architecture.md`.

Convenções de segurança na borda HTTP:
- validação de credenciais e emissão de JWT ocorrem na camada de aplicação com adapters de infraestrutura.
- autorização é aplicada na API por políticas/roles e nunca por parsing textual de mensagens.
- handlers de aplicação recebem identidade já autenticada (claims) e aplicam regras de autorização por caso de uso quando necessário.

## Integrações Externas
- A aprovação ou recusa externa de orçamento entra por webhook HTTP e delega para os handlers de aceite/recusa já existentes.
- A atualização de status por ferramenta externa é demonstrável por chamadas HTTP externas, como Swagger ou Postman, sem SMTP ou parser de e-mail.
- Nenhum adapter externo deve gravar status arbitrário diretamente no banco; toda transição passa pelos use cases da Application e pelas invariantes do domínio.

## Contrato de Autenticação
- `POST /auth/login`
- request:
  - `username` (string)
  - `password` (string)
- response:
  - `accessToken` (JWT)
  - `tokenType` (`Bearer`)
  - `expiresIn` (segundos)
  - `role` (papel do usuário)

Fluxo:
- credenciais são validadas contra base local de usuários de auth.
- senha é verificada por hash.
- token JWT é emitido com claims de identidade e papel.

## Matriz de Políticas (RBAC)
- `Administrative`:
  - acesso administrativo completo, incluindo rotas de ajuste manual sensível.
- `FrontDeskOrAdministrative`:
  - atendimento e gestão de OS na trilha comercial.
- `MechanicOrAdministrative`:
  - operações de diagnóstico e execução.
- `StockistOrAdministrative`:
  - operações de separação, estoque e compras.

## Contratos Internos de Integração
Os eventos de integração seguem o catálogo canônico definido em
`docs/domain/agregados.md`, na seção **Eventos de Integração Canônicos**.

Convenções:
- Handlers de aplicação são responsáveis por orquestração entre módulos.
- Repositório é sempre por agregado raiz.
- Cada caso de uso fecha sua unidade transacional.
