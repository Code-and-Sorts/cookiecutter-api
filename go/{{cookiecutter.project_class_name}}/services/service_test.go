package services

import (
	"context"
	"testing"

	"{{cookiecutter.project_endpoint}}/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type mock{{cookiecutter.project_class_name}}Repository struct {
	mock.Mock
}

func (m *mock{{cookiecutter.project_class_name}}Repository) Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *mock{{cookiecutter.project_class_name}}Repository) GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *mock{{cookiecutter.project_class_name}}Repository) Create(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *mock{{cookiecutter.project_class_name}}Repository) Update(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *mock{{cookiecutter.project_class_name}}Repository) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func setupServiceTest() (*mock{{cookiecutter.project_class_name}}Repository, {{cookiecutter.project_class_name}}Service) {
	mockRepo := new(mock{{cookiecutter.project_class_name}}Repository)
	service := New{{cookiecutter.project_class_name}}Service(mockRepo)
	return mockRepo, service
}

func TestGet_ShouldReturn{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	{{cookiecutter.project_lower_camel_name}}Id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.{{cookiecutter.project_class_name}}Dto{Id: {{cookiecutter.project_lower_camel_name}}Id, Name: "mock{{cookiecutter.project_class_name}}"}
	mockRepo.On("Get", mock.Anything, {{cookiecutter.project_lower_camel_name}}Id).Return(expected, nil)

	// Act
	result, err := service.Get(context.Background(), {{cookiecutter.project_lower_camel_name}}Id)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestGetList_ShouldReturnListOf{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	expected := []models.{{cookiecutter.project_class_name}}Dto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mock{{cookiecutter.project_class_name}}1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mock{{cookiecutter.project_class_name}}2"},
	}
	mockRepo.On("GetList", mock.Anything).Return(expected, nil)

	// Act
	result, err := service.GetList(context.Background())

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreate_ShouldReturnCreated{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	createRequest := models.Create{{cookiecutter.project_class_name}}Request{Name: "mockCreate{{cookiecutter.project_class_name}}"}
	expected := &models.{{cookiecutter.project_class_name}}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreate{{cookiecutter.project_class_name}}"}
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.{{cookiecutter.project_class_name}}")).Return(expected, nil)

	// Act
	result, err := service.Create(context.Background(), createRequest)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestUpdate_ShouldReturnUpdated{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	updateRequest := models.Update{{cookiecutter.project_class_name}}Request{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdate{{cookiecutter.project_class_name}}"}
	expected := &models.{{cookiecutter.project_class_name}}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdate{{cookiecutter.project_class_name}}"}
	mockRepo.On("Update", mock.Anything, mock.AnythingOfType("models.{{cookiecutter.project_class_name}}")).Return(expected, nil)

	// Act
	result, err := service.Update(context.Background(), updateRequest)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestDelete_ShouldCallRepositoryDelete(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	{{cookiecutter.project_lower_camel_name}}Id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockRepo.On("Delete", mock.Anything, {{cookiecutter.project_lower_camel_name}}Id).Return(nil)

	// Act
	err := service.Delete(context.Background(), {{cookiecutter.project_lower_camel_name}}Id)

	// Assert
	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, {{cookiecutter.project_lower_camel_name}}Id)
}
