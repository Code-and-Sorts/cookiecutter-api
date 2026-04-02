package controllers

import (
	"bytes"
	"context"
	"encoding/json"
	"testing"

	"{{cookiecutter.project_endpoint}}/models"
	"{{cookiecutter.project_endpoint}}/services"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type Mock{{cookiecutter.project_class_name}}Service struct {
	mock.Mock
}

func (m *Mock{{cookiecutter.project_class_name}}Service) Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *Mock{{cookiecutter.project_class_name}}Service) GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *Mock{{cookiecutter.project_class_name}}Service) Create(ctx context.Context, req models.Create{{cookiecutter.project_class_name}}Request) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *Mock{{cookiecutter.project_class_name}}Service) Update(ctx context.Context, req models.Update{{cookiecutter.project_class_name}}Request) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *Mock{{cookiecutter.project_class_name}}Service) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func newTestController(mockService *Mock{{cookiecutter.project_class_name}}Service) {{cookiecutter.project_class_name}}Controller {
	return New{{cookiecutter.project_class_name}}Controller(mockService, services.NewSchemaValidator())
}

func TestGetAsync_Returns{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockService := new(Mock{{cookiecutter.project_class_name}}Service)
	controller := newTestController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.{{cookiecutter.project_class_name}}Dto{Id: id, Name: "mock{{cookiecutter.project_class_name}}"}
	mockService.On("Get", mock.Anything, id).Return(expected, nil)

	// Act
	result, err := controller.Get(context.Background(), id)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Get", mock.Anything, id)
}

func TestGetListAsync_ReturnsListOf{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockService := new(Mock{{cookiecutter.project_class_name}}Service)
	controller := newTestController(mockService)
	expected := []models.{{cookiecutter.project_class_name}}Dto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mock{{cookiecutter.project_class_name}}1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mock{{cookiecutter.project_class_name}}2"},
	}
	mockService.On("GetList", mock.Anything).Return(expected, nil)

	// Act
	result, err := controller.GetList(context.Background())

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "GetList", mock.Anything)
}

func TestCreateAsync_ReturnsCreated{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockService := new(Mock{{cookiecutter.project_class_name}}Service)
	controller := newTestController(mockService)
	createReq := models.Create{{cookiecutter.project_class_name}}Request{Name: "mockCreate{{cookiecutter.project_class_name}}", CreatedBy: "TestUser", UpdatedBy: "TestUser"}
	expected := &models.{{cookiecutter.project_class_name}}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreate{{cookiecutter.project_class_name}}"}
	mockService.On("Create", mock.Anything, createReq).Return(expected, nil)

	body, _ := json.Marshal(createReq)

	// Act
	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Create", mock.Anything, createReq)
}

func TestUpdateAsync_ReturnsUpdated{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockService := new(Mock{{cookiecutter.project_class_name}}Service)
	controller := newTestController(mockService)
	{{cookiecutter.project_lower_camel_name}}Id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	updateReq := models.Update{{cookiecutter.project_class_name}}Request{Name: "mockUpdated{{cookiecutter.project_class_name}}", UpdatedBy: "TestUser"}
	expectedReq := models.Update{{cookiecutter.project_class_name}}Request{Id: {{cookiecutter.project_lower_camel_name}}Id, Name: "mockUpdated{{cookiecutter.project_class_name}}", UpdatedBy: "TestUser"}
	expected := &models.{{cookiecutter.project_class_name}}Dto{Id: {{cookiecutter.project_lower_camel_name}}Id, Name: "mockUpdated{{cookiecutter.project_class_name}}"}
	mockService.On("Update", mock.Anything, expectedReq).Return(expected, nil)

	body, _ := json.Marshal(updateReq)

	// Act
	result, err := controller.Update(context.Background(), {{cookiecutter.project_lower_camel_name}}Id, bytes.NewReader(body))

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Update", mock.Anything, expectedReq)
}

func TestDeleteAsync_CallsDeleteOnService(t *testing.T) {
	// Arrange
	mockService := new(Mock{{cookiecutter.project_class_name}}Service)
	controller := newTestController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockService.On("Delete", mock.Anything, id).Return(nil)

	// Act
	err := controller.Delete(context.Background(), id)

	// Assert
	assert.NoError(t, err)
	mockService.AssertCalled(t, "Delete", mock.Anything, id)
}

func TestCreateAsync_ReturnsValidationError_WhenNameIsEmpty(t *testing.T) {
	// Arrange
	mockService := new(Mock{{cookiecutter.project_class_name}}Service)
	controller := newTestController(mockService)
	createReq := models.Create{{cookiecutter.project_class_name}}Request{Name: ""}
	body, _ := json.Marshal(createReq)

	// Act
	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	// Assert
	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestCreateAsync_ReturnsValidationError_WhenBodyIsInvalid(t *testing.T) {
	// Arrange
	mockService := new(Mock{{cookiecutter.project_class_name}}Service)
	controller := newTestController(mockService)

	// Act
	result, err := controller.Create(context.Background(), bytes.NewReader([]byte("invalid json")))

	// Assert
	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}
