package http2

import (
	"context"
	"fmt"
	"net"

	pb "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/http2/interceptors"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/usecases"
	"github.com/sirupsen/logrus"
	"google.golang.org/grpc"
	"google.golang.org/grpc/reflection"
)

type Server struct {
	pb.UnimplementedDiscoveryServiceServer
	RegisterServiceUseCase   *usecases.RegisterServiceUseCase
	UnregisterServiceUseCase *usecases.UnregisterServiceUseCase
	GetAliveServicesUseCase  *usecases.GetAliveServicesUseCase
	log                      *logrus.Logger
	port                     int
}

func NewServer(registerServiceUseCase *usecases.RegisterServiceUseCase, unregisterServiceUseCase *usecases.UnregisterServiceUseCase, getAliveServicesUseCase *usecases.GetAliveServicesUseCase, log *logrus.Logger, port *int) *Server {
	return &Server{RegisterServiceUseCase: registerServiceUseCase, UnregisterServiceUseCase: unregisterServiceUseCase, GetAliveServicesUseCase: getAliveServicesUseCase, log: log, port: *port}
}

func (s *Server) RegisterServiceHandlers(ctx context.Context, in *pb.RegisterServiceHandlersRequest) (*pb.RegisterServiceHandlersResponse, error) {
	return s.RegisterServiceUseCase.Execute(in)
}

func (s *Server) GetServiceHandlers(ctx context.Context, in *pb.DiscoverySearchRequest) (*pb.DiscoverySearchResponse, error) {
	return s.GetAliveServicesUseCase.Execute(in)
}

func (s *Server) UnregisterService(ctx context.Context, in *pb.UnregisterServiceRequest) (*pb.UnregisterServiceResponse, error) {
	return s.UnregisterServiceUseCase.Execute(in)
}

func (srv *Server) Start() error {

	lis, err := net.Listen("tcp", fmt.Sprintf("0.0.0.0:%d", srv.port))
	if err != nil {
		srv.log.Errorf("failed to listen")
		return err
	}

	loggingInterceptor := interceptors.NewLoggingInterceptor(srv.log)

	s := grpc.NewServer(grpc.UnaryInterceptor(loggingInterceptor.ServerInterceptor))
	pb.RegisterDiscoveryServiceServer(s, srv)
	reflection.Register(s)

	srv.log.Infof("server listenning on 0.0.0.0:%d", srv.port)
	s.Serve(lis)

	return nil
}
