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

const testVisitID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

const invalidVisitID = "not-a-uuid"

type MockVisitService struct {
	mock.Mock
}

func (m *MockVisitService) Get(ctx context.Context, id string) (*models.VisitDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.VisitDto), args.Error(1)
}

func (m *MockVisitService) Create(ctx context.Context, req models.CreateVisitRequest, userID string) (*models.VisitDto, error) {
	args := m.Called(ctx, req, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.VisitDto), args.Error(1)
}

func (m *MockVisitService) Update(ctx context.Context, req models.UpdateVisitRequest, userID string) (*models.VisitDto, error) {
	args := m.Called(ctx, req, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.VisitDto), args.Error(1)
}

func newTestVisitController(mockService *MockVisitService) VisitController {
	validator, _ := services.NewSchemaValidator(RequestSchemas())
	return NewVisitController(mockService, validator)
}

func testVisitDto() *models.VisitDto {
	return &models.VisitDto{BaseResponse: models.BaseResponse{Id: testVisitID}}
}

func assertVisitNotFound(t *testing.T, err error) {
	t.Helper()
	var notFound *models.NotFoundError
	if assert.ErrorAs(t, err, &notFound) {
		assert.Equal(t, "Visit with id not-a-uuid was not found.", notFound.Message)
	}
}

func assertVisitBadRequest(t *testing.T, err error) {
	t.Helper()
	assert.ErrorAs(t, err, new(*models.ValidationError))
}

var rejectedVisitValues = [][2]string{
	{"tenantId", "42"},
	{"tenantId", "null"},
	{"tenantId", "\"\""},
	{"tenantId", "\"sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
	{"tenantId", "\"\""},
	{"tenantId", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
	{"region", "\"__invalid__\""},
	{"region", "null"},
	{"region", "\"EU\""},
	{"priority", "\"1\""},
	{"priority", "-1"},
	{"priority", "1.5"},
	{"priority", "9007199254740992"},
	{"rank", "\"1\""},
	{"rank", "null"},
	{"labels", "\"not-a-list\""},
	{"labels", "null"},
	{"labels", "[\"item1\", \"item1\"]"},
	{"reason", "42"},
	{"reason", "null"},
	{"visitedOn", "\"not-a-date\""},
	{"visitedOn", "null"},
	{"visitedOn", "\"2026-02-30\""},
	{"visitedOn", "\"2026-13-01\""},
	{"visitedOn", "\"0000-01-01\""},
	{"cost", "\"1\""},
	{"cost", "null"},
	{"cost", "-1"},
	{"paid", "\"true\""},
	{"paid", "null"},
	{"checkedAt", "\"not-a-list\""},
	{"checkedAt", "null"},
	{"checkedAt", "[null]"},
}

func TestGetVisit_ReturnsVisitDto(t *testing.T) {
	mockService := new(MockVisitService)
	controller := newTestVisitController(mockService)
	expected := testVisitDto()
	mockService.On("Get", mock.Anything, testVisitID).Return(expected, nil)

	result, err := controller.Get(context.Background(), testVisitID)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
}

func TestGetVisit_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockVisitService)
	controller := newTestVisitController(mockService)

	result, err := controller.Get(context.Background(), invalidVisitID)

	assert.Nil(t, result)
	assertVisitNotFound(t, err)
	mockService.AssertNotCalled(t, "Get", mock.Anything, mock.Anything)
}

const validVisitCreateBody = "{\"cost\": 1.5, \"labels\": [], \"priority\": 1, \"rank\": 1.5, \"reason\": \"sample\", \"region\": \"eu\", \"tenantId\": \"public\", \"visitedOn\": \"2026-01-01\"}"

func TestCreateVisit_PassesTheValidatedRequest(t *testing.T) {
	mockService := new(MockVisitService)
	controller := newTestVisitController(mockService)
	expected := testVisitDto()
	mockService.On("Create", mock.Anything, mock.Anything, "alice").Return(expected, nil)

	result, err := controller.Create(context.Background(), "alice", strings.NewReader(validVisitCreateBody))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	req := mockService.Calls[0].Arguments.Get(1).(models.CreateVisitRequest)
	data, _ := json.Marshal(requestProperties(t, req))
	assert.JSONEq(t, validVisitCreateBody, string(data))
}

func TestCreateVisit_GivesFieldsLeftOutTheirDefaults(t *testing.T) {
	mockService := new(MockVisitService)
	controller := newTestVisitController(mockService)
	mockService.On("Create", mock.Anything, mock.Anything, "").Return(testVisitDto(), nil)

	_, err := controller.Create(context.Background(), "", strings.NewReader("{\"rank\": 1.5, \"reason\": \"sample\", \"visitedOn\": \"2026-01-01\"}"))

	assert.NoError(t, err)
	properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
	assert.JSONEq(t, "\"public\"", string(properties["tenantId"]))
	assert.JSONEq(t, "\"eu\"", string(properties["region"]))
	assert.JSONEq(t, "null", string(properties["priority"]))
	assert.JSONEq(t, "[]", string(properties["labels"]))
	assert.JSONEq(t, "null", string(properties["cost"]))
}

func TestCreateVisit_RejectsInvalidBodies(t *testing.T) {
	required := []string{"rank", "reason", "visitedOn"}
	refused := []string{"paid", "checkedAt"}

	for _, body := range invalidBodies(validVisitCreateBody, required, refused, rejectedVisitValues) {
		t.Run(body, func(t *testing.T) {
			mockService := new(MockVisitService)
			controller := newTestVisitController(mockService)

			result, err := controller.Create(context.Background(), "", strings.NewReader(body))

			assert.Nil(t, result)
			assertVisitBadRequest(t, err)
			mockService.AssertNotCalled(t, "Create", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestCreateVisit_AcceptsEdgeValues(t *testing.T) {
	for _, value := range [][2]string{
		{"tenantId", "\"s\""},
		{"tenantId", "\"\\ud83d\\ude3a\""},
		{"tenantId", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
		{"tenantId", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
		{"priority", "0"},
		{"priority", "1.0"},
		{"cost", "0"},
	} {
		mockService := new(MockVisitService)
		controller := newTestVisitController(mockService)
		mockService.On("Create", mock.Anything, mock.Anything, "").Return(testVisitDto(), nil)

		_, err := controller.Create(context.Background(), "", strings.NewReader(withProperty(validVisitCreateBody, value[0], value[1])))

		assert.NoError(t, err, value[0]+"="+value[1])
		properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
		assert.JSONEq(t, value[1], string(properties[value[0]]))
	}
}

func TestCreateVisit_ReturnsValidationError_WhenBodyCannotBeRead(t *testing.T) {
	mockService := new(MockVisitService)
	controller := newTestVisitController(mockService)

	result, err := controller.Create(context.Background(), "", iotest.ErrReader(errors.New("broken")))

	assert.Nil(t, result)
	assertVisitBadRequest(t, err)
}

const validVisitUpdateBody = "{\"checkedAt\": [\"2026-01-15T10:00:00.000Z\"], \"cost\": 1.5, \"labels\": [], \"paid\": false, \"priority\": 1, \"rank\": 1.5, \"tenantId\": \"public\", \"visitedOn\": \"2026-01-01\"}"

func TestUpdateVisit_PassesTheValidatedRequest(t *testing.T) {
	mockService := new(MockVisitService)
	controller := newTestVisitController(mockService)
	expected := testVisitDto()
	mockService.On("Update", mock.Anything, mock.Anything, "alice").Return(expected, nil)

	result, err := controller.Update(context.Background(), testVisitID, "alice", strings.NewReader(validVisitUpdateBody))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateVisitRequest)
	assert.Equal(t, testVisitID, req.Id)
	data, _ := json.Marshal(requestProperties(t, req))
	assert.JSONEq(t, validVisitUpdateBody, string(data))
	assert.Len(t, req.Sent, 8)
}

func TestUpdateVisit_LeavesFieldsTheBodyOmits(t *testing.T) {
	mockService := new(MockVisitService)
	controller := newTestVisitController(mockService)
	mockService.On("Update", mock.Anything, mock.Anything, "").Return(testVisitDto(), nil)

	_, err := controller.Update(context.Background(), testVisitID, "", strings.NewReader(`{}`))

	assert.NoError(t, err)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateVisitRequest)
	assert.Empty(t, req.Sent)
}

func TestUpdateVisit_NullClearsPriority(t *testing.T) {
	mockService := new(MockVisitService)
	controller := newTestVisitController(mockService)
	mockService.On("Update", mock.Anything, mock.Anything, "").Return(testVisitDto(), nil)

	_, err := controller.Update(context.Background(), testVisitID, "", strings.NewReader(`{"priority": null}`))

	assert.NoError(t, err)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateVisitRequest)
	assert.True(t, req.Sent["priority"])
	assert.Nil(t, req.Priority)
}

func TestUpdateVisit_RejectsInvalidBodies(t *testing.T) {
	required := []string{}
	refused := []string{"region", "reason"}

	for _, body := range invalidBodies(validVisitUpdateBody, required, refused, rejectedVisitValues) {
		t.Run(body, func(t *testing.T) {
			mockService := new(MockVisitService)
			controller := newTestVisitController(mockService)

			result, err := controller.Update(context.Background(), testVisitID, "", strings.NewReader(body))

			assert.Nil(t, result)
			assertVisitBadRequest(t, err)
			mockService.AssertNotCalled(t, "Update", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestUpdateVisit_AcceptsEdgeValues(t *testing.T) {
	for _, value := range [][2]string{
		{"tenantId", "\"s\""},
		{"tenantId", "\"\\ud83d\\ude3a\""},
		{"tenantId", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
		{"tenantId", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
		{"priority", "0"},
		{"priority", "1.0"},
		{"cost", "0"},
	} {
		mockService := new(MockVisitService)
		controller := newTestVisitController(mockService)
		mockService.On("Update", mock.Anything, mock.Anything, "").Return(testVisitDto(), nil)

		_, err := controller.Update(context.Background(), testVisitID, "", strings.NewReader(withProperty(validVisitUpdateBody, value[0], value[1])))

		assert.NoError(t, err, value[0]+"="+value[1])
		properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
		assert.JSONEq(t, value[1], string(properties[value[0]]))
	}
}

func TestUpdateVisit_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockVisitService)
	controller := newTestVisitController(mockService)

	result, err := controller.Update(context.Background(), invalidVisitID, "", strings.NewReader(validVisitUpdateBody))

	assert.Nil(t, result)
	assertVisitNotFound(t, err)
}
