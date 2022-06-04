package client

import (
	"context"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	discovery "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"google.golang.org/grpc"
)

type DiscoveryClient struct {
	conn *grpc.ClientConn
}

func NewDiscoveryClient(conn *grpc.ClientConn) *DiscoveryClient {
	return &DiscoveryClient{conn: conn}
}

func (dc *DiscoveryClient) RegisterService(svc *entity.Service) error {
	client := discovery.NewDiscoveryServiceClient(dc.conn)

	_, err := client.
		RegisterServiceHandlers(context.Background(),
			&discovery.RegisterServiceHandlersRequest{Service: svc.Url,
				ServiceId: svc.Id, Handlers: svc.Services})

	if err != nil {
		return err
	}

	return nil
}

func (dc *DiscoveryClient) UnRegisterService(svc *entity.Service) error {
	client := discovery.NewDiscoveryServiceClient(dc.conn)

	_, err := client.
		UnregisterService(context.Background(),
			&discovery.UnregisterServiceRequest{ServiceId: svc.Id})

	if err != nil {
		return err
	}

	return nil
}
