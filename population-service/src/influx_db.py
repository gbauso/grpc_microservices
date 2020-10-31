import config
import os
from influxdb import InfluxDBClient
from influxdb_client import InfluxDBClient as InfluxDBClientv2
from datetime import datetime
from influxdb_client.client.write_api import SYNCHRONOUS


class CallMetrics(object):
    def __init__(self, method, call_type, status_code, request_start):
        self.method = method
        self.call_type = call_type
        self.status_code = status_code
        self.elapsed_time = datetime.now() - request_start


class ServerMetrics(object):
    def __init__(self, memory_free, memory_usage, cpu_usage):
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
        self.username = os.getenv('METRICS_USER', config.metrics['user'])
        token = os.getenv("METRICS_TOKEN", config.metrics['token'])
        self.client = InfluxDBClientv2(host, token)
        self.write = self._writeV2

    def _writeV2(self, data):
        self.client.write_api(write_options=SYNCHRONOUS).write(
            "metrics", self.username, data)

    def collect_call_metrics(self, metrics: CallMetrics):
        json_body = [
            {
                "measurement": "call_data",
                "tags": {
                    "method": metrics.method,
                    "call_type": metrics.call_type,
                    "service": "population",
                    "instance": os.getenv("HOSTNAME", "local")
                },
                "time": datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%S"),
                "fields": {
                    "status": metrics.status_code.name,
                    "duration": metrics.elapsed_time.microseconds / 1000
                }
            }
        ]
        self.write(json_body)

    def collect_server_metrics(self, metrics: ServerMetrics):
        json_body = [
            {
                "measurement": "perf",
                "tags": {
                    "service": "population",
                    "instance": os.getenv("HOSTNAME", "local")
                },
                "time": datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%S"),
                "fields": {
                    "memory_usage": metrics.memory_usage,
                    "memory_free": metrics.memory_free,
                    "cpu_usage": metrics.cpu_usage
                }
            }
        ]
        self.write(json_body)
