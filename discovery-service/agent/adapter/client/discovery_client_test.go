package client

import (
	"context"
	"errors"
	"testing"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent/domain/entity"
	discovery "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"google.golang.org/grpc"
)

type fakeDiscoveryClient struct {
	registerServiceHandlersFn func(ctx context.Context, in *discovery.RegisterServiceHandlersRequest, opts ...grpc.CallOption) (*discovery.RegisterServiceHandlersResponse, error)
	unregisterServiceFn       func(ctx context.Context, in *discovery.UnregisterServiceRequest, opts ...grpc.CallOption) (*discovery.UnregisterServiceResponse, error)
}

func (fake *fakeDiscoveryClient) GetServiceHandlers(ctx context.Context, in *discovery.DiscoverySearchRequest, opts ...grpc.CallOption) (*discovery.DiscoverySearchResponse, error) {
	return nil, nil
}

func (fake *fakeDiscoveryClient) RegisterServiceHandlers(ctx context.Context, in *discovery.RegisterServiceHandlersRequest, opts ...grpc.CallOption) (*discovery.RegisterServiceHandlersResponse, error) {
	if fake.registerServiceHandlersFn != nil {
		return fake.registerServiceHandlersFn(ctx, in, opts...)
	}

	return nil, errors.New("fakeDiscoveryClient was not set up with a response - must set fake.registerServiceHandlersFn")
}

func (fake *fakeDiscoveryClient) UnregisterService(ctx context.Context, in *discovery.UnregisterServiceRequest, opts ...grpc.CallOption) (*discovery.UnregisterServiceResponse, error) {
	if fake.unregisterServiceFn != nil {
		return fake.unregisterServiceFn(ctx, in, opts...)
	}

	return nil, errors.New("fakeDiscoveryClient was not set up with a response - must set fake.unregisterServiceFn")
}

func Test_DiscoveryClient_RegisterService_Success_ShouldNotReturnError(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")
	svc.SetServices([]string{"fake1", "fake2"})

	registerServiceHandlersFn := func(ctx context.Context, in *discovery.RegisterServiceHandlersRequest, opts ...grpc.CallOption) (*discovery.RegisterServiceHandlersResponse, error) {
		return &discovery.RegisterServiceHandlersResponse{}, nil
	}

	fake := &fakeDiscoveryClient{registerServiceHandlersFn: registerServiceHandlersFn}
	discoveryClient := NewDiscoveryClient(fake)

	// Act
	err := discoveryClient.RegisterService(svc)

	// Assert

	if err != nil {
		t.Fatal(err)
	}
}

func Test_DiscoveryClient_RegisterService_Fail_ShouldReturnError(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")
	svc.SetServices([]string{"fake1", "fake2"})

	registerServiceHandlersFn := func(ctx context.Context, in *discovery.RegisterServiceHandlersRequest, opts ...grpc.CallOption) (*discovery.RegisterServiceHandlersResponse, error) {
		return nil, errors.New("Failed")
	}

	fake := &fakeDiscoveryClient{registerServiceHandlersFn: registerServiceHandlersFn}
	discoveryClient := NewDiscoveryClient(fake)

	// Act
	err := discoveryClient.RegisterService(svc)

	// Assert

	if err == nil {
		t.Fatal()
	}
}

func Test_DiscoveryClient_UnRegisterService_Success_ShouldNotReturnError(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")
	svc.SetServices([]string{"fake1", "fake2"})

	unregisterServiceFn := func(ctx context.Context, in *discovery.UnregisterServiceRequest, opts ...grpc.CallOption) (*discovery.UnregisterServiceResponse, error) {
		return &discovery.UnregisterServiceResponse{}, nil
	}

	fake := &fakeDiscoveryClient{unregisterServiceFn: unregisterServiceFn}
	discoveryClient := NewDiscoveryClient(fake)

	// Act
	err := discoveryClient.UnRegisterService(svc)

	// Assert

	if err != nil {
		t.Fatal(err)
	}
}

func Test_DiscoveryClient_UnRegisterService_Fail_ShouldReturnError(t *testing.T) {
	// Arrange
	svc := entity.NewService("localhost:80", "fake", "id")
	svc.SetServices([]string{"fake1", "fake2"})

	unregisterServiceFn := func(ctx context.Context, in *discovery.UnregisterServiceRequest, opts ...grpc.CallOption) (*discovery.UnregisterServiceResponse, error) {
		return nil, errors.New("Failed")
	}

	fake := &fakeDiscoveryClient{unregisterServiceFn: unregisterServiceFn}
	discoveryClient := NewDiscoveryClient(fake)

	// Act
	err := discoveryClient.UnRegisterService(svc)

	// Assert

	if err == nil {
		t.Fatal()
	}
}
