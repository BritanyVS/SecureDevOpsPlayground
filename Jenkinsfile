// ============================================================================
// JENKINS PIPELINE — Laboratorio Snyk (SecureDevOpsPlayground)
//
// Ejecuta las 5 disciplinas de Snyk en CI:
//   1. Snyk Code        (SAST)      -> snyk code test
//   2. Snyk Open Source (SCA)       -> snyk test --all-projects
//   3. Snyk Container   (Imagenes)  -> snyk container test <img> --file=Dockerfile
//   4. Snyk IaC         (Terraform) -> snyk iac test iac/main.tf
//   5. Snyk Secrets     (Secretos)  -> snyk secrets scan .
//
// NOTA PLATAFORMA: usa pasos "bat" (cmd de Windows) porque el agente corre en
// Windows. En Linux cambia cada bat { ... } por sh { ... } y los "\\" por "/".
//
// CREDENCIALES PREVIAS EN JENKINS (una sola vez, NO se escriben en Git):
//   Gestionar credenciales -> Credenciales globales -> Add credentials
//     Kind  : Secret text
//     ID    : snyk-token
//     Secret: <tu SNYK_TOKEN de https://app.snyk.io/account>
//
// El token se inyecta via withCredentials(binding -> env.SNYK_TOKEN); nunca
// se hardcodea en el repositorio ni se imprime en los logs.
// ============================================================================

pipeline {
    agent any

    parameters {
        choice(name: 'SNYK_SEVERITY_FAIL',
               choices: ['high', 'medium', 'low', 'critical'],
               description: 'Severidad minima que hace fallar el build.')
        booleanParam(name: 'FAIL_ON_ISSUES',
                     defaultValue: true,
                     description: 'Si true, el build falla ante hallazgos >= SNYK_SEVERITY_FAIL.')
        string(name: 'SNYK_ORG',
               defaultValue: '',
               description: 'Snyk Org ID (opcional; si se deja vacio usa el default de la cuenta).')
        string(name: 'BACKEND_IMAGE',
               defaultValue: 'snyk-lab-backend',
               description: 'Imagen Docker del backend a escanear con Snyk Container.')
        string(name: 'FRONTEND_IMAGE',
               defaultValue: 'snyk-lab-frontend',
               description: 'Imagen Docker del frontend a escanear con Snyk Container.')
    }

    environment {
        REPORTS_DIR = "${WORKSPACE}\\reports"
        SNYK_CLI_VERSION = 'latest'
        SNYK_PATH_SETUP = '"%APPDATA%\\npm;%PATH%"'
    }

    stages {
        stage('Preparacion: CLI + reportes') {
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    bat '''
                        @echo off
                        set "PATH=%APPDATA%\\npm;%PATH%"
                        echo => Verificando Node/npm...
                        where npm || (echo npm no encontrado. Instala Node.js LTS y reinicia Jenkins. & exit /b 1)
                        where snyk >nul 2>&1 || call npm install -g snyk@%SNYK_CLI_VERSION%
                        snyk --version || exit /b 1
                        if not exist "%REPORTS_DIR%" mkdir "%REPORTS_DIR%"
                    '''
                }
            }
        }

        stage('Snyk Auth (token via Jenkins Credentials)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        bat '''
                            @echo off
                            set "PATH=%APPDATA%\\npm;%PATH%"
                            set "ORG_FLAG="
                            if not "x%SNYK_ORG%"=="x" set "ORG_FLAG=--org=%SNYK_ORG%"
                            call snyk auth "%SNYK_TOKEN%" >nul 2>&1
                            exit /b 0
                        '''
                    }
                }
            }
        }

        stage('Snyk Open Source (SCA)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        bat '''
                            @echo off
                            set "PATH=%APPDATA%\\npm;%PATH%"
                            set "ORG_FLAG="
                            if not "x%SNYK_ORG%"=="x" set "ORG_FLAG=--org=%SNYK_ORG%"
                            call snyk test --all-projects %ORG_FLAG% --json > "%REPORTS_DIR%\\snyk-oss.json" 2>&1
                            exit /b %ERRORLEVEL%
                        '''
                    }
                }
            }
        }

        stage('Snyk Code (SAST)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        bat '''
                            @echo off
                            set "PATH=%APPDATA%\\npm;%PATH%"
                            set "ORG_FLAG="
                            if not "x%SNYK_ORG%"=="x" set "ORG_FLAG=--org=%SNYK_ORG%"
                            call snyk code test %ORG_FLAG% --json > "%REPORTS_DIR%\\snyk-code.json" 2>&1
                            exit /b %ERRORLEVEL%
                        '''
                    }
                }
            }
        }

        stage('Snyk Container (Imagenes)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        bat '''
                            @echo off
                            set "PATH=%APPDATA%\\npm;%PATH%"
                            set "ORG_FLAG="
                            if not "x%SNYK_ORG%"=="x" set "ORG_FLAG=--org=%SNYK_ORG%"
                            call snyk container test "%BACKEND_IMAGE%" --file=Dockerfile %ORG_FLAG% --json > "%REPORTS_DIR%\\snyk-container.json" 2>&1
                            exit /b %ERRORLEVEL%
                        '''
                    }
                }
            }
        }

        stage('Snyk IaC (Terraform)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        bat '''
                            @echo off
                            set "PATH=%APPDATA%\\npm;%PATH%"
                            set "ORG_FLAG="
                            if not "x%SNYK_ORG%"=="x" set "ORG_FLAG=--org=%SNYK_ORG%"
                            call snyk iac test iac\\main.tf %ORG_FLAG% --json > "%REPORTS_DIR%\\snyk-iac.json" 2>&1
                            exit /b %ERRORLEVEL%
                        '''
                    }
                }
            }
        }

        stage('Snyk Secrets') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        bat '''
                            @echo off
                            set "PATH=%APPDATA%\\npm;%PATH%"
                            call snyk secrets scan . > "%REPORTS_DIR%\\snyk-secrets.txt" 2>&1
                            exit /b %ERRORLEVEL%
                        '''
                    }
                }
            }
        }

        stage('Publish Reportes') {
            steps {
                archiveArtifacts artifacts: 'reports/*', allowEmptyArchive: true
                echo '=> Reportes Snyk guardados en artefactos: reports/*.json'
            }
        }
    }

    post {
        always {
            echo '=> Pipeline Snyk finalizado. Revisa: (1) artefactos /reports/*, (2) dashboard en app.snyk.io'
        }
        success {
            echo '=> Sin hallazgos >= severity configurada. Build VERDE.'
        }
        failure {
            echo '=> Hallazgos encontrados (o error de etapa). Consulta los reportes JSON.'
        }
    }
}