package usecases

import (
	"os"
	"os/signal"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/adapter/client"
	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
)

type HandleServicesUseCase struct {
	reflectionClient  client.ReflectionClient
	discoveryClient   client.DiscoveryClient
	healthCheckClient client.HealthCheckClient
}

func NewHandleServicesUseCase(reflectionClient client.ReflectionClient,
	discoveryClient client.DiscoveryClient,
	healthCheckClient client.HealthCheckClient) *HandleServicesUseCase {

	return &HandleServicesUseCase{reflectionClient: reflectionClient,
		discoveryClient:   discoveryClient,
		healthCheckClient: healthCheckClient}
}

func (uc *HandleServicesUseCase) Execute(service *entity.Service) error {
	services, err := uc.reflectionClient.GetImplementedServices(service)

	if err != nil {
		return err
	}

	service.SetServices(services)

	err = uc.discoveryClient.RegisterService(service)
	if err != nil {
		return err
	}

	go func() {
		for _, svc := range service.Services {
			err := uc.healthCheckClient.WatchService(svc)
			if err != nil {
				return
			}
			uc.discoveryClient.UnRegisterService(service)
			os.Exit(1)
		}
	}()

	quit := make(chan os.Signal, 1)
	signal.Notify(quit, os.Interrupt)
	<-quit

	uc.discoveryClient.UnRegisterService(service)

	return nil
}
