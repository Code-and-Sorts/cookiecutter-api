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

func testDogDto() *models.DogDto {
	return &models.DogDto{BaseResponse: models.BaseResponse{Id: testDogID}}
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

var rejectedDogValues = [][2]string{
	{"name", "42"},
	{"name", "null"},
	{"name", "\"\""},
	{"name", "\"\""},
}

func TestGetDog_ReturnsDogDto(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	expected := testDogDto()
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
	expected := []models.DogDto{*testDogDto()}
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

const validDogCreateBody = "{\"name\": \"sample\"}"

func TestCreateDog_PassesTheValidatedRequest(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	expected := testDogDto()
	mockService.On("Create", mock.Anything, mock.Anything, "alice").Return(expected, nil)

	result, err := controller.Create(context.Background(), "alice", strings.NewReader(validDogCreateBody))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	req := mockService.Calls[0].Arguments.Get(1).(models.CreateDogRequest)
	data, _ := json.Marshal(requestProperties(t, req))
	assert.JSONEq(t, validDogCreateBody, string(data))
}

func TestCreateDog_RejectsInvalidBodies(t *testing.T) {
	required := []string{"name"}
	refused := []string{}

	for _, body := range invalidBodies(validDogCreateBody, required, refused, rejectedDogValues) {
		t.Run(body, func(t *testing.T) {
			mockService := new(MockDogService)
			controller := newTestDogController(mockService)

			result, err := controller.Create(context.Background(), "", strings.NewReader(body))

			assert.Nil(t, result)
			assertDogBadRequest(t, err)
			mockService.AssertNotCalled(t, "Create", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestCreateDog_AcceptsEdgeValues(t *testing.T) {
	for _, value := range [][2]string{
		{"name", "\"s\""},
		{"name", "\"\\ud83d\\ude3a\""},
	} {
		mockService := new(MockDogService)
		controller := newTestDogController(mockService)
		mockService.On("Create", mock.Anything, mock.Anything, "").Return(testDogDto(), nil)

		_, err := controller.Create(context.Background(), "", strings.NewReader(withProperty(validDogCreateBody, value[0], value[1])))

		assert.NoError(t, err, value[0]+"="+value[1])
		properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
		assert.JSONEq(t, value[1], string(properties[value[0]]))
	}
}

func TestCreateDog_ReturnsValidationError_WhenBodyCannotBeRead(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)

	result, err := controller.Create(context.Background(), "", iotest.ErrReader(errors.New("broken")))

	assert.Nil(t, result)
	assertDogBadRequest(t, err)
}

const validDogReplaceBody = "{\"name\": \"sample\"}"

func TestReplaceDog_PassesTheValidatedRequest(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)
	expected := testDogDto()
	mockService.On("Replace", mock.Anything, mock.Anything, "alice").Return(expected, nil)

	result, err := controller.Replace(context.Background(), testDogID, "alice", strings.NewReader(validDogReplaceBody))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	req := mockService.Calls[0].Arguments.Get(1).(models.ReplaceDogRequest)
	assert.Equal(t, testDogID, req.Id)
	data, _ := json.Marshal(requestProperties(t, req))
	assert.JSONEq(t, validDogReplaceBody, string(data))
}

func TestReplaceDog_RejectsInvalidBodies(t *testing.T) {
	required := []string{"name"}
	refused := []string{}

	for _, body := range invalidBodies(validDogReplaceBody, required, refused, rejectedDogValues) {
		t.Run(body, func(t *testing.T) {
			mockService := new(MockDogService)
			controller := newTestDogController(mockService)

			result, err := controller.Replace(context.Background(), testDogID, "", strings.NewReader(body))

			assert.Nil(t, result)
			assertDogBadRequest(t, err)
			mockService.AssertNotCalled(t, "Replace", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestReplaceDog_AcceptsEdgeValues(t *testing.T) {
	for _, value := range [][2]string{
		{"name", "\"s\""},
		{"name", "\"\\ud83d\\ude3a\""},
	} {
		mockService := new(MockDogService)
		controller := newTestDogController(mockService)
		mockService.On("Replace", mock.Anything, mock.Anything, "").Return(testDogDto(), nil)

		_, err := controller.Replace(context.Background(), testDogID, "", strings.NewReader(withProperty(validDogReplaceBody, value[0], value[1])))

		assert.NoError(t, err, value[0]+"="+value[1])
		properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
		assert.JSONEq(t, value[1], string(properties[value[0]]))
	}
}

func TestReplaceDog_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockDogService)
	controller := newTestDogController(mockService)

	result, err := controller.Replace(context.Background(), invalidDogID, "", strings.NewReader(validDogReplaceBody))

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
