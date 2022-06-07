import datetime
from typing import Callable
import grpc
from grpc_interceptor import ServerInterceptor
from prometheus import Prometheus, CallMetrics


class MetricsInterceptor(ServerInterceptor):
    def __init__(self, metrics_provider: Prometheus):
        self.metrics_provider = metrics_provider

    def intercept(
        self,
        method: Callable,
        request,
        context: grpc.ServicerContext,
        method_name: str,
    ):
        request_start = datetime.datetime.now()

        try:
            result_status = grpc.StatusCode.OK
            return method(request, context)
        except grpc.RpcError as e:
            result_status = e.__dict__['code']
        except Exception:
            result_status = grpc.StatusCode.UNKNOWN
        finally:
            metrics = CallMetrics(method_name, 'unary', result_status, request_start)
            self.metrics_provider.collect_call_metrics(metrics)


