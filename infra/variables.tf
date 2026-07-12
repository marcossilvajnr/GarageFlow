variable "cluster_name" {
  description = "Nome do cluster Kind local gerenciado pelo Terraform."
  type        = string
  default     = "garageflow"
}

variable "namespace" {
  description = "Namespace Kubernetes usado pelos manifests do GarageFlow."
  type        = string
  default     = "garageflow"
}

variable "app_image" {
  description = "Imagem Docker local esperada pelo Deployment Kubernetes."
  type        = string
  default     = "garageflow-api:latest"
}

variable "app_port" {
  description = "Porta local usada no port-forward do Service da aplicação."
  type        = number
  default     = 8080
}

variable "kind_wait_timeout" {
  description = "Tempo máximo de espera para criação do cluster Kind."
  type        = string
  default     = "120s"
}
