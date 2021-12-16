package agent

import (
	"context"
	"io"

	discovery "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	uuid "github.com/nu7hatch/gouuid"
	"google.golang.org/grpc"
	reflection "google.golang.org/grpc/reflection/grpc_reflection_v1alpha"
)

type Agent struct {
	serviceUrl    string
	serviceName   string
	masterNodeUrl string
}

func NewAgent(serviceUrl string, masterNodeUrl string, serviceName string) *Agent {
	return &Agent{serviceUrl: serviceUrl, masterNodeUrl: masterNodeUrl, serviceName: serviceName}
}

func (a Agent) Init() error {
	reflectionConnection, err := grpc.Dial(a.serviceUrl, grpc.WithInsecure())
	if err != nil {
		return err
	}
	defer reflectionConnection.Close()

	discoveryConnection, err := grpc.Dial(a.masterNodeUrl, grpc.WithInsecure())
	if err != nil {
		return err
	}
	defer discoveryConnection.Close()

	serviceRequest := &reflection.ServerReflectionRequest{Host: a.serviceUrl, MessageRequest: &reflection.ServerReflectionRequest_ListServices{ListServices: "ls"}}
	services, err := a.getServerReflection(reflectionConnection, serviceRequest)
	if err != nil {
		return err
	}

	err = a.registerService(discoveryConnection, services)
	if err != nil {
		return err
	}

	return nil
}

func (a Agent) registerService(conn *grpc.ClientConn, reflection *reflection.ServerReflectionResponse) error {
	client := discovery.NewDiscoveryServiceClient(conn)
	serviceId, err := uuid.NewV4()
	if err != nil {
		return err
	}

	var handlers []string
	for _, service := range reflection.GetListServicesResponse().Service {
		handlers = append(handlers, service.GetName())
	}

	_, err = client.
		RegisterServiceHandlers(context.Background(),
			&discovery.RegisterServiceHandlersRequest{Service: a.serviceUrl,
				ServiceId: serviceId.String(), Handlers: handlers})

	if err != nil {
		return err
	}

	return nil
}

func (a Agent) getServerReflection(conn *grpc.ClientConn, req *reflection.ServerReflectionRequest) (*reflection.ServerReflectionResponse, error) {
	client := reflection.NewServerReflectionClient(conn)
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

	if err := stream.Send(req); err != nil {
		return nil, err
	}
	stream.CloseSend()
	response := <-waitc

	return response, nil
}
