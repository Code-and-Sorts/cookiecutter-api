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

type MockCatService struct {
	mock.Mock
}

func (m *MockCatService) Get(ctx context.Context, id string) (*models.CatDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.CatDto), args.Error(1)
}

func (m *MockCatService) GetList(ctx context.Context, limit int) ([]models.CatDto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.CatDto), args.Error(1)
}

func (m *MockCatService) Create(ctx context.Context, req models.CreateCatRequest) (*models.CatDto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.CatDto), args.Error(1)
}

func (m *MockCatService) Update(ctx context.Context, req models.UpdateCatRequest) (*models.CatDto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.CatDto), args.Error(1)
}

func (m *MockCatService) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func newTestCatController(mockService *MockCatService) CatController {
	validator, _ := services.NewSchemaValidator(map[string]string{
		"create_request":  CreateRequestSchema,
		"update_request":  UpdateRequestSchema,
		"replace_request": ReplaceRequestSchema,
	})
	return NewCatController(mockService, validator)
}

func TestGetCat_ReturnsCatDto(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.CatDto{Id: id, Name: "mockCat"}
	mockService.On("Get", mock.Anything, id).Return(expected, nil)

	result, err := controller.Get(context.Background(), id)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Get", mock.Anything, id)
}

func TestGetCatList_ReturnsListOfCatDto(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	expected := []models.CatDto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCat1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockCat2"},
	}
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(expected, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "GetList", mock.Anything, DefaultListLimit)
}

func TestCreateCat_ReturnsCreatedCatDto(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	createReq := models.CreateCatRequest{Name: "mockCreateCat", CreatedBy: "TestUser", UpdatedBy: "TestUser"}
	expected := &models.CatDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreateCat"}
	mockService.On("Create", mock.Anything, createReq).Return(expected, nil)

	body, _ := json.Marshal(createReq)

	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Create", mock.Anything, createReq)
}

func TestCreateCat_ReturnsValidationError_WhenNameIsEmpty(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	createReq := models.CreateCatRequest{Name: ""}
	body, _ := json.Marshal(createReq)

	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestCreateCat_ReturnsValidationError_WhenBodyIsInvalid(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)

	result, err := controller.Create(context.Background(), bytes.NewReader([]byte("invalid json")))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestUpdateCat_ReturnsUpdatedCatDto(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	updateReq := models.UpdateCatRequest{Name: "mockUpdatedCat", UpdatedBy: "TestUser"}
	expectedReq := models.UpdateCatRequest{Id: id, Name: "mockUpdatedCat", UpdatedBy: "TestUser"}
	expected := &models.CatDto{Id: id, Name: "mockUpdatedCat"}
	mockService.On("Update", mock.Anything, expectedReq).Return(expected, nil)

	body, _ := json.Marshal(updateReq)

	result, err := controller.Update(context.Background(), id, bytes.NewReader(body))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Update", mock.Anything, expectedReq)
}

func TestDeleteCat_CallsDeleteOnService(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockService.On("Delete", mock.Anything, id).Return(nil)

	err := controller.Delete(context.Background(), id)

	assert.NoError(t, err)
	mockService.AssertCalled(t, "Delete", mock.Anything, id)
}

type MockDogService struct {
	mock.Mock
}

func (m *MockDogService) Get(ctx context.Context, id string) (*models.DogDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.DogDto), args.Error(1)
}

func (m *MockDogService) GetList(ctx context.Context, limit int) ([]models.DogDto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.DogDto), args.Error(1)
}

func (m *MockDogService) Create(ctx context.Context, req models.CreateDogRequest) (*models.DogDto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.DogDto), args.Error(1)
}

func (m *MockDogService) Replace(ctx context.Context, req models.ReplaceDogRequest) (*models.DogDto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.DogDto), args.Error(1)
}

func (m *MockDogService) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func newTestDogController(mockService *MockDogService) DogController {
	validator, _ := services.NewSchemaValidator(map[string]string{
		"create_request":  CreateRequestSchema,
		"update_request":  UpdateRequestSchema,
		"replace_request": ReplaceRequestSchema,
	})
	return NewDogController(mockService, validator)
}

func TestGetDog_ReturnsDogDto(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.DogDto{Id: id, Name: "mockDog"}
	mockService.On("Get", mock.Anything, id).Return(expected, nil)

	result, err := controller.Get(context.Background(), id)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Get", mock.Anything, id)
}

func TestGetDogList_ReturnsListOfDogDto(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	expected := []models.DogDto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockDog1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockDog2"},
	}
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(expected, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "GetList", mock.Anything, DefaultListLimit)
}

func TestCreateDog_ReturnsCreatedDogDto(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	createReq := models.CreateDogRequest{Name: "mockCreateDog", CreatedBy: "TestUser", UpdatedBy: "TestUser"}
	expected := &models.DogDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreateDog"}
	mockService.On("Create", mock.Anything, createReq).Return(expected, nil)

	body, _ := json.Marshal(createReq)

	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Create", mock.Anything, createReq)
}

func TestCreateDog_ReturnsValidationError_WhenNameIsEmpty(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	createReq := models.CreateDogRequest{Name: ""}
	body, _ := json.Marshal(createReq)

	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestCreateDog_ReturnsValidationError_WhenBodyIsInvalid(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)

	result, err := controller.Create(context.Background(), bytes.NewReader([]byte("invalid json")))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestReplaceDog_ReturnsReplacedDogDto(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	replaceReq := models.ReplaceDogRequest{Name: "mockReplacedDog", UpdatedBy: "TestUser"}
	expectedReq := models.ReplaceDogRequest{Id: id, Name: "mockReplacedDog", UpdatedBy: "TestUser"}
	expected := &models.DogDto{Id: id, Name: "mockReplacedDog"}
	mockService.On("Replace", mock.Anything, expectedReq).Return(expected, nil)

	body, _ := json.Marshal(replaceReq)

	result, err := controller.Replace(context.Background(), id, bytes.NewReader(body))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Replace", mock.Anything, expectedReq)
}

func TestReplaceDog_ReturnsValidationError_WhenNameIsEmpty(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	replaceReq := models.ReplaceDogRequest{Name: ""}
	body, _ := json.Marshal(replaceReq)

	result, err := controller.Replace(context.Background(), "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", bytes.NewReader(body))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestDeleteDog_CallsDeleteOnService(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockService.On("Delete", mock.Anything, id).Return(nil)

	err := controller.Delete(context.Background(), id)

	assert.NoError(t, err)
	mockService.AssertCalled(t, "Delete", mock.Anything, id)
}
