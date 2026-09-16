# security-lab/iac/ — Snyk IaC (infraestructura insegura controlada)

> Ejemplos **controlados** de Terraform inseguro. Compáralos con la versión segura de
> `infrastructure/terraform/`. **Nunca apliques estos recursos en producción.**

## Cómo ejecutar el análisis

```bash
cd security-lab/iac
terraform fmt -check insecure.tf   # (opcional: formateo)
terraform validate                  # no requiere credenciales

# Análisis con Snyk IaC:
snyk iac test insecure.tf
# o sobre la carpeta completa:
cd security-lab/iac && snyk iac test
```

## Matriz de hallazgos esperados → riesgo → remediación

| Problema (bloque en `insecure.tf`) | Regla Snyk IaC esperada | Riesgo | Remediación (ver `infrastructure/terraform/main.tf`) |
|---|---|---|---|
| `acl = "public-read"` | `S3 Bucket ACL All Users Read` | Exfil/compromiso de datos | Bucket privado + políticas de acceso explícitas; bloqueo de acceso público |
| S3 sin versionado/logging | `S3 versioning disabled`, `S3 logging disabled` | Pérdida de datos, sin auditoría | `versioning` y `logging` activos |
| SG `0.0.0.0/0` en 22/3306 | `SSH Port Open to Internet`, `MySQL open to internet` | Acceso no autorizado / RCE | SG por `security_groups` de origen y CIDR restringido |
| `password = "Str0ngP@ss"` | `PlainText Password` | Credenciales comprometidas | Variables `sensitive=true` o Secrets Manager |
| RDS: `storage_encrypted=false` | `RDS Storage Unencrypted` | Datos en repo en claro | `storage_encrypted = true` |
| RDS: `backup_retention_period=0` | `RDS Backup Disabled` | Pérdida de datos | `backup_retention_period >= 7` |
| RDS: `publicly_accessible=true` | `RDS Publicly Accessible` | Exposición de la BD | Subnets privadas + SG del ALB |
| IAM `Action:"*"`/`Resource:"*"` | `IAM policy grants all access` | Privilegios excesivos | Policies mínimas (logs + GetSecretValue) |
| Credenciales en `provider` | `PlainText` (AWS keys) | Compromiso de cuenta | Uso de `~/.aws/credentials`, OIDC o variables |
| `user_data` con clave hardcodeada | `PlainText Secret` | Clave filtrada en user-data | Metadata service v2 / secret manager |

## Flujo de demo

1. `snyk iac test insecure.tf` → N hallazgos (critico/alto).
2. Abrir la versión segura `infrastructure/terraform` → `snyk iac test` → 0 críticos/altos.
3. Explicar cada hallazgo con la matriz.
4. (Opcional) Corregir `insecure.tf` en vivo y re-ejecutar para ver el count bajar.

> También hay fixtures heredados en `SnykLab/terraform/` (main.tf y kubernetes.tf) que
> amplían la demo con `snyk iac test SnykLab/terraform`.