package usecases

import (
	"os"
	"os/signal"

	"github.com/sirupsen/logrus"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/adapter/client"
	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
)

type HandleServicesUseCase struct {
	reflectionClient  client.ReflectionClient
	discoveryClient   client.DiscoveryClient
	healthCheckClient client.HealthCheckClient
	log               *logrus.Logger
}

func NewHandleServicesUseCase(reflectionClient client.ReflectionClient,
	discoveryClient client.DiscoveryClient,
	healthCheckClient client.HealthCheckClient,
	log *logrus.Logger) *HandleServicesUseCase {

	return &HandleServicesUseCase{reflectionClient: reflectionClient,
		discoveryClient:   discoveryClient,
		healthCheckClient: healthCheckClient,
		log:               log}
}

func (uc *HandleServicesUseCase) Execute(service *entity.Service) error {
	uc.log.Infof("invoking reflection method on target service: %s", service.Name)
	services, err := uc.reflectionClient.GetImplementedServices(service)

	if err != nil {
		uc.log.Errorf("error when called reflection method on target service: %v", err)
		return err
	}

	service.SetServices(services)

	uc.log.Infof("registering implemented services on master node: %v", services)
	err = uc.discoveryClient.RegisterService(service)
	if err != nil {
		uc.log.Errorf("error when register services on master node: %v", err)
		return err
	}

	go func() {
		for _, svc := range service.Services {
			uc.log.Infof("starting health check on: %s", svc)
			err := uc.healthCheckClient.WatchService(svc, service.Id)
			if err != nil {
				uc.log.Errorf("error when invoke health checking on target service: %v", err)
				return
			}
			uc.log.Infof("finished health check on: %s -> unregistering the service", svc)
			uc.discoveryClient.UnRegisterService(service)
			os.Exit(1)
		}
	}()

	quit := make(chan os.Signal, 1)
	signal.Notify(quit, os.Interrupt)
	<-quit

	uc.log.Infof("OS exiting sinal received, unregistering the service")
	uc.discoveryClient.UnRegisterService(service)

	return nil
}
