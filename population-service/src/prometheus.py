import os
from datetime import datetime
from prometheus_client import start_http_server, Counter, Histogram


class CallMetrics(object):
    def __init__(self, method, call_type, status_code, request_start):
        self.method = method
        self.call_type = call_type
        self.status_code = status_code
        self.elapsed_time = datetime.now() - request_start


class Prometheus(object):

    __instance = None

    @staticmethod
    def getInstance():
        """ Static access method. """
        if Prometheus.__instance == None:
            Prometheus.__instance = Prometheus()
        return Prometheus.__instance

    def __init__(self):
        self.grpcServerStartedTotal = Counter('grpc_server_started_total',
                                              'Total number of RPCs started on the server.',
                                              ['grpc_type', 'grpc_method'])
        self.grpcServerHandledTotal = Counter('grpc_server_handled_total',
                                              'Total number of RPCs completed on the server, regardless of success or failure.',
                                              ['grpc_type', 'grpc_method', 'grpc_code'])
        self.grpcServerHandlingSeconds = Histogram('grpc_server_handling_seconds',
                                              'Histogram of response latency (seconds) of gRPC that had been application-level handled by the server.Duration of HTTP response size in bytes',
                                              ['grpc_type', 'grpc_method', 'grpc_code'])

        start_http_server(int(os.getenv('METRICS_PORT')))

    def collect_call_metrics(self, metrics: CallMetrics):
        self.grpcServerStartedTotal.labels(metrics.call_type, metrics.method).inc()
        self.grpcServerHandledTotal.labels(metrics.call_type, metrics.method, metrics.status_code).inc()
        self.grpcServerHandlingSeconds.labels(metrics.call_type, metrics.method, metrics.status_code).observe(metrics.elapsed_time.total_seconds())