terraform {
  backend "remote" {
    hostname = "app.terraform.io"
  }
}

provider "aws" { 
  access_key = var.aws_key
  secret_key = var.aws_secret
  region = var.aws_region
}

provider "random" {}
provider "helm" {
  kubernetes {
    config_path = var.k8s_config_path
  }
}

locals {
  rabbitmq = split(":", aws_mq_broker.rabbitmq.instances[0].endpoints[0])
}

resource "helm_release" "grpc" {
  name      = "grpc"
  chart     = var.charts_path
  namespace = var.k8s_namespace

  set {
    name  = "rabbitmq.port"
    value = local.rabbitmq[2]
  }
  set {
    name  = "rabbitmq.host"
    value = replace(local.rabbitmq[1], "/", "")
  }

  set {
    name  = "rabbitmq.user"
    value = random_string.mq_user.result
  }

  set {
    name  = "rabbitmq.password"
    value = random_password.mq_password.result
  }

  set {
    name  = "rabbitmq.ssl"
    value = true
  }

  set {
    name  = "pgsql.host"
    value = aws_db_instance.sql_discovery.address
  }

  set {
    name  = "pgsql.port"
    value = aws_db_instance.sql_discovery.port
  }

  set {
    name  = "pgsql.user"
    value = random_string.pg_user.result
  }

  set {
    name  = "pgsql.password"
    value = random_password.pg_password.result
  }

  set {
    name  = "elastic.host"
    value = aws_elasticsearch_domain.logs.endpoint
  }

  set {
    name  = "elastic.port"
    value = 443
  }
  set {
    name  = "elastic.password"
    value = random_password.es_password.result
  }

  set {
    name  = "elastic.user"
    value = random_string.es_user.result
  }

  set {
    name  = "weatherService.openWeatherId"
    value = var.openweather_api_key
  }

}


resource "random_password" "mq_password" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "random_password" "pg_password" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "random_password" "es_password" {
  length           = 16
  special          = true
  override_special = "_%@"
  min_upper        = 1
  min_numeric      = 1
}

resource "random_string" "mq_user" {
  length           = 16
  special          = false
  override_special = "_%@"
}

resource "random_string" "pg_user" {
  length           = 16
  special          = false
  override_special = "_%@"
}

resource "random_string" "es_user" {
  length           = 16
  special          = false
  override_special = "_%@"
}


resource "random_string" "es_domain" {
  length  = 8
  special = false
  lower   = true
  upper   = false
}

resource "aws_mq_broker" "rabbitmq" {
  broker_name = "RabbitMQ"


  engine_type         = "RabbitMQ"
  engine_version      = "3.8.11"
  host_instance_type  = "mq.t3.micro"
  publicly_accessible = true
  deployment_mode     = "SINGLE_INSTANCE"

  user {
    username = random_string.mq_user.result
    password = random_password.mq_password.result
  }
}

resource "aws_db_instance" "sql_discovery" {
  allocated_storage   = 20
  engine              = "postgres"
  engine_version      = "12.6"
  instance_class      = "db.t2.micro"
  name                = "sql_discovery"
  username            = random_string.pg_user.result
  password            = random_password.pg_password.result
  publicly_accessible = true
  skip_final_snapshot = true
}

resource "aws_elasticsearch_domain" "logs" {
  domain_name           = "logs${random_string.es_domain.result}"
  elasticsearch_version = "7.10"

  ebs_options {
    ebs_enabled = true
    volume_size = 10
  }

  cluster_config {
    instance_type  = "t3.small.elasticsearch"
    instance_count = 1
  }

  domain_endpoint_options {
    enforce_https       = true
    tls_security_policy = "Policy-Min-TLS-1-2-2019-07"
  }
  advanced_security_options {
    enabled                        = true
    internal_user_database_enabled = true
    master_user_options {
      master_user_name     = random_string.es_user.result
      master_user_password = random_password.es_password.result
    }
  }

  encrypt_at_rest {
    enabled = true
  }
  node_to_node_encryption {
    enabled = true
  }
}

resource "aws_elasticsearch_domain_policy" "main" {
  domain_name = aws_elasticsearch_domain.logs.domain_name

  access_policies = <<POLICIES
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Action": "es:*",
            "Principal": "*",
            "Effect": "Allow",
            "Condition": {
                "IpAddress": {"aws:SourceIp": "0.0.0.0/0"}
            },
            "Resource": "${aws_elasticsearch_domain.logs.arn}/*"
        }
    ]
}
POLICIES
}


