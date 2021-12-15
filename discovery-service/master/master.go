package master

import (
	"context"
	"database/sql"
	"flag"
	"fmt"
	"log"
	"net"

	pb "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"google.golang.org/grpc"
)

var (
	port = flag.Int("port", 50058, "The server port")
)

type Master struct {
	db *sql.DB
	pb.UnimplementedDiscoveryServiceServer
}

func NewMaster(db *sql.DB) *Master {
	return &Master{db: db}
}

func (m *Master) RegisterServiceHandlers(ctx context.Context, in *pb.RegisterServiceHandlersRequest) (*pb.RegisterServiceHandlersResponse, error) {
	tx, _ := m.db.BeginTx(ctx, nil)
	defer tx.Rollback()
	for _, handler := range in.Handlers {
		tx.ExecContext(ctx, "INSERT INTO ServiceMethod(Service, IstanceId, Method, IsAlive) VALUES (?, ?, ?, ?)", in.Service, in.ServiceId, handler, 1)
	}

	if err := tx.Commit(); err != nil {
		return nil, err
	}

	return &pb.RegisterServiceHandlersResponse{}, nil
}

func (m *Master) GetServiceHandlers(ctx context.Context, in *pb.DiscoverySearchRequest) (*pb.DiscoverySearchResponse, error) {
	results, err := m.db.Query("SELECT Service FROM ServiceMethod WHERE Method = ? AND IsAlive = 1", in.ServiceDefinition)
	if err != nil {
		return nil, err
	}

	var services []string

	for results.Next() {
		var service string
		if err := results.Scan(service); err != nil {
			return &pb.DiscoverySearchResponse{Services: services}, err
		}

		services = append(services, service)
	}

	return &pb.DiscoverySearchResponse{Services: services}, nil

}

func (m *Master) UnregisterService(ctx context.Context, in *pb.UnregisterServiceRequest) (*pb.UnregisterServiceResponse, error) {
	return nil, nil
}

func (m *Master) Init() error {
	flag.Parse()
	lis, err := net.Listen("tcp", fmt.Sprintf("localhost:%d", *port))
	if err != nil {
		log.Fatalf("failed to listen: %v", err)
	}
	s := grpc.NewServer()
	pb.RegisterDiscoveryServiceServer(s, m)

	s.Serve(lis)

	return nil
}
