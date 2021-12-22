package factory

import (
	"context"
	"database/sql"

	repo "github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/repository"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/repository"
)

type RepositoryFactory struct {
	db  *sql.DB
	ctx context.Context
}

func NewRepositoryDatabaseFactory(db *sql.DB, ctx context.Context) *RepositoryFactory {
	return &RepositoryFactory{db: db, ctx: ctx}
}

func (r RepositoryFactory) CreateServiceHandlerRepository() repository.ServiceHandlerRepository {
	return repo.NewServiceHandlerRepository(r.db, r.ctx)
}
