package client

import (
	"context"
	"io"

	"github.com/sirupsen/logrus"
	hc "google.golang.org/grpc/health/grpc_health_v1"
	"google.golang.org/grpc/metadata"
)

type HealthCheckClient struct {
	grpc hc.HealthClient
	log  *logrus.Logger
}

func NewHealthCheckClient(grpc hc.HealthClient, log *logrus.Logger) *HealthCheckClient {
	return &HealthCheckClient{grpc: grpc, log: log}
}

func (hcClient *HealthCheckClient) WatchService(service, correlationId string) error {
	ctx := metadata.AppendToOutgoingContext(context.Background(), "correlation_id", correlationId)

	watchClient, err := hcClient.grpc.Watch(ctx, &hc.HealthCheckRequest{Service: service})
	if err != nil {
		return err
	}

	waitc := make(chan *hc.HealthCheckResponse)
	errorStream := make(chan error)
	defer close(waitc)
	defer close(errorStream)

	go func() {
		for {
			response, err := watchClient.Recv()
			hcClient.log.Infof("HealthCheck response -> Service: %s -> %v", service, response)
			if (response != nil && response.Status != hc.HealthCheckResponse_SERVING) ||
				(err != nil && err != io.EOF) {
				waitc <- response
				errorStream <- err
				return
			}
		}
	}()

	<-waitc

	return <-errorStream
}
