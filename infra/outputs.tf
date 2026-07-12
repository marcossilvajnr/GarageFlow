output "cluster_name" {
  description = "Nome do cluster Kind local."
  value       = var.cluster_name
}

output "kube_context" {
  description = "Contexto kubectl criado pelo Kind."
  value       = local.kube_context
}

output "namespace" {
  description = "Namespace da aplicação GarageFlow."
  value       = var.namespace
}

output "app_image" {
  description = "Imagem Docker local esperada pelo workload Kubernetes."
  value       = var.app_image
}

output "load_image_command" {
  description = "Comando para carregar a imagem local no cluster Kind."
  value       = "kind load docker-image ${var.app_image} --name ${var.cluster_name}"
}

output "apply_kubernetes_command" {
  description = "Comando para aplicar os manifests Kubernetes do GarageFlow."
  value       = "kubectl apply -f ../k8s/namespace.yaml && kubectl apply -f ../k8s/"
}

output "port_forward_command" {
  description = "Comando para expor a aplicação localmente via port-forward."
  value       = "kubectl port-forward service/garageflow-webhost ${var.app_port}:8080 -n ${var.namespace}"
}

output "health_url" {
  description = "URL local esperada para health check."
  value       = "http://localhost:${var.app_port}/health"
}

output "swagger_url" {
  description = "URL local esperada para Swagger UI."
  value       = "http://localhost:${var.app_port}/swagger"
}
