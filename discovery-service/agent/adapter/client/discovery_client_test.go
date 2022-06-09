package client

import (
	"context"
	"errors"

	discovery "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"google.golang.org/grpc"
)

type FakeDiscoveryClient struct {
	RegisterServiceHandlersFn func(ctx context.Context, in *discovery.RegisterServiceHandlersRequest, opts ...grpc.CallOption) (*discovery.RegisterServiceHandlersResponse, error)
	UnregisterServiceFn       func(ctx context.Context, in *discovery.UnregisterServiceRequest, opts ...grpc.CallOption) (*discovery.UnregisterServiceResponse, error)
}

func (fake *FakeDiscoveryClient) GetServiceHandlers(ctx context.Context, in *discovery.DiscoverySearchRequest, opts ...grpc.CallOption) (*discovery.DiscoverySearchResponse, error) {
	return nil, nil
}

func (fake *FakeDiscoveryClient) RegisterServiceHandlers(ctx context.Context, in *discovery.RegisterServiceHandlersRequest, opts ...grpc.CallOption) (*discovery.RegisterServiceHandlersResponse, error) {
	if fake.RegisterServiceHandlersFn != nil {
		return fake.RegisterServiceHandlersFn(ctx, in, opts...)
	}

	return nil, errors.New("FakeDiscoveryClient was not set up with a response - must set fake.RegisterServiceHandlersFn")
}

func (fake *FakeDiscoveryClient) UnregisterService(ctx context.Context, in *discovery.UnregisterServiceRequest, opts ...grpc.CallOption) (*discovery.UnregisterServiceResponse, error) {
	if fake.UnregisterServiceFn != nil {
		return fake.UnregisterServiceFn(ctx, in, opts...)
	}

	return nil, errors.New("FakeDiscoveryClient was not set up with a response - must set fake.UnregisterServiceFn")
}
