# ⚠️ LABORATORIO CONTROLADO de Snyk IaC — infraestructura INSEGURA.
# No aplicar en producción. Cada bloque marca el hallazgo esperado.
# La versión segura está en infrastructure/terraform.

terraform {
  required_version = ">= 1.5"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# VULNERABILIDAD: credenciales hardcodeadas en el código IaC (Snyk IaC: "PlainText password").
provider "aws" {
  region     = "us-east-1"
  access_key = "AKIAIOSFODNN7EXAMPLE"
  secret_key = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
}

# VULNERABILIDAD: bucket S3 PÚBLICO de lectura (regla "S3 bucket ACL public-read").
resource "aws_s3_bucket" "public_bucket" {
  bucket = "my-public-demo-bucket"
  acl    = "public-read"
}

# VULNERABILIDAD: bucket S3 SIN versionado ni logging (reglas "S3 versioning disabled",
# "S3 logging disabled").
resource "aws_s3_bucket" "no_logging" {
  bucket = "demo-no-logging"
  acl    = "private"
}

# VULNERABILIDAD: security group con 0.0.0.0/0 en ports administrativos
# (regla "Security Group rule allows all traffic" / "SSH open to the internet").
resource "aws_security_group" "wide_open" {
  name        = "demo-wide-open"
  description = "Todo abierto (demo)"
  vpc_id      = aws_vpc.demo.id

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "SSH del mundo (FALLO)"
  }
  ingress {
    from_port   = 3306
    to_port     = 3306
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "MySQL del mundo (FALLO)"
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# VULNERABILIDADES: RDS sin cifrado, sin backup, sin deletion protection,
# password hardcodeada, subnets públicas expuestas (reglas "RDS storage not encrypted",
# "RDS backup disabled", "RDS password hardcoded"...).
resource "aws_db_instance" "exposed_db" {
  identifier          = "demo-db"
  allocated_storage   = 20
  engine              = "mysql"
  engine_version      = "8.0"
  instance_class      = "db.t3.micro"
  db_name             = "exampledb"
  username            = "adminuser"
  password            = "Str0ngP@ss"
  storage_encrypted   = false                       # FALLO: sin cifrado
  backup_retention_period = 0                       # FALLO: sin backups
  skip_final_snapshot = true                        # FALLO
  publicly_accessible = true                        # FALLO: expuesta a internet
}

# VULNERABILIDAD: IAM con permisos excesivos (regla "IAM policy grants all access").
resource "aws_iam_policy" "allow_everything" {
  name = "demo-allow-everything"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect   = "Allow"
        Action   = ["*"]
        Resource = ["*"]
      }
    ]
  })
}

# VULNERABILIDAD: launch config con user_data echo de secreto (reglas "PlainText secret").
resource "aws_launch_configuration" "leaks_secret" {
  name          = "demo-lc"
  image_id      = "ami-0abcdef1234567890"
  instance_type = "t3.micro"

  user_data = <<-EOT
    #!/bin/bash
    export API_KEY=AKIAIOSFODNN7EXAMPLE
  EOT
}

# VPC mínima para que el plan/validate sea coherente.
resource "aws_vpc" "demo" {
  cidr_block = "10.0.0.0/16"
}

resource "aws_subnet" "public" {
  vpc_id            = aws_vpc.demo.id
  cidr_block        = "10.0.1.0/24"
  map_public_ip_on_launch = true
}