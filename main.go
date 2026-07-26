package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"strconv"

	"cloud.google.com/go/firestore"

	"kitties/controllers"
	"kitties/repositories"
	"kitties/services"
	"kitties/utils"
)

func newValidator() services.SchemaValidator {
	validator, err := services.NewSchemaValidator(map[string]string{
		"create_request":  controllers.CreateRequestSchema,
		"update_request":  controllers.UpdateRequestSchema,
		"replace_request": controllers.ReplaceRequestSchema,
	})
	if err != nil {
		log.Fatalf("Failed to initialize schema validator: %v", err)
	}
	return validator
}

type appControllers struct {
	KittenClaws controllers.KittenClawsController
}

func main() {
	listenAddr := ":8080"
	if val, ok := os.LookupEnv("PORT"); ok {
		listenAddr = ":" + val
	}

	c, cleanup := initControllers()
	defer cleanup()

	mux := http.NewServeMux()

	mux.HandleFunc("GET /api/health", handleHealth())
	mux.HandleFunc("GET /api/kitties", handleGetKittenClawsList(c.KittenClaws))
	mux.HandleFunc("GET /api/kitties/{id}", handleGetKittenClaws(c.KittenClaws))
	mux.HandleFunc("POST /api/kitties", handleCreateKittenClaws(c.KittenClaws))
	mux.HandleFunc("PATCH /api/kitties/{id}", handleUpdateKittenClaws(c.KittenClaws))
	mux.HandleFunc("DELETE /api/kitties/{id}", handleDeleteKittenClaws(c.KittenClaws))

	log.Printf("About to listen on %s", listenAddr)
	log.Fatal(http.ListenAndServe(listenAddr, mux))
}

func initControllers() (appControllers, func()) {
	projectID := os.Getenv("GCP_PROJECT_ID")
	if projectID == "" {
		log.Fatal("GCP_PROJECT_ID environment variable is required")
	}
	databaseName := os.Getenv("FIRESTORE_DATABASE")
	if databaseName == "" {
		databaseName = "(default)"
	}

	ctx := context.Background()
	client, err := firestore.NewClientWithDatabase(ctx, projectID, databaseName)
	if err != nil {
		log.Fatalf("Failed to create Firestore client: %v", err)
	}

	validator := newValidator()
	var c appControllers
	{
		collectionName := os.Getenv("FIRESTORE_COLLECTION_Kitties")
		if collectionName == "" {
			collectionName = "kitties"
		}
		collection := client.Collection(collectionName)
		c.KittenClaws = controllers.NewKittenClawsController(services.NewKittenClawsService(repositories.NewKittenClawsRepository(collection)), validator)
	}

	return c, func() { client.Close() }
}

func handleHealth() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{"status": "ok"})
	}
}

func handleGetKittenClaws(controller controllers.KittenClawsController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("GetKittenClaws processed a request.")

		result, err := controller.Get(r.Context(), id)
		if err != nil {
			log.Printf("Exception in GetKittenClaws: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleGetKittenClawsList(controller controllers.KittenClawsController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("GetKittenClawsList processed a request.")

		limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))
		result, err := controller.GetList(r.Context(), limit)
		if err != nil {
			log.Printf("Exception in GetKittenClawsList: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleCreateKittenClaws(controller controllers.KittenClawsController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("CreateKittenClaws processed a request.")

		result, err := controller.Create(r.Context(), r.Body)
		if err != nil {
			log.Printf("Exception in CreateKittenClaws: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(result)
	}
}

func handleUpdateKittenClaws(controller controllers.KittenClawsController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("UpdateKittenClaws processed a request.")

		result, err := controller.Update(r.Context(), id, r.Body)
		if err != nil {
			log.Printf("Exception in UpdateKittenClaws: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleDeleteKittenClaws(controller controllers.KittenClawsController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("DeleteKittenClaws processed a request.")

		err := controller.Delete(r.Context(), id)
		if err != nil {
			log.Printf("Exception in DeleteKittenClaws: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{
			"message": fmt.Sprintf("KittenClaws with id %s was deleted successfully.", id),
		})
	}
}
