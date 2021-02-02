from fluent import sender
import os

class Logger(object):
    
    __instance = None

    @staticmethod 
    def getInstance():
        """ Static access method. """
        if Logger.__instance == None:
            Logger.__instance = Logger()
        return Logger.__instance

    def __init__(self):
        host = os.getenv('LOGGER_HOST')
        port = os.getenv('LOGGER_PORT')
        self.logger = sender.FluentSender('population', host=host, port=int(port))

    def __log(self, level, message, data):
        self.logger.emit(None ,{"level": level, "m": message, **data})

    def error(self, message, data):
        self.__log('error', message, data)

    def info(self, message, data):
        self.__log('information', message, data)
    
    def debug(self, message, data):
        self.__log('debug', message, data)