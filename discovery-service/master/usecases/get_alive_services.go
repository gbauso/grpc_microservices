package usecases

import (
	pb "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/repository"
)

type GetAliveServicesUseCase struct {
	repo repository.ServiceHandlerRepository
}

func NewGetAliveServicesUseCase(repo repository.ServiceHandlerRepository) *GetAliveServicesUseCase {
	return &GetAliveServicesUseCase{repo: repo}
}

func (uc *GetAliveServicesUseCase) Execute(in *pb.DiscoverySearchRequest) (*pb.DiscoverySearchResponse, error) {
	services, err := uc.repo.GetAliveServices(in.ServiceDefinition)
	if err != nil {
		return nil, err
	}

	return &pb.DiscoverySearchResponse{Services: services}, nil
}
