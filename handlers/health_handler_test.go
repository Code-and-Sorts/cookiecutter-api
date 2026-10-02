package handlers

import (
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/stretchr/testify/assert"
)

func callHealth(method, target string) *httptest.ResponseRecorder {
	mux := NewRouter()
	RegisterHealthRoute(mux)
	w := httptest.NewRecorder()
	mux.ServeHTTP(w, httptest.NewRequest(method, target, nil))
	return w
}

func TestHealth_Get_ReturnsOK(t *testing.T) {
	w := callHealth(http.MethodGet, "/health")

	assert.Equal(t, http.StatusOK, w.Code)
	assert.Equal(t, "application/json", w.Header().Get("Content-Type"))
	assert.JSONEq(t, `{"status": "ok"}`, w.Body.String())
}

func TestHealth_OtherMethod_Returns405(t *testing.T) {
	w := callHealth(http.MethodPost, "/health")

	assert.Equal(t, http.StatusMethodNotAllowed, w.Code)
	assert.JSONEq(t, `{"errorMessage": "Method not allowed."}`, w.Body.String())
}

func TestHealth_OnlyMatchesExactPath(t *testing.T) {
	w := callHealth(http.MethodGet, "/items/health")

	assert.Equal(t, http.StatusNotFound, w.Code)
}
