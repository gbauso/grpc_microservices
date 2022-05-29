package main

import (
	"context"
	"database/sql"
	"flag"
	"io"
	"os"

	logger "github.com/sirupsen/logrus"

	"github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/factory"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/adapter/http2"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/usecases"
	_ "github.com/mattn/go-sqlite3"
)

var (
	port         = flag.Int("port", 50058, "The server port")
	logPath      = flag.String("log-path", "/tmp/discovery_master.log", "Log path")
	databasePath = flag.String("db-path", "../adapter/database/sqlite.db", "SQLite file path")
)

func main() {
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

	db, err := sql.Open("sqlite3", *databasePath)
	if err != nil {
		log.Errorf("error opening database: %v", err)
		panic(err)
	}

	repositoryFactory := factory.NewRepositoryDatabaseFactory(db, context.Background())
	repo := repositoryFactory.CreateServiceHandlerRepository()

	getAliveServicesUseCase := usecases.NewGetAliveServicesUseCase(repo)
	registerServiceUseCase := usecases.NewRegisterServiceUseCase(repo)
	unregisterServiceUseCase := usecases.NewUnregisterServiceUseCase(repo)

	server := http2.NewServer(registerServiceUseCase, unregisterServiceUseCase, getAliveServicesUseCase, log, port)

	err = server.Start()
	if err != nil {
		panic(err)
	}
}
