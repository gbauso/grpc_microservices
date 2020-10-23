import grpc
import json
from collections import ChainMap
import datetime
from influx_db import InfluxDb, CallMetrics

def _wrap_rpc_behavior(handler, details, fn):
    grpc_handler = handler(details)
    call_type = None
    if grpc_handler is None:
        return None

    if grpc_handler.request_streaming and grpc_handler.response_streaming:
        call_type = "bidi"
        behavior_fn = grpc_handler.stream_stream
        handler_factory = grpc.stream_stream_rpc_method_handler
    elif grpc_handler.request_streaming and not grpc_handler.response_streaming:
        call_type = "stream_unary"
        behavior_fn = grpc_handler.stream_unary
        handler_factory = grpc.stream_unary_rpc_method_handler
    elif not grpc_handler.request_streaming and grpc_handler.response_streaming:
        call_type = "unary_stream"
        behavior_fn = grpc_handler.unary_stream
        handler_factory = grpc.unary_stream_rpc_method_handler
    else:
        call_type = "unary"
        behavior_fn = grpc_handler.unary_unary
        handler_factory = grpc.unary_unary_rpc_method_handler

    return handler_factory(fn(call_type,
                              behavior_fn,
                              details,
                              grpc_handler.request_streaming,
                              grpc_handler.response_streaming),
                           request_deserializer=grpc_handler.request_deserializer,
                           response_serializer=grpc_handler.response_serializer)


class MetricsInterceptor(grpc.ServerInterceptor):
    def __init__(self, metrics_provider: InfluxDb):
        self.metrics_provider = metrics_provider

    def intercept_service(self, continuation, handler_call_details):
        self.request_start = datetime.datetime.now()

        return _wrap_rpc_behavior(continuation, handler_call_details, self.__call_wrapper)

    def __getMetadata(self, metadata):
        metadata_raw = [{i.key: i.value} for i in metadata]
        return dict(ChainMap(*metadata_raw))

    def __call_wrapper(self, call_type, behavior, details, request_streaming, response_streaming):
        def new_behavior(request_or_iterator, servicer_context):
            try:
                result_status = grpc.StatusCode.OK
                return behavior(request_or_iterator, servicer_context)
            except grpc.RpcError as e:
                result_status = e.__dict__['code']
            except Exception:
                result_status = grpc.StatusCode.UNKNOWN
            finally:
                metrics = CallMetrics(details.method, call_type, result_status, self.request_start)
                self.metrics_provider.collect_call_metrics(metrics)

        return new_behavior
