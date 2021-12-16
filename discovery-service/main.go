package main

import (
	"database/sql"
	"flag"

	_ "github.com/mattn/go-sqlite3"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent"
	"github.com/gbauso/grpc_microservices/discoveryservice/master"
)

var (
	masterNodeUrl = flag.String("master-node", "", "The master node url")
	serviceUrl    = flag.String("service-url", "", "The service url")
	service       = flag.String("service", "", "The service name")
)

func main() {
	flag.Parse()

	if *masterNodeUrl != "" {
		agentService := agent.NewAgent(*serviceUrl, *masterNodeUrl, *service)
		err := agentService.Init()
		if err != nil {
			panic(err)
		}
	} else {
		db, err := sql.Open("sqlite3", "./database/sqlite.db")
		if err != nil {
			panic(err)
		}
		masterService := master.NewMaster(db)
		err = masterService.Init()
		if err != nil {
			panic(err)
		}
	}
}
