package client

import (
	"context"
	"errors"
	"testing"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	"google.golang.org/grpc"
	reflection "google.golang.org/grpc/reflection/grpc_reflection_v1alpha"
)

type FakeServerReflectionClient struct {
	SendFn func(*reflection.ServerReflectionRequest) error
	RecvFn func() (*reflection.ServerReflectionResponse, error)
	grpc.ClientStream
}

func (fake *FakeServerReflectionClient) ServerReflectionInfo(ctx context.Context, opts ...grpc.CallOption) (reflection.ServerReflection_ServerReflectionInfoClient, error) {
	return fake, nil
}

func (fake *FakeServerReflectionClient) Send(req *reflection.ServerReflectionRequest) error {
	if fake.SendFn != nil {
		return fake.SendFn(req)
	}

	return errors.New("FakeServerReflectionClient was not set up with a response - must set fake.SendFn")
}

func (fake *FakeServerReflectionClient) CloseSend() error {
	return nil
}

func (fake *FakeServerReflectionClient) Recv() (*reflection.ServerReflectionResponse, error) {
	if fake.SendFn != nil {
		return fake.RecvFn()
	}

	return nil, errors.New("FakeServerReflectionClient was not set up with a response - must set fake.RecvFn")
}

func Test_ReflectionClient_GetImplementedServices_SendAndReceiveStreamSuccesful_ShouldReturnServices(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")
	var requestSent bool = false

	sendFn := func(request *reflection.ServerReflectionRequest) error {
		requestSent = true
		return nil
	}

	recvFn := func() (*reflection.ServerReflectionResponse, error) {
		if requestSent {
			return &reflection.ServerReflectionResponse{
				ValidHost:       "",
				OriginalRequest: &reflection.ServerReflectionRequest{},
				MessageResponse: &reflection.ServerReflectionResponse_ListServicesResponse{ListServicesResponse: &reflection.ListServiceResponse{Service: []*reflection.ServiceResponse{&reflection.ServiceResponse{Name: "fake1"}, &reflection.ServiceResponse{Name: "fake2"}}}},
			}, nil
		}

		return nil, errors.New("No message to be received")
	}

	fake := &FakeServerReflectionClient{SendFn: sendFn, RecvFn: recvFn}
	reflectionClient := NewReflectionClient(fake)

	services, err := reflectionClient.GetImplementedServices(svc)

	if services == nil || err != nil {
		t.Fail()
	}
}

func Test_ReflectionClient_GetImplementedServices_FailedStreamSending_ShouldReturnError_AndNilServices(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")

	sendFn := func(request *reflection.ServerReflectionRequest) error {
		return errors.New("StreamSendingError")
	}

	fake := &FakeServerReflectionClient{SendFn: sendFn}
	reflectionClient := NewReflectionClient(fake)

	svcs, err := reflectionClient.GetImplementedServices(svc)

	if err == nil || svcs != nil {
		t.Fail()
	}
}

func Test_ReflectionClient_GetImplementedServices_FailedStreamReceiving_ShouldReturnError_AndNilServices(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")

	sendFn := func(request *reflection.ServerReflectionRequest) error {
		return nil
	}

	recvFn := func() (*reflection.ServerReflectionResponse, error) {
		return nil, errors.New("No message to be received")
	}

	fake := &FakeServerReflectionClient{SendFn: sendFn, RecvFn: recvFn}
	reflectionClient := NewReflectionClient(fake)

	svcs, err := reflectionClient.GetImplementedServices(svc)

	if err == nil || svcs != nil {
		t.Fail()
	}
}
