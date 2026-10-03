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

func (m *MockCatService) Replace(ctx context.Context, req models.ReplaceCatRequest, userID string) (*models.CatDto, error) {
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
	{"name", "42"},
	{"name", "null"},
	{"name", "\"\""},
	{"name", "\"sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
	{"name", "\"\""},
	{"name", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
	{"breed", "\"__invalid__\""},
	{"breed", "null"},
	{"breed", "\"SIAMESE\""},
	{"ageYears", "\"1\""},
	{"ageYears", "null"},
	{"ageYears", "-1"},
	{"ageYears", "41"},
	{"ageYears", "1.5"},
	{"ageYears", "9007199254740992"},
	{"weightKg", "\"1\""},
	{"weightKg", "0"},
	{"weightKg", "100"},
	{"indoor", "\"true\""},
	{"indoor", "null"},
	{"birthDate", "\"not-a-date\""},
	{"birthDate", "null"},
	{"birthDate", "\"2026-02-30\""},
	{"birthDate", "\"2026-13-01\""},
	{"birthDate", "\"0000-01-01\""},
	{"microchipId", "\"not-a-uuid\""},
	{"microchipId", "null"},
	{"ownerEmail", "42"},
	{"ownerEmail", "null"},
	{"ownerEmail", "\"not-an-email\""},
	{"website", "42"},
	{"website", "\"not a uri\""},
	{"tagCode", "42"},
	{"tagCode", "null"},
	{"tagCode", "\"!\""},
	{"tags", "\"not-a-list\""},
	{"tags", "null"},
	{"tags", "[\"item1\", \"item1\"]"},
	{"tags", "[\"item1\", \"item2\", \"item3\", \"item4\"]"},
	{"scores", "\"not-a-list\""},
	{"scores", "[null]"},
	{"scores", "[]"},
	{"adoptedAt", "\"not-a-date-time\""},
	{"adoptedAt", "\"2026-01-31T09:30:00\""},
	{"adoptedAt", "\"2026-01-31T24:00:00Z\""},
	{"adoptedAt", "\"2026-01-31T23:59:60Z\""},
	{"adoptedAt", "\"2026-01-31T09:30:00+14:60\""},
	{"adoptedAt", "\"0000-12-31T23:00:00-01:00\""},
	{"adoptedAt", "\"0001-01-01T00:00:00+01:00\""},
	{"adoptedAt", "\"9999-12-31T23:59:59-01:00\""},
	{"lastVisit", "\"not-a-date-time\""},
	{"lastVisit", "null"},
	{"lastVisit", "\"2026-01-31T09:30:00\""},
	{"lastVisit", "\"2026-01-31T24:00:00Z\""},
	{"lastVisit", "\"2026-01-31T23:59:60Z\""},
	{"lastVisit", "\"2026-01-31T09:30:00+14:60\""},
	{"lastVisit", "\"0000-12-31T23:00:00-01:00\""},
	{"lastVisit", "\"0001-01-01T00:00:00+01:00\""},
	{"lastVisit", "\"9999-12-31T23:59:59-01:00\""},
	{"notes", "42"},
	{"notes", "null"},
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

const validCatCreateBody = "{\"adoptedAt\": \"2026-01-15T10:00:00.000Z\", \"ageYears\": 0, \"birthDate\": \"2026-01-01\", \"breed\": \"tabby\", \"indoor\": true, \"labels\": [], \"lastVisit\": \"2026-01-01T00:00:00.000Z\", \"microchipId\": \"6f1c2a3b-4d5e-4f60-8a7b-000000000000\", \"name\": \"sample\", \"notes\": \"$none\", \"ownerEmail\": \"unknown@example.com\", \"priority\": 1, \"rank\": 1.5, \"region\": \"eu\", \"scores\": [1], \"tagCode\": \"ABC-123\", \"tags\": [], \"tenantId\": \"public\", \"website\": \"https://example.com/items/1\", \"weightKg\": 1.5}"

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

func TestCreateCat_GivesFieldsLeftOutTheirDefaults(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Create", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Create(context.Background(), "", strings.NewReader("{\"name\": \"sample\", \"rank\": 1.5}"))

	assert.NoError(t, err)
	properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
	assert.JSONEq(t, "\"public\"", string(properties["tenantId"]))
	assert.JSONEq(t, "\"eu\"", string(properties["region"]))
	assert.JSONEq(t, "null", string(properties["priority"]))
	assert.JSONEq(t, "[]", string(properties["labels"]))
	assert.JSONEq(t, "\"tabby\"", string(properties["breed"]))
	assert.JSONEq(t, "0", string(properties["ageYears"]))
	assert.JSONEq(t, "null", string(properties["weightKg"]))
	assert.JSONEq(t, "true", string(properties["indoor"]))
	assert.NotEqual(t, "null", string(properties["birthDate"]))
	assert.NotEqual(t, "null", string(properties["microchipId"]))
	assert.JSONEq(t, "\"unknown@example.com\"", string(properties["ownerEmail"]))
	assert.JSONEq(t, "null", string(properties["website"]))
	assert.JSONEq(t, "null", string(properties["tagCode"]))
	assert.JSONEq(t, "[]", string(properties["tags"]))
	assert.JSONEq(t, "null", string(properties["scores"]))
	assert.NotEqual(t, "null", string(properties["adoptedAt"]))
	assert.JSONEq(t, "\"2026-01-01T00:00:00.000Z\"", string(properties["lastVisit"]))
	assert.JSONEq(t, "\"$none\"", string(properties["notes"]))
}

func TestCreateCat_StoresAdoptedAtInUTC(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Create", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Create(context.Background(), "", strings.NewReader(withProperty(validCatCreateBody, "adoptedAt", `"2026-01-31T11:30:00.1239+02:00"`)))

	assert.NoError(t, err)
	properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
	assert.JSONEq(t, `"2026-01-31T09:30:00.123Z"`, string(properties["adoptedAt"]))
}

func TestCreateCat_StoresLastVisitInUTC(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Create", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Create(context.Background(), "", strings.NewReader(withProperty(validCatCreateBody, "lastVisit", `"2026-01-31T11:30:00.1239+02:00"`)))

	assert.NoError(t, err)
	properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
	assert.JSONEq(t, `"2026-01-31T09:30:00.123Z"`, string(properties["lastVisit"]))
}

func TestCreateCat_RejectsInvalidBodies(t *testing.T) {
	required := []string{"rank", "name"}
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
		{"tenantId", "\"s\""},
		{"tenantId", "\"\\ud83d\\ude3a\""},
		{"tenantId", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
		{"tenantId", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
		{"priority", "0"},
		{"priority", "1.0"},
		{"name", "\"s\""},
		{"name", "\"\\ud83d\\ude3a\""},
		{"name", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
		{"name", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
		{"ageYears", "0"},
		{"ageYears", "40"},
		{"ageYears", "0.0"},
		{"tags", "[\"item1\", \"item2\", \"item3\"]"},
		{"scores", "[1]"},
		{"adoptedAt", "\"0001-01-01T00:00:00.000Z\""},
		{"adoptedAt", "\"9999-12-31T23:59:59.999Z\""},
		{"lastVisit", "\"0001-01-01T00:00:00.000Z\""},
		{"lastVisit", "\"9999-12-31T23:59:59.999Z\""},
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

const validCatUpdateBody = "{\"adoptedAt\": \"2026-01-15T10:00:00.000Z\", \"ageYears\": 0, \"indoor\": true, \"labels\": [], \"name\": \"sample\", \"notes\": \"$none\", \"ownerEmail\": \"unknown@example.com\", \"priority\": 1, \"rank\": 1.5, \"tags\": [], \"tenantId\": \"public\", \"website\": \"https://example.com/items/1\", \"weightKg\": 1.5}"

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
	assert.Len(t, req.Sent, 13)
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

func TestUpdateCat_NullClearsPriority(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Update", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Update(context.Background(), testCatID, "", strings.NewReader(`{"priority": null}`))

	assert.NoError(t, err)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateCatRequest)
	assert.True(t, req.Sent["priority"])
	assert.Nil(t, req.Priority)
}

func TestUpdateCat_NullClearsWeightKg(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Update", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Update(context.Background(), testCatID, "", strings.NewReader(`{"weightKg": null}`))

	assert.NoError(t, err)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateCatRequest)
	assert.True(t, req.Sent["weightKg"])
	assert.Nil(t, req.WeightKg)
}

func TestUpdateCat_NullClearsWebsite(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Update", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Update(context.Background(), testCatID, "", strings.NewReader(`{"website": null}`))

	assert.NoError(t, err)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateCatRequest)
	assert.True(t, req.Sent["website"])
	assert.Nil(t, req.Website)
}

func TestUpdateCat_NullClearsAdoptedAt(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Update", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Update(context.Background(), testCatID, "", strings.NewReader(`{"adoptedAt": null}`))

	assert.NoError(t, err)
	req := mockService.Calls[0].Arguments.Get(1).(models.UpdateCatRequest)
	assert.True(t, req.Sent["adoptedAt"])
	assert.Nil(t, req.AdoptedAt)
}

func TestUpdateCat_StoresAdoptedAtInUTC(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Update", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Update(context.Background(), testCatID, "", strings.NewReader(withProperty(validCatUpdateBody, "adoptedAt", `"2026-01-31T11:30:00.1239+02:00"`)))

	assert.NoError(t, err)
	properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
	assert.JSONEq(t, `"2026-01-31T09:30:00.123Z"`, string(properties["adoptedAt"]))
}

func TestUpdateCat_RejectsInvalidBodies(t *testing.T) {
	required := []string{}
	refused := []string{"region", "breed", "birthDate", "microchipId", "tagCode", "scores", "lastVisit"}

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
		{"tenantId", "\"s\""},
		{"tenantId", "\"\\ud83d\\ude3a\""},
		{"tenantId", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
		{"tenantId", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
		{"priority", "0"},
		{"priority", "1.0"},
		{"name", "\"s\""},
		{"name", "\"\\ud83d\\ude3a\""},
		{"name", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
		{"name", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
		{"ageYears", "0"},
		{"ageYears", "40"},
		{"ageYears", "0.0"},
		{"tags", "[\"item1\", \"item2\", \"item3\"]"},
		{"adoptedAt", "\"0001-01-01T00:00:00.000Z\""},
		{"adoptedAt", "\"9999-12-31T23:59:59.999Z\""},
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

const validCatReplaceBody = "{\"adoptedAt\": \"2026-01-15T10:00:00.000Z\", \"ageYears\": 0, \"birthDate\": \"2026-01-01\", \"breed\": \"tabby\", \"indoor\": true, \"labels\": [], \"lastVisit\": \"2026-01-01T00:00:00.000Z\", \"name\": \"sample\", \"notes\": \"$none\", \"ownerEmail\": \"unknown@example.com\", \"priority\": 1, \"rank\": 1.5, \"scores\": [1], \"tagCode\": \"ABC-123\", \"tags\": [], \"tenantId\": \"public\", \"website\": \"https://example.com/items/1\", \"weightKg\": 1.5}"

func TestReplaceCat_PassesTheValidatedRequest(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	expected := testCatDto()
	mockService.On("Replace", mock.Anything, mock.Anything, "alice").Return(expected, nil)

	result, err := controller.Replace(context.Background(), testCatID, "alice", strings.NewReader(validCatReplaceBody))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	req := mockService.Calls[0].Arguments.Get(1).(models.ReplaceCatRequest)
	assert.Equal(t, testCatID, req.Id)
	data, _ := json.Marshal(requestProperties(t, req))
	assert.JSONEq(t, validCatReplaceBody, string(data))
}

func TestReplaceCat_GivesFieldsLeftOutTheirDefaults(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Replace", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Replace(context.Background(), testCatID, "", strings.NewReader("{\"name\": \"sample\", \"rank\": 1.5}"))

	assert.NoError(t, err)
	properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
	assert.JSONEq(t, "\"public\"", string(properties["tenantId"]))
	assert.JSONEq(t, "null", string(properties["priority"]))
	assert.JSONEq(t, "[]", string(properties["labels"]))
	assert.JSONEq(t, "\"tabby\"", string(properties["breed"]))
	assert.JSONEq(t, "0", string(properties["ageYears"]))
	assert.JSONEq(t, "null", string(properties["weightKg"]))
	assert.JSONEq(t, "true", string(properties["indoor"]))
	assert.NotEqual(t, "null", string(properties["birthDate"]))
	assert.JSONEq(t, "\"unknown@example.com\"", string(properties["ownerEmail"]))
	assert.JSONEq(t, "null", string(properties["website"]))
	assert.JSONEq(t, "null", string(properties["tagCode"]))
	assert.JSONEq(t, "[]", string(properties["tags"]))
	assert.JSONEq(t, "null", string(properties["scores"]))
	assert.NotEqual(t, "null", string(properties["adoptedAt"]))
	assert.JSONEq(t, "\"2026-01-01T00:00:00.000Z\"", string(properties["lastVisit"]))
	assert.JSONEq(t, "\"$none\"", string(properties["notes"]))
}

func TestReplaceCat_StoresAdoptedAtInUTC(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Replace", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Replace(context.Background(), testCatID, "", strings.NewReader(withProperty(validCatReplaceBody, "adoptedAt", `"2026-01-31T11:30:00.1239+02:00"`)))

	assert.NoError(t, err)
	properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
	assert.JSONEq(t, `"2026-01-31T09:30:00.123Z"`, string(properties["adoptedAt"]))
}

func TestReplaceCat_StoresLastVisitInUTC(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)
	mockService.On("Replace", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

	_, err := controller.Replace(context.Background(), testCatID, "", strings.NewReader(withProperty(validCatReplaceBody, "lastVisit", `"2026-01-31T11:30:00.1239+02:00"`)))

	assert.NoError(t, err)
	properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
	assert.JSONEq(t, `"2026-01-31T09:30:00.123Z"`, string(properties["lastVisit"]))
}

func TestReplaceCat_RejectsInvalidBodies(t *testing.T) {
	required := []string{"rank", "name"}
	refused := []string{"region", "microchipId"}

	for _, body := range invalidBodies(validCatReplaceBody, required, refused, rejectedCatValues) {
		t.Run(body, func(t *testing.T) {
			mockService := new(MockCatService)
			controller := newTestCatController(mockService)

			result, err := controller.Replace(context.Background(), testCatID, "", strings.NewReader(body))

			assert.Nil(t, result)
			assertCatBadRequest(t, err)
			mockService.AssertNotCalled(t, "Replace", mock.Anything, mock.Anything, mock.Anything)
		})
	}
}

func TestReplaceCat_AcceptsEdgeValues(t *testing.T) {
	for _, value := range [][2]string{
		{"tenantId", "\"s\""},
		{"tenantId", "\"\\ud83d\\ude3a\""},
		{"tenantId", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
		{"tenantId", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
		{"priority", "0"},
		{"priority", "1.0"},
		{"name", "\"s\""},
		{"name", "\"\\ud83d\\ude3a\""},
		{"name", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""},
		{"name", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""},
		{"ageYears", "0"},
		{"ageYears", "40"},
		{"ageYears", "0.0"},
		{"tags", "[\"item1\", \"item2\", \"item3\"]"},
		{"scores", "[1]"},
		{"adoptedAt", "\"0001-01-01T00:00:00.000Z\""},
		{"adoptedAt", "\"9999-12-31T23:59:59.999Z\""},
		{"lastVisit", "\"0001-01-01T00:00:00.000Z\""},
		{"lastVisit", "\"9999-12-31T23:59:59.999Z\""},
	} {
		mockService := new(MockCatService)
		controller := newTestCatController(mockService)
		mockService.On("Replace", mock.Anything, mock.Anything, "").Return(testCatDto(), nil)

		_, err := controller.Replace(context.Background(), testCatID, "", strings.NewReader(withProperty(validCatReplaceBody, value[0], value[1])))

		assert.NoError(t, err, value[0]+"="+value[1])
		properties := requestProperties(t, mockService.Calls[0].Arguments.Get(1))
		assert.JSONEq(t, value[1], string(properties[value[0]]))
	}
}

func TestReplaceCat_ReturnsNotFound_WhenIdIsNotUUID(t *testing.T) {
	mockService := new(MockCatService)
	controller := newTestCatController(mockService)

	result, err := controller.Replace(context.Background(), invalidCatID, "", strings.NewReader(validCatReplaceBody))

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
