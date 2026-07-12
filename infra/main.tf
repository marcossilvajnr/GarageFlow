locals {
  kube_context = "kind-${var.cluster_name}"
}

resource "terraform_data" "kind_cluster" {
  input = {
    cluster_name      = var.cluster_name
    kind_config_sha1  = filesha1("${path.module}/kind-cluster.yaml")
    kind_wait_timeout = var.kind_wait_timeout
  }

  triggers_replace = [
    var.cluster_name,
    filesha1("${path.module}/kind-cluster.yaml")
  ]

  provisioner "local-exec" {
    command     = <<-EOT
      set -eu

      if kind get clusters | grep -qx "$CLUSTER_NAME"; then
        echo "Kind cluster '$CLUSTER_NAME' already exists."
      else
        kind create cluster \
          --name "$CLUSTER_NAME" \
          --config "$KIND_CONFIG" \
          --wait "$KIND_WAIT_TIMEOUT"
      fi
    EOT
    interpreter = ["/bin/sh", "-c"]

    environment = {
      CLUSTER_NAME      = self.input.cluster_name
      KIND_CONFIG       = "${path.module}/kind-cluster.yaml"
      KIND_WAIT_TIMEOUT = self.input.kind_wait_timeout
    }
  }

  provisioner "local-exec" {
    when        = destroy
    command     = <<-EOT
      set -eu

      if kind get clusters | grep -qx "$CLUSTER_NAME"; then
        kind delete cluster --name "$CLUSTER_NAME"
      else
        echo "Kind cluster '$CLUSTER_NAME' does not exist."
      fi
    EOT
    interpreter = ["/bin/sh", "-c"]

    environment = {
      CLUSTER_NAME = self.input.cluster_name
    }
  }
}
