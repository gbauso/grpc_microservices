package interceptors

import (
	"context"

	uuid "github.com/nu7hatch/gouuid"
	"google.golang.org/grpc"
	"google.golang.org/grpc/metadata"
)

type CorrelationInterceptor struct {
}

func NewCorrelationInterceptor() *CorrelationInterceptor {
	return &CorrelationInterceptor{}
}

func (it CorrelationInterceptor) ClientInterceptor(ctx context.Context,
	method string,
	req interface{},
	reply interface{},
	cc *grpc.ClientConn,
	invoker grpc.UnaryInvoker,
	opts ...grpc.CallOption,
) error {
	correlationId, _ := uuid.NewV4()

	context := metadata.AppendToOutgoingContext(ctx, "correlation-id", correlationId.String())

	// Calls the invoker to execute RPC
	err := invoker(context, method, req, reply, cc, opts...)

	return err
}
