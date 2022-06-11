package usecases

import (
	"errors"
	"reflect"
	"testing"

	"github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/repository/stub"
)

func Test_GetAliveServicesUseCase_Execute_Success_ShoudReturnListOfServices(t *testing.T) {
	// Arrange
	fakeReponse := []string{"fake1", "fake2"}
	getAliveServicesFn := func(service string) ([]string, error) {
		return fakeReponse, nil
	}

	repository := &stub.ServiceHandlerRepositoryStub{GetAliveServicesFn: getAliveServicesFn}
	useCase := NewGetAliveServicesUseCase(repository)
	request := &grpc_gen.DiscoverySearchRequest{ServiceDefinition: "svc1"}

	// Act
	response, err := useCase.Execute(request)

	if err != nil || !reflect.DeepEqual(response.Services, fakeReponse) {
		t.Error(err)
	}
}

func Test_GetAliveServicesUseCase_Execute_Fail_ShoudReturnAnError(t *testing.T) {
	// Arrange
	getAliveServicesFn := func(service string) ([]string, error) {
		return nil, errors.New("ErrorOnRepository")
	}

	repository := &stub.ServiceHandlerRepositoryStub{GetAliveServicesFn: getAliveServicesFn}
	useCase := NewGetAliveServicesUseCase(repository)
	request := &grpc_gen.DiscoverySearchRequest{ServiceDefinition: "svc1"}

	// Act
	response, err := useCase.Execute(request)

	if err == nil || response != nil {
		t.Error()
	}
}
