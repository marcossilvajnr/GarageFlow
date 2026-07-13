# GarageFlow — Guia Geral da Documentação

## Objetivo
Este guia organiza a documentação em 3 trilhas oficiais:

1. `docs/domain`: fonte canônica do domínio.
2. `docs/architecture`: arquitetura de referência, operação e qualidade.
3. `docs/specs`: histórico evolutivo versionado por pasta (`V1`, `V2`, ...).

Cada trilha tem propósito próprio e complementar.

## 1) Trilha Canônica de Domínio (`docs/domain`)
Representa as regras de negócio e a linguagem ubíqua.

Arquivos-base:
- [linguagem-ubiqua.md](domain/linguagem-ubiqua.md)
- [regras-de-negocio.md](domain/regras-de-negocio.md)
- [bounded-contexts.md](domain/bounded-contexts.md)
- [glossario-siglas.md](domain/glossario-siglas.md)
- [agregados.md](domain/agregados.md)
- [value-objects.md](domain/value-objects.md)

Política:
- Sempre que o domínio mudar, atualizar primeiro `docs/domain`.
- O conteúdo deve ser autossuficiente para regras e contratos de domínio.
- A rastreabilidade histórica é feita por Git.

## 2) Trilha de Arquitetura (`docs/architecture`)
Define como o sistema é organizado para implementar o domínio com segurança e previsibilidade.

Conteúdo:
- visão arquitetural e dependências;
- mapeamento domínio -> módulos de aplicação;
- fluxos de aplicação e integrações;
- infraestrutura e deploy;
- padrões de persistência, segurança, observabilidade e testes.

Leitura inicial recomendada:
1. [docs/architecture/README.md](architecture/README.md)
2. [docs/architecture/architecture-overview.md](architecture/architecture-overview.md)
3. [docs/architecture/clean-architecture.md](architecture/clean-architecture.md)
4. [docs/architecture/architecture-diagrams.md](architecture/architecture-diagrams.md)
5. [docs/architecture/application-and-integrations.md](architecture/application-and-integrations.md)
6. [docs/architecture/deployment-and-infrastructure.md](architecture/deployment-and-infrastructure.md)
7. [docs/architecture/engineering-standards.md](architecture/engineering-standards.md)
8. [docs/architecture/operations-and-quality.md](architecture/operations-and-quality.md)
9. [docs/architecture/testing-and-quality.md](architecture/testing-and-quality.md)
10. [docs/architecture/ci.md](architecture/ci.md)

## 3) Trilha Evolutiva (`docs/specs`)
Registra instruções e snapshots versionados por pasta.

Convenções:
- cada pasta `V<number>` agrupa um ciclo de evolução;
- a versão ativa é a de maior número;
- criação de versão é manual.

## Fluxo Recomendado de Uso
1. Ler `docs/domain` para entender contratos de negócio vigentes.
2. Ler `docs/architecture` para entender implementação e integração.
3. Consultar `docs/specs` quando necessário para histórico evolutivo.

## Regras de Governança
- `docs/domain` permanece a referência canônica das regras de negócio.
- `docs/architecture` não redefine regra de domínio; apenas operacionaliza implementação.
- `docs/specs` é secundário em relação ao estado canônico e arquitetural vigente.
