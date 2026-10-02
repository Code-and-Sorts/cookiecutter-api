package handlers

import (
	"net/http"
	"strconv"

	"kittenclaws/controllers"
)

func RegisterKittenClawsRoutes(mux *http.ServeMux, controller controllers.KittenClawsController) {
	mux.HandleFunc("GET /kittenclaws", func(w http.ResponseWriter, r *http.Request) {
		limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))
		serve(w, r, http.StatusOK, func() (any, error) { return controller.GetList(r.Context(), limit) })
	})
	mux.HandleFunc("GET /kittenclaws/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, func() (any, error) { return controller.Get(r.Context(), r.PathValue("id")) })
	})
	mux.HandleFunc("POST /kittenclaws", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusCreated, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Create(r.Context(), userID, r.Body)
		}))
	})
	mux.HandleFunc("PATCH /kittenclaws/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Update(r.Context(), r.PathValue("id"), userID, r.Body)
		}))
	})
	mux.HandleFunc("DELETE /kittenclaws/{id}", func(w http.ResponseWriter, r *http.Request) {
		serve(w, r, http.StatusOK, withUserID(r.Header, func(userID string) (any, error) {
			return controller.Delete(r.Context(), r.PathValue("id"), userID)
		}))
	})
	mux.HandleFunc("/kittenclaws", methodNotAllowed)
	mux.HandleFunc("/kittenclaws/{id}", methodNotAllowed)
}
