# infrastructure/terraform — IaC SEGURO (referencia)

Infraestructura de referencia para la app Task Manager en AWS, **diseñada para pasar las
políticas de Snyk IaC** y demostrar la diferencia frente a `security-lab/iac/`.

## Componentes

| Recurso | Decisión de seguridad |
|---|---|
| VPC + subnets | Separación privada/pública; app y BD **sin IP pública** |
| NAT Gateway | Acceso saliente controlado para subnets privadas |
| Security Groups | Menor privilegio: solo el ALB a la app; solo la app a la BD (3306 por SG, no por CIDR 0.0.0.0/0) |
| RDS MySQL | `storage_encrypted=true`, backups (15 días), `deletion_protection`, subredes privadas, parámetros por variables |
| IAM | Policy mínima: solo logs y lectura de Secrets Manager para ECS |
| CloudWatch Logs | Logging de contenedores habilitado |
| ECS Fargate | `awsvpc` sin IP pública; secrets vía Secrets Manager; health check `/health` |
| Variables | `sensitive=true` para `db_password`/`db_username`/`jwt_secret`; nunca literales |

## Uso (opcional; no requiere aplicar)

```bash
cd infrastructure/terraform
terraform init
terraform validate          # valida sintaxis
terraform plan -out=plan.tfplan
# Para ejecutar:
#   export TF_VAR_db_username=... TF_VAR_db_password=... TF_VAR_jwt_secret=...
#   terraform apply plan.tfplan
```

## Análisis con Snyk IaC

```bash
cd infrastructure/terraform
snyk iac test
# → esperamos 0 issues de Alta/Crítica (o solo recomendaciones low/info).
```

## Lo que Snyk IaC valida aquí (ejemplos de reglas)

- `S3 bucket public` / `acl public-read` → no hay buckets públicos.
- `Security group` abierto `0.0.0.0/0` en puertos admin → no existe.
- `RDS` sin cifrado / sin backups / `skip_final_snapshot=true` → configurado seguro.
- `IAM` `Action:*` / `Resource:*` → policy mínima.
- Credenciales hardcodeadas en `.tf` → no existen (variables `sensitive`).
- Logging deshabilitado → CloudWatch activo.