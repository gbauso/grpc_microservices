package usecases

import (
	"errors"
	"testing"

	"github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/repository/stub"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/entity"
)

func Test_UnregisterServiceUseCase_Execute_Success_ShoudReturnListOfServices(t *testing.T) {
	// Arrange
	getServiceByIdFn := func(serviceId string) ([]entity.ServiceHandler, error) {
		return []entity.ServiceHandler{}, nil
	}

	updateFn := func(serviceHandlers ...entity.ServiceHandler) error {
		return nil
	}

	repository := &stub.ServiceHandlerRepositoryStub{UpdateFn: updateFn, GetByServiceIdFn: getServiceByIdFn}
	useCase := NewUnregisterServiceUseCase(repository)
	request := &grpc_gen.UnregisterServiceRequest{ServiceId: "12"}

	// Act
	response, err := useCase.Execute(request)

	if err != nil || response == nil {
		t.Error(err)
	}
}

func Test_UnregisterServiceUseCase_Execute_Fail_ShoudReturnAnError(t *testing.T) {
	// Arrange
	getServiceByIdFn := func(serviceId string) ([]entity.ServiceHandler, error) {
		return nil, errors.New("ErrorOnRepository")
	}

	updateFn := func(serviceHandlers ...entity.ServiceHandler) error {
		return errors.New("ErrorOnRepository")
	}

	repository := &stub.ServiceHandlerRepositoryStub{UpdateFn: updateFn, GetByServiceIdFn: getServiceByIdFn}
	useCase := NewUnregisterServiceUseCase(repository)
	request := &grpc_gen.UnregisterServiceRequest{ServiceId: "12"}

	// Act
	response, err := useCase.Execute(request)

	if err == nil || response != nil {
		t.Error()
	}
}
