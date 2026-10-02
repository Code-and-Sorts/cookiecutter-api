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

const testKittenClawsID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

const invalidKittenClawsID = "not-a-uuid"

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

func (m *MockKittenClawsService) Create(ctx context.Context, req models.CreateKittenClawsRequest, userID string) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, req, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *MockKittenClawsService) Update(ctx context.Context, req models.UpdateKittenClawsRequest, userID string) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, req, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *MockKittenClawsService) Delete(ctx context.Context, id, userID string) error {
	args := m.Called(ctx, id, userID)
	return args.Error(0)
}

func newTestKittenClawsController(mockService *MockKittenClawsService) KittenClawsController {
	validator, _ := services.NewSchemaValidator(RequestSchemas())
	return NewKittenClawsController(mockService, validator)
}

func assertKittenClawsNotFound(t *testing.T, err error) {
	t.Helper()
	var notFound *models.NotFoundError
	if assert.ErrorAs(t, err, &notFound) {
		assert.Equal(t, "KittenClaws with id not-a-uuid was not found.", notFound.Message)
	}
}

func assertKittenClawsBadRequest(t *testing.T, err error) {
	t.Helper()
	assert.ErrorAs(t, err, new(*models.ValidationError))
}

func TestGetKittenClaws_ReturnsKittenClawsDto(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	expected := &models.KittenClawsDto{Id: testKittenClawsID, Name: "mockKittenClaws"}
	mockService.On("Get", mock.Anything, testKittenClawsID).Return(expected, nil)

	result, err := controller.Get(context.Background(), testKittenClawsID)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
}

func TestGetKittenClaws_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)

	result, err := controller.Get(context.Background(), invalidKittenClawsID)

	assert.Nil(t, result)
	assertKittenClawsNotFound(t, err)
	mockService.AssertNotCalled(t, "Get", mock.Anything, mock.Anything)
}

func TestGetKittenClawsList_ReturnsListOfKittenClawsDto(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	expected := []models.KittenClawsDto{
		{Id: testKittenClawsID, Name: "mockKittenClaws1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockKittenClaws2"},
	}
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(expected, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
}

func TestGetKittenClawsList_PassesRequestedLimit(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	mockService.On("GetList", mock.Anything, 2).Return([]models.KittenClawsDto{}, nil)

	_, err := controller.GetList(context.Background(), 2)

	assert.NoError(t, err)
	mockService.AssertCalled(t, "GetList", mock.Anything, 2)
}

func TestGetKittenClawsList_ReturnsEmptyArray_WhenNothingIsStored(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(nil, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	body, _ := json.Marshal(result)
	assert.JSONEq(t, `[]`, string(body))
}

func TestGetKittenClawsList_ReturnsServiceError(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(nil, errors.New("boom"))

	result, err := controller.GetList(context.Background(), 0)

	assert.Nil(t, result)
	assert.EqualError(t, err, "boom")
}

func TestCreateKittenClaws_ReturnsCreatedKittenClawsDto(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	createReq := models.CreateKittenClawsRequest{Name: "mockCreateKittenClaws"}
	expected := &models.KittenClawsDto{Id: testKittenClawsID, Name: "mockCreateKittenClaws"}
	mockService.On("Create", mock.Anything, createReq, "alice").Return(expected, nil)

	result, err := controller.Create(context.Background(), "alice", strings.NewReader(`{"name": "mockCreateKittenClaws"}`))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Create", mock.Anything, createReq, "alice")
}

func TestCreateKittenClaws_ReturnsValidationError_ForInvalidBodies(t *testing.T) {
	bodies := map[string]string{
		"empty name":     `{"name": ""}`,
		"missing name":   `{}`,
		"number name":    `{"name": 5}`,
		"malformed JSON": `invalid json`,
		"array body":     `[]`,
		"unknown field":  `{"name": "a", "color": "red"}`,
		"client id":      `{"name": "a", "id": "` + testKittenClawsID + `"}`,
		"system field":   `{"name": "a", "createdBy": "someone"}`,
	}

	for name, body := range bodies {
		t.Run(name, func(t *testing.T) {
			mockService := new(MockKittenClawsService)
			controller := newTestKittenClawsController(mockService)

			result, err := controller.Create(context.Background(), "", strings.NewReader(body))

			assert.Nil(t, result)
			assertKittenClawsBadRequest(t, err)
			mockService.AssertNotCalled(t, "Create", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestCreateKittenClaws_ReturnsValidationError_WhenBodyCannotBeRead(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)

	result, err := controller.Create(context.Background(), "", iotest.ErrReader(errors.New("broken")))

	assert.Nil(t, result)
	assertKittenClawsBadRequest(t, err)
}

func TestUpdateKittenClaws_ReturnsUpdatedKittenClawsDto(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	expectedReq := models.UpdateKittenClawsRequest{Id: testKittenClawsID, Name: "mockUpdatedKittenClaws"}
	expected := &models.KittenClawsDto{Id: testKittenClawsID, Name: "mockUpdatedKittenClaws"}
	mockService.On("Update", mock.Anything, expectedReq, "alice").Return(expected, nil)

	result, err := controller.Update(context.Background(), testKittenClawsID, "alice", strings.NewReader(`{"name": "mockUpdatedKittenClaws"}`))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Update", mock.Anything, expectedReq, "alice")
}

func TestUpdateKittenClaws_AllowsBodyWithoutName(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	expectedReq := models.UpdateKittenClawsRequest{Id: testKittenClawsID}
	expected := &models.KittenClawsDto{Id: testKittenClawsID, Name: "unchanged"}
	mockService.On("Update", mock.Anything, expectedReq, "alice").Return(expected, nil)

	result, err := controller.Update(context.Background(), testKittenClawsID, "alice", strings.NewReader(`{}`))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
}

func TestUpdateKittenClaws_ReturnsValidationError_ForInvalidBodies(t *testing.T) {
	bodies := map[string]string{
		"empty name":    `{"name": ""}`,
		"boolean name":  `{"name": true}`,
		"null body":     `null`,
		"unknown field": `{"color": "red"}`,
		"client id":     `{"id": "` + testKittenClawsID + `"}`,
		"system field":  `{"updatedTimestamp": "2026-01-01T00:00:00.000Z"}`,
	}

	for name, body := range bodies {
		t.Run(name, func(t *testing.T) {
			mockService := new(MockKittenClawsService)
			controller := newTestKittenClawsController(mockService)

			result, err := controller.Update(context.Background(), testKittenClawsID, "", strings.NewReader(body))

			assert.Nil(t, result)
			assertKittenClawsBadRequest(t, err)
		})
	}
}

func TestUpdateKittenClaws_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)

	result, err := controller.Update(context.Background(), invalidKittenClawsID, "", strings.NewReader(`{"name": "a"}`))

	assert.Nil(t, result)
	assertKittenClawsNotFound(t, err)
}

func TestDeleteKittenClaws_CallsDeleteOnService(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	mockService.On("Delete", mock.Anything, testKittenClawsID, "alice").Return(nil)

	result, err := controller.Delete(context.Background(), testKittenClawsID, "alice")

	assert.NoError(t, err)
	assert.Equal(t, map[string]string{"message": "KittenClaws with id " + testKittenClawsID + " was deleted successfully."}, result)
	mockService.AssertCalled(t, "Delete", mock.Anything, testKittenClawsID, "alice")
}

func TestDeleteKittenClaws_ReturnsServiceError(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	mockService.On("Delete", mock.Anything, testKittenClawsID, "").Return(errors.New("boom"))

	result, err := controller.Delete(context.Background(), testKittenClawsID, "")

	assert.Nil(t, result)
	assert.EqualError(t, err, "boom")
}

func TestDeleteKittenClaws_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)

	result, err := controller.Delete(context.Background(), invalidKittenClawsID, "")

	assert.Nil(t, result)
	assertKittenClawsNotFound(t, err)
	mockService.AssertNotCalled(t, "Delete", mock.Anything, mock.Anything, mock.Anything)
}
