# GarageFlow Architecture

## Objetivo
Esta trilha descreve a arquitetura de referência de implementação do GarageFlow:
- como o sistema é organizado;
- como os módulos colaboram;
- quais padrões técnicos são obrigatórios.

## Relação com outras trilhas
- `docs/domain`: fonte canônica de regras de negócio, invariantes e linguagem.
- `docs/Specs`: histórico versionado de evolução.

Arquitetura não substitui regras de domínio; ela define como essas regras são implementadas.

## Ordem de Leitura Recomendada
1. `architecture-overview.md`
2. `application-and-integrations.md`
3. `engineering-standards.md`
4. `operations-and-quality.md`

## Resultado esperado após leitura
Um novo desenvolvedor deve conseguir:
- entender as camadas e dependências;
- localizar cada bounded context no código;
- implementar fluxos críticos sem quebrar invariantes de domínio;
- seguir padrões mínimos de persistência, segurança e testes;
- operar o ambiente local com Docker e executar a esteira manual de qualidade/segurança.
