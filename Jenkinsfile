// ⚠️ LABORATORIO: Pipeline con los escaneos Snyk
// Requiere credenciales Snyk en Jenkins: snyk-api-token
pipeline {
    agent any

    environment {
        SNYK_TOKEN = credentials('snyk-api-token')
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        SNYK_INTEGRATION_NAME = 'jenkins'
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Obteniendo código fuente...'
                checkout scm
            }
        }

        stage('Build Backend') {
            steps {
                echo 'Compilando el proyecto .NET...'
                bat 'dotnet build SecureDevOps.API/SecureDevOps.API.csproj --configuration Release'
            }
        }

        // 1. SNYK OPEN SOURCE - dependencias del backend
        stage('Snyk Open Source - Backend') {
            steps {
                bat 'cd SecureDevOps.API && snyk test --all-projects --severity-threshold=high'
            }
        }

        // 1b. SNYK OPEN SOURCE - dependencias del frontend
        stage('Snyk Open Source - Frontend') {
            steps {
                bat 'cd SecureDevOps.Web && snyk test --severity-threshold=high'
            }
        }

        // 2. SNYK CODE - análisis estático del código fuente
        stage('Snyk Code') {
            steps {
                bat 'snyk code test --severity-threshold=medium'
            }
        }

        // 3. SNYK IAC - infraestructura como código (Terraform)
        stage('Snyk IaC') {
            steps {
                bat 'cd SnykLab/terraform && snyk iac test main.tf --severity-threshold=high'
            }
        }

        // 4. SNYK CONTAINER - imagen Docker
        stage('Snyk Container') {
            steps {
                bat 'docker build -t securedevops-api:lab .'
                bat 'snyk container test securedevops-api:lab --file=Dockerfile --severity-threshold=high || exit 0'
            }
        }

        // 5. SNYK MONITOR - subir artifacts a la consola/API de Snyk
        stage('Snyk Monitor') {
            steps {
                bat 'snyk monitor --all-projects'
            }
        }
    }

    post {
        always {
            echo 'Pipeline finalizado.'
        }
        failure {
            echo '¡Atención! El pipeline falló o se detectaron vulnerabilidades.'
        }
    }
}