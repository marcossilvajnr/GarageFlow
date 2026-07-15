#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
NAMESPACE="${GARAGEFLOW_NAMESPACE:-garageflow}"
KIND_CLUSTER="${GARAGEFLOW_KIND_CLUSTER:-garageflow}"
APP_IMAGE="${GARAGEFLOW_APP_IMAGE:-garageflow-api}"
AUTO_APPROVE="false"

usage() {
  cat <<EOF
Usage: scripts/teardown-local-infra.sh [--yes]

Removes the local GarageFlow demo infrastructure:
  - kubectl port-forward/watch processes
  - Kubernetes namespace '$NAMESPACE'
  - Docker Compose containers and volumes
  - local Docker images '$APP_IMAGE:*'
  - Kind cluster '$KIND_CLUSTER' through Terraform, when state exists

Options:
  --yes    Run without confirmation.
  -h, --help
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --yes|-y)
      AUTO_APPROVE="true"
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

confirm() {
  if [[ "$AUTO_APPROVE" == "true" ]]; then
    return
  fi

  echo "This will remove the local GarageFlow infrastructure and data."
  echo "Namespace: $NAMESPACE"
  echo "Kind cluster: $KIND_CLUSTER"
  echo "Docker image prefix: $APP_IMAGE"
  echo
  read -r -p "Continue? Type 'yes' to confirm: " answer

  if [[ "$answer" != "yes" ]]; then
    echo "Aborted."
    exit 0
  fi
}

run_if_available() {
  local command_name="$1"
  shift

  if command -v "$command_name" >/dev/null 2>&1; then
    "$@"
  else
    echo "Skipping $command_name: command not found."
  fi
}

stop_local_processes() {
  echo "Stopping local kubectl helpers..."
  pkill -f "kubectl port-forward" 2>/dev/null || true
  pkill -f "kubectl.*watch" 2>/dev/null || true
}

delete_kubernetes_namespace() {
  if ! command -v kubectl >/dev/null 2>&1; then
    echo "Skipping Kubernetes cleanup: kubectl not found."
    return
  fi

  echo "Deleting Kubernetes namespace '$NAMESPACE'..."
  kubectl delete namespace "$NAMESPACE" --ignore-not-found
}

stop_compose() {
  if ! command -v docker >/dev/null 2>&1; then
    echo "Skipping Docker Compose cleanup: docker not found."
    return
  fi

  echo "Stopping Docker Compose services and volumes..."
  (cd "$ROOT_DIR" && docker compose down -v --remove-orphans)
}

remove_app_images() {
  if ! command -v docker >/dev/null 2>&1; then
    echo "Skipping Docker image cleanup: docker not found."
    return
  fi

  echo "Removing local Docker images '$APP_IMAGE:*'..."
  image_ids=()
  while IFS= read -r image_id; do
    image_ids+=("$image_id")
  done < <(docker images "$APP_IMAGE" --format "{{.ID}}" | sort -u)

  if [[ "${#image_ids[@]}" -eq 0 ]]; then
    echo "No local Docker images found for '$APP_IMAGE'."
    return
  fi

  docker image rm "${image_ids[@]}" || true
}

destroy_kind_with_terraform() {
  if ! command -v terraform >/dev/null 2>&1; then
    echo "Skipping Terraform destroy: terraform not found."
    return
  fi

  if [[ ! -f "$ROOT_DIR/infra/terraform.tfstate" ]]; then
    echo "Skipping Terraform destroy: infra/terraform.tfstate not found."
    return
  fi

  echo "Destroying Kind cluster with Terraform..."
  (cd "$ROOT_DIR/infra" && terraform destroy -auto-approve)
}

delete_kind_fallback() {
  if ! command -v kind >/dev/null 2>&1; then
    echo "Skipping Kind fallback cleanup: kind not found."
    return
  fi

  if kind get clusters | grep -qx "$KIND_CLUSTER"; then
    echo "Deleting remaining Kind cluster '$KIND_CLUSTER'..."
    kind delete cluster --name "$KIND_CLUSTER"
  else
    echo "Kind cluster '$KIND_CLUSTER' is not present."
  fi
}

print_final_state() {
  echo
  echo "Final state:"

  if command -v kubectl >/dev/null 2>&1; then
    kubectl get namespace "$NAMESPACE" 2>/dev/null || true
  fi

  if command -v kind >/dev/null 2>&1; then
    kind get clusters || true
  fi

  if command -v docker >/dev/null 2>&1; then
    docker images "$APP_IMAGE" || true
    (cd "$ROOT_DIR" && docker compose ps) || true
  fi
}

confirm
stop_local_processes
delete_kubernetes_namespace
stop_compose
remove_app_images
destroy_kind_with_terraform
delete_kind_fallback
print_final_state

echo
echo "GarageFlow local infrastructure teardown completed."
