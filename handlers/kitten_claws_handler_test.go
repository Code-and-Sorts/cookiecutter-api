package handlers

import (
	"context"
	"io"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
)

const testKittenClawsID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

type fakeKittenClawsController struct {
	err    error
	id     string
	limit  int
	body   string
	userID string
}

func (f *fakeKittenClawsController) Get(ctx context.Context, id string) (*models.KittenClawsDto, error) {
	f.id = id
	if f.err != nil {
		return nil, f.err
	}
	return &models.KittenClawsDto{Id: id, Name: "Fake"}, nil
}

func (f *fakeKittenClawsController) GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error) {
	f.limit = limit
	if f.err != nil {
		return nil, f.err
	}
	return []models.KittenClawsDto{}, nil
}

func (f *fakeKittenClawsController) Create(ctx context.Context, userID string, body io.Reader) (*models.KittenClawsDto, error) {
	data, _ := io.ReadAll(body)
	f.body, f.userID = string(data), userID
	if f.err != nil {
		return nil, f.err
	}
	return &models.KittenClawsDto{Id: testKittenClawsID, Name: "Fake"}, nil
}

func (f *fakeKittenClawsController) Update(ctx context.Context, id, userID string, body io.Reader) (*models.KittenClawsDto, error) {
	data, _ := io.ReadAll(body)
	f.id, f.body, f.userID = id, string(data), userID
	if f.err != nil {
		return nil, f.err
	}
	return &models.KittenClawsDto{Id: id, Name: "Fake"}, nil
}

func (f *fakeKittenClawsController) Delete(ctx context.Context, id, userID string) (map[string]string, error) {
	f.id, f.userID = id, userID
	if f.err != nil {
		return nil, f.err
	}
	return map[string]string{"message": "Fake deleted."}, nil
}

func callKittenClaws(t *testing.T, controller *fakeKittenClawsController, method string, item bool, query, body string) (int, string) {
	t.Helper()
	target := "/api/kittenclaws"
	if item {
		target += "/" + testKittenClawsID
	}
	mux := NewRouter()
	RegisterKittenClawsRoutes(mux, controller)
	w := httptest.NewRecorder()
	request := httptest.NewRequest(method, target+query, strings.NewReader(body))
	request.Header.Set("X-User-Id", "alice")

	Recover(mux).ServeHTTP(w, request)

	assert.Equal(t, "application/json", w.Header().Get("Content-Type"))
	return w.Code, w.Body.String()
}

func TestKittenClawsRoutes_UnknownPath_Returns404(t *testing.T) {
	mux := NewRouter()
	RegisterKittenClawsRoutes(mux, &fakeKittenClawsController{})
	w := httptest.NewRecorder()

	mux.ServeHTTP(w, httptest.NewRequest(http.MethodGet, "/api/kittenclaws/"+testKittenClawsID+"/extra", nil))

	assert.Equal(t, http.StatusNotFound, w.Code)
	assert.JSONEq(t, `{"errorMessage": "Not found."}`, w.Body.String())
}

func TestGetKittenClawsList_ReturnsJSONArray(t *testing.T) {
	controller := &fakeKittenClawsController{}

	status, body := callKittenClaws(t, controller, http.MethodGet, false, "?limit=5", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, `[]`, body)
	assert.Equal(t, 5, controller.limit)
}

func TestGetKittenClawsList_IgnoresInvalidLimit(t *testing.T) {
	controller := &fakeKittenClawsController{}

	status, _ := callKittenClaws(t, controller, http.MethodGet, false, "?limit=abc", "")

	assert.Equal(t, http.StatusOK, status)
	assert.Equal(t, 0, controller.limit)
}

func TestGetKittenClaws_ReturnsItem(t *testing.T) {
	controller := &fakeKittenClawsController{}

	status, body := callKittenClaws(t, controller, http.MethodGet, true, "", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, `{"id": "`+testKittenClawsID+`", "name": "Fake"}`, body)
	assert.Equal(t, testKittenClawsID, controller.id)
}

func TestCreateKittenClaws_Returns201WithItem(t *testing.T) {
	controller := &fakeKittenClawsController{}

	status, body := callKittenClaws(t, controller, http.MethodPost, false, "", `{"name": "Fake"}`)

	assert.Equal(t, http.StatusCreated, status)
	assert.JSONEq(t, `{"id": "`+testKittenClawsID+`", "name": "Fake"}`, body)
	assert.Equal(t, `{"name": "Fake"}`, controller.body)
	assert.Equal(t, "alice", controller.userID)
}

func TestUpdateKittenClaws_ReturnsItem(t *testing.T) {
	controller := &fakeKittenClawsController{}

	status, body := callKittenClaws(t, controller, http.MethodPatch, true, "", `{"name": "Fake"}`)

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, `{"id": "`+testKittenClawsID+`", "name": "Fake"}`, body)
	assert.Equal(t, testKittenClawsID, controller.id)
	assert.Equal(t, `{"name": "Fake"}`, controller.body)
	assert.Equal(t, "alice", controller.userID)
}

func TestDeleteKittenClaws_ReturnsMessage(t *testing.T) {
	controller := &fakeKittenClawsController{}

	status, body := callKittenClaws(t, controller, http.MethodDelete, true, "", "")

	assert.Equal(t, http.StatusOK, status)
	assert.JSONEq(t, `{"message": "Fake deleted."}`, body)
	assert.Equal(t, testKittenClawsID, controller.id)
	assert.Equal(t, "alice", controller.userID)
}

func TestKittenClawsRoutes_ReturnControllerErrorsAsJSON(t *testing.T) {
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
		controller := &fakeKittenClawsController{err: models.NewNotFoundError("KittenClaws", testKittenClawsID)}

		status, body := callKittenClaws(t, controller, request.method, request.item, "", `{"name": "Fake"}`)

		assert.Equal(t, http.StatusNotFound, status, request.method)
		assert.JSONEq(t, `{"errorMessage": "KittenClaws with id `+testKittenClawsID+` was not found."}`, body)
	}
}

func TestKittenClawsRoutes_DisabledMethod_Returns405(t *testing.T) {
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
		status, body := callKittenClaws(t, &fakeKittenClawsController{}, request.method, request.item, "", "")

		assert.Equal(t, http.StatusMethodNotAllowed, status, request.method)
		assert.JSONEq(t, `{"errorMessage": "Method not allowed."}`, body)
	}
}
