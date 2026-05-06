# Operations

## Objetivo
Esta seção define o baseline operacional do GarageFlow para execução local, validação técnica e evolução da esteira.

## Escopo Atual
- Execução local com Docker Compose.
- Verificação de disponibilidade da API via `health` e `swagger`.
- Execução manual da esteira de qualidade e segurança no GitHub Actions.
- Referência operacional para evidências e artefatos de execução.

## Operação Local (Docker)
Artefatos oficiais:
- `Dockerfile`
- `docker-compose.yml`
- `README.md` (raiz)

Princípios operacionais:
- configuração por variáveis de ambiente;
- configuração padrão pronta no `docker-compose.yml` para execução local imediata;
- banco PostgreSQL com healthcheck;
- API exposta para validação de contrato (`swagger`).

## SonarQube Local (Instalação e Execução)
Objetivo:
- executar análise estática e cobertura localmente, sem depender da pipeline remota.

Pré-requisitos:
- Docker Desktop em execução;
- `.env` preenchido com `SONAR_TOKEN`, `SONAR_HOST_URL` e `SONAR_PROJECT_KEY`;
- SDK .NET instalado localmente;
- `curl` e `jq` instalados localmente (para export de issues).

Instalação/validação de ferramentas locais:

```bash
docker --version
dotnet --version
curl --version
jq --version
```

Instalação manual do scanner .NET (opcional):

```bash
dotnet tool install --global dotnet-sonarscanner
```

Observação:
- o script `./scripts/sonar-local.sh` instala automaticamente o `dotnet-sonarscanner` caso ele não exista no ambiente.

Passo a passo:
1. Subir API, banco e SonarQube no mesmo compose:

```bash
docker compose --profile quality up -d --build
```

2. Acessar SonarQube:
URL: `http://localhost:9000`  
Primeiro login: `admin` / `admin` (troca de senha obrigatória no primeiro acesso).

3. Gerar token de acesso:
Fluxo: `My Account` -> `Security` -> `Generate Tokens`  
Ação: copiar token e preencher `SONAR_TOKEN` no arquivo `.env`.

4. Criar/confirmar a project key:
No SonarQube, confirmar que a key do projeto é a mesma usada no `.env` em `SONAR_PROJECT_KEY` (padrão recomendado: `GarageFlow`).

5. Rodar análise local da solução:

```bash
./scripts/sonar-local.sh
```

6. (Opcional) Exportar issues abertas para análise em equipe:

```bash
./scripts/sonar-report.sh
```

Saída:
Arquivo `sonarqube-issues-summary.json` na raiz do projeto.

Troubleshooting rápido:
- SonarQube ainda inicializando: `docker compose logs -f sonarqube`
- token ausente/inválido: revisar `SONAR_TOKEN` no `.env`
- cobertura não refletida: confirmar se houve geração de `coverage.opencover.xml` em `TestResults/`.

## Pipeline Manual (Operação)
A pipeline de qualidade e segurança é executada sob demanda (`workflow_dispatch`) para reduzir custo e manter evidências auditáveis.

Evidências mínimas esperadas por execução:
- build da solução;
- execução de testes automatizados;
- cobertura consolidada de testes;
- relatório de vulnerabilidades de dependências (`dotnet list package --vulnerable --include-transitive`);
- contagem de testes por tipo (`Domain`, `Application`, `Integration`).

Saídas obrigatórias:
- resumo executivo no Job Summary;
- artifacts baixáveis com relatórios completos.
- dashboard visual no Job Summary com KPIs consolidados.
- resultado visual de testes no run do GitHub Actions (a partir de `.trx`).

## Governança e Evolução da Esteira
A evolução deve ser incremental e orientada a custo-benefício:
1. manual on-demand (estado atual);
2. automação seletiva por branch/release;
3. controles adicionais quando houver necessidade operacional.

Regras:
- não introduzir custo recorrente sem evidência de ganho;
- preservar rastreabilidade dos relatórios;
- manter compatibilidade com os contratos públicos da API.

## Referências Relacionadas
- Estratégia de testes e qualidade: `docs/architecture/testing-and-quality.md`
