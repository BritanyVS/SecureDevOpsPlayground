# Laboratorio 3 – Snyk Code

## Objetivo

Evaluar las capacidades de Snyk Code para realizar análisis estático de seguridad sobre el código fuente de una aplicación .NET y comparar este análisis con Snyk Open Source.

## Herramienta utilizada

Snyk Code mediante Snyk CLI.

## Comando ejecutado

snyk code test

## Resultado

El análisis se ejecutó correctamente sobre el proyecto SecureDevOps.API.

### Resumen

- Organización: britanyvsalazar
- Tipo de prueba: Static code analysis
- Proyecto analizado: SecureDevOps.API
- Ruta: C:\Users\Bri\Desktop\SecureDevOpsPlayground\SecureDevOps.API
- Total de issues: 0

## Interpretación

Snyk Code realizó un análisis estático del código fuente del proyecto y no encontró problemas de seguridad en el estado actual de la aplicación.

Este resultado representa la línea base (baseline) del análisis de Snyk Code antes de introducir ejemplos de código vulnerable de manera controlada.

A diferencia de Snyk Open Source, que analiza las dependencias de terceros utilizadas por la aplicación, Snyk Code analiza el código fuente buscando patrones y flujos que puedan representar vulnerabilidades de seguridad.

## Conclusión

El análisis confirmó que Snyk Code está correctamente habilitado y funcionando para el proyecto .NET. El proyecto actualmente no presenta issues detectados por Snyk Code, por lo que se utilizará como punto de comparación para las siguientes pruebas del laboratorio.

--Se metieron algunas vulnerabilidades controladas en el código para probar las herramientas de snyk code