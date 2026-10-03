package handlers

import (
	"context"
	"encoding/json"
	"io"
	"net/http"
	"testing"

	"github.com/aws/aws-lambda-go/events"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
)

const testVisitID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

func fakeVisitJSON() string {
	data, _ := json.Marshal(models.VisitDto{BaseResponse: models.BaseResponse{Id: testVisitID}})
	return string(data)
}

type fakeVisitController struct {
	err    error
	id     string
	body   string
	userID string
}

func (f *fakeVisitController) Get(ctx context.Context, id string) (*models.VisitDto, error) {
	f.id = id
	if f.err != nil {
		return nil, f.err
	}
	return &models.VisitDto{BaseResponse: models.BaseResponse{Id: id}}, nil
}

func (f *fakeVisitController) Create(ctx context.Context, userID string, body io.Reader) (*models.VisitDto, error) {
	data, _ := io.ReadAll(body)
	f.body, f.userID = string(data), userID
	if f.err != nil {
		return nil, f.err
	}
	return &models.VisitDto{BaseResponse: models.BaseResponse{Id: testVisitID}}, nil
}

func (f *fakeVisitController) Update(ctx context.Context, id, userID string, body io.Reader) (*models.VisitDto, error) {
	data, _ := io.ReadAll(body)
	f.id, f.body, f.userID = id, string(data), userID
	if f.err != nil {
		return nil, f.err
	}
	return &models.VisitDto{BaseResponse: models.BaseResponse{Id: id}}, nil
}

func callVisit(t *testing.T, controller *fakeVisitController, method string, item bool, query, body string) (int, string) {
	t.Helper()
	request := events.APIGatewayProxyRequest{
		HTTPMethod: method,
		Resource:   "/visits",
		Headers:    map[string]string{"x-user-id": "alice"},
		Body:       body,
	}
	if item {
		request.Resource = "/visits/{id}"
		request.PathParameters = map[string]string{"id": testVisitID}
	}
	if query != "" {
		request.QueryStringParameters = map[string]string{"limit": query[len("?limit="):]}
	}
	router := NewRouter()
	RegisterVisitRoutes(router, controller)

	response, err := router.ServeRequest(context.Background(), request)

	assert.NoError(t, err)
	assert.Equal(t, "application/json", response.Headers["Content-Type"])
	return response.StatusCode, response.Body
}

func TestGetVisit_ReturnsItem(t *testing.T) {
	controller := &fakeVisitController{}

	status, body := callVisit(t, controller, http.MethodGet, true, "", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, fakeVisitJSON(), body)
	assert.Equal(t, testVisitID, controller.id)
}

func TestCreateVisit_Returns201WithItem(t *testing.T) {
	controller := &fakeVisitController{}

	status, body := callVisit(t, controller, http.MethodPost, false, "", `{"any": "body"}`)

	assert.Equal(t, http.StatusCreated, status)
	assert.JSONEq(t, fakeVisitJSON(), body)
	assert.Equal(t, `{"any": "body"}`, controller.body)
	assert.Equal(t, "alice", controller.userID)
}

func TestUpdateVisit_ReturnsItem(t *testing.T) {
	controller := &fakeVisitController{}

	status, body := callVisit(t, controller, http.MethodPatch, true, "", `{"any": "body"}`)

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, fakeVisitJSON(), body)
	assert.Equal(t, testVisitID, controller.id)
	assert.Equal(t, `{"any": "body"}`, controller.body)
	assert.Equal(t, "alice", controller.userID)
}

func TestVisitRoutes_ReturnControllerErrorsAsJSON(t *testing.T) {
	requests := []struct {
		method string
		item   bool
	}{
		{http.MethodPost, false},
		{http.MethodGet, true},
		{http.MethodPatch, true},
	}

	for _, request := range requests {
		controller := &fakeVisitController{err: models.NewNotFoundError("Visit", testVisitID)}

		status, body := callVisit(t, controller, request.method, request.item, "", `{"any": "body"}`)

		assert.Equal(t, http.StatusNotFound, status, request.method)
		assert.JSONEq(t, `{"errorMessage": "Visit with id `+testVisitID+` was not found."}`, body)
	}
}

func TestVisitRoutes_DisabledMethod_Returns405(t *testing.T) {
	requests := []struct {
		method string
		item   bool
	}{
		{http.MethodGet, false},
		{http.MethodPut, true},
		{http.MethodDelete, true},
		{http.MethodPut, false},
		{http.MethodPatch, false},
		{http.MethodDelete, false},
		{http.MethodPost, true},
	}

	for _, request := range requests {
		status, body := callVisit(t, &fakeVisitController{}, request.method, request.item, "", "")

		assert.Equal(t, http.StatusMethodNotAllowed, status, request.method)
		assert.JSONEq(t, `{"errorMessage": "Method not allowed."}`, body)
	}
}
