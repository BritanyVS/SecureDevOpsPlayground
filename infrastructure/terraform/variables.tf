# Variables de la infraestructura. Los SECRETOS deben inyectarse desde fuera
# (TF_VAR_* / vault / CI) — NUNCA hardcodear passwords en los .tf.

variable "environment" {
  description = "Entorno (dev/staging/prod)"
  type        = string
  default     = "dev"
}

variable "region" {
  description = "Región AWS"
  type        = string
  default     = "us-east-1"
}

variable "project" {
  description = "Prefijo de nombres de recursos"
  type        = string
  default     = "securedevops"
}

variable "db_username" {
  description = "Usuario maestro de la BD (provisto externamente)."
  type        = string
  sensitive   = true
}

variable "db_password" {
  description = "Password maestro de la BD (provisto externamente, p. ej. TF_VAR_db_password)."
  type        = string
  sensitive   = true
}

variable "db_name" {
  description = "Nombre de la base de datos."
  type        = string
  default     = "taskmanager"
}

variable "jwt_secret" {
  description = "Secreto JWT de la API (provisto externamente)."
  type        = string
  sensitive   = true
}

variable "allowed_ingress_cidr" {
  description = "CIDR permitido para el ALB (restringir en producción)."
  type        = string
  default     = "0.0.0.0/0"
}