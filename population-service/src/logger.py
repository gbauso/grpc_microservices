from datetime import datetime, timezone
import logging
import os
import json
import uuid


class Logger(object):

    __instance = None

    @staticmethod
    def getInstance():
        """ Static access method. """
        if Logger.__instance == None:
            Logger.__instance = Logger()
        return Logger.__instance

    def __init__(self):
        self.logger = logging.getLogger(__name__)
        self.logger.setLevel(logging.DEBUG)
        fileName = os.getenv('LOG_PATH', '/tmp/population-{}.log').format(str(uuid.uuid4()))
        
        loggingStreamHandler = logging.StreamHandler()
        loggingStreamHandler = logging.FileHandler(fileName,mode='a')
        loggingStreamHandler.setFormatter(JSONFormatter())
        self.logger.addHandler(loggingStreamHandler)

    def error(self, message, data):
        self.logger.error(message, data)

    def info(self, message, data):
        self.logger.info(message, data)

    def debug(self, message, data):
        self.logger.debug(message, data)


class JSONFormatter(logging.Formatter):
    def __init__(self):
        super().__init__()

    def format(self, record):
        time = datetime.fromtimestamp(record.created, timezone.utc).isoformat('T', 'microseconds')
        record = json.dumps({'msg': record.msg, 'time': time, 'level': record.levelname, **record.args})
        return record
