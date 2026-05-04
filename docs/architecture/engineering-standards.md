# Engineering Standards

## Persistência (EF Core)
- EF Core é o mecanismo padrão de persistência.
- Mapeamentos de agregados e value objects devem ser explícitos por configuração.
- Migrations ficam centralizadas na infraestrutura.
- Repositório por agregado raiz; sem escrita transversal direta fora da aplicação.

## Concorrência e Integridade
- Aplicar concorrência otimista nos agregados transacionais críticos.
- Garantir unicidade por índice único no banco para:
  - CPF
  - CNPJ
  - placa
  - RENAVAM
  - códigos de catálogo
- Violações de unicidade devem ser traduzidas para `DomainException` em português.

## Segurança
- Stack de autenticação/autorização padrão:
  - `Microsoft.AspNetCore.Authentication.JwtBearer` para validação de token.
  - `Microsoft.AspNetCore.Authorization` para políticas e papéis.
  - `PasswordHasher<TUser>` (ASP.NET Core Identity) para hash e verificação de senha.
- Provedor de identidade no escopo atual:
  - base local de usuários e credenciais da aplicação.
  - sem dependência obrigatória de provedor externo OAuth2/OIDC.
- Contrato mínimo de claims no access token:
  - `sub` (identificador do usuário)
  - `role` (papel de autorização)
  - `name` (nome de exibição)
  - `jti` (identificador único do token)
  - `iat` e `exp` (controle temporal)
- Autorização por papéis:
  - `Attendant`
  - `Mechanic`
  - `Stockist`
  - `Administrative`
- Endpoints administrativos devem exigir autenticação e papel adequado.
- Endpoints de negócio devem ser protegidos por política explícita; acesso anônimo só é permitido para rotas públicas definidas por contrato.
- Não é permitido implementar algoritmo criptográfico próprio para senha ou assinatura de token.

## Governança de Erros e Invariantes
- Decisões de transporte (ex.: status HTTP) não devem depender de parsing textual de mensagens.
- Falhas de negócio devem ser representadas por tipos de erro específicos ou códigos estáveis.
- Mensagens de erro de domínio devem permanecer em português para consistência funcional e operacional.

## Invariantes de Estado em Agregados
- Toda mutação de estado deve validar pré-condições de forma explícita no agregado.
- Transições inválidas devem falhar de forma determinística via exceção de domínio.
- Operações de mudança de estado devem atualizar metadados de rastreabilidade (`UpdatedAt` ou equivalente).
- Nenhum método de domínio deve aplicar fallback silencioso que altere significado de negócio.

## Contratos de Entrada e Enumerações
- Valores de enumeração recebidos por contratos de entrada devem ser validados explicitamente.
- Valores não reconhecidos devem ser rejeitados com erro de domínio ou validação, sem coerção implícita.
- Conversões entre contratos externos e tipos de domínio devem ser explícitas, auditáveis e testadas.

## Organização de Tipos e Arquivos
- Cada `class`, `record` ou `enum` público deve residir em arquivo próprio.
- O nome do arquivo deve refletir o tipo principal declarado.
- Não é permitido agrupar múltiplos DTOs públicos em um único arquivo.
- Tipos auxiliares internos podem coexistir no mesmo arquivo somente quando houver forte coesão e escopo estritamente local.

## Catálogo de Mensagens
- Mensagens de erro e validação não devem ficar inline em handlers, endpoints ou repositórios.
- O domínio deve manter um único catálogo central de mensagens em `DomainErrorMessages` (`GarageFlow.Domain.Shared`).
- `Application`, `Infrastructure` e `API` devem reutilizar o catálogo central do domínio, sem redefinir textos locais.
- Exceções de domínio e respostas HTTP devem referenciar o catálogo central, evitando duplicação textual.
- Mudanças de redação em mensagens devem ocorrer exclusivamente no catálogo central.
- Textos de mensagem exibidos ao usuário podem permanecer em português.
- Chaves, constantes e identificadores de código do catálogo devem ser nomeados em inglês (ex.: `InvalidName`, `DuplicateCpf`).

## Nomenclatura de Código
- Todo identificador de código deve ser nomeado em inglês.
- A regra abrange classes, records, enums, interfaces, métodos, propriedades, campos, variáveis, constantes, namespaces e nomes de arquivo.
- Não é permitido criar novos identificadores em português no código-fonte.
- Exceção única: conteúdo textual orientado ao usuário final pode permanecer em português (mensagens, textos de validação e descrição funcional).
- Refactors de nomenclatura devem priorizar consistência semântica, legibilidade e previsibilidade entre camadas.

## Constantes e Defaults
- Regras de aplicação não devem conter magic numbers.
- Valores padrão, limites e parâmetros recorrentes devem ser extraídos para constantes nomeadas com contexto explícito.
- Constantes de escopo transversal devem ficar em artefatos compartilhados do contexto.
- Constantes locais são permitidas apenas quando o uso for exclusivo de um único arquivo e a semântica estiver clara.

## Observabilidade Mínima
- Logging estruturado em todas as camadas de entrada e aplicação.
- Incluir correlação (`CorrelationId`) em requisição e logs de fluxo.
- Registrar transições de status críticas de OS, execução, separação e compra.

## Estratégia de Testes
- `Domain`: testes unitários puros para invariantes e transições.
- `Application`: testes de casos de uso e orquestrações (incluindo RN-009 e RN-020).
- `Integration`: testes de API + persistência + autenticação.

Cobertura:
- priorizar cobertura alta nos domínios críticos e fluxos de negócio.

## Evolução Segura
- Contratos REST devem evoluir de forma retrocompatível.
- Eventos internos devem manter payload estável e adicionar campos de forma não disruptiva.
- Mudanças de regra sempre começam em `docs/Domain` e depois refletem nesta trilha.

## Governança de Tasks
- Toda nova task em `docs/Specs/V1/tasks` deve declarar explicitamente onde ficam:
  - catálogo central de mensagens de domínio;
  - constantes e defaults aplicáveis ao contexto.
- Toda nova task deve declarar conformidade com a regra de nomenclatura em inglês para identificadores de código.
