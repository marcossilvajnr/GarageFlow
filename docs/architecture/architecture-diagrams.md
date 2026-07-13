# Architecture Diagrams

## Objetivo
Este documento registra os diagramas arquiteturais canônicos do GarageFlow usando Mermaid.

Referências de sintaxe:
- Architecture diagrams: `architecture-beta`
- C4 diagrams: `C4Context` e `C4Container`

## Visão Geral De Infraestrutura
```mermaid
architecture-beta
  group local(server)[Local Development]
  group cluster(cloud)[Kubernetes Cluster]
  group cicd(cloud)[CI CD]

  service source(disk)[Source Code] in local
  service docker(server)[Docker Image] in local
  service terraform(server)[Terraform] in local
  service github(server)[GitHub Actions] in cicd
  service kind(server)[Kind Cluster] in cluster
  service manifests(disk)[Kubernetes Manifests] in cluster
  service webhost(server)[GarageFlow WebHost] in cluster
  service postgres(database)[PostgreSQL] in cluster
  service hpa(server)[HPA] in cluster
  service metrics(server)[Metrics Server] in cluster
  service health(internet)[Health Swagger] in cluster

  align column docker terraform

  source:R --> L:docker
  terraform:T --> B:docker
  docker:R --> L:kind
  github:B --> T:docker
  kind:R --> L:manifests
  manifests:R --> L:webhost
  manifests:B --> T:postgres
  manifests:T --> B:hpa
  metrics:R --> L:hpa
  webhost:R --> L:health
  webhost:B --> T:postgres
```

## C1 — System Context
```mermaid
C4Context
  title GarageFlow - System Context

  Person(customer, "Cliente", "Solicita atendimento e decide orçamento")
  Person(frontDesk, "Atendimento", "Abre Ordem de Serviço, acompanha orçamento e realiza entrega")
  Person(mechanic, "Mecânico", "Registra diagnóstico e execução dos serviços")
  Person(stockist, "Estoquista", "Opera separação, estoque e compras")
  Person(admin, "Administrativo", "Gerencia cadastros e acompanha operação")
  System_Ext(externalSystem, "Sistema externo", "Notifica aprovação ou recusa de orçamento")
  System_Ext(githubActions, "GitHub Actions", "Executa qualidade, build e deploy")

  System(garageFlow, "GarageFlow", "Sistema de gestão de oficina mecânica")

  Rel(customer, garageFlow, "Solicita atendimento e decide orçamento")
  Rel(frontDesk, garageFlow, "Opera OS, orçamento e entrega")
  Rel(mechanic, garageFlow, "Registra diagnóstico e execução")
  Rel(stockist, garageFlow, "Opera separação, estoque e compras")
  Rel(admin, garageFlow, "Administra cadastros e operação")
  Rel(externalSystem, garageFlow, "Envia decisão externa de orçamento", "HTTP/Webhook")
  Rel(githubActions, garageFlow, "Valida, empacota e publica", "CI/CD")
```

## C2 — Container
```mermaid
C4Container
  title GarageFlow - Container

  Person(frontDesk, "Atendimento", "Opera OS, orçamento e entrega")
  Person(mechanic, "Mecânico", "Registra diagnóstico e execução")
  Person(stockist, "Estoquista", "Opera separação, estoque e compras")
  Person(admin, "Administrativo", "Gerencia cadastros e acompanha operação")
  System_Ext(externalSystem, "Sistema externo", "Notifica aprovação ou recusa de orçamento")
  System_Ext(apiClient, "REST Client / Postman / Swagger UI", "Cliente HTTP para operação e validação")
  System_Ext(githubActions, "GitHub Actions", "Executa qualidade, build e deploy")

  System_Boundary(garageFlow, "GarageFlow") {
    Container(webhost, "GarageFlow.WebHost", "ASP.NET Core", "Executável web, composition root, API HTTP, autenticação, autorização e Swagger")
    ContainerDb(database, "PostgreSQL", "PostgreSQL", "Persistência relacional da aplicação")
    Container(openapi, "Swagger/OpenAPI", "OpenAPI", "Documentação HTTP exposta pelo WebHost")
  }

  Rel(frontDesk, webhost, "Opera fluxos de OS", "HTTPS/JSON")
  Rel(mechanic, webhost, "Registra diagnóstico e execução", "HTTPS/JSON")
  Rel(stockist, webhost, "Opera estoque, separação e compras", "HTTPS/JSON")
  Rel(admin, webhost, "Administra cadastros e operação", "HTTPS/JSON")
  Rel(apiClient, webhost, "Executa chamadas REST", "HTTPS/JSON")
  Rel(externalSystem, webhost, "Notifica decisão externa de orçamento", "HTTPS/JSON")
  Rel(githubActions, webhost, "Valida health check após deploy", "HTTP")
  Rel(webhost, database, "Lê e grava dados", "Npgsql/EF Core")
  Rel(webhost, openapi, "Expõe contrato HTTP")
```

## Observações
- `GarageFlow.Api`, `GarageFlow.Application`, `GarageFlow.Domain` e `GarageFlow.Infrastructure` são assemblies e camadas internas carregadas pelo `GarageFlow.WebHost`.
- No C2, apenas `GarageFlow.WebHost`, PostgreSQL e Swagger/OpenAPI aparecem como containers por representarem unidades executáveis, persistentes ou publicamente expostas.
- O detalhamento interno das camadas fica em `architecture-overview.md`.
- O detalhamento de infraestrutura e deploy fica em `deployment-and-infrastructure.md`.
