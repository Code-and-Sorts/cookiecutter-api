package services

import (
	"context"
	"testing"

	"{{project_endpoint}}/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type mockItemRepository struct {
	mock.Mock
}

func (m *mockItemRepository) Get(ctx context.Context, id string) (*models.ItemDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.ItemDto), args.Error(1)
}

func (m *mockItemRepository) GetList(ctx context.Context, limit int) ([]models.ItemDto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.ItemDto), args.Error(1)
}

func (m *mockItemRepository) Create(ctx context.Context, item models.Item) (*models.ItemDto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.ItemDto), args.Error(1)
}

func (m *mockItemRepository) Update(ctx context.Context, item models.Item) (*models.ItemDto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.ItemDto), args.Error(1)
}

func (m *mockItemRepository) Replace(ctx context.Context, item models.Item) (*models.ItemDto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.ItemDto), args.Error(1)
}

func (m *mockItemRepository) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func setupServiceTest() (*mockItemRepository, ItemService) {
	mockRepo := new(mockItemRepository)
	service := NewItemService(mockRepo)
	return mockRepo, service
}

func TestGet_ShouldReturnItemDto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	itemId := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.ItemDto{Id: itemId, Name: "mockItem"}
	mockRepo.On("Get", mock.Anything, itemId).Return(expected, nil)

	// Act
	result, err := service.Get(context.Background(), itemId)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestGetList_ShouldReturnListOfItemDto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	expected := []models.ItemDto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockItem1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockItem2"},
	}
	mockRepo.On("GetList", mock.Anything, 50).Return(expected, nil)

	// Act
	result, err := service.GetList(context.Background(), 50)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreate_ShouldReturnCreatedItemDto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	createRequest := models.CreateItemRequest{Name: "mockCreateItem"}
	expected := &models.ItemDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreateItem"}
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.Item")).Return(expected, nil)

	// Act
	result, err := service.Create(context.Background(), createRequest)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestUpdate_ShouldReturnUpdatedItemDto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	updateRequest := models.UpdateItemRequest{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdateItem"}
	expected := &models.ItemDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdateItem"}
	mockRepo.On("Update", mock.Anything, mock.AnythingOfType("models.Item")).Return(expected, nil)

	// Act
	result, err := service.Update(context.Background(), updateRequest)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestReplace_ShouldReturnReplacedItemDto(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	replaceRequest := models.ReplaceItemRequest{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockReplaceItem"}
	expected := &models.ItemDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockReplaceItem"}
	mockRepo.On("Replace", mock.Anything, mock.AnythingOfType("models.Item")).Return(expected, nil)

	// Act
	result, err := service.Replace(context.Background(), replaceRequest)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestDelete_ShouldCallRepositoryDelete(t *testing.T) {
	// Arrange
	mockRepo, service := setupServiceTest()
	itemId := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockRepo.On("Delete", mock.Anything, itemId).Return(nil)

	// Act
	err := service.Delete(context.Background(), itemId)

	// Assert
	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, itemId)
}
