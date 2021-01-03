import config
import os
from influxdb_client import InfluxDBClient as InfluxDBClientv2
from datetime import datetime
from influxdb_client.client.write_api import SYNCHRONOUS
from prometheus_client import start_http_server


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
        start_http_server(3001)

    def collect_call_metrics(self, metrics: CallMetrics):
        pass
