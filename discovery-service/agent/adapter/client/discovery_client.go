package client

import (
	"context"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	discovery "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
)

type DiscoveryClient struct {
	grpc discovery.DiscoveryServiceClient
}

func NewDiscoveryClient(grpc discovery.DiscoveryServiceClient) *DiscoveryClient {
	return &DiscoveryClient{grpc: grpc}
}

func (dc *DiscoveryClient) RegisterService(svc *entity.Service) error {
	_, err := dc.grpc.
		RegisterServiceHandlers(context.Background(),
			&discovery.RegisterServiceHandlersRequest{Service: svc.Url,
				ServiceId: svc.Id, Handlers: svc.Services})

	return err
}

func (dc *DiscoveryClient) UnRegisterService(svc *entity.Service) error {
	_, err := dc.grpc.
		UnregisterService(context.Background(),
			&discovery.UnregisterServiceRequest{ServiceId: svc.Id})

	return err
}
