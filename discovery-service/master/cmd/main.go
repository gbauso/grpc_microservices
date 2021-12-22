package main

import (
	"context"
	"database/sql"

	"github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/factory"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/http2"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/usecases"
	_ "github.com/mattn/go-sqlite3"
)

func main() {
	db, err := sql.Open("sqlite3", "../adapter/database/sqlite.db")
	if err != nil {
		panic(err)
	}

	repositoryFactory := factory.NewRepositoryDatabaseFactory(db, context.Background())
	repo := repositoryFactory.CreateServiceHandlerRepository()

	getAliveServicesUseCase := usecases.NewGetAliveServicesUseCase(repo)
	registerServiceUseCase := usecases.NewRegisterServiceUseCase(repo)
	unregisterServiceUseCase := usecases.NewUnregisterServiceUseCase(repo)

	server := &http2.Server{GetAliveServicesUseCase: *getAliveServicesUseCase, RegisterServiceUseCase: *registerServiceUseCase, UnregisterServiceUseCase: *unregisterServiceUseCase}

	err = server.Start()
	if err != nil {
		panic(err)
	}
}
