# ============================================================
# LABORATORIO Snyk IaC — infraestructura INTENCIONALMENTE insegura.
# NO aplicar. Ejecutar: snyk iac test iac/main.tf
# FIX de cada hallazgo en docs/VULNERABILITIES.md
# ============================================================
terraform {
  required_version = ">= 1.5"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# VULN: credenciales hardcodeadas en el .tf (Snyk IaC: "PlainText").
provider "aws" {
  region     = "us-east-1"
  access_key = "AKIAIOSFODNN7EXAMPLE"
  secret_key = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
}

# VULN: bucket S3 con ACL pública de lectura (regla "S3 Bucket ACL public-read").
# FIX: acl = "private" + bloqueo de acceso público.
resource "aws_s3_bucket" "public_assets" {
  bucket = "snyk-lab-demo-assets"
  acl    = "public-read"
}

# VULN: security group abierto a todo internet en puertos administrativos
# (reglas "SSH open to the internet" / "MySQL open to the internet").
# FIX: limitar CIDR y solo puertos necesarios desde el SG del backend.
resource "aws_security_group" "wide_open" {
  name        = "snyk-lab-sg"
  description = "TODO abierto (demo)"
  vpc_id      = aws_vpc.demo.id

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  ingress {
    from_port   = 3306
    to_port     = 3306
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# VULN: RDS SIN cifrado, SIN backups, PÚBLICO y con contraseña hardcodeada
# (reglas "RDS storage not encrypted", "RDS backup disabled",
#  "RDS publicly accessible", "PlainText password").
# FIX: storage_encrypted=true, backup_retention_period>=7,
#      publicly_accessible=false, variables sensitive.
resource "aws_db_instance" "demo_db" {
  identifier             = "snyk-lab-db"
  engine                 = "mysql"
  engine_version         = "8.0"
  instance_class         = "db.t3.micro"
  allocated_storage      = 20
  db_name                = "tasks"
  username               = "admin"
  password               = "P@ssw0rd_Demo_123!"            # FALLO: hardcodeada
  storage_encrypted      = false                           # FALLO
  backup_retention_period = 0                              # FALLO
  skip_final_snapshot    = true                            # FALLO
  publicly_accessible    = true                            # FALLO
}

# VULN: IAM con permisos totales (regla "IAM policy grants all access").
# FIX: policies específicas de menor privilegio.
resource "aws_iam_policy" "allow_everything" {
  name = "snyk-lab-everything"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = ["*"]
      Resource = ["*"]
    }]
  })
}

resource "aws_vpc" "demo" {
  cidr_block = "10.0.0.0/16"
}