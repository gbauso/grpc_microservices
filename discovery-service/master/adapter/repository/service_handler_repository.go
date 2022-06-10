package repository

import (
	"context"
	"database/sql"

	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/entity"
)

type ServiceHandlerRepository struct {
	db  *sql.DB
	ctx context.Context
}

func NewServiceHandlerRepository(db *sql.DB, ctx context.Context) *ServiceHandlerRepository {
	return &ServiceHandlerRepository{db: db, ctx: ctx}
}

func (r *ServiceHandlerRepository) Insert(serviceHandlers ...entity.ServiceHandler) error {
	tx, _ := r.db.BeginTx(r.ctx, nil)
	for _, serviceHandler := range serviceHandlers {
		_, err := tx.ExecContext(r.ctx, "INSERT INTO ServiceHandler(Service, InstanceId, Handler, IsAlive) VALUES (?, ?, ?, ?)", serviceHandler.Service, serviceHandler.InstanceId, serviceHandler.Handler, 1)
		if err != nil {
			tx.Rollback()
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
	tx, _ := r.db.BeginTx(r.ctx, nil)
	for _, serviceHandler := range serviceHandlers {
		_, err := tx.ExecContext(r.ctx,
			"UPDATE ServiceHandler SET Service =?, Handler = ?, InstanceId = ?, IsAlive = ? WHERE ServiceMethodId = ?",
			serviceHandler.Service, serviceHandler.Handler, serviceHandler.InstanceId, serviceHandler.IsAlive, serviceHandler.ServiceMethodId)

		if err != nil {
			tx.Rollback()
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
	results, err := r.db.Query("SELECT ServiceMethodId, IsAlive, Handler, InstanceId, Service FROM ServiceHandler WHERE InstanceId = ?", serviceId)
	if err != nil {
		return nil, err
	}

	for results.Next() {
		var serviceHandler entity.ServiceHandler
		if err := results.Scan(&serviceHandler.ServiceMethodId, &serviceHandler.IsAlive, &serviceHandler.Handler, &serviceHandler.InstanceId, &serviceHandler.Service); err != nil {
			return nil, err
		}

		serviceHandlers = append(serviceHandlers, serviceHandler)
	}

	return serviceHandlers, nil
}

func (r *ServiceHandlerRepository) GetAliveServices(handler string) ([]string, error) {
	results, err := r.db.Query("SELECT DISTINCT Service FROM ServiceHandler WHERE Handler = ? AND IsAlive = 1", handler)
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
