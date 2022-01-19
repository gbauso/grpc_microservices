package main

import (
	"flag"
	"io"
	"os"

	logger "github.com/sirupsen/logrus"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/adapter/client"
	"github.com/gbauso/grpc_microservices/discoveryservice/agent/adapter/client/interceptors"
	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	usecases "github.com/gbauso/grpc_microservices/discoveryservice/agent/use_cases"
	uuid "github.com/nu7hatch/gouuid"
	"google.golang.org/grpc"
)

var (
	masterNodeUrl = flag.String("master-node", "", "The master node url")
	serviceUrl    = flag.String("service-url", "", "The service url")
	service       = flag.String("service", "", "The service name")
	logPath       = flag.String("log-path", "/tmp/discovery_agent.log", "Log path")
)

func main() {
	log := logger.New()
	log.SetFormatter(&logger.JSONFormatter{})
	flag.Parse()
	f, err := os.OpenFile(*logPath, os.O_RDWR|os.O_CREATE|os.O_APPEND, 0666)
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

	id, _ := uuid.NewV4()
	svc := entity.NewService(*serviceUrl, *service, id.String())

	discoveryClient := client.NewDiscoveryClient(masterConn)
	healthCheckClient := client.NewHealthCheckClient(serviceConn, log)
	reflectionClient := client.NewReflectionClient(serviceConn)

	useCase := usecases.NewHandleServicesUseCase(*reflectionClient, *discoveryClient, *healthCheckClient, log)
	useCase.Execute(svc)
}
