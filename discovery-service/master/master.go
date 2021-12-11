package master

import (
	"context"
	"database/sql"

	pb "github.com/gbauso/grpc_microservices/discoveryservice/grpc_gen"
	"google.golang.org/grpc"
)

type Master struct {
	db *sql.DB
	pb.DiscoveryServiceServer
}

func (m *Master) RegisterServiceHandlers(ctx context.Context, in *pb.RegisterServiceHandlersRequest, opts ...grpc.CallOption) (*pb.RegisterServiceHandlersResponse, error) {
	tx, _ := m.db.BeginTx(ctx, nil)
	for _, handler := range in.Handlers {
		tx.ExecContext(ctx, "INSERT INTO ServiceHandler(Service, Handler) VALUES ()")
	}
}
