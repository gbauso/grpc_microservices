package client

import (
	"context"
	"errors"
	"testing"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	"google.golang.org/grpc"
	reflection "google.golang.org/grpc/reflection/grpc_reflection_v1alpha"
)

type fakeServerReflectionClient struct {
	sendFn func(*reflection.ServerReflectionRequest) error
	recvFn func() (*reflection.ServerReflectionResponse, error)
	grpc.ClientStream
}

func (fake *fakeServerReflectionClient) ServerReflectionInfo(ctx context.Context, opts ...grpc.CallOption) (reflection.ServerReflection_ServerReflectionInfoClient, error) {
	return fake, nil
}

func (fake *fakeServerReflectionClient) Send(req *reflection.ServerReflectionRequest) error {
	if fake.sendFn != nil {
		return fake.sendFn(req)
	}

	return errors.New("fakeServerReflectionClient was not set up with a response - must set fake.sendFn")
}

func (fake *fakeServerReflectionClient) CloseSend() error {
	return nil
}

func (fake *fakeServerReflectionClient) Recv() (*reflection.ServerReflectionResponse, error) {
	if fake.sendFn != nil {
		return fake.recvFn()
	}

	return nil, errors.New("fakeServerReflectionClient was not set up with a response - must set fake.recvFn")
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
				MessageResponse: &reflection.ServerReflectionResponse_ListServicesResponse{
					ListServicesResponse: &reflection.ListServiceResponse{
						Service: []*reflection.ServiceResponse{
							{Name: "fake1"},
							{Name: "fake2"},
						},
					},
				},
			}, nil
		}

		return nil, errors.New("StreamReceivingError")
	}

	fake := &fakeServerReflectionClient{sendFn: sendFn, recvFn: recvFn}
	reflectionClient := NewReflectionClient(fake)

	services, err := reflectionClient.GetImplementedServices(svc)

	if services == nil || err != nil {
		t.Fatal(err)
	}
}

func Test_ReflectionClient_GetImplementedServices_FailedStreamSending_ShouldReturnError_AndNilServices(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")

	sendFn := func(request *reflection.ServerReflectionRequest) error {
		return errors.New("StreamSendingError")
	}

	fake := &fakeServerReflectionClient{sendFn: sendFn}
	reflectionClient := NewReflectionClient(fake)

	svcs, err := reflectionClient.GetImplementedServices(svc)

	if err == nil || svcs != nil {
		t.Fatal()
	}
}

func Test_ReflectionClient_GetImplementedServices_FailedStreamReceiving_ShouldReturnError_AndNilServices(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")

	sendFn := func(request *reflection.ServerReflectionRequest) error {
		return nil
	}

	recvFn := func() (*reflection.ServerReflectionResponse, error) {
		return nil, errors.New("StreamReceivingError")
	}

	fake := &fakeServerReflectionClient{sendFn: sendFn, recvFn: recvFn}
	reflectionClient := NewReflectionClient(fake)

	svcs, err := reflectionClient.GetImplementedServices(svc)

	if err == nil || svcs != nil {
		t.Fatal()
	}
}
