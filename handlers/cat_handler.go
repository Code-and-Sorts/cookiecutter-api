package handlers

import (
	"net/http"
	"strconv"

	"kittenclaws/controllers"
)

func RegisterCatRoutes(mux *http.ServeMux, controller controllers.CatController) {
	mux.HandleFunc("GET /cats", func(w http.ResponseWriter, r *http.Request) {
		limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))
		serve(w, r, http.StatusOK, func() (any, error) { return controller.GetList(r.Context(), limit) })
	})
	mux.HandleFunc("GET /cats/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, func() (any, error) { return controller.Get(r.Context(), r.PathValue("id")) })
	})
	mux.HandleFunc("POST /cats", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusCreated, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Create(r.Context(), userID, r.Body)
		}))
	})
	mux.HandleFunc("PATCH /cats/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Update(r.Context(), r.PathValue("id"), userID, r.Body)
		}))
	})
	mux.HandleFunc("DELETE /cats/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Delete(r.Context(), r.PathValue("id"), userID)
		}))
	})
	mux.HandleFunc("/cats", methodNotAllowed)
	mux.HandleFunc("/cats/{id}", methodNotAllowed)
}
