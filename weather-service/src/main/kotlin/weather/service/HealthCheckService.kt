package weather.service

import io.grpc.health.v1.HealthGrpc

class HealthCheckService() : HealthGrpc.HealthImplBase() {

}