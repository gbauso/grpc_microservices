package main

import (
	"flag"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent"
)

var (
	masterNodeUrl = flag.String("master-node", "", "The master node url")
	serviceUrl    = flag.String("service-url", "", "The service url")
	service       = flag.String("service", "", "The service name")
)

func main() {
	flag.Parse()

	if *masterNodeUrl != "" {
		agentService, err := agent.NewAgent(*serviceUrl, *masterNodeUrl, *service)
		if err != nil {
			panic(err)
		}
		err = agentService.Init()
		if err != nil {
			panic(err)
		}
	}
}
