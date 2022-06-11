package stub

import (
	"errors"

	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/entity"
)

type ServiceHandlerRepositoryStub struct {
	GetAliveServicesFn func(service string) ([]string, error)
	GetByServiceIdFn   func(serviceId string) ([]entity.ServiceHandler, error)
	UpdateFn           func(serviceHandlers ...entity.ServiceHandler) error
	InsertFn           func(serviceHandlers ...entity.ServiceHandler) error
}

func (r *ServiceHandlerRepositoryStub) Insert(serviceHandlers ...entity.ServiceHandler) error {
	if r.InsertFn != nil {
		return r.InsertFn(serviceHandlers...)
	}

	return errors.New("ServiceHandlerRepositoryStub was not set up with a response - must set gc.InsertFn")
}

func (r *ServiceHandlerRepositoryStub) Update(serviceHandlers ...entity.ServiceHandler) error {
	if r.UpdateFn != nil {
		return r.UpdateFn(serviceHandlers...)
	}

	return errors.New("ServiceHandlerRepositoryStub was not set up with a response - must set gc.UpdateFn")
}

func (r *ServiceHandlerRepositoryStub) GetByServiceId(serviceId string) ([]entity.ServiceHandler, error) {
	if r.GetByServiceIdFn != nil {
		return r.GetByServiceIdFn(serviceId)
	}

	return nil, errors.New("ServiceHandlerRepositoryStub was not set up with a response - must set gc.GetByServiceIdFn")
}

func (r *ServiceHandlerRepositoryStub) GetAliveServices(service string) ([]string, error) {
	if r.GetAliveServicesFn != nil {
		return r.GetAliveServicesFn(service)
	}

	return nil, errors.New("ServiceHandlerRepositoryStub was not set up with a response - must set gc.GetAliveServicesFn")
}
