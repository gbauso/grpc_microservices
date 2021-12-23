package client

import (
	"context"
	"io"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	"google.golang.org/grpc"
	reflection "google.golang.org/grpc/reflection/grpc_reflection_v1alpha"
)

type ReflectionClient struct {
	conn *grpc.ClientConn
}

func NewReflectionClient(conn *grpc.ClientConn) *ReflectionClient {
	return &ReflectionClient{conn: conn}
}

func (rc *ReflectionClient) GetImplementedServices(svc *entity.Service) ([]string, error) {

	client := reflection.NewServerReflectionClient(rc.conn)

	stream, err := client.ServerReflectionInfo(context.Background())
	if err != nil {
		return nil, err
	}

	waitc := make(chan *reflection.ServerReflectionResponse)
	defer close(waitc)

	go func() {
		for {
			response, err := stream.Recv()
			if err != io.EOF {
				waitc <- response
			} else {
				return
			}
		}
	}()

	serviceRequest := &reflection.ServerReflectionRequest{Host: rc.conn.Target(), MessageRequest: &reflection.ServerReflectionRequest_ListServices{ListServices: "ls"}}
	if err := stream.Send(serviceRequest); err != nil {
		return nil, err
	}
	stream.CloseSend()
	response := <-waitc

	var services []string
	for _, service := range response.GetListServicesResponse().Service {
		services = append(services, service.GetName())
	}

	return services, nil
}
