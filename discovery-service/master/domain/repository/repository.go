package repository

import "github.com/gbauso/grpc_microservices/discoveryservice/master/domain/entity"

type ServiceHandlerRepository interface {
	Insert(serviceHandlers []entity.ServiceHandler) error
	Update(serviceHandler entity.ServiceHandler) error
	GetByServiceId(serviceid string) (entity.ServiceHandler, error)
	GetAliveServices(service string) ([]string, error)
}
