package usecases

import (
	"github.com/sirupsen/logrus"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/adapter/client/interfaces"
	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
)

type HandleServicesUseCase struct {
	reflectionClient  interfaces.ReflectionClient
	discoveryClient   interfaces.DiscoveryClient
	healthCheckClient interfaces.HealthCheckClient
	log               *logrus.Logger
}

func NewHandleServicesUseCase(reflectionClient interfaces.ReflectionClient,
	discoveryClient interfaces.DiscoveryClient,
	healthCheckClient interfaces.HealthCheckClient,
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

	quit := make(chan bool)
	defer close(quit)

	go func() {
		for _, svc := range service.Services {
			uc.log.Infof("starting health check on: %s", svc)
			err := uc.healthCheckClient.WatchService(svc, service.Id)
			if err != nil {
				uc.log.Errorf("error when invoke health checking on target service %s: %v", svc, err)
			}
			uc.log.Infof("finished health check on: %s -> unregistering the service", svc)
			uc.discoveryClient.UnRegisterService(service)
			quit <- true
			return
		}
	}()

	<-quit

	return nil
}
