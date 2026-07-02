package controllers

import (
	"bytes"
	"context"
	"encoding/json"
	"testing"

	"{{project_endpoint}}/models"
	"{{project_endpoint}}/services"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type MockItemService struct {
	mock.Mock
}

func (m *MockItemService) Get(ctx context.Context, id string) (*models.ItemDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.ItemDto), args.Error(1)
}

func (m *MockItemService) GetList(ctx context.Context, limit int) ([]models.ItemDto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.ItemDto), args.Error(1)
}

func (m *MockItemService) Create(ctx context.Context, req models.CreateItemRequest) (*models.ItemDto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.ItemDto), args.Error(1)
}

func (m *MockItemService) Update(ctx context.Context, req models.UpdateItemRequest) (*models.ItemDto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.ItemDto), args.Error(1)
}

func (m *MockItemService) Replace(ctx context.Context, req models.ReplaceItemRequest) (*models.ItemDto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.ItemDto), args.Error(1)
}

func (m *MockItemService) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func newTestController(mockService *MockItemService) ItemController {
	validator, _ := services.NewSchemaValidator(map[string]string{
		"create_request":  CreateRequestSchema,
		"update_request":  UpdateRequestSchema,
		"replace_request": ReplaceRequestSchema,
	})
	return NewItemController(mockService, validator)
}

func TestGetAsync_ReturnsItemDto(t *testing.T) {
	// Arrange
	mockService := new(MockItemService)
	controller := newTestController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.ItemDto{Id: id, Name: "mockItem"}
	mockService.On("Get", mock.Anything, id).Return(expected, nil)

	// Act
	result, err := controller.Get(context.Background(), id)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Get", mock.Anything, id)
}

func TestGetListAsync_ReturnsListOfItemDto(t *testing.T) {
	// Arrange
	mockService := new(MockItemService)
	controller := newTestController(mockService)
	expected := []models.ItemDto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockItem1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockItem2"},
	}
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(expected, nil)

	// Act
	result, err := controller.GetList(context.Background(), 0)

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "GetList", mock.Anything, DefaultListLimit)
}

func TestCoerceLimit_ClampsToSupportedRange(t *testing.T) {
	assert.Equal(t, DefaultListLimit, CoerceLimit(0))
	assert.Equal(t, DefaultListLimit, CoerceLimit(-5))
	assert.Equal(t, 25, CoerceLimit(25))
	assert.Equal(t, MaxListLimit, CoerceLimit(999999))
}

func TestCreateAsync_ReturnsCreatedItemDto(t *testing.T) {
	// Arrange
	mockService := new(MockItemService)
	controller := newTestController(mockService)
	createReq := models.CreateItemRequest{Name: "mockCreateItem", CreatedBy: "TestUser", UpdatedBy: "TestUser"}
	expected := &models.ItemDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreateItem"}
	mockService.On("Create", mock.Anything, createReq).Return(expected, nil)

	body, _ := json.Marshal(createReq)

	// Act
	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Create", mock.Anything, createReq)
}

func TestUpdateAsync_ReturnsUpdatedItemDto(t *testing.T) {
	// Arrange
	mockService := new(MockItemService)
	controller := newTestController(mockService)
	itemId := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	updateReq := models.UpdateItemRequest{Name: "mockUpdatedItem", UpdatedBy: "TestUser"}
	expectedReq := models.UpdateItemRequest{Id: itemId, Name: "mockUpdatedItem", UpdatedBy: "TestUser"}
	expected := &models.ItemDto{Id: itemId, Name: "mockUpdatedItem"}
	mockService.On("Update", mock.Anything, expectedReq).Return(expected, nil)

	body, _ := json.Marshal(updateReq)

	// Act
	result, err := controller.Update(context.Background(), itemId, bytes.NewReader(body))

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Update", mock.Anything, expectedReq)
}

func TestReplaceAsync_ReturnsReplacedItemDto(t *testing.T) {
	// Arrange
	mockService := new(MockItemService)
	controller := newTestController(mockService)
	itemId := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	replaceReq := models.ReplaceItemRequest{Name: "mockReplacedItem", UpdatedBy: "TestUser"}
	expectedReq := models.ReplaceItemRequest{Id: itemId, Name: "mockReplacedItem", UpdatedBy: "TestUser"}
	expected := &models.ItemDto{Id: itemId, Name: "mockReplacedItem"}
	mockService.On("Replace", mock.Anything, expectedReq).Return(expected, nil)

	body, _ := json.Marshal(replaceReq)

	// Act
	result, err := controller.Replace(context.Background(), itemId, bytes.NewReader(body))

	// Assert
	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Replace", mock.Anything, expectedReq)
}

func TestDeleteAsync_CallsDeleteOnService(t *testing.T) {
	// Arrange
	mockService := new(MockItemService)
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
	mockService := new(MockItemService)
	controller := newTestController(mockService)
	createReq := models.CreateItemRequest{Name: ""}
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
	mockService := new(MockItemService)
	controller := newTestController(mockService)

	// Act
	result, err := controller.Create(context.Background(), bytes.NewReader([]byte("invalid json")))

	// Assert
	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestReplaceAsync_ReturnsValidationError_WhenNameIsEmpty(t *testing.T) {
	// Arrange
	mockService := new(MockItemService)
	controller := newTestController(mockService)
	replaceReq := models.ReplaceItemRequest{Name: ""}
	body, _ := json.Marshal(replaceReq)

	// Act
	result, err := controller.Replace(context.Background(), "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", bytes.NewReader(body))

	// Assert
	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}
