import pika
import json
import os
import config

class AutoDiscovery:
    def __init__(self):
        self.services = []
        credentials = pika.PlainCredentials(os.getenv('SB_USER', config.ampq['user']),
                                            os.getenv('SB_PWD', config.ampq['password']))

        parameters = pika.ConnectionParameters(os.getenv('SB_URL', config.ampq['host']),
                                            os.getenv('SB_PORT', config.ampq['port']),
                                            '/',
                                            credentials)

        self.connection = pika.BlockingConnection(parameters)
        self.channel = self.connection.channel()

    def get_service_name(self, handler):
        definition_var = [i for i in handler.__dict__.keys() if i.endswith('pb2')][0]
        definition = getattr(handler, definition_var)

        service_var = [i for i in definition.__dict__.keys() if i.endswith('SERVICE')][0]
        service = getattr(definition, service_var)

        return getattr(service, "full_name")

    def add_service(self, handler):
        self.services.append(self.get_service_name(handler))

    def register(self, port):
        register_as = os.getenv('REGISTER_AS', config.register_as)

        message = {"service": f'{register_as}:{port}' , "handlers": self.services}
        self.channel.queue_declare(queue='discovery', durable=True)
        self.channel.basic_publish(exchange='',
                                   routing_key='discovery',
                                   body=json.dumps(message))

        self.connection.close()
        

