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
	"google.golang.org/grpc/reflection"
)

var (
	port      = flag.Int("port", 50058, "The server port")
	txContext = context.Background()
)

type Master struct {
	db *sql.DB
	pb.UnimplementedDiscoveryServiceServer
}

func NewMaster(db *sql.DB) *Master {
	return &Master{db: db}
}

func (m *Master) RegisterServiceHandlers(ctx context.Context, in *pb.RegisterServiceHandlersRequest) (*pb.RegisterServiceHandlersResponse, error) {
	tx, _ := m.db.BeginTx(txContext, nil)
	for _, handler := range in.Handlers {
		_, err := tx.ExecContext(txContext, "INSERT INTO ServiceHandler(Service, InstanceId, Handler, IsAlive) VALUES (?, ?, ?, ?)", in.Service, in.ServiceId, handler, 1)
		if err != nil {
			return nil, err
		}
	}

	if err := tx.Commit(); err != nil {
		tx.Rollback()
		return nil, err
	}

	return &pb.RegisterServiceHandlersResponse{}, nil
}

func (m *Master) GetServiceHandlers(ctx context.Context, in *pb.DiscoverySearchRequest) (*pb.DiscoverySearchResponse, error) {
	results, err := m.db.Query("SELECT DISTINCT Service FROM ServiceHandler WHERE Handler = ? AND IsAlive = 1", in.ServiceDefinition)
	if err != nil {
		return nil, err
	}

	var services []string

	for results.Next() {
		var service string
		if err := results.Scan(&service); err != nil {
			return &pb.DiscoverySearchResponse{Services: services}, err
		}

		services = append(services, service)
	}

	return &pb.DiscoverySearchResponse{Services: services}, nil

}

func (m *Master) UnregisterService(ctx context.Context, in *pb.UnregisterServiceRequest) (*pb.UnregisterServiceResponse, error) {
	tx, _ := m.db.BeginTx(txContext, nil)
	_, err := tx.ExecContext(txContext, "UPDATE ServiceHandler SET IsAlive = 0 WHERE InstanceId=?", in.ServiceId)
	if err != nil {
		return nil, err
	}

	if err := tx.Commit(); err != nil {
		tx.Rollback()
		return nil, err
	}

	return &pb.UnregisterServiceResponse{}, nil
}

func (m *Master) Init() error {
	flag.Parse()
	lis, err := net.Listen("tcp", fmt.Sprintf("0.0.0.0:%d", *port))
	if err != nil {
		log.Fatalf("failed to listen: %v", err)
	}
	s := grpc.NewServer()
	pb.RegisterDiscoveryServiceServer(s, m)
	reflection.Register(s)
	s.Serve(lis)

	return nil
}
