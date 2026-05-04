#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
OUT_DIR="$ROOT_DIR/artifacts/test-breakdown"
mkdir -p "$OUT_DIR"

run_count() {
  local label="$1"
  local filter="$2"
  local output

  output=$(dotnet test "$ROOT_DIR/tests/GarageFlow.Tests/GarageFlow.Tests.csproj" \
    --nologo \
    --no-build \
    --configuration Release \
    --filter "$filter" \
    --logger "console;verbosity=minimal" 2>&1)

  local total
  total=$(echo "$output" | grep -Eo 'Total:[[:space:]]*[0-9]+' | tail -n 1 | grep -Eo '[0-9]+')

  if [[ -z "${total:-}" ]]; then
    echo "Unable to parse test total for $label" >&2
    echo "$output" >&2
    exit 1
  fi

  echo "$total"
}

domain_total=$(run_count "Domain" "FullyQualifiedName~GarageFlow.Tests.Domain")
application_total=$(run_count "Application" "FullyQualifiedName~GarageFlow.Tests.Application")
integration_total=$(run_count "Integration" "FullyQualifiedName~GarageFlow.Tests.Integration")
all_total=$((domain_total + application_total + integration_total))

cat > "$OUT_DIR/test-breakdown.json" <<JSON
{
  "domain": $domain_total,
  "application": $application_total,
  "integration": $integration_total,
  "total": $all_total
}
JSON

cat > "$OUT_DIR/test-breakdown.md" <<MD
## Test Breakdown

| Tipo | Quantidade |
| --- | ---: |
| Domain | $domain_total |
| Application | $application_total |
| Integration | $integration_total |
| **Total** | **$all_total** |
MD
