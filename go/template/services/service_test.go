package services

import (
	"context"
	"testing"

	"{{project_endpoint}}/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type mock{{project_class_name}}Repository struct {
	mock.Mock
}

func (m *mock{{project_class_name}}Repository) Get(ctx context.Context, id string) (*models.{{project_class_name}}Dto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{project_class_name}}Dto), args.Error(1)
}

func (m *mock{{project_class_name}}Repository) GetList(ctx context.Context, limit int) ([]models.{{project_class_name}}Dto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.{{project_class_name}}Dto), args.Error(1)
}

func (m *mock{{project_class_name}}Repository) Create(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{project_class_name}}Dto), args.Error(1)
}

func (m *mock{{project_class_name}}Repository) Update(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{project_class_name}}Dto), args.Error(1)
}

func (m *mock{{project_class_name}}Repository) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func setupServiceTest() (*mock{{project_class_name}}Repository, {{project_class_name}}Service) {
	mockRepo := new(mock{{project_class_name}}Repository)
	service := New{{project_class_name}}Service(mockRepo)
	return mockRepo, service
}

func TestGet_ShouldReturn{{project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	{{project_lower_camel_name}}Id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.{{project_class_name}}Dto{Id: {{project_lower_camel_name}}Id, Name: "mock{{project_class_name}}"}
	mockRepo.On("Get", mock.Anything, {{project_lower_camel_name}}Id).Return(expected, nil)

	// Act
	result, err := service.Get(context.Background(), {{project_lower_camel_name}}Id)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestGetList_ShouldReturnListOf{{project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	expected := []models.{{project_class_name}}Dto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mock{{project_class_name}}1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mock{{project_class_name}}2"},
	}
	mockRepo.On("GetList", mock.Anything, 50).Return(expected, nil)

	// Act
	result, err := service.GetList(context.Background(), 50)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreate_ShouldReturnCreated{{project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	createRequest := models.Create{{project_class_name}}Request{Name: "mockCreate{{project_class_name}}"}
	expected := &models.{{project_class_name}}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreate{{project_class_name}}"}
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.{{project_class_name}}")).Return(expected, nil)

	// Act
	result, err := service.Create(context.Background(), createRequest)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestUpdate_ShouldReturnUpdated{{project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	updateRequest := models.Update{{project_class_name}}Request{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdate{{project_class_name}}"}
	expected := &models.{{project_class_name}}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdate{{project_class_name}}"}
	mockRepo.On("Update", mock.Anything, mock.AnythingOfType("models.{{project_class_name}}")).Return(expected, nil)

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
	{{project_lower_camel_name}}Id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockRepo.On("Delete", mock.Anything, {{project_lower_camel_name}}Id).Return(nil)

	// Act
	err := service.Delete(context.Background(), {{project_lower_camel_name}}Id)

	// Assert
	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, {{project_lower_camel_name}}Id)
}
