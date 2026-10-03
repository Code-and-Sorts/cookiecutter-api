package handlers

import (
	"net/http"

	"kittenclaws/utils"
)

func RegisterHealthRoute(mux *http.ServeMux) {
	mux.HandleFunc("GET /health", handleHealth)
	mux.HandleFunc("/health", methodNotAllowed)
}

func handleHealth(w http.ResponseWriter, r *http.Request) {
	utils.WriteJSON(w, http.StatusOK, map[string]string{"status": "ok"})
}
