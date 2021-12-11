from concurrent import futures

import logging
import threading

import grpc

from services import cityinformation_pb2
from services import cityinformation_pb2_grpc

from services import healthcheck_pb2
from services import healthcheck_pb2_grpc

import city
import inspect

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

class HealthCheckService(healthcheck_pb2_grpc.HealthCheckService):

    def GetStatus(self, request, context):
        return healthcheck_pb2.PingResponse(response=str('pong'))

def serve():
    logger = Logger.getInstance()
    metrics = Prometheus.getInstance()
    log_interceptor = logging_interceptor.LoggingInterceptor(logger)
    metric_interceptor = metrics_interceptor.MetricsInterceptor(metrics)

    server = grpc.server(futures.ThreadPoolExecutor(
        max_workers=10), interceptors=(log_interceptor, metric_interceptor,))

    cityinformation_pb2_grpc.add_CityServiceServicer_to_server(CityService(), server)
    healthcheck_pb2_grpc.add_HealthCheckServiceServicer_to_server(HealthCheckService(), server)
    
    port = os.getenv('PORT')

    server.add_insecure_port('[::]:{}'.format(port))
    logger.info('Server running on [::]:{}'.format(port), {'port': port})

    server.start()
    server.wait_for_termination()

if __name__ == '__main__':
    load_dotenv()
    logging.basicConfig()
    serve()
