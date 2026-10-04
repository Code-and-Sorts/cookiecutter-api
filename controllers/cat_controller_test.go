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

var rejectedCatFields = map[string]string{
	"id":               `"` + testCatID + `"`,
	"isDeleted":        `false`,
	"createdTimestamp": `"2026-01-01T00:00:00.000Z"`,
	"updatedTimestamp": `"2026-01-01T00:00:00.000Z"`,
	"createdBy":        `"someone"`,
	"updatedBy":        `"someone"`,
	"color":            `"red"`,
}

func withCatRejectedFields(bodies map[string]string) map[string]string {
	for field, value := range rejectedCatFields {
		bodies[field] = `{"name": "a", "` + field + `": ` + value + `}`
	}
	return bodies
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

func TestGetCat_ReturnsCatDto(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	expected := &models.CatDto{Id: testCatID, Name: "mockCat"}
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
	expected := []models.CatDto{
		{Id: testCatID, Name: "mockCat1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockCat2"},
	}
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

func TestCreateCat_ReturnsCreatedCatDto(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	createReq := models.CreateCatRequest{Name: "mockCreateCat"}
	expected := &models.CatDto{Id: testCatID, Name: "mockCreateCat"}
	mockService.On("Create", mock.Anything, createReq, "alice").Return(expected, nil)

	result, err := controller.Create(context.Background(), "alice", strings.NewReader(`{"name": "mockCreateCat"}`))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Create", mock.Anything, createReq, "alice")
}

func TestCreateCat_ReturnsValidationError_ForInvalidBodies(t *testing.T) {
	bodies := withCatRejectedFields(map[string]string{
		"empty name":     `{"name": ""}`,
		"missing name":   `{}`,
		"number name":    `{"name": 5}`,
		"malformed JSON": `invalid json`,
		"array body":     `[]`,
	})

	for name, body := range bodies {
		t.Run(name, func(t *testing.T) {
			mockService := new(MockCatService)
			controller := newTestCatController(mockService)

			result, err := controller.Create(context.Background(), "", strings.NewReader(body))

			assert.Nil(t, result)
			assertCatBadRequest(t, err)
			mockService.AssertNotCalled(t, "Create", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestCreateCat_ReturnsValidationError_WhenBodyCannotBeRead(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)

	result, err := controller.Create(context.Background(), "", iotest.ErrReader(errors.New("broken")))

	assert.Nil(t, result)
	assertCatBadRequest(t, err)
}

func TestUpdateCat_ReturnsUpdatedCatDto(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	expectedReq := models.UpdateCatRequest{Id: testCatID, Name: "mockUpdatedCat"}
	expected := &models.CatDto{Id: testCatID, Name: "mockUpdatedCat"}
	mockService.On("Update", mock.Anything, expectedReq, "alice").Return(expected, nil)

	result, err := controller.Update(context.Background(), testCatID, "alice", strings.NewReader(`{"name": "mockUpdatedCat"}`))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Update", mock.Anything, expectedReq, "alice")
}

func TestUpdateCat_AllowsBodyWithoutName(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	expectedReq := models.UpdateCatRequest{Id: testCatID}
	expected := &models.CatDto{Id: testCatID, Name: "unchanged"}
	mockService.On("Update", mock.Anything, expectedReq, "alice").Return(expected, nil)

	result, err := controller.Update(context.Background(), testCatID, "alice", strings.NewReader(`{}`))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
}

func TestUpdateCat_ReturnsValidationError_ForInvalidBodies(t *testing.T) {
	bodies := withCatRejectedFields(map[string]string{
		"empty name":         `{"name": ""}`,
		"boolean name":       `{"name": true}`,
		"null body":          `null`,
		"only unknown field": `{"color": "red"}`,
	})

	for name, body := range bodies {
		t.Run(name, func(t *testing.T) {
			mockService := new(MockCatService)
			controller := newTestCatController(mockService)

			result, err := controller.Update(context.Background(), testCatID, "", strings.NewReader(body))

			assert.Nil(t, result)
			assertCatBadRequest(t, err)
		})
	}
}

func TestUpdateCat_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)

	result, err := controller.Update(context.Background(), invalidCatID, "", strings.NewReader(`{"name": "a"}`))

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
