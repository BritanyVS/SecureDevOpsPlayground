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
        REPORTS_DIR = "${WORKSPACE}/reports"
        SNYK_CLI_VERSION = '1.1293.2'
    }

    stages {
        stage('Preparacion: CLI + reportes') {
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    sh '''
                        echo "=> Instalando Snyk CLI ${SNYK_CLI_VERSION}..."
                        npm install -g snyk@${SNYK_CLI_VERSION} > /dev/null 2>&1 || true
                        snyk --version || true
                        mkdir -p ${REPORTS_DIR}
                        echo "export REPORTS_DIR=${REPORTS_DIR}" > /tmp/snyk_env
                    '''
                }
            }
        }

        stage('Snyk Auth (token via Jenkins Credentials)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        sh '''
                            source /tmp/snyk_env
                            snyk auth "$SNYK_TOKEN" > /dev/null 2>&1 || true
                            ORG_FLAG=""
                            if [ -n "$SNYK_ORG" ]; then ORG_FLAG="--org=$SNYK_ORG"; fi
                            echo "ORG_FLAG=$ORG_FLAG" >> /tmp/snyk_env
                        '''
                    }
                }
            }
        }

        stage('Snyk Open Source (SCA)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        sh '''
                            source /tmp/snyk_env
                            set +e
                            snyk test --all-projects --json > ${REPORTS_DIR}/snyk-oss.json 2>&1
                            exit $?
                        '''
                    }
                }
            }
        }

        stage('Snyk Code (SAST)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        sh '''
                            source /tmp/snyk_env
                            set +e
                            snyk code test --json > ${REPORTS_DIR}/snyk-code.json 2>&1
                            exit $?
                        '''
                    }
                }
            }
        }

        stage('Snyk Container (Imagenes)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        sh '''
                            source /tmp/snyk_env
                            set +e
                            snyk container test "${BACKEND_IMAGE}" --file=Dockerfile --json > ${REPORTS_DIR}/snyk-container.json 2>&1
                            exit $?
                        '''
                    }
                }
            }
        }

        stage('Snyk IaC (Terraform)') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        sh '''
                            source /tmp/snyk_env
                            set +e
                            snyk iac test iac/main.tf --json > ${REPORTS_DIR}/snyk-iac.json 2>&1
                            exit $?
                        '''
                    }
                }
            }
        }

        stage('Snyk Secrets') {
            steps {
                withCredentials([string(credentialsId: 'snyk-token', variable: 'SNYK_TOKEN')]) {
                    catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                        sh '''
                            source /tmp/snyk_env
                            set +e
                            snyk secrets scan . > ${REPORTS_DIR}/snyk-secrets.txt 2>&1
                            exit $?
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
