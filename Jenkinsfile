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
                bat 'dotnet build SecureDevOps.API/SecureDevOps.API.csproj --configuration Release'
            }
        }

        stage('Snyk Security Scan') {
            steps {
                echo 'Ejecutando análisis de vulnerabilidades con Snyk...'
                bat 'cd SecureDevOps.API && "C:\\Users\\Bri\\AppData\\Roaming\\npm\\snyk.cmd" test'
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