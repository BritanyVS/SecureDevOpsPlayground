# ============================================================
# Infraestructura SEGURA de referencia para el Task Manager.
# Analizable por Snyk IaC (políticas AWS/CloudFormation "Infrastructure as Code Issues").
# No incluye despliegue real obligatorio: es la "versión segura" que se compara
# con security-lab/iac (inseguro).
# ============================================================

# ------------------------------------------------------------------
# Networking: VPC con subnets privadas (app/db) y públicas (ALB)
# ------------------------------------------------------------------
resource "aws_vpc" "main" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = { Name = "${var.project}-vpc", Environment = var.environment }
}

resource "aws_subnet" "public" {
  count                   = 2
  vpc_id                  = aws_vpc.main.id
  cidr_block              = "10.0.${count.index}.0/24"
  map_public_ip_on_launch = true
  availability_zone       = data.aws_availability_zones.available.names[count.index]

  tags = { Name = "${var.project}-public-${count.index}", Environment = var.environment }
}

resource "aws_subnet" "private_app" {
  count             = 2
  vpc_id            = aws_vpc.main.id
  cidr_block        = "10.0.${count.index + 10}.0/24"
  availability_zone = data.aws_availability_zones.available.names[count.index]

  tags = { Name = "${var.project}-private-app-${count.index}", Environment = var.environment }
}

resource "aws_subnet" "private_db" {
  count             = 2
  vpc_id            = aws_vpc.main.id
  cidr_block        = "10.0.${count.index + 20}.0/24"
  availability_zone = data.aws_availability_zones.available.names[count.index]

  tags = { Name = "${var.project}-private-db-${count.index}", Environment = var.environment }
}

resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.main.id

  tags = { Name = "${var.project}-igw", Environment = var.environment }
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id
  tags   = { Name = "${var.project}-rt-public", Environment = var.environment }
}

resource "aws_route" "public_internet" {
  route_table_id         = aws_route_table.public.id
  destination_cidr_block = "0.0.0.0/0"
  gateway_id             = aws_internet_gateway.igw.id
}

resource "aws_route_table_association" "public" {
  count          = 2
  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

resource "aws_eip" "nat" {
  depends_on = [aws_internet_gateway.igw]
  domain     = "vpc"
}

resource "aws_nat_gateway" "nat" {
  allocation_id = aws_eip.nat.id
  subnet_id     = aws_subnet.public[0].id
  tags          = { Name = "${var.project}-nat", Environment = var.environment }
}

resource "aws_route_table" "private_app" {
  vpc_id = aws_vpc.main.id
  tags   = { Name = "${var.project}-rt-private-app", Environment = var.environment }
}

resource "aws_route" "private_nat" {
  route_table_id         = aws_route_table.private_app.id
  destination_cidr_block = "0.0.0.0/0"
  nat_gateway_id         = aws_nat_gateway.nat.id
}

resource "aws_route_table_association" "private_app" {
  count          = 2
  subnet_id      = aws_subnet.private_app[count.index].id
  route_table_id = aws_route_table.private_app.id
}

data "aws_availability_zones" "available" {
  state = "available"
}

# ------------------------------------------------------------------
# Security groups: PRINCIPIO DE MENOR PRIVILEGIO
# ------------------------------------------------------------------
resource "aws_security_group" "alb" {
  name        = "${var.project}-alb"
  description = "ALB: allow 80/443 from allowed_cidr only"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = [var.allowed_ingress_cidr]
  }
  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = [var.allowed_ingress_cidr]
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  tags = { Name = "${var.project}-alb", Environment = var.environment }
}

resource "aws_security_group" "app" {
  name        = "${var.project}-app"
  description = "App: solo trafico desde el ALB"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "Desde ALB"
    from_port       = 8080
    to_port         = 8080
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  tags = { Name = "${var.project}-app", Environment = var.environment }
}

resource "aws_security_group" "db" {
  name        = "${var.project}-db"
  description = "DB: solo trafico desde la app"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "MySQL desde app"
    from_port       = 3306
    to_port         = 3306
    protocol        = "tcp"
    security_groups = [aws_security_group.app.id]
  }
  tags = { Name = "${var.project}-db", Environment = var.environment }
}

# ------------------------------------------------------------------
# RDS: CIFRADO ACTIVO + backups + subredes privadas
# ------------------------------------------------------------------
resource "aws_db_subnet_group" "db" {
  name       = "${var.project}-db-subnet-group"
  subnet_ids = aws_subnet.private_db[*].id

  tags = { Name = "${var.project}-db-subnet", Environment = var.environment }
}

resource "aws_db_instance" "main" {
  identifier          = "${var.project}-${var.environment}"
  engine              = "mysql"
  engine_version      = "8.0.28"
  instance_class      = "db.t3.micro"
  allocated_storage   = 20
  db_name             = var.db_name
  username            = var.db_username
  password            = var.db_password
  storage_encrypted   = true                          # ✔ cifrado habilitado
  storage_type        = "gp3"
  multi_az            = var.environment == "prod"     # HA solo en prod
  backup_retention_period = 15                        # ✔ backups
  backup_window        = "03:00-04:00"
  maintenance_window   = "sun:04:00-05:00"
  deletion_protection  = true                         # ✔ protege borrado accidental
  skip_final_snapshot  = false
  db_subnet_group_name = aws_db_subnet_group.db.name
  vpc_security_group_ids = [aws_security_group.db.id]

  tags = { Name = "${var.project}-db", Environment = var.environment }
}

# ------------------------------------------------------------------
# IAM menor privilegio para ECS
# ------------------------------------------------------------------
resource "aws_iam_role" "ecs_task" {
  name = "${var.project}-ecs-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect    = "Allow"
        Principal = { Service = "ecs-tasks.amazonaws.com" }
        Action    = "sts:AssumeRole"
      }
    ]
  })
}

resource "aws_iam_role_policy" "ecs_task" {
  name = "${var.project}-ecs-task-policy"
  role = aws_iam_role.ecs_task.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect   = "Allow"
        Action   = ["logs:CreateLogStream", "logs:PutLogEvents"]
        Resource = ["${aws_cloudwatch_log_group.app.arn}:*"]
      },
      {
        Effect   = "Allow"
        Action = ["secretsmanager:GetSecretValue"]
        Resource = ["arn:aws:secretsmanager:${var.region}:*:secret:${var.project}/*"]
      }
    ]
  })
}

# ------------------------------------------------------------------
# CloudWatch Logs (✔ logging habilitado)
# ------------------------------------------------------------------
resource "aws_cloudwatch_log_group" "app" {
  name              = "/ecs/${var.project}"
  retention_in_days = 30
}

# ------------------------------------------------------------------
# ALB + listener + target group
# ------------------------------------------------------------------
resource "aws_lb" "main" {
  name               = "${var.project}-alb"
  internal           = false
  load_balancer_type = "application"
  subnets            = aws_subnet.public[*].id
  security_groups    = [aws_security_group.alb.id]

  tags = { Name = "${var.project}-alb", Environment = var.environment }
}

resource "aws_lb_target_group" "app" {
  name     = "${var.project}-tg"
  port     = 8080
  protocol = "HTTP"
  vpc_id   = aws_vpc.main.id

  health_check {
    path                = "/health"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 3
    matcher             = "200"
  }
}

resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.main.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.app.arn
  }
}

# ------------------------------------------------------------------
# ECS Fargate: imagen del backend SecureDevOps.API
# (completar image/container_definitions con el ECR/registry propio)
# ------------------------------------------------------------------
resource "aws_ecs_cluster" "main" {
  name = "${var.project}-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }
}

resource "aws_ecs_task_definition" "app" {
  family                   = "${var.project}-task"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = aws_iam_role.ecs_task.arn
  task_role_arn            = aws_iam_role.ecs_task.arn

  # Los secretos SE REFERENCIAN desde Secrets Manager / env vars injectadas en runtime.
  container_definitions = jsonencode([
    {
      name  = "backend"
      image = "${var.project}-backend:v1"   # reemplazar por imagen real
      essential = true
      portMappings = [{ containerPort = 8080, protocol = "tcp" }]
      environment = [
        { name = "ASPNETCORE_URLS", value = "http://+:8080" },
        { name = "ASPNETCORE_ENVIRONMENT", value = var.environment },
        { name = "SECURITYLAB__ENABLED", value = "false" }
      ]
      secrets = [
        { name = "JWT_SECRET", valueFrom = "arn:aws:secretsmanager:${var.region}:*:secret:${var.project}/jwt" },
        { name = "ConnectionStrings__DefaultConnection", valueFrom = "arn:aws:secretsmanager:${var.region}:*:secret:${var.project}/dbconn" }
      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.app.name
          "awslogs-region"        = var.region
          "awslogs-stream-prefix" = "backend"
        }
      }
    }
  ])
}

resource "aws_ecs_service" "app" {
  name            = "${var.project}-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.app.arn
  desired_count   = 2
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.private_app[*].id
    security_groups  = [aws_security_group.app.id]
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.app.arn
    container_name   = "backend"
    container_port   = 8080
  }

  depends_on = [aws_lb_listener.http]
}