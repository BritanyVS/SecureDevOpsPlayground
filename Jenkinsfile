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
                
                // 1. Snyk Code (Análisis estático de código fuente / SAST)
                echo 'Ejecutando Snyk Code...'
                bat 'snyk code test'

                // 2. Snyk Open Source (Análisis de dependencias y paquetes NuGet)
                echo 'Ejecutando Snyk Open Source...'
                bat 'cd SecureDevOps.API && "C:\\Users\\Bri\\AppData\\Roaming\\npm\\snyk.cmd" test'

                // 3. Snyk IaC (Análisis de configuraciones de infraestructura como código, ej. Kubernetes)
                echo 'Ejecutando Snyk IaC...'
                bat 'snyk iac test k8s/'

                // 4. Snyk Container (Análisis de vulnerabilidades de la imagen y el Dockerfile)
                echo 'Ejecutando Snyk Container...'
                bat 'cd SecureDevOps.API && snyk container test --file=Dockerfile'
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