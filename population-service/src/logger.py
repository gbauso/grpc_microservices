from fluent import sender
import config
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
        host = os.getenv('LOGGER_HOST',config.logger['host'])
        port = os.getenv('LOGGER_PORT',config.logger['port'])
        self.logger = sender.FluentSender('population', host=host, port=int(port))

    def __log(self, level, message, data):
        self.logger.emit(None ,{"Level": level, "m": message, "data": data})

    def error(self, message, data):
        self.__log('Error', message, data)

    def info(self, message, data):
        self.__log('Information', message, data)
    
    def debug(self, message, data):
        self.__log('Debug', message, data)