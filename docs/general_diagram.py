from turtle import width
from diagrams import Cluster, Diagram, Edge
from diagrams.programming.language import Go, Csharp, Python, NodeJS, Kotlin
from diagrams.onprem.aggregator import Fluentd
from diagrams.onprem.monitoring import Prometheus
from diagrams.generic.database import SQL
from diagrams.saas.alerting import Newrelic
from diagrams.onprem.client import Client
import types


with Diagram("", show=True, direction= "RL"):
    
    new_relic = Newrelic("Logging")

    with Cluster("Discovery Master"):
        discovery_master = Go("Discovery Master")
        sqllite = SQL("Sqlite")
        discovery_master >> sqllite >> discovery_master
        new_relic << Edge(color="darkgreen") << Fluentd("Log Ingestion") >> Edge(color="darkgreen", style="dashed") >> discovery_master

    with Cluster("Gateway"):
        gateway = Csharp("GRPC Gateway") 
        new_relic << Edge(color="darkgreen") << Fluentd("Log Ingestion") >> Edge(color="darkgreen", style="dashed") >> gateway
        gateway >> Edge(color="red", style="dashed") >> discovery_master


    with Cluster("Backend"):
        services = [types.SimpleNamespace(name = 'Population', language = Python), 
                    types.SimpleNamespace(name = 'Nearby Cities', language = NodeJS), 
                    types.SimpleNamespace(name = 'Weather', language = Kotlin)]

        for svc in services:
            with Cluster("{} Service".format(svc.name)):
                logger = Fluentd("Log Ingestion")
                discovery_agent = Go("Discovery Agent")
                service = svc.language("Grpc Server")        
                Prometheus("Metrics") >> service
                new_relic << Edge(color="darkgreen") << logger >> Edge(color="darkgreen", style="dashed") >> service
                discovery_master << Edge(color="purple", minlen="6") << discovery_agent >> Edge(color="purple", style="dashed") >> service
                service << Edge(color="red", minlen="5") << gateway
                logger >> Edge(color="darkgreen", style="dashed") >> discovery_agent
