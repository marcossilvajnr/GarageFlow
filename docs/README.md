# GarageFlow — Guia Geral da Documentação

## Objetivo
Este guia organiza a documentação em 3 trilhas oficiais:

1. `docs/domain`: fonte canônica do domínio.
2. `docs/architecture`: arquitetura de referência e padrões de engenharia.
3. `docs/Specs`: histórico evolutivo versionado por pasta (`V1`, `V2`, ...).

Cada trilha tem propósito próprio e complementar.

## 1) Trilha Canônica de Domínio (`docs/domain`)
Representa o estado atual das regras de negócio e da linguagem ubíqua.

Arquivos-base:
- `linguagem-ubiqua.md`
- `regras-de-negocio.md`
- `bounded-contexts.md`
- `glossario-siglas.md`
- `agregados.md`
- `value-objects.md`

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
- padrões de persistência, segurança, observabilidade e testes.

Leitura inicial recomendada:
1. `docs/architecture/README.md`
2. `docs/architecture/architecture-overview.md`
3. `docs/architecture/application-and-integrations.md`
4. `docs/architecture/engineering-standards.md`
5. `docs/architecture/operations-and-quality.md`
6. `docs/architecture/testing-and-quality.md`

## 3) Trilha Evolutiva (`docs/Specs`)
Registra instruções e snapshots versionados por pasta.

Convenções:
- cada pasta `V<number>` agrupa um ciclo de evolução;
- a versão ativa é a de maior número;
- criação de versão é manual.

## Fluxo Recomendado de Uso
1. Ler `docs/domain` para entender contratos de negócio vigentes.
2. Ler `docs/architecture` para entender implementação e integração.
3. Consultar `docs/Specs` quando necessário para histórico evolutivo.

## Regras de Governança
- `docs/domain` permanece a referência canônica das regras de negócio.
- `docs/architecture` não redefine regra de domínio; apenas operacionaliza implementação.
- `docs/Specs` é secundário em relação ao estado canônico e arquitetural vigente.
