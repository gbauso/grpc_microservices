package main

import (
	"database/sql"
	"os"

	_ "github.com/mattn/go-sqlite3"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent"
	"github.com/gbauso/grpc_microservices/discoveryservice/master"
)

func main() {
	masterNodeUrl := os.Getenv("MASTER_NODE")

	if masterNodeUrl != "" {
		agentService := agent.NewAgent(os.Getenv("SERVICE_URL"), masterNodeUrl, os.Getenv("SERVICE_NAME"))
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
