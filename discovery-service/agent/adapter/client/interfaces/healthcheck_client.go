package interfaces

type HealthCheckClient interface {
	WatchService(service, correlationId string) error
}
