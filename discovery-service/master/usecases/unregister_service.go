package usecases

import (
	pb "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/repository"
)

type UnregisterServiceUseCase struct {
	repo repository.ServiceHandlerRepository
}

func NewUnregisterServiceUseCase(repo repository.ServiceHandlerRepository) *UnregisterServiceUseCase {
	return &UnregisterServiceUseCase{repo: repo}
}

func (uc *UnregisterServiceUseCase) Execute(in *pb.UnregisterServiceRequest) (*pb.UnregisterServiceResponse, error) {
	serviceHander, err := uc.repo.GetByServiceId(in.ServiceId)
	if err != nil {
		return nil, err
	}

	serviceHander.MarkAsNotAlive()

	err = uc.repo.Update(serviceHander)
	if err != nil {
		return nil, err
	}

	return &pb.UnregisterServiceResponse{}, nil
}
