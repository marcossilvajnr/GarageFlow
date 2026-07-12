#!/usr/bin/env bash
set -euo pipefail

CLUSTER_NAME="${CLUSTER_NAME:-garageflow-ci}"
NAMESPACE="${NAMESPACE:-garageflow}"
APP_IMAGE="${APP_IMAGE:-garageflow-api:latest}"
CI_IMAGE="${CI_IMAGE:-garageflow-api:ci}"
PORT_FORWARD_PORT="${PORT_FORWARD_PORT:-18080}"
OUT_DIR="${OUT_DIR:-artifacts/kubernetes}"
SKIP_DOCKER_BUILD="${SKIP_DOCKER_BUILD:-false}"

mkdir -p "$OUT_DIR"

log_section() {
  echo ""
  echo "==> $1"
}

capture() {
  local name="$1"
  shift

  "$@" | tee "$OUT_DIR/$name.txt"
}

log_section "Docker image"
if [ "$SKIP_DOCKER_BUILD" = "true" ]; then
  docker image inspect "$APP_IMAGE" > "$OUT_DIR/docker-image-inspect.json"
  echo "Using prebuilt Docker image '$APP_IMAGE'."
else
  docker build -t "$APP_IMAGE" -t "$CI_IMAGE" .
fi

log_section "Kind cluster"
kind get clusters | tee "$OUT_DIR/kind-clusters-before.txt"
if ! kind get clusters | grep -qx "$CLUSTER_NAME"; then
  kind create cluster --name "$CLUSTER_NAME" --wait 120s
fi

kind load docker-image "$APP_IMAGE" --name "$CLUSTER_NAME"
kubectl cluster-info --context "kind-$CLUSTER_NAME" | tee "$OUT_DIR/cluster-info.txt"

log_section "Apply Kubernetes manifests"
kubectl apply -f k8s/namespace.yaml | tee "$OUT_DIR/kubectl-apply-namespace.txt"
kubectl apply -f k8s/ | tee "$OUT_DIR/kubectl-apply-workloads.txt"

log_section "Rollout"
kubectl rollout status deployment/garageflow-postgres -n "$NAMESPACE" --timeout=180s | tee "$OUT_DIR/postgres-rollout.txt"
kubectl rollout status deployment/garageflow-webhost -n "$NAMESPACE" --timeout=240s | tee "$OUT_DIR/webhost-rollout.txt"

log_section "Metrics server"
set +e
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml | tee "$OUT_DIR/metrics-server-apply.txt"
metrics_apply_status=${PIPESTATUS[0]}
kubectl patch deployment metrics-server -n kube-system --type='json' -p='[{"op":"add","path":"/spec/template/spec/containers/0/args/-","value":"--kubelet-insecure-tls"}]' | tee "$OUT_DIR/metrics-server-patch.txt"
metrics_patch_status=${PIPESTATUS[0]}
kubectl rollout status deployment/metrics-server -n kube-system --timeout=180s | tee "$OUT_DIR/metrics-server-rollout.txt"
metrics_rollout_status=${PIPESTATUS[0]}
sleep 30
kubectl top nodes > "$OUT_DIR/top-nodes.txt" 2>&1
top_nodes_status=$?
kubectl top pods -n "$NAMESPACE" > "$OUT_DIR/top-pods.txt" 2>&1
top_pods_status=$?
set -e

log_section "Cluster evidence"
capture nodes kubectl get nodes -o wide
capture pods kubectl get pods -n "$NAMESPACE" -o wide
capture services kubectl get services -n "$NAMESPACE"
capture hpa kubectl get hpa -n "$NAMESPACE"
capture configmaps kubectl get configmap -n "$NAMESPACE"
capture secrets kubectl get secret -n "$NAMESPACE"

log_section "Health check"
kubectl port-forward service/garageflow-webhost "$PORT_FORWARD_PORT:8080" -n "$NAMESPACE" > "$OUT_DIR/port-forward.log" 2>&1 &
port_forward_pid=$!

cleanup() {
  kill "$port_forward_pid" >/dev/null 2>&1 || true
}
trap cleanup EXIT

health_status=1
for _ in $(seq 1 30); do
  if curl --fail --silent --show-error "http://127.0.0.1:$PORT_FORWARD_PORT/health" | tee "$OUT_DIR/health-response.json"; then
    health_status=0
    break
  fi

  sleep 2
done

if [ "$health_status" -ne 0 ]; then
  echo "Health check failed after retries." >&2
  cat "$OUT_DIR/port-forward.log" >&2 || true
  exit 1
fi

log_section "Summary"
{
  echo "# Kubernetes Deploy Evidence"
  echo ""
  echo "| Check | Status |"
  echo "| --- | --- |"
  if [ "$SKIP_DOCKER_BUILD" = "true" ]; then
    echo "| Docker image | OK |"
  else
    echo "| Docker build | OK |"
  fi
  echo "| Kind cluster | OK |"
  echo "| Kubernetes manifests | OK |"
  echo "| PostgreSQL rollout | OK |"
  echo "| WebHost rollout | OK |"
  echo "| Health check | OK |"
  if [ "$metrics_apply_status" -eq 0 ] && [ "$metrics_patch_status" -eq 0 ] && [ "$metrics_rollout_status" -eq 0 ]; then
    echo "| Metrics server rollout | OK |"
  else
    echo "| Metrics server rollout | WARN |"
  fi
  if [ "$top_nodes_status" -eq 0 ] && [ "$top_pods_status" -eq 0 ]; then
    echo "| Metrics collection | OK |"
  else
    echo "| Metrics collection | WARN |"
  fi
  echo ""
  echo "## Pods"
  echo '```text'
  cat "$OUT_DIR/pods.txt"
  echo '```'
  echo ""
  echo "## Services"
  echo '```text'
  cat "$OUT_DIR/services.txt"
  echo '```'
  echo ""
  echo "## HPA"
  echo '```text'
  cat "$OUT_DIR/hpa.txt"
  echo '```'
  echo ""
  echo "## Health"
  echo '```json'
  cat "$OUT_DIR/health-response.json"
  echo ""
  echo '```'
  echo ""
  echo "## Metrics"
  echo '```text'
  cat "$OUT_DIR/top-nodes.txt" || true
  cat "$OUT_DIR/top-pods.txt" || true
  echo '```'
} | tee "$OUT_DIR/kubernetes-deploy-summary.md"
