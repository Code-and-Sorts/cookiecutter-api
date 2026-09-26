package controllers

import (
	"bytes"
	"context"
	"encoding/json"
	"testing"

	"kitties/models"
	"kitties/services"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

func TestCoerceLimit_ClampsToSupportedRange(t *testing.T) {
	assert.Equal(t, DefaultListLimit, CoerceLimit(0))
	assert.Equal(t, DefaultListLimit, CoerceLimit(-5))
	assert.Equal(t, 25, CoerceLimit(25))
	assert.Equal(t, MaxListLimit, CoerceLimit(999999))
}

type MockKittenClawsService struct {
	mock.Mock
}

func (m *MockKittenClawsService) Get(ctx context.Context, id string) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *MockKittenClawsService) GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.KittenClawsDto), args.Error(1)
}

func (m *MockKittenClawsService) Create(ctx context.Context, req models.CreateKittenClawsRequest) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *MockKittenClawsService) Update(ctx context.Context, req models.UpdateKittenClawsRequest) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *MockKittenClawsService) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func newTestKittenClawsController(mockService *MockKittenClawsService) KittenClawsController {
	validator, _ := services.NewSchemaValidator(map[string]string{
		"create_request":  CreateRequestSchema,
		"update_request":  UpdateRequestSchema,
		"replace_request": ReplaceRequestSchema,
	})
	return NewKittenClawsController(mockService, validator)
}

func TestGetKittenClaws_ReturnsKittenClawsDto(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.KittenClawsDto{Id: id, Name: "mockKittenClaws"}
	mockService.On("Get", mock.Anything, id).Return(expected, nil)

	result, err := controller.Get(context.Background(), id)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Get", mock.Anything, id)
}

func TestGetKittenClawsList_ReturnsListOfKittenClawsDto(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	expected := []models.KittenClawsDto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockKittenClaws1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockKittenClaws2"},
	}
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(expected, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "GetList", mock.Anything, DefaultListLimit)
}

func TestCreateKittenClaws_ReturnsCreatedKittenClawsDto(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	createReq := models.CreateKittenClawsRequest{Name: "mockCreateKittenClaws", CreatedBy: "TestUser", UpdatedBy: "TestUser"}
	expected := &models.KittenClawsDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreateKittenClaws"}
	mockService.On("Create", mock.Anything, createReq).Return(expected, nil)

	body, _ := json.Marshal(createReq)

	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Create", mock.Anything, createReq)
}

func TestCreateKittenClaws_ReturnsValidationError_WhenNameIsEmpty(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	createReq := models.CreateKittenClawsRequest{Name: ""}
	body, _ := json.Marshal(createReq)

	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestCreateKittenClaws_ReturnsValidationError_WhenBodyIsInvalid(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)

	result, err := controller.Create(context.Background(), bytes.NewReader([]byte("invalid json")))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestUpdateKittenClaws_ReturnsUpdatedKittenClawsDto(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	updateReq := models.UpdateKittenClawsRequest{Name: "mockUpdatedKittenClaws", UpdatedBy: "TestUser"}
	expectedReq := models.UpdateKittenClawsRequest{Id: id, Name: "mockUpdatedKittenClaws", UpdatedBy: "TestUser"}
	expected := &models.KittenClawsDto{Id: id, Name: "mockUpdatedKittenClaws"}
	mockService.On("Update", mock.Anything, expectedReq).Return(expected, nil)

	body, _ := json.Marshal(updateReq)

	result, err := controller.Update(context.Background(), id, bytes.NewReader(body))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Update", mock.Anything, expectedReq)
}

func TestDeleteKittenClaws_CallsDeleteOnService(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockService.On("Delete", mock.Anything, id).Return(nil)

	err := controller.Delete(context.Background(), id)

	assert.NoError(t, err)
	mockService.AssertCalled(t, "Delete", mock.Anything, id)
}
