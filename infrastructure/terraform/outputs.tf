output "alb_dns" {
  description = "DNS del balanceador"
  value       = aws_lb.main.dns_name
}

output "db_endpoint" {
  description = "Endpoint de la base de datos (no incluye credenciales)"
  value       = aws_db_instance.main.address
  sensitive   = false
}

output "ecs_cluster" {
  description = "Cluster ECS"
  value       = aws_ecs_cluster.main.name
}