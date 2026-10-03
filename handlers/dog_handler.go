package handlers

import (
	"net/http"
	"strconv"

	"kittenclaws/controllers"
)

func RegisterDogRoutes(mux *http.ServeMux, controller controllers.DogController) {
	mux.HandleFunc("GET /dogs", func(w http.ResponseWriter, r *http.Request) {
		limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))
		serve(w, r, http.StatusOK, func() (any, error) { return controller.GetList(r.Context(), limit) })
	})
	mux.HandleFunc("GET /dogs/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, func() (any, error) { return controller.Get(r.Context(), r.PathValue("id")) })
	})
	mux.HandleFunc("POST /dogs", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusCreated, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Create(r.Context(), userID, r.Body)
		}))
	})
	mux.HandleFunc("PUT /dogs/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Replace(r.Context(), r.PathValue("id"), userID, r.Body)
		}))
	})
	mux.HandleFunc("DELETE /dogs/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Delete(r.Context(), r.PathValue("id"), userID)
		}))
	})
	mux.HandleFunc("/dogs", methodNotAllowed)
	mux.HandleFunc("/dogs/{id}", methodNotAllowed)
}
