# ⚠️ LABORATORIO: Terraform inseguro para Kubernetes (Snyk IaC)
# Un pod privilegiado con secretos en variables de entorno

resource "kubernetes_secret" "db_credentials" {
  metadata {
    name = "db-credentials"
  }
  data = {
    # VULNERABILIDAD: secretos en texto plano
    "username" = "admin"
    "password" = "P@ssw0rd" # FALLO: credenciales hardcodeadas
  }
}

resource "kubernetes_deployment" "insecure_app" {
  metadata {
    name = "insecure-app"
  }

  spec {
    replicas = 2

    selector {
      match_labels = {
        app = "insecure-app"
      }
    }

    template {
      metadata {
        labels = {
          app = "insecure-app"
        }
      }

      spec {
        # VULNERABILIDAD: correr como root
        security_context {
          run_as_user = 0
        }

        container {
          image = "nginx:1.14" # VULNERABILIDAD: imagen antigua
          name  = "app"

          # VULNERABILIDAD: puertos expuestos
          port {
            container_port = 80
          }

          # VULNERABILIDAD: variables de entorno con secretos directo
          env {
            name  = "DATABASE_PASSWORD"
            value = "P@ssw0rd"
          }
        }
      }
    }
  }
}