pipeline {
    agent any

    environment {
        SNYK_TOKEN = credentials('snyk-api-token')
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Obteniendo código fuente...'
                checkout scm
            }
        }

        stage('Build') {
            steps {
                echo 'Compilando el proyecto .NET...'
                sh 'dotnet build SecureDevOps.API.csproj --configuration Release'
            }
        }

        stage('Snyk Security Scan') {
            steps {
                echo 'Ejecutando análisis de vulnerabilidades con Snyk...'
                sh 'snyk test --file=SecureDevOps.API.csproj'
            }
        }
    }

    post {
        always {
            echo 'Pipeline finalizado.'
        }
        failure {
            echo '¡Atención! El pipeline falló en la compilación o por vulnerabilidades detectadas.'
        }
    }
}