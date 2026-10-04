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

const testDogID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

const fakeDogJSON = `{"id": "` + testDogID + `", "name": "Fake", "createdTimestamp": "2026-01-01T00:00:00.000Z", "createdBy": "alice", "updatedTimestamp": "2026-01-02T00:00:00.000Z"}`

func fakeDogDto() *models.DogDto {
	return &models.DogDto{Id: testDogID, Name: "Fake", AuditDto: models.AuditDto{
		CreatedTimestamp: "2026-01-01T00:00:00.000Z",
		CreatedBy:        "alice",
		UpdatedTimestamp: "2026-01-02T00:00:00.000Z",
	}}
}

type fakeDogController struct {
	err    error
	id     string
	limit  int
	body   string
	userID string
}

func (f *fakeDogController) Get(ctx context.Context, id string) (*models.DogDto, error) {
	f.id = id
	if f.err != nil {
		return nil, f.err
	}
	return fakeDogDto(), nil
}

func (f *fakeDogController) GetList(ctx context.Context, limit int) ([]models.DogDto, error) {
	f.limit = limit
	if f.err != nil {
		return nil, f.err
	}
	return []models.DogDto{*fakeDogDto()}, nil
}

func (f *fakeDogController) Create(ctx context.Context, userID string, body io.Reader) (*models.DogDto, error) {
	data, _ := io.ReadAll(body)
	f.body, f.userID = string(data), userID
	if f.err != nil {
		return nil, f.err
	}
	return fakeDogDto(), nil
}

func (f *fakeDogController) Replace(ctx context.Context, id, userID string, body io.Reader) (*models.DogDto, error) {
	data, _ := io.ReadAll(body)
	f.id, f.body, f.userID = id, string(data), userID
	if f.err != nil {
		return nil, f.err
	}
	return fakeDogDto(), nil
}

func (f *fakeDogController) Delete(ctx context.Context, id, userID string) (map[string]string, error) {
	f.id, f.userID = id, userID
	if f.err != nil {
		return nil, f.err
	}
	return map[string]string{"message": "Fake deleted."}, nil
}

func callDog(t *testing.T, controller *fakeDogController, method string, item bool, query, body string) (int, string) {
	t.Helper()
	request := events.APIGatewayProxyRequest{
		HTTPMethod: method,
		Resource:   "/dogs",
		Headers:    map[string]string{"x-user-id": "alice"},
		Body:       body,
	}
	if item {
		request.Resource = "/dogs/{id}"
		request.PathParameters = map[string]string{"id": testDogID}
	}
	if query != "" {
		request.QueryStringParameters = map[string]string{"limit": query[len("?limit="):]}
	}
	router := NewRouter()
	RegisterDogRoutes(router, controller)

	response, err := router.ServeRequest(context.Background(), request)

	assert.NoError(t, err)
	assert.Equal(t, "application/json", response.Headers["Content-Type"])
	return response.StatusCode, response.Body
}

func TestGetDogList_ReturnsJSONArray(t *testing.T) {
	controller := &fakeDogController{}

	status, body := callDog(t, controller, http.MethodGet, false, "?limit=5", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, `[`+fakeDogJSON+`]`, body)
	assert.Equal(t, 5, controller.limit)
}

func TestGetDogList_IgnoresInvalidLimit(t *testing.T) {
	controller := &fakeDogController{}

	status, _ := callDog(t, controller, http.MethodGet, false, "?limit=abc", "")

	assert.Equal(t, http.StatusOK, status)
	assert.Equal(t, 0, controller.limit)
}

func TestGetDog_ReturnsItem(t *testing.T) {
	controller := &fakeDogController{}

	status, body := callDog(t, controller, http.MethodGet, true, "", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, fakeDogJSON, body)
	assert.Equal(t, testDogID, controller.id)
}

func TestCreateDog_Returns201WithItem(t *testing.T) {
	controller := &fakeDogController{}

	status, body := callDog(t, controller, http.MethodPost, false, "", `{"name": "Fake"}`)

	assert.Equal(t, http.StatusCreated, status)
	assert.JSONEq(t, fakeDogJSON, body)
	assert.Equal(t, `{"name": "Fake"}`, controller.body)
	assert.Equal(t, "alice", controller.userID)
}

func TestReplaceDog_ReturnsItem(t *testing.T) {
	controller := &fakeDogController{}

	status, body := callDog(t, controller, http.MethodPut, true, "", `{"name": "Fake"}`)

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, fakeDogJSON, body)
	assert.Equal(t, testDogID, controller.id)
	assert.Equal(t, `{"name": "Fake"}`, controller.body)
	assert.Equal(t, "alice", controller.userID)
}

func TestDeleteDog_ReturnsMessage(t *testing.T) {
	controller := &fakeDogController{}

	status, body := callDog(t, controller, http.MethodDelete, true, "", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, `{"message": "Fake deleted."}`, body)
	assert.Equal(t, testDogID, controller.id)
	assert.Equal(t, "alice", controller.userID)
}

func TestDogRoutes_ReturnControllerErrorsAsJSON(t *testing.T) {
	requests := []struct {
		method string
		item   bool
	}{
		{http.MethodGet, false},
		{http.MethodPost, false},
		{http.MethodGet, true},
		{http.MethodPut, true},
		{http.MethodDelete, true},
	}

	for _, request := range requests {
		controller := &fakeDogController{err: models.NewNotFoundError("Dog", testDogID)}

		status, body := callDog(t, controller, request.method, request.item, "", `{"name": "Fake"}`)

		assert.Equal(t, http.StatusNotFound, status, request.method)
		assert.JSONEq(t, `{"errorMessage": "Dog with id `+testDogID+` was not found."}`, body)
	}
}

func TestDogRoutes_DisabledMethod_Returns405(t *testing.T) {
	requests := []struct {
		method string
		item   bool
	}{
		{http.MethodPatch, true},
		{http.MethodPut, false},
		{http.MethodPatch, false},
		{http.MethodDelete, false},
		{http.MethodPost, true},
	}

	for _, request := range requests {
		status, body := callDog(t, &fakeDogController{}, request.method, request.item, "", "")

		assert.Equal(t, http.StatusMethodNotAllowed, status, request.method)
		assert.JSONEq(t, `{"errorMessage": "Method not allowed."}`, body)
	}
}
