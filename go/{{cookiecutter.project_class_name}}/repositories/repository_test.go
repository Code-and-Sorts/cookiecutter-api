package repositories

import (
	"context"
	"testing"

	"{{cookiecutter.project_endpoint}}/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type Mock{{cookiecutter.project_class_name}}Repository struct {
	mock.Mock
}

func (m *Mock{{cookiecutter.project_class_name}}Repository) Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *Mock{{cookiecutter.project_class_name}}Repository) GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *Mock{{cookiecutter.project_class_name}}Repository) Create(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *Mock{{cookiecutter.project_class_name}}Repository) Update(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{cookiecutter.project_class_name}}Dto), args.Error(1)
}

func (m *Mock{{cookiecutter.project_class_name}}Repository) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func TestMock{{cookiecutter.project_class_name}}Repository_Get_ShouldReturn{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo := new(Mock{{cookiecutter.project_class_name}}Repository)
	expected := &models.{{cookiecutter.project_class_name}}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mock{{cookiecutter.project_class_name}}"}
	mockRepo.On("Get", mock.Anything, "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c").Return(expected, nil)

	// Act
	result, err := mockRepo.Get(context.Background(), "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c")

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestMock{{cookiecutter.project_class_name}}Repository_GetList_ShouldReturnListOf{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo := new(Mock{{cookiecutter.project_class_name}}Repository)
	expected := []models.{{cookiecutter.project_class_name}}Dto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mock{{cookiecutter.project_class_name}}1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mock{{cookiecutter.project_class_name}}2"},
	}
	mockRepo.On("GetList", mock.Anything).Return(expected, nil)

	// Act
	result, err := mockRepo.GetList(context.Background())

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	assert.Len(t, result, 2)
	mockRepo.AssertExpectations(t)
}

func TestMock{{cookiecutter.project_class_name}}Repository_Create_ShouldReturnCreated{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo := new(Mock{{cookiecutter.project_class_name}}Repository)
	item := models.{{cookiecutter.project_class_name}}{BaseEntity: models.BaseEntity{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"}, Name: "mock{{cookiecutter.project_class_name}}"}
	expected := &models.{{cookiecutter.project_class_name}}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mock{{cookiecutter.project_class_name}}"}
	mockRepo.On("Create", mock.Anything, item).Return(expected, nil)

	// Act
	result, err := mockRepo.Create(context.Background(), item)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestMock{{cookiecutter.project_class_name}}Repository_Update_ShouldReturnUpdated{{cookiecutter.project_class_name}}Dto(t *testing.T) {
	// Arrange
	mockRepo := new(Mock{{cookiecutter.project_class_name}}Repository)
	item := models.{{cookiecutter.project_class_name}}{BaseEntity: models.BaseEntity{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"}, Name: "mock{{cookiecutter.project_class_name}}New"}
	expected := &models.{{cookiecutter.project_class_name}}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mock{{cookiecutter.project_class_name}}New"}
	mockRepo.On("Update", mock.Anything, item).Return(expected, nil)

	// Act
	result, err := mockRepo.Update(context.Background(), item)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestMock{{cookiecutter.project_class_name}}Repository_Delete_ShouldMarkItemAsDeleted(t *testing.T) {
	// Arrange
	mockRepo := new(Mock{{cookiecutter.project_class_name}}Repository)
	mockRepo.On("Delete", mock.Anything, "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c").Return(nil)

	// Act
	err := mockRepo.Delete(context.Background(), "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c")

	// Assert
	assert.NoError(t, err)
	mockRepo.AssertExpectations(t)
}
