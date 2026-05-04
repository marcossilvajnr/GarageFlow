#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
OUT_DIR="$ROOT_DIR/artifacts/security"
mkdir -p "$OUT_DIR"

# Do not fail pipeline at this stage for discovered vulnerabilities.
dotnet list "$ROOT_DIR/GarageFlow.slnx" package --vulnerable --include-transitive --format json > "$OUT_DIR/security-report.json" || true

python3 - <<'PY'
import json
from pathlib import Path

path = Path("artifacts/security/security-report.json")
raw = path.read_text(encoding="utf-8").strip()

report = {"high_or_critical": 0, "total_vulnerabilities": 0, "packages_with_vulnerabilities": 0}

if raw:
    data = json.loads(raw)
    sources = data.get("sources", [])
    projects = data.get("projects", [])
    vuln_items = []
    package_count = 0

    for project in projects:
        frameworks = project.get("frameworks", [])
        for fw in frameworks:
            top = fw.get("topLevelPackages", [])
            trans = fw.get("transitivePackages", [])
            for pkg in top + trans:
                vulns = pkg.get("vulnerabilities", [])
                if vulns:
                    package_count += 1
                for v in vulns:
                    vuln_items.append(v)

    high_critical = sum(1 for v in vuln_items if str(v.get("severity", "")).lower() in {"high", "critical"})

    report["high_or_critical"] = high_critical
    report["total_vulnerabilities"] = len(vuln_items)
    report["packages_with_vulnerabilities"] = package_count

md = f"""## Security Report

| Métrica | Valor |
| --- | ---: |
| Pacotes com vulnerabilidades | {report['packages_with_vulnerabilities']} |
| Vulnerabilidades totais | {report['total_vulnerabilities']} |
| Vulnerabilidades high/critical | {report['high_or_critical']} |

Fonte: `dotnet list package --vulnerable --include-transitive --format json`
"""

Path("artifacts/security/security-report.md").write_text(md, encoding="utf-8")
PY
