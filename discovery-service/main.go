package discoveryservice

import (
	"os"

	"github.com/gbauso/grpc_microservices/discoveryservice/agent"
)

func main() {
	masterNodeUrl := os.Getenv("MASTER_NODE")

	if masterNodeUrl != "" {
		agentService := agent.NewAgent(os.Getenv("SERVICE_URL"), masterNodeUrl)
		err := agentService.Init()
		if err != nil {
			panic(err)
		}
	}
}
