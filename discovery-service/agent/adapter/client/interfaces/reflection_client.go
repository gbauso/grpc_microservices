package interfaces

import "github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"

type ReflectionClient interface {
	GetImplementedServices(svc *entity.Service) ([]string, error)
}
