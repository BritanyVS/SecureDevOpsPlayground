# Prompt: Revisar IaC / Snyk IaC

```text
Revisa la infraestructura como código del repositorio.

ALCANCE
- infrastructure/terraform/*.tf (versión SECURA de referencia)
- security-lab/iac/insecure.tf (fixture INSEGURO, documentar, no "corregir")
- SnykLab/terraform/*.tf (heredado, informativo)

PASOS
1. `snyk iac test infrastructure/terraform`
2. `snyk iac test security-lab/iac/insecure.tf`
3. Compara: qué reglas disparan en insecure.tf y cómo main.tf las evita.
4. Analiza al menos: SG abiertos (0.0.0.0/0), buckets S3 públicos, cifrado/backups RDS,
   IAM con `*`, secrets hardcodeados, logging, VPC/subnets privadas, NAT.
5. Si detectas algo en la version segura → cómo corregirlo.

ENTREGABLE
- Tabla: recurso | hallazgo | regla Snyk | riesgo | remediación | dónde está bien hecho.
- Nota explícita de que insecure.tf NO debe desplegarse.
```