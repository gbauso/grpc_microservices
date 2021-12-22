package repository

import (
	"context"
	"database/sql"

	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/entity"
)

type ServiceHandlerRepository struct {
	db *sql.DB
}

var (
	txContext = context.Background()
)

func NewServiceHandlerRepository(db *sql.DB) *ServiceHandlerRepository {
	return &ServiceHandlerRepository{db: db}
}

func (r *ServiceHandlerRepository) Insert(serviceHandlers []entity.ServiceHandler) error {
	tx, _ := r.db.BeginTx(txContext, nil)
	for _, serviceHandler := range serviceHandlers {
		_, err := tx.ExecContext(txContext, "INSERT INTO ServiceHandler(Service, InstanceId, Handler, IsAlive) VALUES (?, ?, ?, ?)", serviceHandler.Service, serviceHandler.InstanceId, serviceHandler.Handler, 1)
		if err != nil {
			return err
		}
	}

	if err := tx.Commit(); err != nil {
		tx.Rollback()
		return err
	}

	return nil
}

func (r *ServiceHandlerRepository) Update(serviceHandlers ...entity.ServiceHandler) error {
	tx, _ := r.db.BeginTx(txContext, nil)
	for _, serviceHandler := range serviceHandlers {
		_, err := tx.ExecContext(txContext,
			"UPDATE ServiceHandler SET Service =?, Handler = ?, InstanceId = ?, IsAlive = ? WHERE Id=?",
			serviceHandler.Service, serviceHandler.Handler, serviceHandler.InstanceId, serviceHandler.IsAlive, serviceHandler.Id)

		if err != nil {
			return err
		}
	}

	if err := tx.Commit(); err != nil {
		tx.Rollback()
		return err
	}

	return nil
}

func (r *ServiceHandlerRepository) GetByServiceId(serviceId string) ([]entity.ServiceHandler, error) {
	var serviceHandlers []entity.ServiceHandler
	results, err := r.db.Query("SELECT * FROM ServiceHandler WHERE ServiceId = ?", serviceId)
	if err != nil {
		return nil, err
	}

	for results.Next() {
		var serviceHandler entity.ServiceHandler
		if err := results.Scan(&serviceHandler); err != nil {
			return nil, err
		}

		serviceHandlers = append(serviceHandlers, serviceHandler)
	}

	return serviceHandlers, nil
}

func (r *ServiceHandlerRepository) GetAliveServices(service string) ([]string, error) {
	results, err := r.db.Query("SELECT DISTINCT Service FROM ServiceHandler WHERE Handler = ? AND IsAlive = 1", service)
	if err != nil {
		return nil, err
	}

	var services []string

	for results.Next() {
		var service string
		if err := results.Scan(&service); err != nil {
			return nil, err
		}

		services = append(services, service)
	}

	return services, nil
}
