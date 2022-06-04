package factory

import "github.com/gbauso/grpc_microservices/discoveryservice/master/domain/repository"

type RepositoryFactory interface {
	CreateServiceHandlerRepository() repository.ServiceHandlerRepository
}
