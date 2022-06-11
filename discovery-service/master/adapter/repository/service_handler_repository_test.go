package repository

import (
	"context"
	"errors"
	"reflect"
	"testing"

	"github.com/DATA-DOG/go-sqlmock"
	"github.com/gbauso/grpc_microservices/discoveryservice/master/domain/entity"
)

func Test_ServiceHandlerRepository_Insert_Success_ShouldInsert_AndCommit_AndNotReturnAnError(t *testing.T) {
	// Arrange
	db, mock, err := sqlmock.New()
	ctx := context.Background()

	if err != nil {
		t.Fatalf("an error '%s' was not expected when opening a stub database connection", err)
	}
	defer db.Close()

	serviceHandlers := []entity.ServiceHandler{*entity.NewServiceHandler("svc1", "id1", "fake1"), *entity.NewServiceHandler("svc2", "id2", "fake2")}

	repository := NewServiceHandlerRepository(db, ctx)

	// Act - Assert
	mock.ExpectBegin()
	for i := 0; i < len(serviceHandlers); i++ {
		mock.ExpectExec("INSERT INTO ServiceHandler").WillReturnResult(sqlmock.NewResult(1, 1))
	}
	mock.ExpectCommit()

	if err = repository.Insert(serviceHandlers...); err != nil {
		t.Error(err)
	}

	if err := mock.ExpectationsWereMet(); err != nil {
		t.Errorf("there were unfulfilled expectations: %s", err)
	}

}

func Test_ServiceHandlerRepository_Insert_Fail_ShouldRollback_AndNotReturnAnError(t *testing.T) {
	// Arrange
	db, mock, err := sqlmock.New()
	ctx := context.Background()

	if err != nil {
		t.Fatalf("an error '%s' was not expected when opening a stub database connection", err)
	}
	defer db.Close()

	serviceHandlers := []entity.ServiceHandler{*entity.NewServiceHandler("svc1", "id1", "fake1"), *entity.NewServiceHandler("svc2", "id2", "fake2")}

	repository := NewServiceHandlerRepository(db, ctx)

	// Act - Assert
	mock.ExpectBegin()

	mock.ExpectExec("INSERT INTO ServiceHandler").WillReturnResult(sqlmock.NewResult(1, 1))
	mock.ExpectExec("INSERT INTO ServiceHandler").WillReturnError(errors.New("InsertError"))

	mock.ExpectRollback()

	if err = repository.Insert(serviceHandlers...); err == nil {
		t.Error(err)
	}

	if err := mock.ExpectationsWereMet(); err != nil {
		t.Errorf("there were unfulfilled expectations: %s", err)
	}

}

func Test_ServiceHandlerRepository_Update_Success_ShouldUpdate_AndCommit_AndNotReturnAnError(t *testing.T) {
	// Arrange
	db, mock, err := sqlmock.New()
	ctx := context.Background()

	if err != nil {
		t.Fatalf("an error '%s' was not expected when opening a stub database connection", err)
	}
	defer db.Close()

	serviceHandlers := []entity.ServiceHandler{*entity.NewServiceHandler("svc1", "id1", "fake1"), *entity.NewServiceHandler("svc2", "id2", "fake2")}

	repository := NewServiceHandlerRepository(db, ctx)

	// Act - Assert
	mock.ExpectBegin()
	for i := 0; i < len(serviceHandlers); i++ {
		mock.ExpectExec("UPDATE ServiceHandler").WillReturnResult(sqlmock.NewResult(1, 1))
	}
	mock.ExpectCommit()

	if err = repository.Update(serviceHandlers...); err != nil {
		t.Error(err)
	}

	if err := mock.ExpectationsWereMet(); err != nil {
		t.Errorf("there were unfulfilled expectations: %s", err)
	}

}

func Test_ServiceHandlerRepository_Update_Fail_ShouldRollback_AndNotReturnAnError(t *testing.T) {
	// Arrange
	db, mock, err := sqlmock.New()
	ctx := context.Background()

	if err != nil {
		t.Fatalf("an error '%s' was not expected when opening a stub database connection", err)
	}
	defer db.Close()

	serviceHandlers := []entity.ServiceHandler{*entity.NewServiceHandler("svc1", "id1", "fake1"), *entity.NewServiceHandler("svc2", "id2", "fake2")}

	repository := NewServiceHandlerRepository(db, ctx)

	// Act - Assert
	mock.ExpectBegin()

	mock.ExpectExec("UPDATE ServiceHandler").WillReturnResult(sqlmock.NewResult(1, 1))
	mock.ExpectExec("UPDATE ServiceHandler").WillReturnError(errors.New("UpdateError"))

	mock.ExpectRollback()

	if err = repository.Update(serviceHandlers...); err == nil {
		t.Error(err)
	}

	if err := mock.ExpectationsWereMet(); err != nil {
		t.Errorf("there were unfulfilled expectations: %s", err)
	}

}

func Test_ServiceHandlerRepository_GetAliveServices_Fail__ShouldReturnAnError(t *testing.T) {
	// Arrange
	db, mock, err := sqlmock.New()
	ctx := context.Background()

	if err != nil {
		t.Fatalf("an error '%s' was not expected when opening a stub database connection", err)
	}
	defer db.Close()

	repository := NewServiceHandlerRepository(db, ctx)

	// Act - Assert

	mock.ExpectQuery("SELECT (.+) FROM ServiceHandler").WillReturnError(errors.New("QueryFail"))

	if _, err = repository.GetAliveServices("fail"); err == nil {
		t.Error(err)
	}

	if err := mock.ExpectationsWereMet(); err != nil {
		t.Errorf("there were unfulfilled expectations: %s", err)
	}

}

func Test_ServiceHandlerRepository_GetAliveServices_Success__ShouldReturnServiceList(t *testing.T) {
	// Arrange
	db, mock, err := sqlmock.New()
	ctx := context.Background()

	if err != nil {
		t.Fatalf("an error '%s' was not expected when opening a stub database connection", err)
	}
	defer db.Close()

	repository := NewServiceHandlerRepository(db, ctx)

	rows := sqlmock.NewRows([]string{"Service"}).
		AddRow("fake1").
		AddRow("fake2")

	// Act - Assert

	mock.ExpectQuery("SELECT (.+) FROM ServiceHandler").WillReturnRows(rows)

	services, err := repository.GetAliveServices("success")
	if err != nil || !reflect.DeepEqual(services, []string{"fake1", "fake2"}) {
		t.Error(err)
	}

	if err := mock.ExpectationsWereMet(); err != nil {
		t.Errorf("there were unfulfilled expectations: %s", err)
	}

}

func Test_ServiceHandlerRepository_GetByServiceId_Fail_ShouldReturnAnError(t *testing.T) {
	// Arrange
	db, mock, err := sqlmock.New()
	ctx := context.Background()

	if err != nil {
		t.Fatalf("an error '%s' was not expected when opening a stub database connection", err)
	}
	defer db.Close()

	repository := NewServiceHandlerRepository(db, ctx)

	// Act - Assert

	mock.ExpectQuery("SELECT (.+) FROM ServiceHandler").WillReturnError(errors.New("QueryFail"))

	if _, err = repository.GetByServiceId("fail"); err == nil {
		t.Error(err)
	}

	if err := mock.ExpectationsWereMet(); err != nil {
		t.Errorf("there were unfulfilled expectations: %s", err)
	}

}

func Test_ServiceHandlerRepository_GetByServiceId_Success_ShouldReturnServiceList(t *testing.T) {
	// Arrange
	db, mock, err := sqlmock.New()
	ctx := context.Background()

	if err != nil {
		t.Fatalf("an error '%s' was not expected when opening a stub database connection", err)
	}
	defer db.Close()

	repository := NewServiceHandlerRepository(db, ctx)

	rows := sqlmock.NewRows([]string{"ServiceMethodId", "IsAlive", "Handler", "InstanceId", "Service"}).
		AddRow(1, 1, "handler1", "id1", "svc1")

	// Act - Assert

	mock.ExpectQuery("SELECT (.+) FROM ServiceHandler").WillReturnRows(rows)

	expectedReturn := entity.NewServiceHandler("svc1", "id1", "handler1")
	expectedReturn.IsAlive = true
	expectedReturn.InstanceId = "id1"

	service, err := repository.GetByServiceId("success")
	if err != nil || reflect.DeepEqual(service, []entity.ServiceHandler{*expectedReturn}) {
		t.Error(err)
	}

	if err := mock.ExpectationsWereMet(); err != nil {
		t.Errorf("there were unfulfilled expectations: %s", err)
	}

}
