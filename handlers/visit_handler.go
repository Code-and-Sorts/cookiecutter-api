package handlers

import (
	"net/http"

	"kittenclaws/controllers"
)

func RegisterVisitRoutes(mux *http.ServeMux, controller controllers.VisitController) {
	mux.HandleFunc("GET /api/visits/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, func() (any, error) { return controller.Get(r.Context(), r.PathValue("id")) })
	})
	mux.HandleFunc("POST /api/visits", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusCreated, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Create(r.Context(), userID, r.Body)
		}))
	})
	mux.HandleFunc("PATCH /api/visits/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Update(r.Context(), r.PathValue("id"), userID, r.Body)
		}))
	})
	mux.HandleFunc("/api/visits", methodNotAllowed)
	mux.HandleFunc("/api/visits/{id}", methodNotAllowed)
}
