# 07 — Hardcoded credentials / secretos en código

## Código vulnerable
Ver `hardcoded-credentials.cs`.

```csharp
private const string DbPassword = "P@ssw0rd123!";
private static readonly string ApiKey = "AKIAIOSFODNN7EXAMPLE";
private const string JwtSecret = "sup3rs3cr3t-jwt-key-0123456789abcdef";
```

## Vulnerabilidad esperada
- **Snyk Code**: `Hardcoded credential` / `Hardcoded secret` (Severity: Medium/High).
- **Snyk Secrets**: reporta el mismo valor como un secreto expuesto con formato conocido
  (AWS Access Key, Stripe, etc.).

## Impacto
- Compromiso de servicios (BD, cloud, API de pagos).
- Los secretos versionados **nunca se eliminan del historial de git** → rotarlos siempre.

## Remediación
1. Mover a **variables de entorno** (12-factor) o a un **secret manager**
   (AWS Secrets Manager, Azure Key Vault, HashiCorp Vault, Doppler, Snyk Secrets/Env).
2. Rotar la credencial expuesta y revocar la anterior.
3. `git filter-repo` / `BFG` si ya se subió, y añadir la clave a los supresiones de Snyk
   solo si es falsa.

## Cómo verificar
```bash
snyk code test security-lab/code
snyk secrets scan security-lab/code          # o `snyk code test` en la CLI actual
# Tras corregir, Snyk no reporta hardcoded credentials/secretos.
```