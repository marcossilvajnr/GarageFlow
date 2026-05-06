# Testing and Quality

## Objetivo
Definir a estratégia corporativa e atemporal de testes e qualidade do GarageFlow, com critérios claros de cobertura, evidência e governança.

## Escopo
- Pirâmide de testes e responsabilidade por camada.
- Critérios de criação de novos testes por tipo.
- Padrão de evidência para validação técnica e apresentação.
- Governança de qualidade e segurança de dependências.

## Pirâmide de Testes
### Unitários
- foco: regras de domínio, invariantes, value objects, decisões de aplicação isoladas;
- velocidade alta, isolamento total, sem dependência de infraestrutura externa.

### Integração
- foco: contratos HTTP, mapeamento de erros, persistência e orquestração entre camadas;
- validações de comportamento real da API com banco e componentes acoplados.

### E2E
- foco: fluxos críticos ponta a ponta do produto;
- validação de estados de processo completos e consistência cross-aggregate.

## Fluxos E2E Críticos (Baseline)
1. OS ponta a ponta com estoque suficiente.
2. OS com falta de estoque, compra e retomada da separação.
3. OS com cancelamento no último estágio permitido.

## Princípios de Qualidade de Testes
- determinismo: cada cenário define dados e pré-condições explícitas;
- rastreabilidade: IDs de agregados-chave devem ser observáveis nas evidências;
- isolamento: testes não dependem de efeitos colaterais de outros cenários;
- legibilidade: cenário descreve claramente entrada, ação e resultado esperado;
- aderência canônica: regra validada em teste deve refletir docs canônicos.

## Critérios de Criação de Testes
### Quando criar Unitário
- nova regra de negócio, pré-condição, transição de estado ou cálculo.

### Quando criar Integração
- novo endpoint, mudança de contrato HTTP, mapeamento de exceção, consulta com persistência.

### Quando criar E2E
- novo fluxo crítico de negócio envolvendo múltiplos agregados/processos.

## Padrão de Evidência
Para cada fluxo crítico, evidenciar no mínimo:
- respostas HTTP das etapas críticas;
- transições de estado esperadas;
- estado final consistente de `ServiceOrder`, `SeparationOrder`, `ExecutionOrder` e `PurchaseOrder` (quando aplicável);
- identificadores rastreáveis das entidades-chave.

Meios válidos de observação:
- execução de testes automatizados;
- inspeção de endpoints (ex.: Swagger).

## Cobertura e Governança
- cobertura mínima exigida no enunciado deve ser monitorada nos domínios críticos;
- segurança de dependências deve ser acompanhada por relatório automatizado;
- exceções temporárias devem ter justificativa e prazo de revisão.

## Integração com Operação
- execução operacional da esteira manual e publicação de artefatos estão em:
  - `docs/architecture/operations-and-quality.md`.
