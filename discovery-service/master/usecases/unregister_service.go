package usecases

import (
	pb "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/entity"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/repository"
)

type UnregisterServiceUseCase struct {
	repo repository.ServiceHandlerRepository
}

func NewUnregisterServiceUseCase(repo repository.ServiceHandlerRepository) *UnregisterServiceUseCase {
	return &UnregisterServiceUseCase{repo: repo}
}

func (uc *UnregisterServiceUseCase) Execute(in *pb.UnregisterServiceRequest) (*pb.UnregisterServiceResponse, error) {
	queryResult, err := uc.repo.GetByServiceId(in.ServiceId)
	if err != nil {
		return nil, err
	}

	var serviceHanders []entity.ServiceHandler
	for _, result := range queryResult {
		result.MarkAsNotAlive()
		serviceHanders = append(serviceHanders, result)
	}

	err = uc.repo.Update(serviceHanders...)
	if err != nil {
		return nil, err
	}

	return &pb.UnregisterServiceResponse{}, nil
}
