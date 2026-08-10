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

        stage('Snyk Security Scans') {
            steps {
                echo 'Iniciando análisis de seguridad integrados con Snyk...'
                
                // 1. Snyk Code
                echo 'Ejecutando Snyk Code...'
                bat 'npx snyk code test'

                // 2. Snyk Open Source
                echo 'Ejecutando Snyk Open Source...'
                bat 'cd SecureDevOps.API && npx snyk test'

                // 3. Snyk IaC
                echo 'Ejecutando Snyk IaC...'
                bat 'npx snyk iac test k8s/'

                // 4. Snyk Container
                echo 'Ejecutando Snyk Container...'
                bat 'cd SecureDevOps.API && npx snyk container test --file=Dockerfile'
            }
        }
    }

    post {
        always {
            echo 'Pipeline finalizado.'
        }
        failure {
            echo '¡Atención! El pipeline falló en la compilación o debido a vulnerabilidades detectadas en alguno de los escaneos de Snyk.'
        }
    }
}