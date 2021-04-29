variable "openweather_api_key" {
  type = string
}

variable "charts_path" {
  type = string
}

variable "k8s_config_path" {
  type = string
}

variable "k8s_namespace" {
  type = string
}

variable "aws_key" {
  type = string
}

variable "aws_secret" {
  type = string
}

variable "aws_region" {
  type = string
  default = "us-east-2"
}