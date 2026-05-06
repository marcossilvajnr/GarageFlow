#!/usr/bin/env bash
set -euo pipefail

# Local SonarQube analysis for GarageFlow.
# Usage:
#   ./scripts/sonar-local.sh
# Optional env vars:
#   SONAR_HOST_URL (default: http://localhost:9000)
#   SONAR_PROJECT_KEY (default: GarageFlow)
#   SOLUTION_FILE (default: GarageFlow.slnx)
#   BUILD_CONFIGURATION (default: Release)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

if [[ -f "$ROOT_DIR/.env" ]]; then
  # Load local project environment for easier local runs.
  set -a
  # shellcheck disable=SC1091
  source "$ROOT_DIR/.env"
  set +a
fi

SONAR_HOST_URL="${SONAR_HOST_URL:-http://localhost:9000}"
SONAR_PROJECT_KEY="${SONAR_PROJECT_KEY:-GarageFlow}"
SOLUTION_FILE="${SOLUTION_FILE:-GarageFlow.slnx}"
BUILD_CONFIGURATION="${BUILD_CONFIGURATION:-Release}"
TEST_RESULTS_DIR="${TEST_RESULTS_DIR:-TestResults}"
COVERAGE_GLOB="$TEST_RESULTS_DIR/**/coverage.opencover.xml"

# Ensure global dotnet tools are available in this shell session.
export PATH="$PATH:$HOME/.dotnet/tools"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Erro: dotnet não encontrado no PATH."
  exit 1
fi

if ! command -v dotnet-sonarscanner >/dev/null 2>&1; then
  echo "dotnet-sonarscanner não encontrado. Instalando..."
  dotnet tool install --global dotnet-sonarscanner
fi

if [[ -z "${SONAR_TOKEN:-}" ]]; then
  echo "Erro: SONAR_TOKEN não definido. Preencha no arquivo .env."
  exit 1
fi

cd "$ROOT_DIR"

if [[ ! -f "$SOLUTION_FILE" ]]; then
  echo "Erro: solução não encontrada: $SOLUTION_FILE"
  exit 1
fi

echo "Iniciando análise SonarQube local..."
echo "Host: $SONAR_HOST_URL"
echo "Projeto: $SONAR_PROJECT_KEY"

dotnet sonarscanner begin /k:"$SONAR_PROJECT_KEY" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.token="$SONAR_TOKEN" \
  /d:sonar.cs.opencover.reportsPaths="$COVERAGE_GLOB"

dotnet build "$SOLUTION_FILE" --configuration "$BUILD_CONFIGURATION"
rm -rf "$TEST_RESULTS_DIR"
dotnet test "$SOLUTION_FILE" \
  --configuration "$BUILD_CONFIGURATION" \
  --no-build \
  --results-directory "$TEST_RESULTS_DIR" \
  --collect:"XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

if ! find "$TEST_RESULTS_DIR" -type f -name "coverage.opencover.xml" | grep -q .; then
  echo "Erro: nenhum arquivo de cobertura OpenCover foi gerado em $TEST_RESULTS_DIR."
  exit 1
fi

dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"

echo "Análise concluída. Veja o painel em: $SONAR_HOST_URL"
