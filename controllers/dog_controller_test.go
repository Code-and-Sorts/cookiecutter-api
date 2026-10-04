package controllers

import (
	"context"
	"encoding/json"
	"errors"
	"strings"
	"testing"
	"testing/iotest"

	"kittenclaws/models"
	"kittenclaws/services"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

const testDogID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

const invalidDogID = "not-a-uuid"

var rejectedDogFields = map[string]string{
	"id":               `"` + testDogID + `"`,
	"isDeleted":        `false`,
	"createdTimestamp": `"2026-01-01T00:00:00.000Z"`,
	"updatedTimestamp": `"2026-01-01T00:00:00.000Z"`,
	"createdBy":        `"someone"`,
	"updatedBy":        `"someone"`,
	"color":            `"red"`,
}

func withDogRejectedFields(bodies map[string]string) map[string]string {
	for field, value := range rejectedDogFields {
		bodies[field] = `{"name": "a", "` + field + `": ` + value + `}`
	}
	return bodies
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

func (m *MockDogService) Create(ctx context.Context, req models.CreateDogRequest, userID string) (*models.DogDto, error) {
	args := m.Called(ctx, req, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.DogDto), args.Error(1)
}

func (m *MockDogService) Replace(ctx context.Context, req models.ReplaceDogRequest, userID string) (*models.DogDto, error) {
	args := m.Called(ctx, req, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.DogDto), args.Error(1)
}

func (m *MockDogService) Delete(ctx context.Context, id, userID string) error {
	args := m.Called(ctx, id, userID)
	return args.Error(0)
}

func newTestDogController(mockService *MockDogService) DogController {
	validator, _ := services.NewSchemaValidator(RequestSchemas())
	return NewDogController(mockService, validator)
}

func assertDogNotFound(t *testing.T, err error) {
	t.Helper()
	var notFound *models.NotFoundError
	if assert.ErrorAs(t, err, &notFound) {
		assert.Equal(t, "Dog with id not-a-uuid was not found.", notFound.Message)
	}
}

func assertDogBadRequest(t *testing.T, err error) {
	t.Helper()
	assert.ErrorAs(t, err, new(*models.ValidationError))
}

func TestGetDog_ReturnsDogDto(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	expected := &models.DogDto{Id: testDogID, Name: "mockDog"}
	mockService.On("Get", mock.Anything, testDogID).Return(expected, nil)

	result, err := controller.Get(context.Background(), testDogID)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
}

func TestGetDog_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)

	result, err := controller.Get(context.Background(), invalidDogID)

	assert.Nil(t, result)
	assertDogNotFound(t, err)
	mockService.AssertNotCalled(t, "Get", mock.Anything, mock.Anything)
}

func TestGetDogList_ReturnsListOfDogDto(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	expected := []models.DogDto{
		{Id: testDogID, Name: "mockDog1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockDog2"},
	}
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(expected, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
}

func TestGetDogList_PassesRequestedLimit(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	mockService.On("GetList", mock.Anything, 2).Return([]models.DogDto{}, nil)

	_, err := controller.GetList(context.Background(), 2)

	assert.NoError(t, err)
	mockService.AssertCalled(t, "GetList", mock.Anything, 2)
}

func TestGetDogList_ReturnsEmptyArray_WhenNothingIsStored(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(nil, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	body, _ := json.Marshal(result)
	assert.JSONEq(t, `[]`, string(body))
}

func TestGetDogList_ReturnsServiceError(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(nil, errors.New("boom"))

	result, err := controller.GetList(context.Background(), 0)

	assert.Nil(t, result)
	assert.EqualError(t, err, "boom")
}

func TestCreateDog_ReturnsCreatedDogDto(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	createReq := models.CreateDogRequest{Name: "mockCreateDog"}
	expected := &models.DogDto{Id: testDogID, Name: "mockCreateDog"}
	mockService.On("Create", mock.Anything, createReq, "alice").Return(expected, nil)

	result, err := controller.Create(context.Background(), "alice", strings.NewReader(`{"name": "mockCreateDog"}`))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Create", mock.Anything, createReq, "alice")
}

func TestCreateDog_ReturnsValidationError_ForInvalidBodies(t *testing.T) {
	bodies := withDogRejectedFields(map[string]string{
		"empty name":     `{"name": ""}`,
		"missing name":   `{}`,
		"number name":    `{"name": 5}`,
		"malformed JSON": `invalid json`,
		"array body":     `[]`,
	})

	for name, body := range bodies {
		t.Run(name, func(t *testing.T) {
			mockService := new(MockDogService)
			controller := newTestDogController(mockService)

			result, err := controller.Create(context.Background(), "", strings.NewReader(body))

			assert.Nil(t, result)
			assertDogBadRequest(t, err)
			mockService.AssertNotCalled(t, "Create", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestCreateDog_ReturnsValidationError_WhenBodyCannotBeRead(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)

	result, err := controller.Create(context.Background(), "", iotest.ErrReader(errors.New("broken")))

	assert.Nil(t, result)
	assertDogBadRequest(t, err)
}

func TestReplaceDog_ReturnsReplacedDogDto(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	expectedReq := models.ReplaceDogRequest{Id: testDogID, Name: "mockReplacedDog"}
	expected := &models.DogDto{Id: testDogID, Name: "mockReplacedDog"}
	mockService.On("Replace", mock.Anything, expectedReq, "alice").Return(expected, nil)

	result, err := controller.Replace(context.Background(), testDogID, "alice", strings.NewReader(`{"name": "mockReplacedDog"}`))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Replace", mock.Anything, expectedReq, "alice")
}

func TestReplaceDog_ReturnsValidationError_ForInvalidBodies(t *testing.T) {
	bodies := withDogRejectedFields(map[string]string{
		"empty name":   `{"name": ""}`,
		"missing name": `{}`,
	})

	for name, body := range bodies {
		t.Run(name, func(t *testing.T) {
			mockService := new(MockDogService)
			controller := newTestDogController(mockService)

			result, err := controller.Replace(context.Background(), testDogID, "", strings.NewReader(body))

			assert.Nil(t, result)
			assertDogBadRequest(t, err)
		})
	}
}

func TestReplaceDog_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)

	result, err := controller.Replace(context.Background(), invalidDogID, "", strings.NewReader(`{"name": "a"}`))

	assert.Nil(t, result)
	assertDogNotFound(t, err)
}

func TestDeleteDog_CallsDeleteOnService(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	mockService.On("Delete", mock.Anything, testDogID, "alice").Return(nil)

	result, err := controller.Delete(context.Background(), testDogID, "alice")

	assert.NoError(t, err)
	assert.Equal(t, map[string]string{"message": "Dog with id " + testDogID + " was deleted successfully."}, result)
	mockService.AssertCalled(t, "Delete", mock.Anything, testDogID, "alice")
}

func TestDeleteDog_ReturnsServiceError(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	mockService.On("Delete", mock.Anything, testDogID, "").Return(errors.New("boom"))

	result, err := controller.Delete(context.Background(), testDogID, "")

	assert.Nil(t, result)
	assert.EqualError(t, err, "boom")
}

func TestDeleteDog_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)

	result, err := controller.Delete(context.Background(), invalidDogID, "")

	assert.Nil(t, result)
	assertDogNotFound(t, err)
	mockService.AssertNotCalled(t, "Delete", mock.Anything, mock.Anything, mock.Anything)
}
