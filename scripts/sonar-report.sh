#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
OUTPUT_FILE="${OUTPUT_FILE:-$ROOT_DIR/sonarqube-issues-summary.json}"

if [[ -f "$ROOT_DIR/.env" ]]; then
  set -a
  # shellcheck disable=SC1091
  source "$ROOT_DIR/.env"
  set +a
fi

SONAR_HOST_URL="${SONAR_HOST_URL:-http://localhost:9000}"
SONAR_PROJECT_KEY="${SONAR_PROJECT_KEY:-GarageFlow}"

if [[ -z "${SONAR_TOKEN:-}" ]]; then
  echo "Erro: SONAR_TOKEN não definido. Preencha no arquivo .env."
  exit 1
fi

if ! command -v curl >/dev/null 2>&1; then
  echo "Erro: curl não encontrado no PATH."
  exit 1
fi

if ! command -v jq >/dev/null 2>&1; then
  echo "Erro: jq não encontrado no PATH."
  exit 1
fi

issues_payload="$(curl -sS -u "$SONAR_TOKEN:" \
  "$SONAR_HOST_URL/api/issues/search?componentKeys=$SONAR_PROJECT_KEY&resolved=false&ps=500&p=1&additionalFields=_all")"

if [[ "$(echo "$issues_payload" | jq -r 'has("errors")')" == "true" ]]; then
  echo "Erro ao consultar issues no SonarQube:"
  echo "$issues_payload" | jq
  exit 1
fi

total="$(echo "$issues_payload" | jq -r '.total // 0')"
page_size="$(echo "$issues_payload" | jq -r '.paging.pageSize // 500')"
issues_json="$(echo "$issues_payload" | jq '.issues // []')"
rules_json="$(echo "$issues_payload" | jq '.rules // []')"
components_json="$(echo "$issues_payload" | jq '.components // []')"
facets_json="$(echo "$issues_payload" | jq '.facets // []')"

if [[ "$total" -gt "$page_size" ]]; then
  total_pages=$(( (total + page_size - 1) / page_size ))
  for ((page=2; page<=total_pages; page++)); do
    page_payload="$(curl -sS -u "$SONAR_TOKEN:" \
      "$SONAR_HOST_URL/api/issues/search?componentKeys=$SONAR_PROJECT_KEY&resolved=false&ps=$page_size&p=$page&additionalFields=_all")"

    if [[ "$(echo "$page_payload" | jq -r 'has("errors")')" == "true" ]]; then
      echo "Erro ao consultar issues no SonarQube na página $page:"
      echo "$page_payload" | jq
      exit 1
    fi

    issues_json="$(jq -c -n --argjson left "$issues_json" --argjson right "$(echo "$page_payload" | jq '.issues // []')" '$left + $right')"
    rules_json="$(jq -c -n --argjson left "$rules_json" --argjson right "$(echo "$page_payload" | jq '.rules // []')" '$left + $right | unique_by(.key)')"
    components_json="$(jq -c -n --argjson left "$components_json" --argjson right "$(echo "$page_payload" | jq '.components // []')" '$left + $right | unique_by(.key)')"
  done
fi

summary_json="$(jq -n \
  --arg generatedAt "$(date -u +"%Y-%m-%dT%H:%M:%SZ")" \
  --arg host "$SONAR_HOST_URL" \
  --arg project "$SONAR_PROJECT_KEY" \
  --argjson total "$total" \
  --argjson pageSize "$page_size" \
  --argjson issues "$issues_json" \
  --argjson rules "$rules_json" \
  --argjson components "$components_json" \
  --argjson facets "$facets_json" '
{
  generatedAt: $generatedAt,
  sonarHost: $host,
  projectKey: $project,
  totalOpenIssues: $total,
  pageSize: $pageSize,
  issues: $issues,
  rules: $rules,
  components: $components,
  facets: $facets
}')"

printf '%s\n' "$summary_json" > "$OUTPUT_FILE"

echo "Relatório gerado em: $OUTPUT_FILE"
