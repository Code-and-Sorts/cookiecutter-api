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

func testKittenClawsDto() *models.KittenClawsDto {
	return &models.KittenClawsDto{BaseResponse: models.BaseResponse{Id: testKittenClawsID}}
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

var rejectedKittenClawsValues = [][2]string{
	{"name", "42"},
	{"name", "null"},
	{"name", "\"\""},
	{"name", "\"\""},
}

func TestGetKittenClaws_ReturnsKittenClawsDto(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	expected := testKittenClawsDto()
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
	expected := []models.KittenClawsDto{*testKittenClawsDto()}
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

const validKittenClawsCreateBody = "{\"name\": \"sample\"}"

func TestCreateKittenClaws_PassesTheValidatedRequest(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	expected := testKittenClawsDto()
	mockService.On("Create", mock.Anything, mock.Anything, "alice").Return(expected, nil)

	result, err := controller.Create(context.Background(), "alice", strings.NewReader(validKittenClawsCreateBody))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	req := mockService.Calls[0].Arguments.Get(1).(models.CreateKittenClawsRequest)
	data, _ := json.Marshal(requestProperties(t, req))
	assert.JSONEq(t, validKittenClawsCreateBody, string(data))
}

func TestCreateKittenClaws_RejectsInvalidBodies(t *testing.T) {
	required := []string{"name"}
	refused := []string{}

	for _, body := range invalidBodies(validKittenClawsCreateBody, required, refused, rejectedKittenClawsValues) {
		t.Run(body, func(t *testing.T) {
			mockService := new(MockKittenClawsService)
			controller := newTestKittenClawsController(mockService)

			result, err := controller.Create(context.Background(), "", strings.NewReader(body))

			assert.Nil(t, result)
			assertKittenClawsBadRequest(t, err)
			mockService.AssertNotCalled(t, "Create", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestCreateKittenClaws_AcceptsEdgeValues(t *testing.T) {
	for _, value := range [][2]string{
		{"name", "\"s\""},
		{"name", "\"\\ud83d\\ude3a\""},
	} {
		mockService := new(MockKittenClawsService)
		controller := newTestKittenClawsController(mockService)
		mockService.On("Create", mock.Anything, mock.Anything, "").Return(testKittenClawsDto(), nil)

		_, err := controller.Create(context.Background(), "", strings.NewReader(withProperty(validKittenClawsCreateBody, value[0], value[1])))

		assert.NoError(t, err, value[0]+"="+value[1])
		properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
		assert.JSONEq(t, value[1], string(properties[value[0]]))
	}
}

func TestCreateKittenClaws_ReturnsValidationError_WhenBodyCannotBeRead(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)

	result, err := controller.Create(context.Background(), "", iotest.ErrReader(errors.New("broken")))

	assert.Nil(t, result)
	assertKittenClawsBadRequest(t, err)
}

const validKittenClawsUpdateBody = "{\"name\": \"sample\"}"

func TestUpdateKittenClaws_PassesTheValidatedRequest(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	expected := testKittenClawsDto()
	mockService.On("Update", mock.Anything, mock.Anything, "alice").Return(expected, nil)

	result, err := controller.Update(context.Background(), testKittenClawsID, "alice", strings.NewReader(validKittenClawsUpdateBody))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateKittenClawsRequest)
	assert.Equal(t, testKittenClawsID, req.Id)
	data, _ := json.Marshal(requestProperties(t, req))
	assert.JSONEq(t, validKittenClawsUpdateBody, string(data))
	assert.Len(t, req.Sent, 1)
}

func TestUpdateKittenClaws_LeavesFieldsTheBodyOmits(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)
	mockService.On("Update", mock.Anything, mock.Anything, "").Return(testKittenClawsDto(), nil)

	_, err := controller.Update(context.Background(), testKittenClawsID, "", strings.NewReader(`{}`))

	assert.NoError(t, err)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateKittenClawsRequest)
	assert.Empty(t, req.Sent)
}

func TestUpdateKittenClaws_RejectsInvalidBodies(t *testing.T) {
	required := []string{}
	refused := []string{}

	for _, body := range invalidBodies(validKittenClawsUpdateBody, required, refused, rejectedKittenClawsValues) {
		t.Run(body, func(t *testing.T) {
			mockService := new(MockKittenClawsService)
			controller := newTestKittenClawsController(mockService)

			result, err := controller.Update(context.Background(), testKittenClawsID, "", strings.NewReader(body))

			assert.Nil(t, result)
			assertKittenClawsBadRequest(t, err)
			mockService.AssertNotCalled(t, "Update", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestUpdateKittenClaws_AcceptsEdgeValues(t *testing.T) {
	for _, value := range [][2]string{
		{"name", "\"s\""},
		{"name", "\"\\ud83d\\ude3a\""},
	} {
		mockService := new(MockKittenClawsService)
		controller := newTestKittenClawsController(mockService)
		mockService.On("Update", mock.Anything, mock.Anything, "").Return(testKittenClawsDto(), nil)

		_, err := controller.Update(context.Background(), testKittenClawsID, "", strings.NewReader(withProperty(validKittenClawsUpdateBody, value[0], value[1])))

		assert.NoError(t, err, value[0]+"="+value[1])
		properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
		assert.JSONEq(t, value[1], string(properties[value[0]]))
	}
}

func TestUpdateKittenClaws_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockKittenClawsService)
	controller := newTestKittenClawsController(mockService)

	result, err := controller.Update(context.Background(), invalidKittenClawsID, "", strings.NewReader(validKittenClawsUpdateBody))

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
