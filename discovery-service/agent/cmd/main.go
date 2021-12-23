package main

import (
	"flag"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/adapter/client"
	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	usecases "github.com/gbauso/grpc_microservices/discoveryservice/agent/use_cases"
	uuid "github.com/nu7hatch/gouuid"
	"google.golang.org/grpc"
)

var (
	masterNodeUrl = flag.String("master-node", "", "The master node url")
	serviceUrl    = flag.String("service-url", "", "The service url")
	service       = flag.String("service", "", "The service name")
)

func main() {
	flag.Parse()
	serviceConn, err := grpc.Dial(*serviceUrl, grpc.WithInsecure())
	if err != nil {
		panic(err)
	}
	defer serviceConn.Close()

	masterConn, err := grpc.Dial(*masterNodeUrl, grpc.WithInsecure())
	if err != nil {
		panic(err)
	}
	defer masterConn.Close()

	id, _ := uuid.NewV4()
	svc := entity.NewService(*serviceUrl, *service, id.String())

	discoveryClient := client.NewDiscoveryClient(masterConn)
	healthCheckClient := client.NewHealthCheckClient(serviceConn)
	reflectionClient := client.NewReflectionClient(serviceConn)

	useCase := usecases.NewHandleServicesUseCase(*reflectionClient, *discoveryClient, *healthCheckClient)
	useCase.Execute(svc)
}
