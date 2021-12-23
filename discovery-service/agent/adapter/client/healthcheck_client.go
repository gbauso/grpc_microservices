package client

import (
	"context"
	"io"

	"google.golang.org/grpc"
	hc "google.golang.org/grpc/health/grpc_health_v1"
)

type HealthCheckClient struct {
	conn *grpc.ClientConn
}

func NewHealthCheckClient(conn *grpc.ClientConn) *HealthCheckClient {
	return &HealthCheckClient{conn: conn}
}

func (hcClient *HealthCheckClient) WatchService(service string) error {
	client := hc.NewHealthClient(hcClient.conn)

	ctx := context.Background()

	watchClient, err := client.Watch(ctx, &hc.HealthCheckRequest{Service: service})
	if err != nil {
		return err
	}

	waitc := make(chan *hc.HealthCheckResponse)
	defer close(waitc)

	go func() {
		for {
			response, err := watchClient.Recv()
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
