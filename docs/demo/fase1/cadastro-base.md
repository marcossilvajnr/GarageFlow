# Demo Fase 1 - Cadastro Base (Swagger)

## Objetivo
Preparar os dados mestres para os fluxos de OS, separacao e compra.

## Pre-requisito de autenticacao
Todos os endpoints abaixo exigem perfil `Administrative`.

1. `POST /auth/login`

```json
{
  "username": "admin",
  "password": "admin123"
}
```

No Swagger, use o `accessToken` retornado no botao `Authorize`.

## Ordem sugerida de cadastro
1. Fornecedores
2. Pecas
3. Insumos
4. Servicos
5. Composicao do servico (pecas + insumos)
6. Clientes
7. Veiculos
8. Funcionarios (atendente, mecanico, estoquista)

## 1) Fornecedores
Endpoints:
- `POST /suppliers/`
- `GET /suppliers/`

```json
{
  "name": "Fornecedor Centro Automotivo LTDA",
  "cnpj": "12345678000190",
  "email": "contato@fornecedorcentro.com.br",
  "phoneNumber": "11940028922",
  "street": "Av. Industrial",
  "number": "1000",
  "complement": "Bloco B",
  "neighborhood": "Distrito Industrial",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "04567000"
}
```

Guardar: `supplierId`.

## 2) Pecas
Endpoints:
- `POST /parts/`
- `GET /parts/`

```json
{
  "name": "Filtro de Oleo",
  "code": "PART-FILTRO-001",
  "sku": "SKU-FILTRO-001",
  "unitOfMeasure": "UN",
  "unitPrice": 45.90
}
```

Guardar: `partId`.

## 3) Insumos
Endpoints:
- `POST /supplies/`
- `GET /supplies/`

```json
{
  "name": "Oleo 5W30",
  "code": "SUP-5W30-001",
  "unitOfMeasure": "L",
  "baseCost": 32.50,
  "preferredSupplierId": "{{supplierId}}"
}
```

Guardar: `supplyId`.

## 4) Servicos
Endpoints:
- `POST /services/`
- `GET /services/`

```json
{
  "code": "SRV-TROCA-OLEO-001",
  "name": "Troca de Oleo e Filtro",
  "description": "Troca completa de oleo e filtro do motor",
  "basePrice": 220.00,
  "estimatedDurationMinutes": 60
}
```

Guardar: `serviceId`.

## 5) Composicao do servico
### 5.1 Adicionar peca ao servico
Endpoint:
- `POST /services/{serviceId}/parts`

```json
{
  "partId": "{{partId}}",
  "quantity": 1
}
```

### 5.2 Adicionar insumo ao servico
Endpoint:
- `POST /services/{serviceId}/supplies`

```json
{
  "supplyId": "{{supplyId}}",
  "quantity": 4
}
```

## 6) Clientes
Endpoints:
- `POST /customers/`
- `GET /customers/`

`documentType`: `Cpf` ou `Cnpj`.

```json
{
  "name": "Marcos Silva",
  "documentType": "Cpf",
  "document": "12345678901",
  "email": "marcos.silva@email.com",
  "phoneNumber": "11987654321",
  "street": "Rua das Oficinas",
  "number": "145",
  "complement": "Casa",
  "neighborhood": "Centro",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "01010000"
}
```

Guardar: `customerId`.

## 7) Veiculos
Endpoints:
- `POST /vehicles/`
- `GET /vehicles/`

```json
{
  "customerId": "{{customerId}}",
  "licensePlate": "BRA2E19",
  "renavam": "12345678901",
  "make": "Toyota",
  "model": "Corolla",
  "year": 2020,
  "color": "Prata"
}
```

Guardar: `vehicleId`.

## 8) Funcionarios
Endpoints:
- `POST /employees/`
- `GET /employees/`

`documentType`: `Cpf` ou `Cnpj`  
`role`: `Attendant`, `Mechanic`, `Stockist` ou `Administrative`

### 8.1 Atendente
```json
{
  "name": "Ana Atendimento",
  "documentType": "Cpf",
  "document": "11122233344",
  "email": "ana.atendimento@garageflow.com",
  "phoneNumber": "11911112222",
  "street": "Rua A",
  "number": "10",
  "complement": null,
  "neighborhood": "Centro",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "01111000",
  "role": "Attendant"
}
```

### 8.2 Mecanico
```json
{
  "name": "Carlos Mecanico",
  "documentType": "Cpf",
  "document": "55566677788",
  "email": "carlos.mecanico@garageflow.com",
  "phoneNumber": "11933334444",
  "street": "Rua B",
  "number": "20",
  "complement": null,
  "neighborhood": "Vila Nova",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "02222000",
  "role": "Mechanic"
}
```

### 8.3 Estoquista
```json
{
  "name": "Bruno Estoque",
  "documentType": "Cpf",
  "document": "99988877766",
  "email": "bruno.estoque@garageflow.com",
  "phoneNumber": "11955556666",
  "street": "Rua C",
  "number": "30",
  "complement": null,
  "neighborhood": "Industrial",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "03333000",
  "role": "Stockist"
}
```

Guardar:
- `attendantEmployeeId`
- `mechanicEmployeeId`
- `stockistEmployeeId`

## Checklist final da base
1. `supplierId` criado
2. `partId` criado
3. `supplyId` criado
4. `serviceId` criado e com peca/insumo vinculados
5. `customerId` criado
6. `vehicleId` criado (do `customerId`)
7. IDs dos 3 funcionarios criados
