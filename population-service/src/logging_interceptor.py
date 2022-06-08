from collections import ChainMap
import json
from typing import Callable
import grpc
from grpc_interceptor import ServerInterceptor
from grpc_interceptor.exceptions import GrpcException


class LoggingInterceptor(ServerInterceptor):
    def __init__(self, logger):
        self.logger = logger
        
    def __getMetadata(self, metadata):
        metadata_raw = [{i.key: i.value} for i in metadata]
        return dict(ChainMap(*metadata_raw))
    
    def intercept(
        self,
        method: Callable,
        request,
        context: grpc.ServicerContext,
        method_name: str,
    ):
        metadata = self.__getMetadata(context.invocation_metadata())
        correlation_id = metadata.get('correlation_id')
        self.logger.info('Request for {} STARTED. correlation-id: {}'.format(method_name, correlation_id),
                         metadata)

        try:
            return method(request, context)
        except GrpcException as err:
             self.logger.error('Request for {} FAILED. correlation-id: {}'.format(method_name, correlation_id), {
                    "err":  json.dumps(err)})
        finally:
            self.logger.info('Request for {} FINISHED. correlation-id: {}'.format(method_name, correlation_id),
                metadata)

