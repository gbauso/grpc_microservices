package usecases

import (
	pb "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/entity"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/repository"
)

type RegisterServiceUseCase struct {
	repo repository.ServiceHandlerRepository
}

func NewRegisterServiceUseCase(repo repository.ServiceHandlerRepository) *RegisterServiceUseCase {
	return &RegisterServiceUseCase{repo: repo}
}

func (uc *RegisterServiceUseCase) Execute(in *pb.RegisterServiceHandlersRequest) (*pb.RegisterServiceHandlersResponse, error) {
	var serviceHandlers []entity.ServiceHandler
	for _, handler := range in.Handlers {
		serviceHandler := entity.NewServiceHandler(in.Service, in.ServiceId, handler)
		serviceHandlers = append(serviceHandlers, *serviceHandler)
	}

	err := uc.repo.Insert(serviceHandlers)
	if err != nil {
		return nil, err
	}

	return &pb.RegisterServiceHandlersResponse{}, nil
}
