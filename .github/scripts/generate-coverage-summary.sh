#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
OUT_DIR="$ROOT_DIR/artifacts/coverage"
mkdir -p "$OUT_DIR"

coverage_files=()
while IFS= read -r file; do
  coverage_files+=("$file")
done < <(find "$ROOT_DIR" -type f -name "coverage.cobertura.xml")

if [[ ${#coverage_files[@]} -eq 0 ]]; then
  echo "No coverage.cobertura.xml file found." >&2
  exit 1
fi

reports_arg=$(IFS=';'; echo "${coverage_files[*]}")

reportgenerator \
  -reports:"$reports_arg" \
  -targetdir:"$OUT_DIR/html" \
  -reporttypes:"Html;MarkdownSummaryGithub"

if [[ ! -f "$OUT_DIR/html/SummaryGithub.md" ]]; then
  echo "Coverage summary file was not generated." >&2
  exit 1
fi

cp "$OUT_DIR/html/SummaryGithub.md" "$OUT_DIR/coverage-summary.md"
tar -czf "$OUT_DIR/coverage-html.tar.gz" -C "$OUT_DIR/html" .
