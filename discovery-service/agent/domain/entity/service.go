package entity

type Service struct {
	Url      string
	Services []string
	Name     string
	Id       string
}

func NewService(url, name, id string) *Service {
	return &Service{Url: url, Name: name, Id: id}
}

func (s *Service) SetServices(services []string) {
	s.Services = services
}
