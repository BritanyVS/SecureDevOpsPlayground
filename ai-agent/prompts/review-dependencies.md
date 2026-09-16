# Prompt: Revisar dependencias / Snyk Open Source

```text
Revisa la postura de dependencias del proyecto usando Snyk Open Source.

ALCANCE
- Manifests: SecureDevOps.API/*.csproj, SecureDevOps.Web/package.json (+ lockfiles), security-lab/**, snyk-api/package.json.

PASOS
1. `snyk test --all-projects --json`
2. Categoriza: direct vs transitive, prod vs dev, severity.
3. Para cada finding importante: paquete, versión afectada, versión fija, CVE/CWE,
   ¿es explotable en este proyecto? (¿se usa la función vulnerable?)
4. Comenta paquetes que mantienen fixtures a propósito (lodash, minimist, yargs-parser
   en la demo de Snyk Open Source) y confírmalo en docs.

ENTREGABLE
- Tabla de vulnerabilidades (severity, paquete, path, versión fix, estado).
- Recomendación: upgrade directo ASAP / pinning / reemplazo / aceptar riesgo.
- Si haces cambios: vuelve a correr `dotnet test`, `npm run build` y `snyk test`.
```