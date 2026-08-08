# Resultados del análisis de vulnerabilidades con Snyk Open Source

snyk test
snyk test --json > snyk-before.json // Para un reporte profesional.
snyk monitor // Dashboard en snyk

## Introducción

Como parte del laboratorio de análisis de seguridad de dependencias, se realizó una prueba controlada utilizando Snyk Open Source sobre el proyecto **SecureDevOps.API**, desarrollado en .NET 8.

El objetivo fue comprobar el comportamiento de Snyk al analizar dependencias NuGet, identificar vulnerabilidades conocidas (CVE), clasificar su nivel de severidad mediante CVSS y proporcionar recomendaciones de remediación.

Para este propósito se agregaron intencionalmente paquetes con versiones vulnerables con el fin de simular un escenario real de desarrollo donde una aplicación incorpora componentes de terceros con riesgos conocidos.

---

# Resultado inicial del análisis

Al ejecutar el comando:

```bash
snyk test
```

Snyk realizó el análisis de las dependencias del proyecto utilizando el archivo:

```
obj/project.assets.json
```

Resultado obtenido:

* Dependencias analizadas: **83**
* Vulnerabilidades encontradas: **2**
* Rutas vulnerables detectadas: **4**

El paquete identificado como vulnerable fue:

| Paquete     | Versión analizada | Tipo de dependencia |
| ----------- | ----------------- | ------------------- |
| SharpZipLib | 1.0.0             | Directa             |

---

# Vulnerabilidad 1: CVE-2021-32842

## Información general

| Campo              | Información                     |
| ------------------ | ------------------------------- |
| Identificador Snyk | SNYK-DOTNET-SHARPZIPLIB-2385702 |
| CVE                | CVE-2021-32842                  |
| Paquete afectado   | SharpZipLib                     |
| Versión vulnerable | 1.0.0                           |
| Vulnerabilidad     | Directory Traversal             |
| Severidad          | Medium                          |
| CVSS               | 4.0                             |
| Versión corregida  | 1.3.2                           |

---

## Descripción

La vulnerabilidad corresponde a un problema de **Directory Traversal**, donde la librería no valida correctamente las rutas utilizadas durante la extracción de archivos comprimidos.

Un atacante podría manipular las rutas internas de un archivo comprimido utilizando patrones como:

```
../
```

para acceder o crear archivos fuera del directorio esperado.

Este tipo de problema puede ocasionar escritura de archivos en ubicaciones no autorizadas y afectar la integridad del sistema.

---

## Análisis CVSS

Vector:

```
CVSS:3.1/AV:L/AC:L/PR:N/UI:N/S:U/C:N/I:L/A:N
```

Interpretación:

| Métrica             | Resultado |
| ------------------- | --------- |
| Attack Vector       | Local     |
| Attack Complexity   | Low       |
| Privileges Required | None      |
| User Interaction    | None      |
| Confidentiality     | None      |
| Integrity           | Low       |
| Availability        | None      |

La puntuación obtenida indica un impacto moderado, principalmente relacionado con la modificación de archivos dentro del sistema.

---

# Vulnerabilidad 2: CVE-2021-32840

## Información general

| Campo              | Información                     |
| ------------------ | ------------------------------- |
| Identificador Snyk | SNYK-DOTNET-SHARPZIPLIB-2385941 |
| CVE                | CVE-2021-32840                  |
| Paquete afectado   | SharpZipLib                     |
| Versión vulnerable | 1.0.0                           |
| Vulnerabilidad     | Directory Traversal             |
| Severidad Snyk     | High                            |
| CVSS Snyk          | 7.3                             |
| CVSS NVD           | 9.8                             |
| Versión corregida  | 1.3.3                           |

---

## Descripción

Esta vulnerabilidad permite que un atacante manipule rutas de archivos durante la extracción de archivos comprimidos debido a una validación insuficiente del directorio destino.

El impacto puede permitir:

* Escritura de archivos fuera del directorio permitido.
* Modificación de archivos críticos.
* Posible compromiso de la integridad del sistema.

---

# Análisis CVSS 9.8

Vector:

```
CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:H/I:H/A:H
```

Interpretación:

| Métrica             | Resultado |
| ------------------- | --------- |
| Attack Vector       | Network   |
| Attack Complexity   | Low       |
| Privileges Required | None      |
| User Interaction    | None      |
| Confidentiality     | High      |
| Integrity           | High      |
| Availability        | High      |

La puntuación crítica se debe a que la vulnerabilidad puede ser explotada remotamente, sin autenticación y sin
interacción del usuario, afectando los tres pilares de seguridad:

* Confidencialidad
* Integridad
* Disponibilidad

---

# Análisis del camino vulnerable

Snyk identificó la ruta:

```
SecureDevOps.API

        ↓

SharpZipLib 1.0.0

        ↓

CVE-2021-32842
CVE-2021-32840
```

En este caso la vulnerabilidad corresponde a una dependencia directa debido a que el paquete fue agregado explícitamente al proyecto.

Ejemplo dentro del archivo `.csproj`:

```xml
<PackageReference Include="SharpZipLib" Version="1.0.0" />
```

---

# Recomendación de remediación proporcionada por Snyk

Snyk recomendó realizar un proceso de actualización:

```
SharpZipLib 1.0.0
        ↓
SharpZipLib 1.3.3
```

La recomendación corresponde a una estrategia de **Upgrade**, donde se sustituye la versión vulnerable por una versión corregida del mismo paquete.

Comando utilizado:

```bash
dotnet add package SharpZipLib --version 1.3.3
```

Posteriormente se ejecutó nuevamente:

```bash
snyk test
```

para verificar que las vulnerabilidades fueran eliminadas.

---

# Conclusión

El laboratorio permitió comprobar el funcionamiento de Snyk Open Source como herramienta de análisis de seguridad de dependencias en aplicaciones .NET.

Durante la prueba se logró identificar vulnerabilidades reales asociadas a una dependencia externa, analizar sus identificadores CVE, interpretar la puntuación CVSS y comprender cómo Snyk determina el nivel de riesgo.

Además, se comprobó que una vulnerabilidad en una librería de terceros puede afectar directamente la seguridad de una aplicación, incluso cuando el código desarrollado por el equipo no contiene errores propios.

Finalmente, mediante la recomendación de actualización (**Upgrade**) proporcionada por Snyk, fue posible aplicar una estrategia de remediación y validar posteriormente la reducción del riesgo mediante un nuevo análisis de seguridad.
