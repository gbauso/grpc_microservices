from diagrams import Cluster, Diagram, Edge
from diagrams.programming.language import Go, Csharp, Python, NodeJS, Kotlin
from diagrams.onprem.aggregator import Fluentd
from diagrams.onprem.monitoring import Prometheus
from diagrams.generic.database import SQL
from diagrams.saas.alerting import Newrelic
import types

graph_attrs = {
    "pad": "1.0",
    "splines": "ortho",
    "nodesep": "0.60",
    "ranksep": "0.75",
    "fontname": "Sans-Serif",
    "fontsize": "20",
    "fontcolor": "black",
}

node_attrs = {
    "shape": "box",
    "style": "rounded",
    "fixedsize": "true",
    "width": "1.4",
    "height": "1.4",
    "labelloc": "b",
    "imagescale": "true",
    "fontname": "Sans-Serif",
    "fontsize": "16",
    "fontcolor": "black",
}

with Diagram("", show=True, direction= "RL", graph_attr=graph_attrs, node_attr=node_attrs):
    
    new_relic = Newrelic("LOGGING")

    with Cluster("DISCOVERY MASTER", graph_attr=graph_attrs):
        discovery_master = Go("\n\nDISCOVERY MASTER")
        sqllite = SQL("\n\nSQLITE")
        discovery_master >> sqllite >> discovery_master
        new_relic << Edge(color="darkgreen") << Fluentd("\n\nLOG INGESTION") >> Edge(color="darkgreen", style="dashed") >> discovery_master

    with Cluster("GATEWAY", graph_attr=graph_attrs):
        gateway = Csharp("\n\nGRPC GATEWAY") 
        new_relic << Edge(color="darkgreen") << Fluentd("\n\nLOG INGESTION") >> Edge(color="darkgreen", style="dashed") >> gateway
        gateway >> Edge(color="red", style="dashed") >> discovery_master


    with Cluster("BACKEND", graph_attr=graph_attrs):
        services = [types.SimpleNamespace(name = 'POPULATION', language = Python), 
                    types.SimpleNamespace(name = 'NEARBY CITIES', language = NodeJS), 
                    types.SimpleNamespace(name = 'WEATHER', language = Kotlin)]

        for svc in services:
            with Cluster("\n\n{} SERVICE".format(svc.name), graph_attr=graph_attrs):
                logger = Fluentd("\n\nLOG INGESTION")
                discovery_agent = Go("\n\nDISCOVERY AGENT")
                service = svc.language("\n\nGRPC SERVER")        
                Prometheus("\n\nMETRICS") >> service
                new_relic << Edge(color="darkgreen") << logger >> Edge(color="darkgreen", style="dashed") >> service
                discovery_master << Edge(color="purple", minlen="6") << discovery_agent >> Edge(color="purple", style="dashed") >> service
                service << Edge(color="red", minlen="5") << gateway
                logger >> Edge(color="darkgreen", style="dashed") >> discovery_agent
