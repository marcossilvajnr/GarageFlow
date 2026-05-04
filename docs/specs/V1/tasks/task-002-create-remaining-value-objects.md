# Task-002 — Create Remaining Value Objects (`LicensePlate` and `Renavam`)

## 0) Metadata
- `task_id`: `task-002`
- `slug`: `create-remaining-value-objects`
- `owner`: `Domain Team`
- `status`: `Ready`
- `depends_on`: `task-000-template.md`, `task-001-create-customer-crud.md`

## 1) Objetivo
Implementar os Value Objects canônicos restantes `LicensePlate` e `Renavam` no domínio, com validação, normalização e testes unitários completos, consolidando a base para evolução do agregado de veículo.

## 2) Escopo
### In
- Implementar `LicensePlate` em `GarageFlow.Domain.ValueObjects`.
- Implementar `Renavam` em `GarageFlow.Domain.ValueObjects`.
- Adicionar mensagens canônicas no catálogo central `DomainErrorMessages` com identificadores em inglês e conteúdo em português.
- Atualizar referências em `Application` e `Infrastructure` para consumir o catálogo central de mensagens do domínio.
- Criar/atualizar testes unitários de domínio para ambos os VOs.

### Out
- Alterações funcionais em `API`, `Application` e `Infrastructure` além do ajuste de referência para catálogo central de mensagens.
- Criação de endpoints, handlers, repositórios ou migrations.
- Refactor funcional de VOs já existentes (`Cpf`, `Cnpj`, `Email`, `PhoneNumber`, `Address`).
- Uso dos novos VOs no agregado `Vehicle` (será tratado em task posterior).

## 3) Contexto Canônico Obrigatório
Leitura obrigatória antes de implementar:
- [docs/domain/value-objects.md](/Users/marcos/Projects/GarageFlow/docs/domain/value-objects.md)
- [docs/domain/linguagem-ubiqua.md](/Users/marcos/Projects/GarageFlow/docs/domain/linguagem-ubiqua.md)
- [docs/domain/regras-de-negocio.md](/Users/marcos/Projects/GarageFlow/docs/domain/regras-de-negocio.md)
- [docs/architecture/engineering-standards.md](/Users/marcos/Projects/GarageFlow/docs/architecture/engineering-standards.md)

Referências de governança:
- Catálogo canônico de eventos: [docs/domain/agregados.md](/Users/marcos/Projects/GarageFlow/docs/domain/agregados.md)
- Regras de nomenclatura: identificadores em inglês; mensagens para usuário em português.

## 4) Regras de Negócio Aplicáveis (RN)
- RN de validação de dados cadastrais e identificadores veiculares conforme catálogo de VOs canônico.
- Toda entrada inválida deve falhar por `DomainException` com mensagem de domínio clara em português.
- Normalização é mandatória antes da validação final.

## 5) Contratos e Interfaces
### Contrato Interno — `LicensePlate`
- Namespace: `GarageFlow.Domain.ValueObjects`
- Tipo: `public sealed record LicensePlate`
- Factory: `public static LicensePlate Create(string value)`
- Regras:
  - rejeitar `null`, vazio e whitespace;
  - normalizar removendo separadores e espaços, aplicar uppercase;
  - aceitar apenas formato válido definido no canônico (Mercosul e/ou padrão legado conforme domínio vigente);
  - expor valor normalizado por propriedade (`Value`).

### Contrato Interno — `Renavam`
- Namespace: `GarageFlow.Domain.ValueObjects`
- Tipo: `public sealed record Renavam`
- Factory: `public static Renavam Create(string value)`
- Regras:
  - rejeitar `null`, vazio e whitespace;
  - aceitar somente dígitos;
  - exigir 11 dígitos após normalização;
  - validar dígito verificador do RENAVAM;
  - expor valor normalizado por propriedade (`Value`).

### Erros
- Falhas de validação devem lançar `DomainException`.
- Mensagens devem vir do catálogo central (`DomainErrorMessages`), sem strings inline.
- Chaves/constantes de mensagens obrigatoriamente em inglês.

## 6) Plano Técnico por Camada
### Domain
- Criar `LicensePlate` e `Renavam` como `record` imutável.
- Construtor privado para garantir invariantes via `Create()`.
- Implementar normalização e validações específicas por VO.
- Reutilizar `DomainException` existente.

### Application
- Atualizar apenas referências de mensagens para `DomainErrorMessages` quando necessário.

### Infrastructure
- Atualizar apenas referências de mensagens para `DomainErrorMessages` quando necessário.

### API
- Sem alterações.

### Tests
- Criar suíte unitária dedicada para cada VO.
- Cobrir cenários válidos, inválidos e normalização.

## 7) Arquivos a Criar/Alterar
### Criar
- `src/GarageFlow.Domain/ValueObjects/LicensePlate.cs`
- `src/GarageFlow.Domain/ValueObjects/Renavam.cs`
- `src/GarageFlow.Domain/Shared/DomainErrorMessages.cs`
- `tests/GarageFlow.Tests/Domain/ValueObjects/LicensePlateTests.cs`
- `tests/GarageFlow.Tests/Domain/ValueObjects/RenavamTests.cs`

### Alterar
- `src/GarageFlow.Domain/Customers/Customer.cs`
- `src/GarageFlow.Domain/ValueObjects/Cpf.cs`
- `src/GarageFlow.Domain/ValueObjects/Cnpj.cs`
- `src/GarageFlow.Domain/ValueObjects/Email.cs`
- `src/GarageFlow.Domain/ValueObjects/PhoneNumber.cs`
- `src/GarageFlow.Domain/ValueObjects/Address.cs`
- `src/GarageFlow.Application/Customers/Handlers/DeactivateCustomerHandler.cs`
- `src/GarageFlow.Application/Customers/Handlers/UpdateCustomerHandler.cs`
- `src/GarageFlow.Infrastructure/Persistence/Repositories/CustomerRepository.cs`
- `tests/GarageFlow.Tests/Application/Customers/FakeCustomerRepository.cs`

## 8) Critérios de Pronto
- `dotnet build` executa sem erros.
- Testes unitários de domínio passam para `LicensePlate` e `Renavam`.
- Normalização e validação de ambos os VOs aderem ao canônico.
- Nenhuma string de erro inline introduzida.
- Identificadores de código em inglês e mensagens de domínio em português.
- Ajustes em `Application` e `Infrastructure` limitados à troca de referência para `DomainErrorMessages`, sem alteração funcional.

## 9) Estratégia de Testes
### `LicensePlate`
- Deve criar com placa válida em uppercase.
- Deve normalizar entradas com hífen/espaço para formato canônico.
- Deve rejeitar nulo, vazio, whitespace.
- Deve rejeitar caracteres inválidos.
- Deve rejeitar tamanho inválido.
- Deve rejeitar padrão inválido.

### `Renavam`
- Deve criar com RENAVAM válido (11 dígitos).
- Deve normalizar entrada contendo máscara/separadores para apenas dígitos.
- Deve rejeitar nulo, vazio, whitespace.
- Deve rejeitar não numéricos.
- Deve rejeitar comprimento diferente de 11.
- Deve rejeitar dígito verificador inválido.

### Qualidade
- Usar AAA (Arrange, Act, Assert).
- Nomear testes em inglês com intenção clara.
- Garantir independência entre testes (sem estado compartilhado mutável).

## 10) Riscos e Mitigações
- Risco: divergência do algoritmo de dígito verificador do RENAVAM.
  - Mitigação: validar com casos conhecidos e testes parametrizados.
- Risco: aceitar formatos ambíguos de placa fora do padrão do domínio.
  - Mitigação: seguir estritamente o canônico e documentar premissas no código.
- Risco: regressão em convenções de mensagens/identificadores.
  - Mitigação: reutilizar catálogo de mensagens e revisar naming antes do merge.

## 11) Checklist de Execução para IA
- [ ] Ler documentação canônica obrigatória (Domain + Architecture).
- [ ] Implementar `LicensePlate` com normalização + validação.
- [ ] Implementar `Renavam` com normalização + DV.
- [ ] Centralizar novas mensagens em `DomainErrorMessages` com chaves em inglês.
- [ ] Implementar testes unitários completos para ambos os VOs.
- [ ] Rodar build e testes de domínio.
- [ ] Garantir que mudanças em `Application`/`Infrastructure` foram apenas de referência para catálogo central de mensagens.
- [ ] Validar aderência a `engineering-standards.md`.

## 12) Guardrails Não-Negociáveis
- Proibido parsing de `ex.Message` para decidir status HTTP.
- Proibido mapear toda `DomainException` para um único status HTTP sem distinguir causa.
- Proibido criar caminhos de arquivo fora da seção de arquivos sem justificativa explícita na resposta final.
- Proibido alterar itens de `Out` sem registrar impacto e aprovação.

## 13) Contrato de Arquivos e Estrutura
- Os caminhos definidos na seção de arquivos desta task são mandatórios.
- Qualquer desvio de estrutura deve ser registrado na resposta final com justificativa técnica.
- Não criar estrutura paralela de pastas para o mesmo contexto funcional.
