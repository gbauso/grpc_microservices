terraform {
  backend "remote" {
    hostname = "app.terraform.io"
  }
}

provider "helm" {
  kubernetes {
    config_path = var.k8s_config_path
  }
}


resource "helm_release" "grpc" {
  name      = "grpc"
  chart     = var.charts_path
  namespace = var.k8s_namespace

  set {
    name  = "weatherService.openWeatherId"
    value = var.openweather_api_key
  }
  set {
    name  = "newRelic.baseurl"
    value = var.new_relic_base_url
  }
  set {
    name  = "newRelic.apiKey"
    value = var.new_relic_api_key
  }

}


