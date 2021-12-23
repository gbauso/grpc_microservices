from concurrent import futures

import logging
import threading

import grpc

from grpc_health.v1 import health, health_pb2
from grpc_health.v1 import health_pb2_grpc

from grpc_reflection.v1alpha import reflection

from services import cityinformation_pb2
from services import cityinformation_pb2_grpc

import city

import os

import logging_interceptor
from logger import Logger

import metrics_interceptor
from prometheus import Prometheus
from dotenv import load_dotenv


class ThreadJob(threading.Thread):
    def __init__(self, callback, event, interval):
        '''runs the callback function after interval seconds

        :param callback:  callback function to invoke
        :param event: external event for controlling the update operation
        :param interval: time in seconds after which are required to fire the callback
        :type callback: function
        :type interval: int
        '''
        self.callback = callback
        self.event = event
        self.interval = interval
        super(ThreadJob, self).__init__()

    def run(self):
        while not self.event.wait(self.interval):
            self.callback()


class CityService(cityinformation_pb2_grpc.CityService):

    def GetCityInformation(self, request, context):
        cityData = city.City.get_city_opendata(request.city, request.country)
        return cityinformation_pb2.SearchResponse(population=str(cityData['population']))

def serve():
    logger = Logger.getInstance()
    metrics = Prometheus.getInstance()
    log_interceptor = logging_interceptor.LoggingInterceptor(logger)
    metric_interceptor = metrics_interceptor.MetricsInterceptor(metrics)

    server = grpc.server(futures.ThreadPoolExecutor(
        max_workers=10), 
        interceptors=(log_interceptor,metric_interceptor,)
        )

    
    health_servicer = health.HealthServicer()
    cityinformation_pb2_grpc.add_CityServiceServicer_to_server(CityService(), server)
    health_pb2_grpc.add_HealthServicer_to_server(health_servicer, server)
    
    services = tuple(
        service.full_name
        for service in cityinformation_pb2.DESCRIPTOR.services_by_name.values()) + (
            reflection.SERVICE_NAME, health.SERVICE_NAME)
    
    for service in services:
        health_servicer.set(service, health_pb2.HealthCheckResponse.SERVING)
    
    reflection.enable_server_reflection(services, server)
    
    port = os.getenv('PORT')

    server.add_insecure_port('[::]:{}'.format(port))
    logger.info('Server running on [::]:{}'.format(port), {'port': port})

    server.start()
    server.wait_for_termination()

if __name__ == '__main__':
    load_dotenv()
    logging.basicConfig()
    serve()
