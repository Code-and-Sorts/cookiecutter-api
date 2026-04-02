package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"

	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"

	"{{cookiecutter.project_endpoint}}/controllers"
	"{{cookiecutter.project_endpoint}}/repositories"
	"{{cookiecutter.project_endpoint}}/services"
	"{{cookiecutter.project_endpoint}}/utils"
)

func main() {
	listenAddr := ":80"
	if val, ok := os.LookupEnv("FUNCTIONS_CUSTOMHANDLER_PORT"); ok {
		listenAddr = ":" + val
	}

	controller := initController()

	mux := http.NewServeMux()

	mux.HandleFunc("GET /api/{{cookiecutter.project_endpoint}}/{id}", handleGet(controller))
	mux.HandleFunc("GET /api/{{cookiecutter.project_endpoint}}", handleGetList(controller))
	mux.HandleFunc("POST /api/{{cookiecutter.project_endpoint}}", handleCreate(controller))
	mux.HandleFunc("PATCH /api/{{cookiecutter.project_endpoint}}/{id}", handleUpdate(controller))
	mux.HandleFunc("DELETE /api/{{cookiecutter.project_endpoint}}/{id}", handleDelete(controller))

	log.Printf("About to listen on %s", listenAddr)
	log.Fatal(http.ListenAndServe(listenAddr, mux))
}

func initController() controllers.{{cookiecutter.project_class_name}}Controller {
	endpoint := os.Getenv("CosmosDbEndpoint")
	key := os.Getenv("CosmosDbKey")
	databaseName := os.Getenv("CosmosDbDatabaseName")
	containerName := os.Getenv("CosmosDbContainerName")

	cred, err := azcosmos.NewKeyCredential(key)
	if err != nil {
		log.Fatalf("Failed to create Cosmos DB credential: %v", err)
	}

	client, err := azcosmos.NewClientWithKey(endpoint, cred, nil)
	if err != nil {
		log.Fatalf("Failed to create Cosmos DB client: %v", err)
	}

	container, err := client.NewContainer(databaseName, containerName)
	if err != nil {
		log.Fatalf("Failed to get Cosmos DB container: %v", err)
	}

	repo := repositories.New{{cookiecutter.project_class_name}}Repository(container)
	svc := services.New{{cookiecutter.project_class_name}}Service(repo)
	validator := services.NewSchemaValidator()
	ctrl := controllers.New{{cookiecutter.project_class_name}}Controller(svc, validator)

	return ctrl
}

func handleGet(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Get{{cookiecutter.project_class_name}} processed a request.")

		result, err := controller.Get(r.Context(), id)
		if err != nil {
			log.Printf("Exception in Get{{cookiecutter.project_class_name}}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleGetList(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Get{{cookiecutter.project_class_name}}List processed a request.")

		result, err := controller.GetList(r.Context())
		if err != nil {
			log.Printf("Exception in Get{{cookiecutter.project_class_name}}List: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleCreate(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Create{{cookiecutter.project_class_name}} processed a request.")

		result, err := controller.Create(r.Context(), r.Body)
		if err != nil {
			log.Printf("Exception in Create{{cookiecutter.project_class_name}}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(result)
	}
}

func handleUpdate(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Update{{cookiecutter.project_class_name}} processed a request.")

		result, err := controller.Update(r.Context(), id, r.Body)
		if err != nil {
			log.Printf("Exception in Update{{cookiecutter.project_class_name}}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleDelete(controller controllers.{{cookiecutter.project_class_name}}Controller) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("Delete{{cookiecutter.project_class_name}} processed a request.")

		err := controller.Delete(r.Context(), id)
		if err != nil {
			log.Printf("Exception in Delete{{cookiecutter.project_class_name}}: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{
			"message": fmt.Sprintf("{{cookiecutter.project_class_name}} with id %s was deleted successfully.", id),
		})
	}
}
