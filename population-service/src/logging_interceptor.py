import grpc
import json
from collections import ChainMap


class LoggingInterceptor(grpc.ServerInterceptor):
    def __init__(self, logger):
        self.logger = logger

    
    def _wrap_rpc_behavior(handler, details, fn):
        grpc_handler = handler(details)
        if grpc_handler is None:
            return None

        if grpc_handler.request_streaming and grpc_handler.response_streaming:
            behavior_fn = grpc_handler.stream_stream
            handler_factory = grpc.stream_stream_rpc_method_handler
        elif grpc_handler.request_streaming and not grpc_handler.response_streaming:
            behavior_fn = grpc_handler.stream_unary
            handler_factory = grpc.stream_unary_rpc_method_handler
        elif not grpc_handler.request_streaming and grpc_handler.response_streaming:
            behavior_fn = grpc_handler.unary_stream
            handler_factory = grpc.unary_stream_rpc_method_handler
        else:
            behavior_fn = grpc_handler.unary_unary
            handler_factory = grpc.unary_unary_rpc_method_handler

        return handler_factory(fn(behavior_fn,
                                details,
                                grpc_handler.request_streaming,
                                grpc_handler.response_streaming),
                            request_deserializer=grpc_handler.request_deserializer,
                            response_serializer=grpc_handler.response_serializer)


    def intercept_service(self, continuation, handler_call_details):
        metadata = self.__getMetadata(handler_call_details.invocation_metadata)
        self.correlation_id = metadata.get('correlation_id')
        self.logger.info('Request for {} STARTED. correlation-id: {}'.format(handler_call_details.method, self.correlation_id),
                         metadata)

        return self._wrap_rpc_behavior(continuation, handler_call_details, self.__call_wrapper)

    def __getMetadata(self, metadata):
        metadata_raw = [{i.key: i.value} for i in metadata]
        return dict(ChainMap(*metadata_raw))

    def __call_wrapper(self, behavior, details, request_streaming, response_streaming):
        def new_behavior(request_or_iterator, servicer_context):
            metadata = self.__getMetadata(details.invocation_metadata)
            try:
                return behavior(request_or_iterator, servicer_context)
            except Exception as err:
                self.logger.error('Request for {} FAILED. correlation-id: {}'.format(details.method, self.correlation_id), {
                    "err":  json.dumps(err)})
            finally:
                self.logger.info('Request for {} FINISHED. correlation-id: {}'.format(details.method, self.correlation_id),
                    self.__getMetadata(metadata))

        return new_behavior
