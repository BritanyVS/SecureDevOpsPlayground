# Prompt: Ejecutar y reportar todos los escaneos de seguridad

```text
Ejecuta el inventario completo de escaneos Snyk de este repositorio y entrega un reporte.

COMANDOS (ajusta al CLI instalado; usa `--json` para parsing)
1. `snyk auth`            # si no está autenticado; nunca anotes el token
2. `snyk test --all-projects --json`            # Snyk Open Source
3. `snyk code test --json`                       # Snyk Code
4. `snyk iac test infrastructure/terraform --json`    # IaC (seguro)
5. `snyk iac test security-lab/iac/insecure.tf --json`# IaC (fixture)
6. `snyk container test securedevops-api:lab --file=Dockerfile`  # Container (si hay imagen local)
7. `snyk secrets scan security-lab/secrets`      # Snyk Secrets (fixtures)

REPORTE (markdown)
- Tabla por producto: comando | resultados | severidad | ficheros/reglas principales.
- Añade: `dotnet test` y `npm run build` al inicio como prefiltro (si el build falla, para).
- Distingue findings REALES (borrables) vs FIXTURES (security-lab, SnykLab, snyk-complete-demo).

REGLAS
- No corregir código en este prompt; solo escanear y reportar.
- No guardes tokens en archivos; si necesitas salvar resultados a JSON anonimizado, hazlo en /tmp (o fuera del repo).
```