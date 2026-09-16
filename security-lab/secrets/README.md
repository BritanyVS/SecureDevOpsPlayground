# security-lab/secrets/ — Snyk Secrets

> ⚠️ **TODOS los valores en esta carpeta son FICTICIOS**, a propósito **inútiles** y con formato
> reconocible para que Snyk Secrets / Snyk Code los detecte. **Nunca** usar credenciales reales.

## Las tres formas de gestionar secretos

| Forma | Cómo funciona | Snyk lo detecta | Uso recomendado |
|---|---|---|---|
| ❌ **Hardcodeado** | El secreto está literal en el código/archivo versionado | ✅ Lo reporta como `hardcoded secret` | Solo en fixtures de laboratorio |
| ✅ **Variable de entorno** | El valor se inyecta en runtime (`JWT_SECRET=...`) y NO se commit-ea | ❌ No se detecta (no hay valor en git) | Configuración de la app en dev/prod |
| ✅ **Secret manager** | AWS Secrets Manager, Azure Key Vault, Vault, Snyk Env, Doppler… | ❌ El valor vive fuera del repo | Producción multi-servicio / rotación |

**Regla de oro**: un secreto **solo vale si no está en el repositorio**. Si alguna vez se
commit-ea, **asumir que está comprometido** y rotarlo (revocar + regenerar), incluso tras
borrarlo del código — el historial de git lo conserva.

## Archivos de la carpeta

| Archivo | Contenido | Demo |
|---|---|---|
| `cloud-creds.example.json` | Credenciales cloud ficticias (AWS/Azure/GCP) | Detección de claves por formato |
| `database.env.example` | Cadena de conexión con credencial falsa | Detección de DB password |
| `github-token.example` | Token GitHub/npm ficticio | Detección de tokengit-hub |
| `aws-secrets-manager.example.json` | Ejemplo de cómo debe vivir fuera del repo | Contraste con lo anterior |

Y en el repositorio ya existen otros fixtures de secretos que complementan:
- `SecureDevOps.API/.env.lab` — secretos ficticios para el demo.
- `SnykLab/secrets/*.txt` — dataset con claves AWS/Azure/Stripe/GitHub/Slack/RSA.
- `security-lab/code/hardcoded-credentials.cs` — secretos hardcodeados dentro de código.

## Cómo ejecutar la demo

```bash
# 1) Detectar (Snyk Code indexa secretos del repo)
snyk code test
#   → buscamos los findings con reglas "Hardcoded secret" / tipo de formato (Github, AWS, ...)

# 2) Detectar con la CLI de secrets (según versión)
snyk secrets scan security-lab/secrets

# 3) Remediar: pon los valores en variables de entorno (ver .env.example raíz),
#    borra los literales y rota los secretos.
```

## Qué debe detectar

- `AKIAIOSFODNN7EXAMPLE` + par secreto → **AWS Access Key / Secret Key**.
- `sk_test_...` → **Stripe Secret Key**.
- `ghp_...` → **GitHub Personal Access Token**.
- Cadenas de conexión con `Password=` en texto plano → **Hardcoded DB credentials**.
- `-----BEGIN RSA PRIVATE KEY-----` → **Private key**.

> Estos valores son los **documentados por los propios proveedores como ejemplo** para tests
> (p. ej. `AKIAIOSFODNN7EXAMPLE` de la documentación AWS). No están asociados a ninguna cuenta.