package handlers

import (
	"context"
	"io"
	"net/http"
	"testing"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
)

const testCatID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

const fakeCatJSON = `{"id": "` + testCatID + `", "name": "Fake", "createdTimestamp": "2026-01-01T00:00:00.000Z", "createdBy": "alice", "updatedTimestamp": "2026-01-02T00:00:00.000Z"}`

func fakeCatDto() *models.CatDto {
	return &models.CatDto{Id: testCatID, Name: "Fake", AuditDto: models.AuditDto{
		CreatedTimestamp: "2026-01-01T00:00:00.000Z",
		CreatedBy:        "alice",
		UpdatedTimestamp: "2026-01-02T00:00:00.000Z",
	}}
}

type fakeCatController struct {
	err    error
	id     string
	limit  int
	body   string
	userID string
}

func (f *fakeCatController) Get(ctx context.Context, id string) (*models.CatDto, error) {
	f.id = id
	if f.err != nil {
		return nil, f.err
	}
	return fakeCatDto(), nil
}

func (f *fakeCatController) GetList(ctx context.Context, limit int) ([]models.CatDto, error) {
	f.limit = limit
	if f.err != nil {
		return nil, f.err
	}
	return []models.CatDto{*fakeCatDto()}, nil
}

func (f *fakeCatController) Create(ctx context.Context, userID string, body io.Reader) (*models.CatDto, error) {
	data, _ := io.ReadAll(body)
	f.body, f.userID = string(data), userID
	if f.err != nil {
		return nil, f.err
	}
	return fakeCatDto(), nil
}

func (f *fakeCatController) Update(ctx context.Context, id, userID string, body io.Reader) (*models.CatDto, error) {
	data, _ := io.ReadAll(body)
	f.id, f.body, f.userID = id, string(data), userID
	if f.err != nil {
		return nil, f.err
	}
	return fakeCatDto(), nil
}

func (f *fakeCatController) Delete(ctx context.Context, id, userID string) (map[string]string, error) {
	f.id, f.userID = id, userID
	if f.err != nil {
		return nil, f.err
	}
	return map[string]string{"message": "Fake deleted."}, nil
}

func callCat(t *testing.T, controller *fakeCatController, method string, item bool, query, body string) (int, string) {
	t.Helper()
	request := events.APIGatewayProxyRequest{
		HTTPMethod: method,
		Resource:   "/cats",
		Headers:    map[string]string{"x-user-id": "alice"},
		Body:       body,
	}
	if item {
		request.Resource = "/cats/{id}"
		request.PathParameters = map[string]string{"id": testCatID}
	}
	if query != "" {
		request.QueryStringParameters = map[string]string{"limit": query[len("?limit="):]}
	}
	router := NewRouter()
	RegisterCatRoutes(router, controller)

	response, err := router.ServeRequest(context.Background(), request)

	assert.NoError(t, err)
	assert.Equal(t, "application/json", response.Headers["Content-Type"])
	return response.StatusCode, response.Body
}

func TestGetCatList_ReturnsJSONArray(t *testing.T) {
	controller := &fakeCatController{}

	status, body := callCat(t, controller, http.MethodGet, false, "?limit=5", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, `[`+fakeCatJSON+`]`, body)
	assert.Equal(t, 5, controller.limit)
}

func TestGetCatList_IgnoresInvalidLimit(t *testing.T) {
	controller := &fakeCatController{}

	status, _ := callCat(t, controller, http.MethodGet, false, "?limit=abc", "")

	assert.Equal(t, http.StatusOK, status)
	assert.Equal(t, 0, controller.limit)
}

func TestGetCat_ReturnsItem(t *testing.T) {
	controller := &fakeCatController{}

	status, body := callCat(t, controller, http.MethodGet, true, "", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, fakeCatJSON, body)
	assert.Equal(t, testCatID, controller.id)
}

func TestCreateCat_Returns201WithItem(t *testing.T) {
	controller := &fakeCatController{}

	status, body := callCat(t, controller, http.MethodPost, false, "", `{"name": "Fake"}`)

	assert.Equal(t, http.StatusCreated, status)
	assert.JSONEq(t, fakeCatJSON, body)
	assert.Equal(t, `{"name": "Fake"}`, controller.body)
	assert.Equal(t, "alice", controller.userID)
}

func TestUpdateCat_ReturnsItem(t *testing.T) {
	controller := &fakeCatController{}

	status, body := callCat(t, controller, http.MethodPatch, true, "", `{"name": "Fake"}`)

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, fakeCatJSON, body)
	assert.Equal(t, testCatID, controller.id)
	assert.Equal(t, `{"name": "Fake"}`, controller.body)
	assert.Equal(t, "alice", controller.userID)
}

func TestDeleteCat_ReturnsMessage(t *testing.T) {
	controller := &fakeCatController{}

	status, body := callCat(t, controller, http.MethodDelete, true, "", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, `{"message": "Fake deleted."}`, body)
	assert.Equal(t, testCatID, controller.id)
	assert.Equal(t, "alice", controller.userID)
}

func TestCatRoutes_ReturnControllerErrorsAsJSON(t *testing.T) {
	requests := []struct {
		method string
		item   bool
	}{
		{http.MethodGet, false},
		{http.MethodPost, false},
		{http.MethodGet, true},
		{http.MethodPatch, true},
		{http.MethodDelete, true},
	}

	for _, request := range requests {
		controller := &fakeCatController{err: models.NewNotFoundError("Cat", testCatID)}

		status, body := callCat(t, controller, request.method, request.item, "", `{"name": "Fake"}`)

		assert.Equal(t, http.StatusNotFound, status, request.method)
		assert.JSONEq(t, `{"errorMessage": "Cat with id `+testCatID+` was not found."}`, body)
	}
}

func TestCatRoutes_DisabledMethod_Returns405(t *testing.T) {
	requests := []struct {
		method string
		item   bool
	}{
		{http.MethodPut, true},
		{http.MethodPut, false},
		{http.MethodPatch, false},
		{http.MethodDelete, false},
		{http.MethodPost, true},
	}

	for _, request := range requests {
		status, body := callCat(t, &fakeCatController{}, request.method, request.item, "", "")

		assert.Equal(t, http.StatusMethodNotAllowed, status, request.method)
		assert.JSONEq(t, `{"errorMessage": "Method not allowed."}`, body)
	}
}
