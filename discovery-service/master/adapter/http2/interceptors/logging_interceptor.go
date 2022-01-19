package interceptors

import (
	"context"
	"time"

	"github.com/sirupsen/logrus"
	"google.golang.org/grpc"
	"google.golang.org/grpc/metadata"
)

type LoggingInterceptor struct {
	log *logrus.Logger
}

func NewLoggingInterceptor(log *logrus.Logger) *LoggingInterceptor {
	return &LoggingInterceptor{log: log}
}

func (it LoggingInterceptor) ServerInterceptor(ctx context.Context,
	req interface{},
	info *grpc.UnaryServerInfo,
	handler grpc.UnaryHandler) (interface{}, error) {
	start := time.Now()

	headers, _ := metadata.FromIncomingContext(ctx)
	correlationId := headers["correlation-id"]

	it.log.WithFields(logrus.Fields{"request": req, "correlationId": correlationId}).
		Infof("method %s called", info.FullMethod)

	// Calls the handler
	h, err := handler(ctx, req)

	if err != nil {
		it.log.WithFields(logrus.Fields{"error": err, "correlationId": correlationId}).
			Errorf("Error when calling %s", info.FullMethod)
	}

	it.log.WithFields(logrus.Fields{"correlationId": correlationId}).
		Infof("Request - Method: %s Duration: %s",
			info.FullMethod,
			time.Since(start))

	return h, err
}
