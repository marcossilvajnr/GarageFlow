# Architecture Overview

## Arquitetura de Referência
O GarageFlow adota um monolito modular organizado por Clean Architecture e Hexagonal Architecture.

A aplicação separa entrada HTTP, casos de uso, domínio, infraestrutura e composition root para manter regra de negócio independente de tecnologia. A regra de dependências, responsabilidades das camadas e padrões de fronteira estão em `docs/architecture/clean-architecture.md`.

## Deploy E Operação
- Docker Compose executa o `GarageFlow.WebHost`.
- Kubernetes local usa os manifests em `/k8s`.
- Terraform em `/infra` provisiona o cluster Kind local.
- GitHub Actions executa CI/CD manual com `Quality`, `E2E`, `Build` e `Deploy Kind`.
- AWS/EKS não faz parte do caminho local padrão; pode ser adicionado como estratégia de deploy cloud.

## Mapeamento DDD para Módulos
Cada bounded context do domínio é implementado como módulo lógico em `Domain` e `Application`:
- `Customers`
- `Catalog`
- `Suppliers`
- `ServiceOrders`
- `Executions`
- `Stock`
- `Purchasing`

## Limites dos Módulos
- Cada módulo controla seus próprios agregados e invariantes.
- Integrações entre módulos ocorrem por Application Services e eventos internos de integração.
- Um módulo não altera estado interno de outro módulo diretamente.

## Contratos Públicos
Os contratos públicos são:
- endpoints REST expostos pela API;
- contratos de eventos internos entre módulos dentro do monólito.

## Referências Relacionadas
- Clean Architecture: `docs/architecture/clean-architecture.md`
- Diagramas de arquitetura: `docs/architecture/architecture-diagrams.md`
