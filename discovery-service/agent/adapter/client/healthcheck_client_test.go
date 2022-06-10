package client

import (
	"context"
	"errors"
	"testing"

	"github.com/sirupsen/logrus"
	"google.golang.org/grpc"
	hc "google.golang.org/grpc/health/grpc_health_v1"
)

type fakeHealthCheckClient struct {
	recvFn func() (*hc.HealthCheckResponse, error)
	grpc.ClientStream
}

func (fake *fakeHealthCheckClient) Watch(ctx context.Context, in *hc.HealthCheckRequest, opts ...grpc.CallOption) (hc.Health_WatchClient, error) {
	if in.Service == "error" {
		return nil, errors.New("StreamSendingError")
	}

	return fake, nil
}

func (fake *fakeHealthCheckClient) Check(ctx context.Context, in *hc.HealthCheckRequest, opts ...grpc.CallOption) (*hc.HealthCheckResponse, error) {
	return nil, nil
}

func (fake *fakeHealthCheckClient) Recv() (*hc.HealthCheckResponse, error) {
	if fake.recvFn != nil {
		return fake.recvFn()
	}

	return nil, errors.New("fakeHealthCheckClient was not set up with a response - must set fake.recvFn")
}

func Test_HealthCheckClient_WatchService_SendAndReceiveStreamSuccesfu_ServiceNotServing_ShouldNotReturnError(t *testing.T) {
	// Arrange
	recvFn := func() (*hc.HealthCheckResponse, error) {
		return &hc.HealthCheckResponse{Status: hc.HealthCheckResponse_NOT_SERVING}, nil
	}

	fake := &fakeHealthCheckClient{recvFn: recvFn}
	healthCheckClient := NewHealthCheckClient(fake, logrus.New())

	// Act
	err := healthCheckClient.WatchService("fake1", "correlation")

	// Assert
	if err != nil {
		t.Fatal(err)
	}
}

func Test_HealthCheckClient_WatchService_FailedStreamSending_ShouldReturnError(t *testing.T) {
	// Arrange
	fake := &fakeHealthCheckClient{}
	healthCheckClient := NewHealthCheckClient(fake, logrus.New())

	// Act
	err := healthCheckClient.WatchService("error", "correlation")

	// Assert
	if err == nil {
		t.Fatal()
	}
}

func Test_HealthCheckClient_WatchService_FailedStreamReceiving_ShouldReturnError(t *testing.T) {
	// Arrange
	recvFn := func() (*hc.HealthCheckResponse, error) {

		return nil, errors.New("StreamReceivingError")
	}

	fake := &fakeHealthCheckClient{recvFn: recvFn}
	healthCheckClient := NewHealthCheckClient(fake, logrus.New())

	// Act
	err := healthCheckClient.WatchService("fake1", "correlation")

	// Assert
	if err == nil {
		t.Fatal()
	}
}
