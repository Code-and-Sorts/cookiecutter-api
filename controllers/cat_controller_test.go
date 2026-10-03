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

const testCatID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

const invalidCatID = "not-a-uuid"

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

func (m *MockCatService) Create(ctx context.Context, req models.CreateCatRequest, userID string) (*models.CatDto, error) {
	args := m.Called(ctx, req, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.CatDto), args.Error(1)
}

func (m *MockCatService) Update(ctx context.Context, req models.UpdateCatRequest, userID string) (*models.CatDto, error) {
	args := m.Called(ctx, req, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.CatDto), args.Error(1)
}

func (m *MockCatService) Delete(ctx context.Context, id, userID string) error {
	args := m.Called(ctx, id, userID)
	return args.Error(0)
}

func newTestCatController(mockService *MockCatService) CatController {
	validator, _ := services.NewSchemaValidator(RequestSchemas())
	return NewCatController(mockService, validator)
}

func testCatDto() *models.CatDto {
	return &models.CatDto{BaseResponse: models.BaseResponse{Id: testCatID}}
}

func assertCatNotFound(t *testing.T, err error) {
	t.Helper()
	var notFound *models.NotFoundError
	if assert.ErrorAs(t, err, &notFound) {
		assert.Equal(t, "Cat with id not-a-uuid was not found.", notFound.Message)
	}
}

func assertCatBadRequest(t *testing.T, err error) {
	t.Helper()
	assert.ErrorAs(t, err, new(*models.ValidationError))
}

var rejectedCatValues = [][2]string{
	{"name", "42"},
	{"name", "null"},
	{"name", "\"\""},
	{"name", "\"\""},
}

func TestGetCat_ReturnsCatDto(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	expected := testCatDto()
	mockService.On("Get", mock.Anything, testCatID).Return(expected, nil)

	result, err := controller.Get(context.Background(), testCatID)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
}

func TestGetCat_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)

	result, err := controller.Get(context.Background(), invalidCatID)

	assert.Nil(t, result)
	assertCatNotFound(t, err)
	mockService.AssertNotCalled(t, "Get", mock.Anything, mock.Anything)
}

func TestGetCatList_ReturnsListOfCatDto(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	expected := []models.CatDto{*testCatDto()}
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(expected, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
}

func TestGetCatList_PassesRequestedLimit(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("GetList", mock.Anything, 2).Return([]models.CatDto{}, nil)

	_, err := controller.GetList(context.Background(), 2)

	assert.NoError(t, err)
	mockService.AssertCalled(t, "GetList", mock.Anything, 2)
}

func TestGetCatList_ReturnsEmptyArray_WhenNothingIsStored(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(nil, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	body, _ := json.Marshal(result)
	assert.JSONEq(t, `[]`, string(body))
}

func TestGetCatList_ReturnsServiceError(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(nil, errors.New("boom"))

	result, err := controller.GetList(context.Background(), 0)

	assert.Nil(t, result)
	assert.EqualError(t, err, "boom")
}

const validCatCreateBody = "{\"name\": \"sample\"}"

func TestCreateCat_PassesTheValidatedRequest(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	expected := testCatDto()
	mockService.On("Create", mock.Anything, mock.Anything, "alice").Return(expected, nil)

	result, err := controller.Create(context.Background(), "alice", strings.NewReader(validCatCreateBody))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	req := mockService.Calls[0].Arguments.Get(1).(models.CreateCatRequest)
	data, _ := json.Marshal(requestProperties(t, req))
	assert.JSONEq(t, validCatCreateBody, string(data))
}

func TestCreateCat_RejectsInvalidBodies(t *testing.T) {
	required := []string{"name"}
	refused := []string{}

	for _, body := range invalidBodies(validCatCreateBody, required, refused, rejectedCatValues) {
		t.Run(body, func(t *testing.T) {
			mockService := new(MockCatService)
			controller := newTestCatController(mockService)

			result, err := controller.Create(context.Background(), "", strings.NewReader(body))

			assert.Nil(t, result)
			assertCatBadRequest(t, err)
			mockService.AssertNotCalled(t, "Create", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestCreateCat_AcceptsEdgeValues(t *testing.T) {
	for _, value := range [][2]string{
		{"name", "\"s\""},
		{"name", "\"\\ud83d\\ude3a\""},
	} {
		mockService := new(MockCatService)
		controller := newTestCatController(mockService)
		mockService.On("Create", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

		_, err := controller.Create(context.Background(), "", strings.NewReader(withProperty(validCatCreateBody, value[0], value[1])))

		assert.NoError(t, err, value[0]+"="+value[1])
		properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
		assert.JSONEq(t, value[1], string(properties[value[0]]))
	}
}

func TestCreateCat_ReturnsValidationError_WhenBodyCannotBeRead(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)

	result, err := controller.Create(context.Background(), "", iotest.ErrReader(errors.New("broken")))

	assert.Nil(t, result)
	assertCatBadRequest(t, err)
}

const validCatUpdateBody = "{\"name\": \"sample\"}"

func TestUpdateCat_PassesTheValidatedRequest(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	expected := testCatDto()
	mockService.On("Update", mock.Anything, mock.Anything, "alice").Return(expected, nil)

	result, err := controller.Update(context.Background(), testCatID, "alice", strings.NewReader(validCatUpdateBody))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateCatRequest)
	assert.Equal(t, testCatID, req.Id)
	data, _ := json.Marshal(requestProperties(t, req))
	assert.JSONEq(t, validCatUpdateBody, string(data))
	assert.Len(t, req.Sent, 1)
}

func TestUpdateCat_LeavesFieldsTheBodyOmits(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Update", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Update(context.Background(), testCatID, "", strings.NewReader(`{}`))

	assert.NoError(t, err)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateCatRequest)
	assert.Empty(t, req.Sent)
}

func TestUpdateCat_RejectsInvalidBodies(t *testing.T) {
	required := []string{}
	refused := []string{}

	for _, body := range invalidBodies(validCatUpdateBody, required, refused, rejectedCatValues) {
		t.Run(body, func(t *testing.T) {
			mockService := new(MockCatService)
			controller := newTestCatController(mockService)

			result, err := controller.Update(context.Background(), testCatID, "", strings.NewReader(body))

			assert.Nil(t, result)
			assertCatBadRequest(t, err)
			mockService.AssertNotCalled(t, "Update", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestUpdateCat_AcceptsEdgeValues(t *testing.T) {
	for _, value := range [][2]string{
		{"name", "\"s\""},
		{"name", "\"\\ud83d\\ude3a\""},
	} {
		mockService := new(MockCatService)
		controller := newTestCatController(mockService)
		mockService.On("Update", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

		_, err := controller.Update(context.Background(), testCatID, "", strings.NewReader(withProperty(validCatUpdateBody, value[0], value[1])))

		assert.NoError(t, err, value[0]+"="+value[1])
		properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
		assert.JSONEq(t, value[1], string(properties[value[0]]))
	}
}

func TestUpdateCat_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)

	result, err := controller.Update(context.Background(), invalidCatID, "", strings.NewReader(validCatUpdateBody))

	assert.Nil(t, result)
	assertCatNotFound(t, err)
}

func TestDeleteCat_CallsDeleteOnService(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Delete", mock.Anything, testCatID, "alice").Return(nil)

	result, err := controller.Delete(context.Background(), testCatID, "alice")

	assert.NoError(t, err)
	assert.Equal(t, map[string]string{"message": "Cat with id " + testCatID + " was deleted successfully."}, result)
	mockService.AssertCalled(t, "Delete", mock.Anything, testCatID, "alice")
}

func TestDeleteCat_ReturnsServiceError(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Delete", mock.Anything, testCatID, "").Return(errors.New("boom"))

	result, err := controller.Delete(context.Background(), testCatID, "")

	assert.Nil(t, result)
	assert.EqualError(t, err, "boom")
}

func TestDeleteCat_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)

	result, err := controller.Delete(context.Background(), invalidCatID, "")

	assert.Nil(t, result)
	assertCatNotFound(t, err)
	mockService.AssertNotCalled(t, "Delete", mock.Anything, mock.Anything, mock.Anything)
}
