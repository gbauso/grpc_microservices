package entity

type ServiceHandler struct {
	ServiceMethodId int
	Service         string
	InstanceId      string
	Handler         string
	IsAlive         bool
}

func NewServiceHandler(service string, instanceId string, handler string) *ServiceHandler {
	return &ServiceHandler{Service: service, InstanceId: instanceId, Handler: handler, IsAlive: true}
}

func (sh *ServiceHandler) MarkAsNotAlive() {
	sh.IsAlive = false
}
