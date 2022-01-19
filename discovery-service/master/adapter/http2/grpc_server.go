package http2

import (
	"context"
	"flag"
	"fmt"
	"io"
	"net"
	"os"

	pb "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/http2/interceptors"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/usecases"
	logger "github.com/sirupsen/logrus"
	"google.golang.org/grpc"
	"google.golang.org/grpc/reflection"
)

var (
	port    = flag.Int("port", 50058, "The server port")
	logPath = flag.String("log-path", "/tmp/discovery_master.log", "Log path")
)

type Server struct {
	pb.UnimplementedDiscoveryServiceServer
	RegisterServiceUseCase   usecases.RegisterServiceUseCase
	UnregisterServiceUseCase usecases.UnregisterServiceUseCase
	GetAliveServicesUseCase  usecases.GetAliveServicesUseCase
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
	flag.Parse()

	log := logger.New()
	log.SetFormatter(&logger.JSONFormatter{})
	f, err := os.OpenFile(*logPath, os.O_RDWR|os.O_CREATE|os.O_APPEND, 0666)
	if err != nil {
		log.Errorf("error opening file: %v", err)
		panic(err)
	}
	defer f.Close()
	wrt := io.MultiWriter(os.Stdout, f)

	log.SetOutput(wrt)

	lis, err := net.Listen("tcp", fmt.Sprintf("0.0.0.0:%d", *port))
	if err != nil {
		log.Errorf("failed to listen")
		return err
	}

	loggingInterceptor := interceptors.NewLoggingInterceptor(log)

	s := grpc.NewServer(grpc.UnaryInterceptor(loggingInterceptor.ServerInterceptor))
	pb.RegisterDiscoveryServiceServer(s, srv)
	reflection.Register(s)

	log.Infof("server listenning on 0.0.0.0:%d", *port)
	s.Serve(lis)

	return nil
}
