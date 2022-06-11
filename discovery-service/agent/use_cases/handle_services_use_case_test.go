package usecases

import (
	"errors"
	"testing"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	"github.com/sirupsen/logrus"
)

type fakeReflectionClient struct {
	getImplementedServicesFn func(svc *entity.Service) ([]string, error)
}

func (rc *fakeReflectionClient) GetImplementedServices(svc *entity.Service) ([]string, error) {
	if rc.getImplementedServicesFn != nil {
		return rc.getImplementedServicesFn(svc)
	}
	return nil, errors.New("fakeReflectionClient was not set up with a response - must set gc.getImplementedServicesFn")
}

type fakeHealthCheckClient struct {
	WatchServiceFn func(service, correlationId string) error
}

func (rc *fakeHealthCheckClient) WatchService(service, correlationId string) error {
	if rc.WatchServiceFn != nil {
		return rc.WatchServiceFn(service, correlationId)
	}
	return errors.New("fakeHealthCheckClient was not set up with a response - must set gc.WatchServiceFn")
}

type fakeDiscoveryClient struct {
	RegisterServiceFn   func(svc *entity.Service) error
	UnRegisterServiceFn func(svc *entity.Service) error
}

func (rc *fakeDiscoveryClient) RegisterService(svc *entity.Service) error {
	if rc.RegisterServiceFn != nil {
		return rc.RegisterServiceFn(svc)
	}
	return errors.New("fakeDiscoveryClient was not set up with a response - must set gc.RegisterServiceFn")
}

func (rc *fakeDiscoveryClient) UnRegisterService(svc *entity.Service) error {
	if rc.UnRegisterServiceFn != nil {
		return rc.UnRegisterServiceFn(svc)
	}
	return errors.New("fakeDiscoveryClient was not set up with a response - must set gc.UnRegisterServiceFn")
}

func Test_HandleServicesUseCase_Execute_WhenHealthCheckReturnsNotServing_ShouldUnregister_AndNotReturnError(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")
	var unregistered bool = false
	implementedServicesFn := func(svc *entity.Service) ([]string, error) {
		return []string{"fake1", "fake2"}, nil
	}

	watchServiceFn := func(service, correlationId string) error {
		return nil
	}

	registerServiceFn := func(svc *entity.Service) error {
		return nil
	}

	unRegisterServiceFn := func(svc *entity.Service) error {
		unregistered = true
		return nil
	}

	rc := &fakeReflectionClient{getImplementedServicesFn: implementedServicesFn}
	hc := &fakeHealthCheckClient{WatchServiceFn: watchServiceFn}
	dc := &fakeDiscoveryClient{RegisterServiceFn: registerServiceFn, UnRegisterServiceFn: unRegisterServiceFn}

	// Act
	useCase := NewHandleServicesUseCase(rc, dc, hc, logrus.New())
	err := useCase.Execute(svc)

	// Assert
	if err != nil || !unregistered {
		t.Fatal(err)
	}
}

func Test_HandleServicesUseCase_Execute_WhenReflectionClientCallFail_ShouldReturnError(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")
	implementedServicesFn := func(svc *entity.Service) ([]string, error) {
		return nil, errors.New("ReflectionClientError")
	}

	rc := &fakeReflectionClient{getImplementedServicesFn: implementedServicesFn}
	hc := &fakeHealthCheckClient{}
	dc := &fakeDiscoveryClient{}

	// Act
	useCase := NewHandleServicesUseCase(rc, dc, hc, logrus.New())
	err := useCase.Execute(svc)

	// Assert
	if err == nil {
		t.Fatal()
	}
}

func Test_HandleServicesUseCase_Execute_WhenDiscoveryClientRegisterServiceCallFail_ShouldReturnError(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")
	implementedServicesFn := func(svc *entity.Service) ([]string, error) {
		return []string{"fake1", "fake2"}, nil
	}

	registerServiceFn := func(svc *entity.Service) error {
		return errors.New("DiscoveryClientRegisterServiceError")
	}

	rc := &fakeReflectionClient{getImplementedServicesFn: implementedServicesFn}
	hc := &fakeHealthCheckClient{}
	dc := &fakeDiscoveryClient{RegisterServiceFn: registerServiceFn}

	// Act
	useCase := NewHandleServicesUseCase(rc, dc, hc, logrus.New())
	err := useCase.Execute(svc)

	// Assert
	if err == nil {
		t.Fatal()
	}
}

func Test_HandleServicesUseCase_Execute_WhenHealthCheckWatchCallFail_ShouldUnregister_AndNotReturnError(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")
	var unregistered bool = false
	implementedServicesFn := func(svc *entity.Service) ([]string, error) {
		return []string{"fake1"}, nil
	}

	watchServiceFn := func(service, correlationId string) error {
		return errors.New("HealthCheckWatchCallError")
	}

	registerServiceFn := func(svc *entity.Service) error {
		unregistered = true
		return nil
	}

	unRegisterServiceFn := func(svc *entity.Service) error {
		return nil
	}

	rc := &fakeReflectionClient{getImplementedServicesFn: implementedServicesFn}
	hc := &fakeHealthCheckClient{WatchServiceFn: watchServiceFn}
	dc := &fakeDiscoveryClient{RegisterServiceFn: registerServiceFn, UnRegisterServiceFn: unRegisterServiceFn}

	// Act
	useCase := NewHandleServicesUseCase(rc, dc, hc, logrus.New())
	err := useCase.Execute(svc)

	// Assert
	if err != nil || !unregistered {
		t.Fatal(err)
	}
}
