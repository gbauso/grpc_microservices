package main

import (
	"flag"
	"fmt"
	"io"
	"os"

	logger "github.com/sirupsen/logrus"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/adapter/client"
	"github.com/gbauso/grpc_microservices/discoveryservice/agent/adapter/client/interceptors"
	"github.com/gbauso/grpc_microservices/discoveryservice/agent/adapter/client/interfaces"
	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	usecases "github.com/gbauso/grpc_microservices/discoveryservice/agent/use_cases"
	discovery "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	uuid "github.com/nu7hatch/gouuid"
	"google.golang.org/grpc"
	hc "google.golang.org/grpc/health/grpc_health_v1"
	reflection "google.golang.org/grpc/reflection/grpc_reflection_v1alpha"
)

var (
	masterNodeUrl = flag.String("master-node", "", "The master node url")
	serviceUrl    = flag.String("service-url", "", "The service url")
	service       = flag.String("service", "", "The service name")
	logPath       = flag.String("log-path", "/tmp/discovery_agent-%.log", "Log path")
)

func main() {
	log := logger.New()
	id, _ := uuid.NewV4()
	log.SetFormatter(&logger.JSONFormatter{})
	flag.Parse()
	fileName := fmt.Sprintf(*logPath, id)
	f, err := os.OpenFile(fileName, os.O_RDWR|os.O_CREATE|os.O_APPEND, 0666)
	if err != nil {
		log.Errorf("error opening file: %v", err)
		panic(err)
	}
	defer f.Close()
	wrt := io.MultiWriter(os.Stdout, f)

	log.SetOutput(wrt)

	correlationInterceptor := interceptors.NewCorrelationInterceptor()

	serviceConn, err := grpc.Dial(*serviceUrl, grpc.WithInsecure(), grpc.WithUnaryInterceptor(correlationInterceptor.ClientInterceptor))
	if err != nil {
		log.Errorf("error when connect to %s: %v", *serviceUrl, err)
		panic(err)
	}
	defer serviceConn.Close()

	masterConn, err := grpc.Dial(*masterNodeUrl, grpc.WithInsecure(), grpc.WithUnaryInterceptor(correlationInterceptor.ClientInterceptor))
	if err != nil {
		log.Errorf("error when connect to %s: %v", *masterNodeUrl, err)
		panic(err)
	}
	defer masterConn.Close()

	svc := entity.NewService(*serviceUrl, *service, id.String())

	discoveryGrpcClient := discovery.NewDiscoveryServiceClient(masterConn)
	healthCheckGrpcClient := hc.NewHealthClient(serviceConn)
	reflectionGrpcClient := reflection.NewServerReflectionClient(serviceConn)

	var discoveryClient interfaces.DiscoveryClient = client.NewDiscoveryClient(discoveryGrpcClient)
	var healthCheckClient interfaces.HealthCheckClient = client.NewHealthCheckClient(healthCheckGrpcClient, log)
	var reflectionClient interfaces.ReflectionClient = client.NewReflectionClient(reflectionGrpcClient)

	useCase := usecases.NewHandleServicesUseCase(reflectionClient, discoveryClient, healthCheckClient, log)
	useCase.Execute(svc)
}
