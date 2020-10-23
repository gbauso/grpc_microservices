import config
import os
from influxdb import InfluxDBClient
import datetime

class CallMetrics(object):
    def __init__(self, method, call_type, status_code, request_start):
        self.method = method
        self.call_type = call_type
        self.status_code = status_code
        self.elapsed_time = datetime.datetime.now() - request_start

class ServerMetrics(object):
    def __init__(self, memory_free, memory_usage, cpu_usage)
        self.memory_free = memory_free
        self.memory_usage = memory_usage
        self.cpu_usage = cpu_usage

class InfluxDb(object):

    __instance = None

    @staticmethod
    def getInstance():
        """ Static access method. """
        if InfluxDb.__instance == None:
            InfluxDb.__instance = InfluxDb()
        return InfluxDb.__instance

    def __init__(self):
        host = os.getenv('METRICS_HOST', config.metrics['host'])
        username = os.getenv('METRICS_USER', config.metrics['user'])
        password = os.getenv('METRICS_PASSWORD', config.metrics['password'])
        self.client = InfluxDBClient(
            host=host, username=username, password=password, database='population')

    def collect_call_metrics(self, metrics: CallMetrics):
        json_body = [
            {
                "measurement": "call_data",
                "tags": {
                    "method": metrics.method,
                    "callType": metrics.call_type
                },
                "time": datetime.datetime.now(),
                "fields": {
                    "status": metrics.status_code,
                    "duration": metrics.elapsed_time
                }
            }
        ]
        self.client.write_points(json_body)

    def collect_server_metrics(self, metrics: ServerMetrics):
        json_body = [
            {
                "measurement": "perf",
                "tags": {
                    "hostname": "any"
                },
                "time": datetime.datetime.now(),
                "fields": {
                    "memory_usage": metrics.memory_usage,
                    "memory_free": metrics.memory_free,
                    "cpu_usage": metrics.cpu_usage
                }
            }
        ]
        self.client.write_points(json_body)
