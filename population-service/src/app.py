from concurrent import futures
import config

import logging
import config

import grpc

import cityinformation_pb2
import cityinformation_pb2_grpc

import city
import inspect

import os
import autodiscovery

import interceptor

from logger import Logger

class CityService(cityinformation_pb2_grpc.CityService):

    def GetCityInformation(self, request, context):
        cityData = city.City.get_city_opendata(request.city, request.country)
        return cityinformation_pb2.SearchResponse(population=str(cityData['population']))


def serve():
    logger = Logger.getInstance()
    log_interceptor = interceptor.LoggingInterceptor(logger)
    discovery = autodiscovery.AutoDiscovery()
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10), interceptors=(log_interceptor,))
    
    registerAutoDiscovery(server, CityService(), cityinformation_pb2_grpc, discovery)
    port = os.getenv('PORT', config.port)

    server.add_insecure_port('[::]:{}'.format(port))
    logger.info('Server running on [::]:{}'.format(port), {'port': port })

    server.start()
    discovery.register(port)
    server.wait_for_termination()

def registerAutoDiscovery(server, service, grpc, autodiscovery):
    methodName = [i for i in grpc.__dict__.keys() if i[:3] == 'add'][0]
    method = getattr(grpc, methodName)
    method(service, server)
    autodiscovery.add_service(grpc)   

if __name__ == '__main__':
    logging.basicConfig()
    serve()