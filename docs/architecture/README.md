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
2. `architecture-diagrams.md`
3. `application-and-integrations.md`
4. `deployment-and-infrastructure.md`
5. `engineering-standards.md`
6. `operations-and-quality.md`
7. `testing-and-quality.md`
8. `ci.md`

## Resultado esperado após leitura
Um novo desenvolvedor deve conseguir:
- entender as camadas e dependências;
- visualizar o sistema em C1, C2 e visão de infraestrutura;
- localizar cada bounded context no código;
- implementar fluxos críticos sem quebrar invariantes de domínio;
- entender a infraestrutura provisionada e o fluxo de deploy;
- seguir padrões mínimos de persistência, segurança e testes;
- operar o ambiente local com Docker, Kubernetes, Terraform local e executar a esteira de CI/CD manual.
