#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="$SCRIPT_DIR/.env"
GENERATED_HTTP="$SCRIPT_DIR/demo-phase-2-showcase.generated.http"
STATE_FILE="$SCRIPT_DIR/.phase2-demo-state.json"
RUN_ID="$(date +%Y%m%d%H%M%S)"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "Missing $ENV_FILE. Create it from the expected REST Client variables first." >&2
  exit 1
fi

if ! command -v jq >/dev/null 2>&1; then
  echo "Missing jq. Install jq before running this script." >&2
  exit 1
fi

set -a
# shellcheck disable=SC1090
source "$ENV_FILE"
set +a

BASE_URL="http://localhost:${API_HTTP_PORT:?API_HTTP_PORT is required}"
ADMIN_USERNAME="${API_USERNAME:?API_USERNAME is required}"
ADMIN_PASSWORD="${API_PASSWORD:?API_PASSWORD is required}"
EXTERNAL_USERNAME="${EXTERNAL_USERNAME:?EXTERNAL_USERNAME is required}"
EXTERNAL_PASSWORD="${EXTERNAL_PASSWORD:?EXTERNAL_PASSWORD is required}"

log_request_start() {
  local method="$1"
  local path="$2"

  echo "--> $method $path" >&2
}

log_request_success() {
  local method="$1"
  local path="$2"
  local status="$3"

  echo "<-- $status $method $path" >&2
}

request_json() {
  local method="$1"
  local path="$2"
  local token="$3"
  local body="$4"
  local expected="${5:-200}"
  local response_file
  response_file="$(mktemp)"

  local status
  log_request_start "$method" "$path"

  if [[ -n "$token" ]]; then
    status="$(curl -sS -o "$response_file" -w "%{http_code}" \
      -X "$method" "$BASE_URL$path" \
      -H "Authorization: Bearer $token" \
      -H "Content-Type: application/json" \
      -H "Accept: application/json" \
      --data "$body")"
  else
    status="$(curl -sS -o "$response_file" -w "%{http_code}" \
      -X "$method" "$BASE_URL$path" \
      -H "Content-Type: application/json" \
      -H "Accept: application/json" \
      --data "$body")"
  fi

  if [[ "$status" != "$expected" ]]; then
    echo "<-- $status $method $path expected=$expected" >&2
    cat "$response_file" >&2
    rm -f "$response_file"
    exit 1
  fi

  log_request_success "$method" "$path" "$status"
  cat "$response_file"
  rm -f "$response_file"
}

request_empty() {
  local method="$1"
  local path="$2"
  local token="$3"
  local expected="${4:-200}"
  local response_file
  response_file="$(mktemp)"

  local status
  log_request_start "$method" "$path"

  status="$(curl -sS -o "$response_file" -w "%{http_code}" \
    -X "$method" "$BASE_URL$path" \
    -H "Authorization: Bearer $token" \
    -H "Accept: application/json")"

  if [[ "$status" != "$expected" ]]; then
    echo "<-- $status $method $path expected=$expected" >&2
    cat "$response_file" >&2
    rm -f "$response_file"
    exit 1
  fi

  log_request_success "$method" "$path" "$status"
  cat "$response_file"
  rm -f "$response_file"
}

id_from() {
  jq -r '.id // empty'
}

echo "== GarageFlow phase 2 demo setup =="
echo "Base URL: $BASE_URL"

echo "Logging in..."
token="$(
  request_json POST /auth/login "" "$(jq -n \
    --arg username "$ADMIN_USERNAME" \
    --arg password "$ADMIN_PASSWORD" \
    '{username:$username,password:$password}')" 200 | jq -r '.accessToken'
)"

external_token="$(
  request_json POST /auth/login "" "$(jq -n \
    --arg username "$EXTERNAL_USERNAME" \
    --arg password "$EXTERNAL_PASSWORD" \
    '{username:$username,password:$password}')" 200 | jq -r '.accessToken'
)"

echo "Resetting database..."
request_json POST /dev/database/reset "$token" '{"confirm":true}' 200 >/dev/null
request_empty GET /health "$token" 200 >/dev/null

echo "Creating base catalog..."
supplier_id="$(request_json POST /suppliers/ "$token" '{
  "name": "Fornecedor Fase Dois LTDA",
  "cnpj": "11222333000181",
  "email": "fase2.fornecedor@garageflow.com",
  "phoneNumber": "11940020002",
  "street": "Avenida das Integracoes",
  "number": "200",
  "complement": null,
  "neighborhood": "Centro",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "01010000"
}' 201 | id_from)"

part_id="$(request_json POST /parts/ "$token" '{
  "name": "Filtro Fase Dois",
  "code": "F2-PART-FILTER",
  "sku": "F2-SKU-FILTER",
  "unitOfMeasure": "UN",
  "unitPrice": 49.90
}' 201 | id_from)"

supply_id="$(request_json POST /supplies/ "$token" "$(jq -n \
  --arg supplierId "$supplier_id" \
  '{
    name:"Oleo Fase Dois",
    code:"F2-SUP-OIL",
    unitOfMeasure:"L",
    baseCost:35.00,
    preferredSupplierId:$supplierId
  }')" 201 | id_from)"

primary_service_id="$(request_json POST /services/ "$token" '{
  "code": "F2-SRV-OIL",
  "name": "Troca de Oleo Fase Dois",
  "description": "Servico base usado na demonstracao da entrega",
  "basePrice": 250.00,
  "estimatedDurationMinutes": 60
}' 201 | id_from)"

request_json POST "/services/$primary_service_id/parts" "$token" "$(jq -n \
  --arg partId "$part_id" \
  '{partId:$partId,quantity:1}')" 200 >/dev/null

request_json POST "/services/$primary_service_id/supplies" "$token" "$(jq -n \
  --arg supplyId "$supply_id" \
  '{supplyId:$supplyId,quantity:4}')" 200 >/dev/null

secondary_service_id="$(request_json POST /services/ "$token" '{
  "code": "F2-SRV-ALIGN",
  "name": "Alinhamento Fase Dois",
  "description": "Servico adicional para demonstrar OS com multiplos servicos",
  "basePrice": 180.00,
  "estimatedDurationMinutes": 45
}' 201 | id_from)"

echo "Creating people..."
customer_id="$(request_json POST /customers/ "$token" '{
  "name": "Cliente Fase Dois",
  "documentType": 0,
  "document": "12345678909",
  "email": "cliente.fase2@email.com",
  "phoneNumber": "11987654321",
  "street": "Rua da Demo",
  "number": "42",
  "complement": null,
  "neighborhood": "Centro",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "01010000"
}' 201 | id_from)"

front_desk_id="$(request_json POST /employees/ "$token" '{
  "name": "Atendente Fase Dois",
  "documentType": 0,
  "document": "31415926590",
  "email": "atendente.fase2@garageflow.com",
  "phoneNumber": "11911112222",
  "street": "Rua A",
  "number": "10",
  "complement": null,
  "neighborhood": "Centro",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "01111000",
  "role": 1
}' 201 | id_from)"

mechanic_id="$(request_json POST /employees/ "$token" '{
  "name": "Mecanico Fase Dois",
  "documentType": 0,
  "document": "27182818205",
  "email": "mecanico.fase2@garageflow.com",
  "phoneNumber": "11933334444",
  "street": "Rua B",
  "number": "20",
  "complement": null,
  "neighborhood": "Vila Nova",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "02222000",
  "role": 2
}' 201 | id_from)"

stockist_id="$(request_json POST /employees/ "$token" '{
  "name": "Estoquista Fase Dois",
  "documentType": 0,
  "document": "13579246828",
  "email": "estoquista.fase2@garageflow.com",
  "phoneNumber": "11955556666",
  "street": "Rua C",
  "number": "30",
  "complement": null,
  "neighborhood": "Industrial",
  "city": "Sao Paulo",
  "state": "SP",
  "zipCode": "03333000",
  "role": 3
}' 201 | id_from)"

request_json POST /stock/entries "$token" "$(jq -n \
  --arg partId "$part_id" \
  '{
    itemId:$partId,
    itemType:1,
    quantity:20,
    minimumQuantity:5,
    reason:"Carga inicial para separacao da demonstracao de entrega",
    referenceId:null
  }')" 200 >/dev/null

create_vehicle() {
  local plate="$1"
  local renavam="$2"
  local make="$3"
  local model="$4"
  local year="$5"
  local color="$6"

  request_json POST /vehicles/ "$token" "$(jq -n \
    --arg customerId "$customer_id" \
    --arg plate "$plate" \
    --arg renavam "$renavam" \
    --arg make "$make" \
    --arg model "$model" \
    --argjson year "$year" \
    --arg color "$color" \
    '{
      customerId:$customerId,
      licensePlate:$plate,
      renavam:$renavam,
      make:$make,
      model:$model,
      year:$year,
      color:$color
    }')" 201 | id_from
}

create_service_order() {
  local vehicle_id="$1"
  shift
  local services_json
  services_json="$(printf '%s\n' "$@" | jq -R . | jq -s .)"

  request_json POST /service-orders/ "$token" "$(jq -n \
    --arg customerId "$customer_id" \
    --arg vehicleId "$vehicle_id" \
    --arg frontDeskEmployeeId "$front_desk_id" \
    --argjson serviceIds "$services_json" \
    '{
      customerId:$customerId,
      vehicleId:$vehicleId,
      frontDeskEmployeeId:$frontDeskEmployeeId,
      serviceIds:$serviceIds
    }')" 201 | id_from
}

echo "Creating service orders..."
vehicle_received_id="$(create_vehicle FAA1A01 11144477731 Toyota Corolla 2020 Prata)"
received_order_id="$(create_service_order "$vehicle_received_id" "$primary_service_id" "$secondary_service_id")"

vehicle_diagnostic_id="$(create_vehicle FBB2B02 22244477734 Honda Civic 2021 Preto)"
diagnostic_order_id="$(create_service_order "$vehicle_diagnostic_id" "$primary_service_id")"

vehicle_waiting_id="$(create_vehicle FCC3C03 33344477737 Volkswagen Jetta 2019 Branco)"
waiting_order_id="$(create_service_order "$vehicle_waiting_id" "$primary_service_id")"

vehicle_approved_id="$(create_vehicle FDD4D04 44444477730 Chevrolet Onix 2022 Azul)"
approved_order_id="$(create_service_order "$vehicle_approved_id" "$primary_service_id")"

vehicle_in_execution_id="$(create_vehicle FEE5E05 55544477732 Fiat Pulse 2023 Cinza)"
in_execution_order_id="$(create_service_order "$vehicle_in_execution_id" "$primary_service_id")"

vehicle_delivered_id="$(create_vehicle FFF6F06 66644477735 Renault Duster 2020 Vermelho)"
delivered_order_id="$(create_service_order "$vehicle_delivered_id" "$primary_service_id")"

vehicle_demo_id="$(create_vehicle FGG7G07 77744477738 Hyundai HB20 2024 Grafite)"

start_diagnostic() {
  local order_id="$1"
  request_json POST "/service-orders/$order_id/diagnostic/start" "$token" "$(jq -n \
    --arg mechanicId "$mechanic_id" \
    '{mechanicId:$mechanicId}')" 200 >/dev/null
}

prepare_waiting_approval() {
  local order_id="$1"
  local description="$2"
  start_diagnostic "$order_id"
  request_json POST "/service-orders/$order_id/diagnostic/services" "$token" "$(jq -n \
    --arg serviceId "$secondary_service_id" \
    '{serviceId:$serviceId}')" 200 >/dev/null
  request_json POST "/service-orders/$order_id/diagnostic/complete" "$token" "$(jq -n \
    --arg description "$description" \
    '{description:$description}')" 200 >/dev/null
  request_empty POST "/service-orders/$order_id/diagnostic/consolidate-services" "$token" 200 >/dev/null
  request_empty POST "/service-orders/$order_id/quote/generate" "$token" 200 >/dev/null
}

echo "Moving orders to target statuses..."
start_diagnostic "$diagnostic_order_id"

prepare_waiting_approval "$waiting_order_id" "Diagnostico concluido para demonstracao de status aguardando aprovacao."
prepare_waiting_approval "$approved_order_id" "Diagnostico concluido para demonstracao de aprovacao externa."

prepare_waiting_approval "$in_execution_order_id" "Diagnostico concluido para demonstracao de fila em execucao."
request_empty POST "/service-orders/$in_execution_order_id/quote/accept" "$token" 200 >/dev/null
in_execution_execution_id="$(request_json POST /execution-orders/ "$token" "$(jq -n \
  --arg serviceOrderId "$in_execution_order_id" \
  --arg serviceId "$secondary_service_id" \
  --arg mechanicId "$mechanic_id" \
  '{serviceOrderId:$serviceOrderId,serviceId:$serviceId,mechanicId:$mechanicId}')" 201 | id_from)"
request_empty POST "/execution-orders/$in_execution_execution_id/mark-ready" "$token" 200 >/dev/null
request_empty POST "/execution-orders/$in_execution_execution_id/start" "$token" 200 >/dev/null

prepare_waiting_approval "$delivered_order_id" "Diagnostico concluido para demonstracao de exclusao da fila operacional."
request_empty POST "/service-orders/$delivered_order_id/quote/accept" "$token" 200 >/dev/null
delivered_execution_id="$(request_json POST /execution-orders/ "$token" "$(jq -n \
  --arg serviceOrderId "$delivered_order_id" \
  --arg serviceId "$secondary_service_id" \
  --arg mechanicId "$mechanic_id" \
  '{serviceOrderId:$serviceOrderId,serviceId:$serviceId,mechanicId:$mechanicId}')" 201 | id_from)"
request_empty POST "/execution-orders/$delivered_execution_id/mark-ready" "$token" 200 >/dev/null
request_empty POST "/execution-orders/$delivered_execution_id/start" "$token" 200 >/dev/null

delivered_separation_id="$(request_json POST /separation-orders/ "$token" "$(jq -n \
  --arg executionOrderId "$delivered_execution_id" \
  --arg partId "$part_id" \
  '{
    executionOrderId:$executionOrderId,
    parts:[{partId:$partId,partName:"Filtro Fase Dois",quantity:1}],
    supplies:[]
  }')" 201 | id_from)"
request_empty POST "/separation-orders/$delivered_separation_id/reserve" "$token" 200 >/dev/null
request_json POST "/separation-orders/$delivered_separation_id/confirm-stockist-withdrawal" "$token" "$(jq -n \
  --arg stockistId "$stockist_id" \
  '{stockistId:$stockistId}')" 200 >/dev/null
request_empty POST "/separation-orders/$delivered_separation_id/confirm-mechanic-receipt" "$token" 200 >/dev/null
request_empty POST "/execution-orders/$delivered_execution_id/complete" "$token" 200 >/dev/null
request_empty POST "/service-orders/$delivered_order_id/deliver" "$token" 200 >/dev/null

cat >"$STATE_FILE" <<EOF
{
  "baseUrl": "$BASE_URL",
  "receivedOrderId": "$received_order_id",
  "diagnosticOrderId": "$diagnostic_order_id",
  "waitingApprovalOrderId": "$waiting_order_id",
  "externalApprovalOrderId": "$approved_order_id",
  "inExecutionOrderId": "$in_execution_order_id",
  "deliveredOrderId": "$delivered_order_id",
  "customerId": "$customer_id",
  "vehicleDemoCreatedId": "$vehicle_demo_id",
  "frontDeskEmployeeId": "$front_desk_id",
  "primaryServiceId": "$primary_service_id",
  "secondaryServiceId": "$secondary_service_id"
}
EOF

cat >"$GENERATED_HTTP" <<EOF
# GarageFlow - Phase 2 Showcase (generated)
# Generated by tools/rest-client/prepare-phase-2-demo.sh
# Do not commit this file. It contains local runtime tokens and ids.

@baseUrl = $BASE_URL
@token = $token
@externalToken = $external_token

@customerId = $customer_id
@vehicleDemoCreatedId = $vehicle_demo_id
@frontDeskEmployeeId = $front_desk_id
@primaryServiceId = $primary_service_id
@secondaryServiceId = $secondary_service_id

@receivedOrderId = $received_order_id
@diagnosticOrderId = $diagnostic_order_id
@waitingApprovalOrderId = $waiting_order_id
@approvedOrderId = $approved_order_id
@inExecutionOrderId = $in_execution_order_id
@deliveredOrderId = $delivered_order_id
@demoCreatedOrderId = {{demoCreateServiceOrder.response.body.$.id}}

###
### 10) Demo Opening Service Order With Initial Services
###

# @name demoCreateServiceOrder
POST {{baseUrl}}/service-orders/
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "customerId": "{{customerId}}",
  "vehicleId": "{{vehicleDemoCreatedId}}",
  "frontDeskEmployeeId": "{{frontDeskEmployeeId}}",
  "serviceIds": [
    "{{primaryServiceId}}",
    "{{secondaryServiceId}}"
  ]
}

###

# Expected: Received / Recebida
GET {{baseUrl}}/service-orders/{{demoCreatedOrderId}}/status
Authorization: Bearer {{token}}
Accept: application/json

###
### 11) Demo External Quote Approval Notification
###

POST {{baseUrl}}/external/service-order-quote-notifications
Authorization: Bearer {{externalToken}}
Content-Type: application/json

{
  "serviceOrderId": "{{approvedOrderId}}",
  "decision": "Approved",
  "reason": null,
  "externalNotificationId": "demo-approved-$RUN_ID",
  "source": "rest-client-generated-demo"
}

###

# Expected: Approved / Orcamento aprovado
GET {{baseUrl}}/service-orders/{{approvedOrderId}}/status
Authorization: Bearer {{token}}
Accept: application/json

###
### 12) Demo Status Read Model Labels
###

# Expected: Received / Recebida
GET {{baseUrl}}/service-orders/{{receivedOrderId}}/status
Authorization: Bearer {{token}}
Accept: application/json

###

# Expected: InDiagnostic / Diagnostico
GET {{baseUrl}}/service-orders/{{diagnosticOrderId}}/status
Authorization: Bearer {{token}}
Accept: application/json

###

# Expected: WaitingApproval / Aguardando Aprovacao
GET {{baseUrl}}/service-orders/{{waitingApprovalOrderId}}/status
Authorization: Bearer {{token}}
Accept: application/json

###

# Expected: Approved / Orcamento aprovado
GET {{baseUrl}}/service-orders/{{approvedOrderId}}/status
Authorization: Bearer {{token}}
Accept: application/json

###

# Expected: InExecution / Execucao
GET {{baseUrl}}/service-orders/{{inExecutionOrderId}}/status
Authorization: Bearer {{token}}
Accept: application/json

###

# Expected: Delivered / Entregue
GET {{baseUrl}}/service-orders/{{deliveredOrderId}}/status
Authorization: Bearer {{token}}
Accept: application/json

###
### 13) Demo Operational Queue Ordering
###

# Expected simplified items:
# serviceOrderId, status, label
GET {{baseUrl}}/service-orders/operational?page=1&pageSize=20
Authorization: Bearer {{token}}
Accept: application/json

###
### 14) Full List For Comparison
###

# Full list keeps the generic listing behavior and includes delivered orders.
GET {{baseUrl}}/service-orders?page=1&pageSize=20
Authorization: Bearer {{token}}
Accept: application/json
EOF

echo
echo "Setup complete."
echo "Generated showcase file:"
echo "  $GENERATED_HTTP"
echo
echo "Prepared service orders:"
echo "  Received:        $received_order_id"
echo "  InDiagnostic:    $diagnostic_order_id"
echo "  WaitingApproval: $waiting_order_id"
echo "  External approve candidate: $approved_order_id"
echo "  InExecution:     $in_execution_order_id"
echo "  Delivered:       $delivered_order_id"
echo
echo "Bearer tokens for Swagger:"
echo "  Admin token:"
echo "  $token"
echo
echo "  External token:"
echo "  $external_token"
