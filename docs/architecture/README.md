# GarageFlow Architecture

## Objetivo
Esta trilha descreve a arquitetura de referência de implementação do GarageFlow:
- como o sistema é organizado;
- como os módulos colaboram;
- quais padrões técnicos são obrigatórios.

## Relação com outras trilhas
- `docs/domain`: fonte canônica de regras de negócio, invariantes e linguagem.
- `docs/specs`: histórico versionado de evolução.

Arquitetura não substitui regras de domínio; ela define como essas regras são implementadas.

## Ordem de Leitura Recomendada
1. `architecture-overview.md`
2. `application-and-integrations.md`
3. `deployment-and-infrastructure.md`
4. `engineering-standards.md`
5. `operations-and-quality.md`
6. `testing-and-quality.md`
7. `ci.md`

## Resultado esperado após leitura
Um novo desenvolvedor deve conseguir:
- entender as camadas e dependências;
- localizar cada bounded context no código;
- implementar fluxos críticos sem quebrar invariantes de domínio;
- entender a infraestrutura provisionada e o fluxo de deploy;
- seguir padrões mínimos de persistência, segurança e testes;
- operar o ambiente local com Docker, Kubernetes, Terraform local e executar a esteira de CI/CD manual.
