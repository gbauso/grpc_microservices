package client

import (
	"context"
	"io"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	"google.golang.org/grpc/metadata"
	reflection "google.golang.org/grpc/reflection/grpc_reflection_v1alpha"
)

type ReflectionClient struct {
	grpc reflection.ServerReflectionClient
}

func NewReflectionClient(grpc reflection.ServerReflectionClient) *ReflectionClient {
	return &ReflectionClient{grpc: grpc}
}

func (rc *ReflectionClient) GetImplementedServices(svc *entity.Service) ([]string, error) {

	ctx := metadata.AppendToOutgoingContext(context.Background(), "correlation_id", svc.Id)

	stream, err := rc.grpc.ServerReflectionInfo(ctx)
	if err != nil {
		return nil, err
	}

	waitc := make(chan *reflection.ServerReflectionResponse)
	streamErrorC := make(chan error)
	defer close(waitc)
	defer close(streamErrorC)

	serviceRequest := &reflection.ServerReflectionRequest{Host: svc.Url, MessageRequest: &reflection.ServerReflectionRequest_ListServices{ListServices: "ls"}}
	if err := stream.Send(serviceRequest); err != nil {
		return nil, err
	}

	go func() {
		for {
			response, err := stream.Recv()
			if err != io.EOF {
				waitc <- response
				streamErrorC <- err
				return
			}

		}
	}()

	stream.CloseSend()
	response := <-waitc
	streamError := <-streamErrorC

	if response == nil {
		return nil, streamError
	}

	var services []string
	for _, service := range response.GetListServicesResponse().Service {
		services = append(services, service.GetName())
	}

	return services, nil
}
