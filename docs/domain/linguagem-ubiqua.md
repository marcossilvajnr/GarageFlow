# GarageFlow - Linguagem Ubiqua

## Objetivo
Este documento define os termos oficiais do negocio usados no GarageFlow.
A ideia e garantir que atendimento, oficina, estoque, administrativo e tecnologia usem o mesmo vocabulario.

---

## Termos Principais

- **Ordem de Servico (OS):** registro principal do atendimento de um veiculo, do recebimento ate a entrega.
- **Diagnostico:** avaliacao tecnica inicial para identificar os servicos necessarios.
- **Orcamento:** proposta de valores e itens para aprovacao do cliente.
- **Ordem de Execucao:** ordem operacional para realizar um servico aprovado.
- **Ordem de Separacao (ordem de retirada):** lista de pecas e insumos que o estoque separa para a execucao.
- **Ordem de Compra:** ordem para reposicao de itens quando o estoque esta insuficiente.
- **Estoque:** controle de saldo, reserva, consumo e ajustes de pecas e insumos.

---

## Cadastros Mestres

- **Cliente:** pessoa fisica ou juridica que contrata o servico.
- **Veiculo:** bem atendido na OS; cada veiculo fica vinculado a um cliente.
- **Servico:** atividade que pode ser executada pela oficina.
- **Peca:** item fisico aplicado no servico.
- **Insumo:** material de consumo usado no servico.
- **Fornecedor:** empresa que fornece pecas e insumos.
- **Funcionario:** colaborador interno com papel operacional definido.

---

## Papeis Operacionais

- **Atendente:** abre e conduz a OS no fluxo de atendimento.
- **Mecanico:** realiza diagnostico e execucao tecnica.
- **Estoquista:** separa e entrega itens para a execucao.
- **Administrativo:** governa cadastros e pode atuar em fluxos criticos conforme regra.

---

## Status da Ordem de Servico (OS)

- **Recebida:** OS aberta no atendimento e pronta para iniciar o diagnostico.
- **Em Diagnostico:** veiculo em avaliacao tecnica.
- **Aguardando Aprovacao:** orcamento gerado e aguardando decisao do cliente.
- **Aprovada:** cliente aprovou o orcamento e a OS pode seguir para execucao.
- **Rejeitada:** cliente recusou o orcamento e a OS nao avanca para execucao.
- **Em Execucao:** servicos aprovados estao sendo realizados.
- **Finalizada:** execucao concluida, aguardando entrega do veiculo.
- **Entregue:** veiculo entregue ao cliente e atendimento encerrado.

---

## Status do Diagnostico

- **Em andamento:** mecanico ainda esta avaliando e montando escopo.
- **Concluido:** diagnostico fechado e pronto para consolidacao dos servicos.

---

## Status da Ordem de Execucao

- **Pendente:** ordem criada, aguardando liberacao operacional.
- **Pronta para Inicio:** itens necessarios ja estao disponiveis para iniciar.
- **Em Execucao:** servico em andamento na oficina.
- **Concluida:** servico finalizado tecnicamente.

---

## Status da Ordem de Separacao (Retirada)

- **Pendente:** ordem criada e aguardando acao do estoque.
- **Aguardando Compra:** falta item e a separacao depende de reposicao.
- **Aguardando Retirada:** itens reservados e prontos para retirada.
- **Separada:** retirada confirmada pelo estoque.
- **Concluida:** recebimento confirmado pelo mecanico.

---

## Status da Ordem de Compra

- **Criada:** necessidade de compra registrada.
- **Iniciada:** compra em andamento com fornecedor definido.
- **Concluida:** itens recebidos e compra encerrada.

---

## Status do Orcamento

- **Aguardando Aprovacao do Cliente:** proposta enviada e sem decisao final.
- **Aprovado pelo Cliente:** cliente aceitou a proposta.
- **Rejeitado pelo Cliente:** cliente recusou a proposta.

---

## Regras de Linguagem para o Time

- Usar sempre **OS** para falar do fluxo principal de atendimento.
- Usar **ordem de retirada** como sinonimo operacional de **ordem de separacao**.
- Diferenciar claramente:
  - **Aprovada** (status da OS)
  - **Aprovado pelo Cliente** (status do orcamento)
- Quando houver falta de item, falar em **insuficiencia de estoque** e nao em "erro de separacao".

---

## Handoffs Entre Areas (Visao de Negocio)

- Atendimento libera OS para diagnostico.
- Producao conclui diagnostico e devolve para atendimento gerar/seguir com orcamento.
- Aprovacao do cliente libera producao e estoque para execucao.
- Estoque conclui retirada para permitir avanco da execucao.
- Insuficiencia de estoque aciona compra no administrativo.
- Compra concluida permite retomada de separacoes pendentes.
