import pika
import json
import os

class AutoDiscovery:
    def __init__(self):
        self.services = []
        protocol = "amqps" if bool(os.getenv("SB_SSL")) else "amqp"

        uri = "{}://{}:{}@{}:{}{}".format(protocol, os.getenv('SB_USER'), os.getenv('SB_PWD'), os.getenv('SB_URL'), os.getenv('SB_PORT'), '/')

        self.connection = pika.BlockingConnection(uri)
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
        register_as = os.getenv('REGISTER_AS')

        message = {"service": f'{register_as}:{port}' , "handlers": self.services}
        self.channel.queue_declare(queue='discovery', durable=True)
        self.channel.basic_publish(exchange='',
                                   routing_key='discovery',
                                   body=json.dumps(message))

        self.connection.close()
        

