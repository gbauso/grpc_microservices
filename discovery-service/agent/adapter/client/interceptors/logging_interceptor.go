package interceptors

import (
	"context"

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

func (it LoggingInterceptor) ClientInterceptor(ctx context.Context,
	method string,
	req interface{},
	reply interface{},
	cc *grpc.ClientConn,
	invoker grpc.UnaryInvoker,
	opts ...grpc.CallOption,
) error {

	headers, _ := metadata.FromIncomingContext(ctx)
	correlationId := headers["correlation-id"]

	it.log.WithFields(logrus.Fields{"request": req, "correlationId": correlationId}).
		Infof("Calling %s -> Method: %s", cc.Target(), method)

	// Calls the invoker to execute RPC
	err := invoker(ctx, method, req, reply, cc, opts...)

	return err
}
