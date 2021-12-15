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
	var opts []grpc.DialOption
	reflectionConnection, err := grpc.Dial(a.serviceUrl, opts...)
	if err != nil {
		return err
	}
	defer reflectionConnection.Close()

	discoveryConnection, err := grpc.Dial(a.serviceUrl, opts...)
	if err != nil {
		return err
	}
	defer discoveryConnection.Close()

	services, err := a.getImplementedServices(reflectionConnection)
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

	_, err = client.
		RegisterServiceHandlers(context.Background(),
			&discovery.RegisterServiceHandlersRequest{Service: a.serviceName,
				ServiceId: serviceId.String()})

	if err != nil {
		return err
	}

	return nil
}

func (a Agent) getImplementedServices(conn *grpc.ClientConn) (*reflection.ServerReflectionResponse, error) {
	client := reflection.NewServerReflectionClient(conn)
	stream, err := client.ServerReflectionInfo(context.Background())
	if err != nil {
		return nil, err
	}

	waitc := make(chan *reflection.ServerReflectionResponse)

	go func() {
		for {
			response, err := stream.Recv()
			if err == io.EOF {
				waitc <- response
				close(waitc)
				return
			}
		}
	}()

	if err := stream.Send(&reflection.ServerReflectionRequest{Host: a.serviceUrl}); err != nil {
		return nil, err
	}
	stream.CloseSend()

	return <-waitc, nil
}
