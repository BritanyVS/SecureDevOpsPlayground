# Prompt: Auditoría de secretos en el repositorio

```text
Audita que no haya secretos reales commiteados.

ALCANCE
- Todo el repo EXCEPTO security-lab/secrets/* (fixtures ficticios a propósito), .env.lab, snyk-api/.env.
- Ficheros a mirar: *.env*, appsettings*.json, *.json con "key"/"secret"/"token"/"password",
  docker-compose*, .github/workflows/*, Jenkinsfile, infra terraform.

PASOS
1. `snyk secrets scan --report <carpetas>` (si la CLI lo soporta) o búsqueda manual de patrones:
   password/secret/api key/token/Authorization Bearer/private key blocks.
2. Comprueba las reglas de Snyk Secrets sobre los fixtures intencionales (security-lab/secrets/*).
3. Verifica `.gitignore`: ¿hay algo real que se pueda commit-ear? (demo: no).
   Confirmar que `.env`, `.env.*.local`, `*.env.local` están ignorados.
4. Investiga SIEMPRE antes de afirmar: los valores sample (AKIAIOSFODNN7EXAMPLE, etc.)
   son placeholders públicos de AWS; se marcan como "demo".

ENTREGABLE
- Tabla: fichero | tipo de secreto | real o fixture | acción (remediar/rotar/ignorar).
- Regla de oro final: SI hay un secreto real → recomendación de rotación inmediata y quitar de git.
- No crear archivos de credenciales nuevos con valores reales.
```