from turtle import width
from diagrams import Cluster, Diagram, Edge
from diagrams.programming.language import Go, Csharp, Python, NodeJS, Kotlin
from diagrams.onprem.aggregator import Fluentd
from diagrams.onprem.monitoring import Prometheus
from diagrams.generic.database import SQL
from diagrams.saas.alerting import Newrelic
from diagrams.onprem.client import Client
import types

graph_attr = {
    "bgcolor": "transparent"
}

with Diagram("Service", show=True, direction= "TB", graph_attr=graph_attr):
    
    new_relic = Newrelic("Logging")

    with Cluster("Discovery Master"):
        discovery_master = Go("Discovery Master")
        sqllite = SQL("Sqlite")
        discovery_master >> sqllite >> discovery_master
        new_relic << Edge(color="darkgreen", style="dashed") << Fluentd("Log Ingestion") >> Edge(color="darkgreen") >> discovery_master

    with Cluster("Gateway"):
        gateway = Csharp("GRPC Gateway") 
        Prometheus("Metrics") >> gateway
        new_relic << Edge(color="darkgreen", style="dashed") << Fluentd("Log Ingestion") >> Edge(color="darkgreen") >> gateway
        gateway >> Edge(color="red", style="bold") >> discovery_master


    with Cluster("Backend"):
        services = [types.SimpleNamespace(name = 'Population', language = Python), 
                    types.SimpleNamespace(name = 'Nearby Cities', language = NodeJS), 
                    types.SimpleNamespace(name = 'Weather', language = Kotlin)]

        for svc in services:
            with Cluster("{} Service".format(svc.name)):
                service = svc.language("Grpc Server")        
                Prometheus("Metrics") >> service
                new_relic << Edge(color="darkgreen", style="dashed") << Fluentd("Log Ingestion") >> Edge(color="darkgreen") >> service
                discovery_master << Edge(color="purple", style="dashed") << Go("Discovery Agent") >> Edge(color="purple") >> service
                service << Edge(color="red", style="dashed") << gateway
