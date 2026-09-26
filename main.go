package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"strconv"

	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"

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
	Cat controllers.CatController
	Dog controllers.DogController
}

func main() {
	listenAddr := ":80"
	if val, ok := os.LookupEnv("FUNCTIONS_CUSTOMHANDLER_PORT"); ok {
		listenAddr = ":" + val
	}

	c := initControllers()

	mux := http.NewServeMux()

	mux.HandleFunc("GET /api/health", handleHealth())
	mux.HandleFunc("GET /api/cats", handleGetCatList(c.Cat))
	mux.HandleFunc("GET /api/cats/{id}", handleGetCat(c.Cat))
	mux.HandleFunc("POST /api/cats", handleCreateCat(c.Cat))
	mux.HandleFunc("PATCH /api/cats/{id}", handleUpdateCat(c.Cat))
	mux.HandleFunc("DELETE /api/cats/{id}", handleDeleteCat(c.Cat))
	mux.HandleFunc("GET /api/dogs", handleGetDogList(c.Dog))
	mux.HandleFunc("GET /api/dogs/{id}", handleGetDog(c.Dog))
	mux.HandleFunc("POST /api/dogs", handleCreateDog(c.Dog))
	mux.HandleFunc("PUT /api/dogs/{id}", handleReplaceDog(c.Dog))
	mux.HandleFunc("DELETE /api/dogs/{id}", handleDeleteDog(c.Dog))

	log.Printf("About to listen on %s", listenAddr)
	log.Fatal(http.ListenAndServe(listenAddr, mux))
}

func initControllers() appControllers {
	endpoint := os.Getenv("CosmosDbEndpoint")
	key := os.Getenv("CosmosDbKey")
	databaseName := os.Getenv("CosmosDbDatabaseName")

	if endpoint == "" || key == "" || databaseName == "" {
		log.Fatal("CosmosDbEndpoint, CosmosDbKey and CosmosDbDatabaseName environment variables are required")
	}

	cred, err := azcosmos.NewKeyCredential(key)
	if err != nil {
		log.Fatalf("Failed to create Cosmos DB credential: %v", err)
	}

	client, err := azcosmos.NewClientWithKey(endpoint, cred, nil)
	if err != nil {
		log.Fatalf("Failed to create Cosmos DB client: %v", err)
	}

	validator := newValidator()
	var c appControllers
	{
		containerName := os.Getenv("CosmosDbContainerName_Animals")
		if containerName == "" {
			containerName = "animals"
		}
		containerClient, err := client.NewContainer(databaseName, containerName)
		if err != nil {
			log.Fatalf("Failed to get Cosmos DB container %s: %v", containerName, err)
		}
		c.Cat = controllers.NewCatController(services.NewCatService(repositories.NewCatRepository(containerClient)), validator)
	}
	{
		containerName := os.Getenv("CosmosDbContainerName_Animals")
		if containerName == "" {
			containerName = "animals"
		}
		containerClient, err := client.NewContainer(databaseName, containerName)
		if err != nil {
			log.Fatalf("Failed to get Cosmos DB container %s: %v", containerName, err)
		}
		c.Dog = controllers.NewDogController(services.NewDogService(repositories.NewDogRepository(containerClient)), validator)
	}

	return c
}

func handleHealth() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{"status": "ok"})
	}
}

func handleGetCat(controller controllers.CatController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("GetCat processed a request.")

		result, err := controller.Get(r.Context(), id)
		if err != nil {
			log.Printf("Exception in GetCat: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleGetCatList(controller controllers.CatController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("GetCatList processed a request.")

		limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))
		result, err := controller.GetList(r.Context(), limit)
		if err != nil {
			log.Printf("Exception in GetCatList: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleCreateCat(controller controllers.CatController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("CreateCat processed a request.")

		result, err := controller.Create(r.Context(), r.Body)
		if err != nil {
			log.Printf("Exception in CreateCat: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(result)
	}
}

func handleUpdateCat(controller controllers.CatController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("UpdateCat processed a request.")

		result, err := controller.Update(r.Context(), id, r.Body)
		if err != nil {
			log.Printf("Exception in UpdateCat: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleDeleteCat(controller controllers.CatController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("DeleteCat processed a request.")

		err := controller.Delete(r.Context(), id)
		if err != nil {
			log.Printf("Exception in DeleteCat: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{
			"message": fmt.Sprintf("Cat with id %s was deleted successfully.", id),
		})
	}
}

func handleGetDog(controller controllers.DogController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("GetDog processed a request.")

		result, err := controller.Get(r.Context(), id)
		if err != nil {
			log.Printf("Exception in GetDog: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleGetDogList(controller controllers.DogController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("GetDogList processed a request.")

		limit, _ := strconv.Atoi(r.URL.Query().Get("limit"))
		result, err := controller.GetList(r.Context(), limit)
		if err != nil {
			log.Printf("Exception in GetDogList: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleCreateDog(controller controllers.DogController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("CreateDog processed a request.")

		result, err := controller.Create(r.Context(), r.Body)
		if err != nil {
			log.Printf("Exception in CreateDog: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(result)
	}
}

func handleReplaceDog(controller controllers.DogController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("ReplaceDog processed a request.")

		result, err := controller.Replace(r.Context(), id, r.Body)
		if err != nil {
			log.Printf("Exception in ReplaceDog: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(result)
	}
}

func handleDeleteDog(controller controllers.DogController) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		log.Printf("DeleteDog processed a request.")

		err := controller.Delete(r.Context(), id)
		if err != nil {
			log.Printf("Exception in DeleteDog: %v", err)
			utils.DetectError(w, err)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{
			"message": fmt.Sprintf("Dog with id %s was deleted successfully.", id),
		})
	}
}
