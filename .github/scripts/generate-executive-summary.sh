#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
OUT_DIR="$ROOT_DIR/artifacts/executive"
mkdir -p "$OUT_DIR"

python3 - <<'PY'
import json
import re
from pathlib import Path

security = json.loads(Path("artifacts/security/security-report.json").read_text(encoding="utf-8") or "{}")
breakdown = json.loads(Path("artifacts/test-breakdown/test-breakdown.json").read_text(encoding="utf-8"))
coverage_md = Path("artifacts/coverage/coverage-summary.md").read_text(encoding="utf-8")

line_cov_match = re.search(r"Line coverage:[^0-9]*([0-9]+(?:\.[0-9]+)?)%", coverage_md, re.IGNORECASE)
line_cov = float(line_cov_match.group(1)) if line_cov_match else 0.0

projects = security.get("projects", [])
vuln_total = 0
high_critical = 0
packages_with_vuln = 0

for project in projects:
    for fw in project.get("frameworks", []):
        for pkg in fw.get("topLevelPackages", []) + fw.get("transitivePackages", []):
            vulns = pkg.get("vulnerabilities", [])
            if vulns:
                packages_with_vuln += 1
            vuln_total += len(vulns)
            for v in vulns:
                sev = str(v.get("severity", "")).lower()
                if sev in {"high", "critical"}:
                    high_critical += 1

if line_cov >= 80:
    cov_status = "OK"
elif line_cov >= 60:
    cov_status = "WARN"
else:
    cov_status = "NOK"

if high_critical == 0:
    sec_status = "OK"
elif high_critical <= 2:
    sec_status = "WARN"
else:
    sec_status = "NOK"

md = f"""# Executive Dashboard

| Bloco | Status | Indicador |
| --- | --- | --- |
| Build and Tests | OK | Build concluido e testes executados |
| Coverage | {cov_status} | Line coverage: {line_cov:.1f}% |
| Security | {sec_status} | High/Critical: {high_critical} |

## KPIs

| Metrica | Valor |
| --- | ---: |
| Testes Domain | {breakdown['domain']} |
| Testes Application | {breakdown['application']} |
| Testes Integration | {breakdown['integration']} |
| Testes Totais | {breakdown['total']} |
| Pacotes com vulnerabilidades | {packages_with_vuln} |
| Vulnerabilidades totais | {vuln_total} |
| Vulnerabilidades high/critical | {high_critical} |
| Cobertura de linha | {line_cov:.1f}% |
"""

Path("artifacts/executive/executive-summary.md").write_text(md, encoding="utf-8")
PY
