from diagrams import Cluster, Diagram
from diagrams.programming.language import Go, Csharp, Python, NodeJS, Kotlin
from diagrams.onprem.aggregator import Fluentd
from diagrams.onprem.monitoring import Prometheus
from diagrams.generic.database import SQL
from diagrams.saas.alerting import Newrelic

with Diagram("Service", show=True):
    gateway = Csharp("GRPC Gateway") 
    discovery_master = Go("Discovery Master")
    sqllite = SQL("Sqlite")
    new_relic = Newrelic("Logging")
    
    with Cluster("Population Service"):
        population = Python("Population")
        Prometheus("Metrics") >> population
        new_relic << Fluentd("Log Ingestion") >> population
        discovery_master << Go("Discovery Agent") >> population

    with Cluster("Nearby Cities Service"):
        nearby_cities = NodeJS("Nearby Cities")
        Prometheus("Metrics") >> nearby_cities
        new_relic << Fluentd("Log Ingestion") >> nearby_cities
        discovery_master << Go("Discovery Agent") >> nearby_cities

    with Cluster("Weather Service"):
        weather = Kotlin("Weather")
        Prometheus("Metrics") >> weather
        new_relic << Fluentd("Log Ingestion") >> weather
        discovery_master << Go("Discovery Agent") >> weather
    
    with Cluster("Backend"):
        [weather, nearby_cities, population] << gateway

    gateway >> discovery_master >> gateway 
    discovery_master >> sqllite >> discovery_master