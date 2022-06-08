package client

import (
	"context"
	"io"

	"github.com/sirupsen/logrus"
	"google.golang.org/grpc"
	hc "google.golang.org/grpc/health/grpc_health_v1"
	"google.golang.org/grpc/metadata"
)

type HealthCheckClient struct {
	conn *grpc.ClientConn
	log  *logrus.Logger
}

func NewHealthCheckClient(conn *grpc.ClientConn, log *logrus.Logger) *HealthCheckClient {
	return &HealthCheckClient{conn: conn, log: log}
}

func (hcClient *HealthCheckClient) WatchService(service, correlationId string) error {
	client := hc.NewHealthClient(hcClient.conn)

	ctx := metadata.AppendToOutgoingContext(context.Background(), "correlation_id", correlationId)

	watchClient, err := client.Watch(ctx, &hc.HealthCheckRequest{Service: service})
	if err != nil {
		return err
	}

	waitc := make(chan *hc.HealthCheckResponse)
	defer close(waitc)

	go func() {
		for {
			response, err := watchClient.Recv()
			hcClient.log.Infof("HealthCheck response -> Service: %s -> %#v", service, response)
			if (response != nil && response.Status != hc.HealthCheckResponse_SERVING) ||
				(err != nil && err != io.EOF) {
				waitc <- response
				return
			}
		}
	}()

	<-waitc

	return nil
}
