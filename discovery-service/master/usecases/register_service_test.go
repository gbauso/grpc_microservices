package usecases

import (
	"errors"
	"testing"

	"github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/repository/stub"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/entity"
)

func Test_RegisterServiceUseCase_Execute_Success_ShoudReturnListOfServices(t *testing.T) {
	// Arrange
	insertFn := func(serviceHandlers ...entity.ServiceHandler) error {
		return nil
	}

	repository := &stub.ServiceHandlerRepositoryStub{InsertFn: insertFn}
	useCase := NewRegisterServiceUseCase(repository)
	request := &grpc_gen.RegisterServiceHandlersRequest{Service: "svc", ServiceId: "12", Handlers: []string{"host1"}}

	// Act
	response, err := useCase.Execute(request)

	if err != nil || response == nil {
		t.Error(err)
	}
}

func Test_RegisterServiceUseCase_Execute_Fail_ShoudReturnAnError(t *testing.T) {
	// Arrange
	insertFn := func(serviceHandlers ...entity.ServiceHandler) error {
		return errors.New("ErrorOnRepository")
	}

	repository := &stub.ServiceHandlerRepositoryStub{InsertFn: insertFn}
	useCase := NewRegisterServiceUseCase(repository)
	request := &grpc_gen.RegisterServiceHandlersRequest{Service: "svc", ServiceId: "12", Handlers: []string{"host1"}}

	// Act
	response, err := useCase.Execute(request)

	if err == nil || response != nil {
		t.Error()
	}
}
