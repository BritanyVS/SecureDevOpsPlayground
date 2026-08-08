# Laboratorio 1 – Introducción a Snyk Open Source

## Objetivo

Comprender cómo funciona Snyk Open Source y cómo analiza las dependencias de un proyecto .NET.

## Proyecto

SecureDevOpsPlayground


## Herramientas

- .NET 8
- Visual Studio Code
- Git
- GitHub
- Snyk CLI
- Snyk Extension

Inicio de sesión
snyk auth  // Autorizar a la aplicación e iniciar sesión
snyk whoami // Verificar el inicio de sesión


Snyk Test: 

cd .\SecureDevOps.API\
snyk test
Para que encuentre el .csproj

Al ejecutar snyk test, se lee el .csproj se obtienen los paquetes NuGet, resuelve todas las dependencias, 
construye un árbol completo, consulta la bd de vulnerabilidades de snyk, compara versiones y genera un reporte. 

El análisis se ejecutó correctamente sobre el proyecto SecureDevOps.API, utilizando el administrador de paquetes NuGet. Snyk
identificó el archivo obj/project.assets.json como fuente para resolver todas las dependencias del proyecto y comparó sus versiones 
contra su base de datos de vulnerabilidades.

El análisis confirmó que el proyecto se encuentra utilizando versiones de dependencias que, al momento de la prueba, no presentan vulnerabilidades conocidas dentro de la base de datos de Snyk.

Aunque en el archivo SecureDevOps.API.csproj únicamente se declararon unas pocas dependencias directas, Snyk analizó un total de 80 dependencias. Esto se debe a que la herramienta también resuelve y evalúa las dependencias transitivas, es decir, aquellas bibliotecas que son instaladas automáticamente como requisito de otras dependencias del proyecto.

Otro aspecto importante es que Snyk no analiza directamente el archivo .csproj. Primero utiliza la información generada durante la restauración de paquetes (dotnet restore), específicamente el archivo obj/project.assets.json, el cual contiene el árbol completo de dependencias con todas las versiones efectivamente instaladas.
