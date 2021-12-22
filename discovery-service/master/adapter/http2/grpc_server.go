package http2

import (
	"context"
	"flag"
	"fmt"
	"log"
	"net"

	pb "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/usecases"
	"google.golang.org/grpc"
	"google.golang.org/grpc/reflection"
)

var (
	port = flag.Int("port", 50058, "The server port")
)

type Server struct {
	pb.UnimplementedDiscoveryServiceServer
	registerServiceUseCase   usecases.RegisterServiceUseCase
	unregisterServiceUseCase usecases.UnregisterServiceUseCase
	GetAliveServicesUseCase  usecases.GetAliveServicesUseCase
}

func (s *Server) RegisterServiceHandlers(ctx context.Context, in *pb.RegisterServiceHandlersRequest) (*pb.RegisterServiceHandlersResponse, error) {
	return s.registerServiceUseCase.Execute(in)
}

func (s *Server) GetServiceHandlers(ctx context.Context, in *pb.DiscoverySearchRequest) (*pb.DiscoverySearchResponse, error) {
	return s.GetAliveServicesUseCase.Execute(in)
}

func (s *Server) UnregisterService(ctx context.Context, in *pb.UnregisterServiceRequest) (*pb.UnregisterServiceResponse, error) {
	return s.unregisterServiceUseCase.Execute(in)
}

func (srv *Server) Start() error {
	flag.Parse()
	lis, err := net.Listen("tcp", fmt.Sprintf("0.0.0.0:%d", *port))
	if err != nil {
		log.Fatalf("failed to listen: %v", err)
		return err
	}
	s := grpc.NewServer()
	pb.RegisterDiscoveryServiceServer(s, srv)
	reflection.Register(s)
	s.Serve(lis)

	return nil
}
