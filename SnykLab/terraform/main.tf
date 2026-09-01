# ⚠️ LABORATORIO: Terraform inseguro intencional (Snyk IaC)

terraform {
  required_version = ">= 0.14"
}

# VULNERABILIDAD: versión antigua de provider con vulnerabilidades conocidas
provider "aws" {
  region     = "us-east-1"
  access_key = "AKIAIOSFODNN7EXAMPLE"
  secret_key = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
}

# VULNERABILIDAD 1: bucket S3 público (permite acceso público)
resource "aws_s3_bucket" "public_bucket" {
  bucket = "my-public-bucket"
  acl    = "public-read"

  # VULNERABILIDAD 2: cifrado deshabilitado
  server_side_encryption_configuration {
    rule {
      apply_server_side_encryption_by_default {
        sse_algorithm = "AES256"
      }
    }
  }
}

# VULNERABILIDAD 3: security group abierto a todo el mundo (0.0.0.0/0)
resource "aws_security_group" "open_sg" {
  name        = "open-security-group"
  description = "Security group abierto a todo el mundo"

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"] # FALLO: puerto SSH abierto al público
  }

  ingress {
    from_port   = 3306
    to_port     = 3306
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"] # FALLO: MySQL expuesto
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# VULNERABILIDAD 4: RDS sin cifrado
resource "aws_db_instance" "insecure_db" {
  allocated_storage    = 20
  storage_type         = "gp2"
  engine               = "mysql"
  engine_version       = "8.0"
  instance_class       = "db.t3.micro"
  name                 = "exampledb"
  username             = "adminuser"
  password             = "Str0ngP@ss" # FALLO: password hardcodeada
  skip_final_snapshot  = true

  # VULNERABILIDAD: no cifrar el almacenamiento
  storage_encrypted    = false
}

# VULNERABILIDAD 5: IAM policy con privilegios excesivos
resource "aws_iam_policy" "too_much_permission" {
  name = "allow-everything"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action   = ["*"] # FALLO: todos los permisos
        Effect   = "Allow"
        Resource = "*"   # FALLO: todos los recursos
      }
    ]
  })
}