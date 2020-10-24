from concurrent import futures
import config
import psutil

import logging
import config
import threading

import grpc

import cityinformation_pb2
import cityinformation_pb2_grpc

import city
import inspect

import os
import autodiscovery

import logging_interceptor
from logger import Logger

import metrics_interceptor
from influx_db import InfluxDb, ServerMetrics


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
    metrics = InfluxDb.getInstance()
    log_interceptor = logging_interceptor.LoggingInterceptor(logger)
    metric_interceptor = metrics_interceptor.MetricsInterceptor(metrics)

    discovery = autodiscovery.AutoDiscovery()
    server = grpc.server(futures.ThreadPoolExecutor(
        max_workers=10), interceptors=(log_interceptor, metric_interceptor,))

    registerAutoDiscovery(server, CityService(),
                          cityinformation_pb2_grpc, discovery)
    port = os.getenv('PORT', config.port)

    server.add_insecure_port('[::]:{}'.format(port))
    logger.info('Server running on [::]:{}'.format(port), {'port': port})

    server.start()
    discovery.register(port)
    server.wait_for_termination()


def registerAutoDiscovery(server, service, grpc, autodiscovery):
    methodName = [i for i in grpc.__dict__.keys() if i[:3] == 'add'][0]
    method = getattr(grpc, methodName)
    method(service, server)
    autodiscovery.add_service(grpc)


def collect_server_metrics():
    metrics = InfluxDb.getInstance()
    metrics.collect_server_metrics(ServerMetrics(psutil.virtual_memory().available * 100 / psutil.virtual_memory().total,
                                                 psutil.virtual_memory().percent,
                                                 psutil.cpu_percent()))


if __name__ == '__main__':
    logging.basicConfig()
    event = threading.Event()
    ThreadJob(collect_server_metrics, event, 60).start()
    serve()
